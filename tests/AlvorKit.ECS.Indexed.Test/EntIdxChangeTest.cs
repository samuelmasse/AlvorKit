namespace AlvorKit;

[TestClass]
public class EntIdxChangeTest
{
    /// <summary>Mixed subscriptions preserve registration order within indexes and reactions on changed and equal writes.</summary>
    [TestMethod]
    public void MixedSubscriptions_PreservePhaseAndRegistrationOrder()
    {
        using var context = new EntIdxContext();
        var events = new List<string>();
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add("write1"));
        context.OnChange<int, EntIdxTestComponents.Value>((ent, in change) => events.Add("change"));
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add("write2"));
        context.AddIndex(ent => { }).OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add("index-write1"));
        context.AddIndex(ent => { }).OnChange<int, EntIdxTestComponents.Value>((ent, in change) => events.Add("index-change"));
        context.AddIndex(ent => { }).OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add("index-write2"));
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.Value = 7;
        CollectionAssert.AreEqual(new[]
        {
            "index-write1", "index-change", "index-write2", "write1", "change", "write2",
        }, events);
        events.Clear();
        ent.Value = 7;
        CollectionAssert.AreEqual(new[] { "index-write1", "index-write2", "write1", "write2" }, events);
    }

    /// <summary>Change reactions cannot evade reentrancy protection by attempting an equal write.</summary>
    [TestMethod]
    public void EqualNestedWrite_IsRejectedBeforeSuppression()
    {
        using var context = new EntIdxContext();
        context.OnChange<int, EntIdxTestComponents.Value>((ent, in change) =>
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.Value = ent.Value);
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.Clear());
        });
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.Value = 7;
        ent.Value = 7;
        Assert.AreEqual(7, ent.Value);
    }

    /// <summary>Change indexes reject equal and absent writes just like ordinary index callbacks.</summary>
    [TestMethod]
    public void ChangeIndex_RejectsMutationBeforeNoOpChecks()
    {
        using var context = new EntIdxContext();
        context.AddIndex(ent => { }).OnChange<int, EntIdxTestComponents.Value>((ent, in change) =>
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.IsOther = false);
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.UnsetName());
        });
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.IsOther = false;
        ent.Value = 7;
        Assert.IsFalse(ent.HasName);
    }

    /// <summary>Equal reference values still replace storage and reach write notifications.</summary>
    [TestMethod]
    public void EqualValue_CommitsReplacementWithoutChangeDelivery()
    {
        using var context = new EntIdxContext();
        int changes = 0;
        int writes = 0;
        context.OnChange<string?, EntIdxTestComponents.Name>((ent, in change) => changes++);
        context.OnWrite<string?, EntIdxTestComponents.Name>(ent => writes++);
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        var first = new string('x', 3);
        var second = new string('x', 3);
        ent.Name = first;
        ent.Name = second;
        Assert.AreSame(second, ent.Name);
        Assert.AreEqual(1, changes);
        Assert.AreEqual(2, writes);
    }

    /// <summary>Equal bag markers still publish writes, while membership stays unique and ready before reactions.</summary>
    [TestMethod]
    public void BagMarker_EqualPublicationPreservesMembership()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        int writes = 0;
        context.OnWrite<bool, EntIdxTestComponents.IsThing>(ent =>
        {
            writes++;
            Assert.AreEqual(ent.IsThing, bag.Contains(ent));
        });
        context.AddBag(bag);
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.IsThing = true;
        ent.IsThing = true;
        Assert.AreEqual(1, bag.Count);
        ent.UnsetIsThing();
        Assert.AreEqual(0, bag.Count);
        Assert.AreEqual(3, writes);
    }

    /// <summary>A throwing change observer unwinds guards, and later changes can complete.</summary>
    [TestMethod]
    public void ChangeException_UnwindsActiveOperation()
    {
        using var context = new EntIdxContext();
        bool fail = true;
        int changes = 0;
        context.OnChange<int, EntIdxTestComponents.Value>((ent, in change) =>
        {
            if (fail)
                throw new InvalidDataException("test change");

            changes++;
        });
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        Assert.ThrowsExactly<InvalidDataException>(() => ent.Value = 7);
        Assert.AreEqual(7, ent.Value);
        fail = false;
        ent.Value = 8;
        ent.Clear();
        Assert.AreEqual(1, changes);
    }

    /// <summary>Growing context and typed plan tables preserve each live context's registrations.</summary>
    [TestMethod]
    public void ContextGrowth_KeepsPlansIsolated()
    {
        const int count = 40;
        var contexts = new EntIdxContext[count];
        var arenas = new EntIdxArena[count];
        var ents = new EntPtrIdx[count];
        var calls = new int[count];

        try
        {
            for (int i = 0; i < count; i++)
            {
                int index = i;
                var context = contexts[i] = new();
                context.OnChange<int, EntIdxTestComponents.Value>((ent, in change) => calls[index] += change.After);
                var arena = arenas[i] = new(context);
                ents[i] = arena.Alloc();
            }

            for (int i = 0; i < count; i++)
                ents[i].Value = i + 1;

            for (int i = 0; i < count; i++)
                Assert.AreEqual(i + 1, calls[i]);
        }
        finally
        {
            for (int i = 0; i < count; i++)
            {
                arenas[i]?.Dispose();
                contexts[i]?.Dispose();
            }
        }
    }

    /// <summary>A reused context allocation never exposes the prior context's observer plan.</summary>
    [TestMethod]
    public void ContextReuse_DoesNotRetainSubscriptions()
    {
        int changes = 0;
        EntPtrIdx stale;

        using (var context = new EntIdxContext())
        {
            context.OnChange<int, EntIdxTestComponents.Value>((ent, in change) => changes++);
            using var arena = new EntIdxArena(context);
            stale = arena.Alloc();
            stale.Value = 7;
        }

        using (var context = new EntIdxContext())
        {
            using var arena = new EntIdxArena(context);
            var current = arena.Alloc();
            current.Value = 8;
            stale.Value = 9;
            Assert.IsFalse(stale.UnsetValue());
            stale.Clear();
            stale.Dispose();
            Assert.AreEqual(8, current.Value);
        }

        Assert.AreEqual(1, changes);
    }
}
