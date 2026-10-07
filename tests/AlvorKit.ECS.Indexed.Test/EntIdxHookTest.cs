namespace AlvorKit;

[TestClass]
public class EntIdxHookTest
{
    /// <summary>All indexes precede reactions regardless of registration order, and storage is already committed.</summary>
    [TestMethod]
    public void Write_IndexesPrecedeReactions()
    {
        using var context = new EntIdxContext();
        var events = new List<string>();
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add($"reaction1 {ent.Value}"));
        context.AddIndex(ent => { }).OnChange<int, EntIdxTestComponents.Value>(
            (ent, in change) => events.Add($"index1 {change.Before}->{change.After} {ent.Value}"));
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add($"reaction2 {ent.Value}"));
        context.AddIndex(ent => { }).OnWrite<int, EntIdxTestComponents.Value>(
            ent => events.Add($"index2 {ent.Value}"));

        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.Value = 10;
        events.Clear();
        ent.Value = 20;

        CollectionAssert.AreEqual(new[] { "index1 10->20 20", "index2 20", "reaction1 20", "reaction2 20" }, events);
    }

    /// <summary>Presence distinguishes absent, present default, equal writes, and removal; absent Unset is silent.</summary>
    [TestMethod]
    public void Write_ReportsValuesAndPresence()
    {
        using var context = new EntIdxContext();
        var writes = new List<(int Before, int After, bool WasPresent, bool IsPresent)>();
        int notifications = 0;
        context.OnChange<int, EntIdxTestComponents.Value>((ent, in change) =>
            writes.Add((change.Before, change.After, change.WasPresent, change.IsPresent)));
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => notifications++);
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();

        Assert.IsFalse(ent.UnsetValue());
        ent.Value = 0;
        ent.Value = 0;
        ent.Value = 12;
        Assert.IsTrue(ent.UnsetValue());
        Assert.IsFalse(ent.UnsetValue());

        CollectionAssert.AreEqual(new[]
        {
            (0, 0, false, true),
            (0, 12, true, true),
            (12, 0, true, false),
        }, writes);
        Assert.AreEqual(4, notifications);
    }

    /// <summary>Same-reference array publications and present null values remain observable.</summary>
    [TestMethod]
    public void Write_PublishesSameReferenceAndNull()
    {
        using var context = new EntIdxContext();
        var writes = new List<(int[]? Value, bool Present)>();
        var changes = new List<(int[]? Before, int[]? After, bool WasPresent, bool IsPresent)>();
        context.OnWrite<int[]?, EntIdxTestComponents.Values>(ent => writes.Add((ent.Values, ent.HasValues)));
        context.OnChange<int[]?, EntIdxTestComponents.Values>((ent, in change) =>
            changes.Add((change.Before, change.After, change.WasPresent, change.IsPresent)));
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        int[] values = [1];
        ent.Values = values;
        values[0] = 2;
        ent.Values = values;
        ent.Values = null;
        ent.UnsetValues();

        Assert.AreEqual(4, writes.Count);
        Assert.AreSame(values, writes[1].Value);
        Assert.AreEqual(2, writes[1].Value![0]);
        Assert.IsTrue(writes[2].Present);
        Assert.IsNull(writes[2].Value);
        Assert.IsFalse(writes[3].Present);
        Assert.AreEqual(3, changes.Count);
        Assert.AreSame(values, changes[1].Before);
        Assert.IsTrue(changes[2].WasPresent);
        Assert.IsFalse(changes[2].IsPresent);
    }

    /// <summary>Clear and Dispose notify separately with intact state, then remove each grouped index exactly once.</summary>
    [TestMethod]
    public void Teardown_SeparatesLifecycleFromWrites()
    {
        using var context = new EntIdxContext();
        var events = new List<string>();
        context.OnClearing(ent => events.Add($"clear {ent.Value}"));
        context.OnDisposing(ent => events.Add($"dispose {ent.Value}"));
        context.AddIndex(ent => events.Add($"remove {ent.Value}"))
            .OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add("index"))
            .OnWrite<bool, EntIdxTestComponents.IsThing>(ent => events.Add("index"));
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => events.Add("reaction"));
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.Value = 33;
        ent.IsThing = true;
        events.Clear();

        ent.Clear();
        Assert.IsTrue(ent.IsAlive);
        Assert.IsFalse(ent.HasValue);
        CollectionAssert.AreEqual(new[] { "clear 33", "remove 33" }, events);
        events.Clear();
        ent.Clear();
        CollectionAssert.AreEqual(new[] { "clear 0", "remove 0" }, events);
        ent.Value = 44;
        events.Clear();
        ent.Dispose();
        ent.Dispose();
        ent.Clear();
        ent.Value = 55;

        CollectionAssert.AreEqual(new[] { "dispose 44", "remove 44" }, events);
        Assert.IsFalse(ent.IsAlive);
        Assert.IsFalse(ent.UnsetValue());
        Assert.AreEqual(0, arena.Allocated);
    }

    /// <summary>A key index handles Set, reassignment, Unset, generic Clear, and Dispose.</summary>
    [TestMethod]
    public void Index_TracksAllSupportedLifetimeOperations()
    {
        using var context = new EntIdxContext();
        var ids = new Dictionary<Guid, EntMutIdx>();
        context.AddIndex(ent =>
        {
            if (ent.Id != Guid.Empty)
                ids.Remove(ent.Id);
        }).OnChange<Guid, EntIdxTestComponents.Id>((ent, in write) =>
        {
            if (write.Before == write.After)
                return;

            if (write.Before != Guid.Empty)
                ids.Remove(write.Before);

            if (write.After != Guid.Empty)
                ids.Add(write.After, ent);
        });
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        ent.Id = first;
        Assert.AreEqual((EntMutIdx)ent, ids[first]);
        ent.Id = second;
        Assert.IsFalse(ids.ContainsKey(first));
        Assert.AreEqual((EntMutIdx)ent, ids[second]);
        ent.UnsetId();
        Assert.AreEqual(0, ids.Count);
        ent.Id = first;
        ClearGeneric((EntMutIdx)ent);
        Assert.AreEqual(0, ids.Count);
        Assert.IsTrue(ent.IsAlive);
        ent.Id = second;
        ent.Dispose();
        Assert.AreEqual(0, ids.Count);
    }

    /// <summary>Reactions can synchronously update other components and another context's Ent.</summary>
    [TestMethod]
    public void Reactions_CanWriteOtherComponentsAndContexts()
    {
        using var world = new EntIdxContext();
        using var dimension = new EntIdxContext();
        var calls = new List<int>();
        world.OnWrite<int, EntIdxTestComponents.Value>(ent => calls.Add(ent.Value));
        using var worldArena = new EntIdxArena(world);
        var profile = worldArena.Alloc();
        dimension.OnWrite<int, EntIdxTestComponents.Value>(ent =>
        {
            ent.IsOther = true;
            profile.Value = ent.Value;
        });
        using var dimensionArena = new EntIdxArena(dimension);
        var player = dimensionArena.Alloc();
        player.Value = 7;

        Assert.IsTrue(player.IsOther);
        Assert.AreEqual(7, profile.Value);
        CollectionAssert.AreEqual(new[] { 7 }, calls);
    }

    /// <summary>Bulk arena teardown invalidates handles without write, lifecycle, or index-removal callbacks.</summary>
    [TestMethod]
    public void ArenaDispose_DoesNotNotify()
    {
        using var context = new EntIdxContext();
        int calls = 0;
        context.OnClearing(ent => calls++);
        context.OnDisposing(ent => calls++);
        context.AddIndex(ent => calls++);
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => calls++);
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.Value = 1;
        calls = 0;
        arena.Dispose();
        Assert.IsFalse(ent.IsAlive);
        Assert.AreEqual(0, calls);
    }

    private static void ClearGeneric<T>(T ent) where T : IEntMut => ent.Clear();
}
