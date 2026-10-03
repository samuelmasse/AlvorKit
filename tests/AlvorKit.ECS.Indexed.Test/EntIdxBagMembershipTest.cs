namespace AlvorKit;

[TestClass]
public class EntIdxBagMembershipTest
{
    /// <summary>Rejects another context's plain-bag member regardless of the receiving bag's occupied slots.</summary>
    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 1)]
    [DataRow(1, 2)]
    [DataRow(1, 3)]
    public void Contains_PlainBag_RejectsForeignMember(int localCount, int foreignCount)
    {
        var localContext = new EntIdxContextBuilder();
        var foreignContext = new EntIdxContextBuilder();
        var localBag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        var foreignBag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        var read = new EntIdxBag<EntIdxTestComponents.IsThing>(localBag);
        localContext.AddBag(localBag);
        foreignContext.AddBag(foreignBag);

        using var localArena = new EntIdxArena(localContext.Ent);
        using var foreignArena = new EntIdxArena(foreignContext.Ent);
        var local = AllocateMembers(localArena, localCount);
        var foreign = AllocateMembers(foreignArena, foreignCount);

        Assert.AreEqual(localCount, localBag.Count);
        Assert.AreEqual(localCount, read.Ents.Length);
        Assert.AreEqual(foreignCount, foreignBag.Count);
        Assert.IsTrue(foreignBag.Contains(foreign));
        Assert.IsFalse(localBag.Ents.Contains((EntMutIdx)foreign));
        Assert.IsFalse(localBag.Contains(foreign));
        Assert.IsFalse(read.Contains(foreign));
        Assert.AreEqual(localCount > 0, localBag.Contains(local));
        Assert.AreEqual(localCount > 0, read.Contains(local));
    }

    /// <summary>Rejects another context's gated-bag member even when both bags use the same marker and gate.</summary>
    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 1)]
    [DataRow(1, 2)]
    [DataRow(1, 3)]
    public void Contains_GatedBag_RejectsForeignMember(int localCount, int foreignCount)
    {
        var localContext = new EntIdxContextBuilder();
        var foreignContext = new EntIdxContextBuilder();
        var localBag = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        var foreignBag = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        var read = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(localBag);
        localContext.AddGatedBag(localBag);
        foreignContext.AddGatedBag(foreignBag);

        using var localArena = new EntIdxArena(localContext.Ent);
        using var foreignArena = new EntIdxArena(foreignContext.Ent);
        var local = AllocateMembers(localArena, localCount);
        var foreign = AllocateMembers(foreignArena, foreignCount);

        Assert.AreEqual(localCount, localBag.Count);
        Assert.AreEqual(localCount, read.Ents.Length);
        Assert.AreEqual(foreignCount, foreignBag.Count);
        Assert.IsTrue(foreignBag.Contains(foreign));
        Assert.IsFalse(localBag.Ents.Contains((EntMutIdx)foreign));
        Assert.IsFalse(localBag.Contains(foreign));
        Assert.IsFalse(read.Contains(foreign));
        Assert.AreEqual(localCount > 0, localBag.Contains(local));
        Assert.AreEqual(localCount > 0, read.Contains(local));
    }

    /// <summary>Unregistered bags cannot claim members of registered bags with the same component keys.</summary>
    [TestMethod]
    public void Contains_UnregisteredBags_RejectRegisteredMember()
    {
        var context = new EntIdxContextBuilder();
        var plain = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        var gated = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddBag(plain);
        context.AddGatedBag(gated);

        using var arena = new EntIdxArena(context.Ent);
        var ent = AllocateMembers(arena, 1);
        Assert.IsTrue(plain.Contains(ent));
        Assert.IsTrue(gated.Contains(ent));
        Assert.IsFalse(new EntIdxBagMut<EntIdxTestComponents.IsThing>().Contains(ent));
        Assert.IsFalse(new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>().Contains(ent));
    }

    /// <summary>Default, removed, and disposed handles remain nonmembers while surviving handles remain indexed.</summary>
    [TestMethod]
    public void Contains_RejectsDefaultRemovedAndDisposedHandles()
    {
        var context = new EntIdxContextBuilder();
        var plain = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        var gated = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddBag(plain);
        context.AddGatedBag(gated);

        using var arena = new EntIdxArena(context.Ent);
        var removed = AllocateMembers(arena, 1);
        var disposed = AllocateMembers(arena, 1);
        var survivor = AllocateMembers(arena, 1);
        removed.IsThing = false;
        disposed.Dispose();

        Assert.IsFalse(plain.Contains(default));
        Assert.IsFalse(gated.Contains(default));
        Assert.IsFalse(plain.Contains(removed));
        Assert.IsFalse(gated.Contains(removed));
        Assert.IsFalse(plain.Contains(disposed));
        Assert.IsFalse(gated.Contains(disposed));
        Assert.IsTrue(plain.Contains(survivor));
        Assert.IsTrue(gated.Contains(survivor));
        Assert.AreEqual(1, plain.Count);
        Assert.AreEqual(1, gated.Count);
    }

    /// <summary>Membership checks through mutable and read wrappers allocate nothing after warmup.</summary>
    [TestMethod]
    public void Contains_AllWrappers_DoesNotAllocate()
    {
        var context = new EntIdxContextBuilder();
        var plain = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        var gated = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        var readPlain = new EntIdxBag<EntIdxTestComponents.IsThing>(plain);
        var readGated = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(gated);
        context.AddBag(plain);
        context.AddGatedBag(gated);

        using var arena = new EntIdxArena(context.Ent);
        EntMutIdx ent = AllocateMembers(arena, 1);

        for (int i = 0; i < 16; i++)
        {
            plain.Contains(ent);
            gated.Contains(ent);
            readPlain.Contains(ent);
            readGated.Contains(ent);
        }

        int matches = 0;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 4096; i++)
        {
            if (plain.Contains(ent))
                matches++;

            if (gated.Contains(ent))
                matches++;

            if (readPlain.Contains(ent))
                matches++;

            if (readGated.Contains(ent))
                matches++;
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.AreEqual(16384, matches);
        Assert.AreEqual(0L, allocated);
    }

    private static EntPtrIdx AllocateMembers(EntIdxArena arena, int count)
    {
        EntPtrIdx last = default;

        for (int i = 0; i < count; i++)
        {
            last = arena.Alloc();
            last.IsThing = true;
            last.IsReady = true;
        }

        return last;
    }
}
