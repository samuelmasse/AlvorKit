namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class EntArchQueryLifetimeTest
{
    /// <summary>A retained query rejects its disposed arena even before the allocator ID is reused.</summary>
    [TestMethod]
    public void SpanQuery_DisposedOwner_ThrowsAtEnumerationEntry()
    {
        using var arena = new EntArena();
        var query = arena.QueryArchetypal<LifetimeArch>().With<int, C0>();
        arena.Dispose();

        Assert.ThrowsExactly<EntArenaDisposedException>(() => query.GetEnumerator());
    }

    /// <summary>Root, selected, copied, and extended queries cannot bind to a replacement arena with the same ID.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void SpanQuery_RecycledAllocator_RejectsRetainedDescriptors(bool enumerateBeforeDisposal)
    {
        using var arena = new EntArena();
        var root = arena.QueryArchetypal<LifetimeArch>();
        var query = root.With<int, C0>();
        var copy = query;
        EntMut original = arena.Alloc();
        original.SetArchetypal<int, C0, LifetimeArch>(1);

        if (enumerateBeforeDisposal)
            Assert.AreEqual(1, Count(query));

        arena.Dispose();
        using var replacement = new EntArena();
        Assert.AreEqual(arena.Index, replacement.Index);
        Assert.AreEqual(arena.Generation + 1, replacement.Generation);
        Assert.IsFalse(arena.IsAlive);
        Assert.IsFalse(original.IsAlive);
        EntMut ent = replacement.Alloc();
        ent.SetArchetypal<int, C0, LifetimeArch>(42);
        ent.SetArchetypal<int, C1, LifetimeArch>(7);
        var delayed = root.With<int, C0>();
        var extended = query.With<int, C1>();

        Assert.ThrowsExactly<EntArenaDisposedException>(() => query.GetEnumerator());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => copy.GetEnumerator());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => delayed.GetEnumerator());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => extended.GetEnumerator());
        Assert.AreEqual(42, ent.GetArchetypal<int, C0, LifetimeArch>());
        Assert.AreEqual(7, ent.GetArchetypal<int, C1, LifetimeArch>());
        Assert.AreEqual(1, Count(replacement.QueryArchetypal<LifetimeArch>().With<int, C0>()));
    }

    /// <summary>Default query descriptors have no live arena and reject enumeration.</summary>
    [TestMethod]
    public void SpanQuery_DefaultDescriptors_ThrowAtEnumerationEntry()
    {
        EntArchQuery<LifetimeArch> root = default;
        EntArchQuery<LifetimeArch, EntArchSelect<int, C0, LifetimeArch>> selected = default;

        Assert.ThrowsExactly<EntArenaDisposedException>(() => root.With<int, C0>().GetEnumerator());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => selected.GetEnumerator());
    }

    /// <summary>Rows validates the retained query when requested, before any row is advanced or exposed.</summary>
    [TestMethod]
    public void Rows_DisposedOwner_ThrowsWhenRequested()
    {
        using var arena = new EntArena();
        var query = arena.QueryArchetypal<EntArchRowComponents>().WithC0();
        arena.Dispose();

        Assert.ThrowsExactly<EntArenaDisposedException>(() => query.Rows());
    }

    /// <summary>Generated selectors and rows preserve the arena generation across copies and extended selections.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Rows_RecycledAllocator_RejectsRetainedDescriptors(bool enumerateBeforeDisposal)
    {
        using var arena = new EntArena();
        var root = arena.QueryArchetypal<EntArchRowComponents>();
        var query = root.WithC0();
        var copy = query;
        EntMut original = arena.Alloc();
        original.C0 = 1;

        if (enumerateBeforeDisposal)
        {
            int count = 0;

            foreach (var row in query.Rows())
            {
                Assert.AreEqual(1, row.C0);
                count++;
            }

            Assert.AreEqual(1, count);
        }

        arena.Dispose();
        using var replacement = new EntArena();
        Assert.AreEqual(arena.Index, replacement.Index);
        Assert.AreEqual(arena.Generation + 1, replacement.Generation);
        EntMut ent = replacement.Alloc();
        ent.C0 = 42;
        ent.C1 = 7;
        var delayed = root.WithC0();
        var extended = query.WithC1();

        Assert.ThrowsExactly<EntArenaDisposedException>(() => query.Rows());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => copy.Rows());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => delayed.Rows());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => extended.Rows());
        Assert.AreEqual(42, ent.C0);
        Assert.AreEqual(7, ent.C1);
        Assert.AreEqual(1, Count(replacement.QueryArchetypal<EntArchRowComponents>().WithC0().WithC1()));
    }

    /// <summary>Generated rows reject a query selected from a default root descriptor.</summary>
    [TestMethod]
    public void Rows_DefaultDescriptor_ThrowsWhenRequested()
    {
        EntArchQuery<EntArchRowComponents> root = default;
        var query = root.WithC0();

        Assert.ThrowsExactly<EntArenaDisposedException>(() => query.Rows());
    }

    /// <summary>Repeated validated row enumeration remains allocation-free and writes through to live component storage.</summary>
    [TestMethod]
    public void Rows_LiveOwner_WarmEnumerationAllocatesNothing()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        ent.C0 = 0;
        var query = arena.QueryArchetypal<EntArchRowComponents>().WithC0();

        foreach (var row in query.Rows())
            row.C0++;

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        for (int iteration = 0; iteration < 1_000; iteration++)
        {
            foreach (var row in query.Rows())
                row.C0++;
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        Assert.AreEqual(1_001, ent.C0);
        Assert.AreEqual(0, allocated);
    }

    private static int Count<A, TSelect>(EntArchQuery<A, TSelect> query) where TSelect : struct, IEntArchSelect<A>
    {
        int count = 0;

        foreach (var chunk in query)
            count += chunk.Ents.Length;

        return count;
    }

    private readonly record struct LifetimeArch;
    private readonly record struct C0;
    private readonly record struct C1;
}
