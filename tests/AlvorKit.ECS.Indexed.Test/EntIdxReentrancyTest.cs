namespace AlvorKit;

[TestClass]
public class EntIdxReentrancyTest
{
    /// <summary>Dirty reactions never resurrect a marker during teardown, regardless of page-field creation order.</summary>
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void Teardown_DoesNotRecreateDirtyBagEntries(bool markerFirst, bool dispose)
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        var gated = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => ent.IsThing = true);
        context.AddBag(bag);
        context.AddGatedBag(gated);
        using var arena = new EntIdxArena(context);
        var target = arena.Alloc();

        if (markerFirst)
            target.IsThing = true;

        target.Value = 10;
        target.IsReady = true;
        Assert.AreEqual(1, bag.Count);
        Assert.AreEqual(1, gated.Count);

        if (dispose)
            target.Dispose();
        else ClearGeneric((EntMutIdx)target);

        Assert.AreEqual(0, bag.Count);
        Assert.AreEqual(0, gated.Count);
        Assert.IsFalse(bag.Contains(target));
        Assert.IsFalse(target.IsThing);
        Assert.AreEqual(!dispose, target.IsAlive);
    }

    /// <summary>Whole-Ent notifications cannot add fields, unset fields, recurse, or mutate through copied aliases.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Teardown_RejectsTargetMutationThroughAllIndexedAliases(bool dispose)
    {
        using var context = new EntIdxContext();
        EntPtrIdx owner = default;
        EntMutIdx alias = default;
        int calls = 0;
        void inspect(EntMutIdx ent)
        {
            calls++;
            Assert.AreEqual(10, ent.Value);
            Assert.ThrowsExactly<InvalidOperationException>(() => owner.Value = 20);
            Assert.ThrowsExactly<InvalidOperationException>(() => alias.IsOther = true);
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.UnsetValue());
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.UnsetName());
            Assert.ThrowsExactly<InvalidOperationException>(() => ClearGeneric(alias));
            Assert.ThrowsExactly<InvalidOperationException>(() => owner.Dispose());
        }
        context.OnClearing(inspect);
        context.OnDisposing(inspect);
        using var arena = new EntIdxArena(context);
        owner = arena.Alloc();
        alias = owner;
        owner.Value = 10;

        if (dispose)
            owner.Dispose();
        else owner.Clear();

        Assert.AreEqual(1, calls);
        Assert.IsFalse(alias.HasValue);
        Assert.IsFalse(alias.HasIsOther);
    }

    /// <summary>Reactions cannot tear down their Ent or owning arena/context while write delivery is active.</summary>
    [TestMethod]
    public void Reaction_RejectsDestructiveReentrancy()
    {
        using var context = new EntIdxContext();
        using var arena = new EntIdxArena(context);
        EntPtrIdx owner = default;
        context.OnWrite<int, EntIdxTestComponents.Value>(ent =>
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => owner.Dispose());
            Assert.ThrowsExactly<InvalidOperationException>(() => ClearGeneric(ent));
            Assert.ThrowsExactly<InvalidOperationException>(() => arena.Dispose());
            Assert.ThrowsExactly<InvalidOperationException>(() => context.Dispose());
        });
        owner = arena.Alloc();
        owner.Value = 5;
        Assert.IsTrue(owner.IsAlive);
        Assert.AreEqual(5, owner.Value);
    }

    /// <summary>A same-component cycle is rejected before the nested write commits.</summary>
    [TestMethod]
    public void Reaction_RejectsDirectAndIndirectCycles()
    {
        using var context = new EntIdxContext();
        context.OnWrite<int, EntIdxTestComponents.Value>(ent =>
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.Value = 99);
            ent.IsOther = true;
        });
        context.OnWrite<bool, EntIdxTestComponents.IsOther>(ent =>
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.Value = 100));
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.Value = 7;
        Assert.AreEqual(7, ent.Value);
        Assert.IsTrue(ent.IsOther);
        ent.Value = 8;
        Assert.AreEqual(8, ent.Value);
    }

    /// <summary>Index updates and removal cannot trigger Indexed writes, including writes into a different context.</summary>
    [TestMethod]
    public void Index_RejectsMutationAndAllocationAcrossContexts()
    {
        using var first = new EntIdxContext();
        using var second = new EntIdxContext();
        using var firstArena = new EntIdxArena(first);
        using var secondArena = new EntIdxArena(second);
        var foreign = secondArena.Alloc();
        int removals = 0;
        first.AddIndex(ent =>
        {
            removals++;
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.IsThing = true);
            Assert.ThrowsExactly<InvalidOperationException>(() => foreign.Value = 10);
        }).OnWrite<int, EntIdxTestComponents.Value>(ent =>
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.IsThing = true);
            Assert.ThrowsExactly<InvalidOperationException>(() => foreign.Value = 10);
            Assert.ThrowsExactly<InvalidOperationException>(() => secondArena.Alloc());
            Assert.ThrowsExactly<InvalidOperationException>(() => secondArena.Dispose());
        });
        var target = firstArena.Alloc();
        target.Value = 5;
        target.Clear();
        Assert.AreEqual(1, removals);
        Assert.IsFalse(target.IsThing);
        Assert.IsFalse(foreign.HasValue);
    }

    /// <summary>Lifecycle callbacks may operate on another Ent while the current target stays frozen.</summary>
    [TestMethod]
    public void Lifecycle_AllowsOtherEntWork()
    {
        using var context = new EntIdxContext();
        EntPtrIdx survivor = default;
        context.OnDisposing(ent => survivor.Name = "survived");
        using var arena = new EntIdxArena(context);
        var target = arena.Alloc();
        survivor = arena.Alloc();
        target.Dispose();
        Assert.AreEqual("survived", survivor.Name);
        Assert.IsFalse(target.IsAlive);
        Assert.IsTrue(survivor.IsAlive);
    }

    /// <summary>Call-stack guards unwind on an exception without retaining pointers into dead stack frames.</summary>
    [TestMethod]
    public void Throw_UnwindsDispatchGuardsWithoutRollback()
    {
        using var context = new EntIdxContext();
        bool fail = true;
        context.OnWrite<int, EntIdxTestComponents.Value>(ent =>
        {
            if (fail)
                throw new InvalidDataException("test callback");
        });
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        Assert.ThrowsExactly<InvalidDataException>(() => ent.Value = 7);
        Assert.AreEqual(7, ent.Value);
        fail = false;
        ent.Clear();
        ent.Value = 8;
        Assert.AreEqual(8, ent.Value);
    }

    /// <summary>Warmed writes, nested dirty reactions, bag maintenance, Clear, and Dispose allocate no managed memory.</summary>
    [TestMethod]
    public void DispatchAndTeardown_DoNotAllocateAfterWarmup()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => ent.IsThing = true);
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();

        for (int i = 0; i < 32; i++)
        {
            ent.Value = i;
            ent.Clear();
        }

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 4096; i++)
        {
            ent.Value = i;
            ent.Clear();
        }

        ent.Dispose();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.AreEqual(0L, allocated);
        Assert.AreEqual(0, bag.Count);
        Assert.IsFalse(ent.IsAlive);
    }

    private static void ClearGeneric<T>(T ent) where T : IEntMut => ent.Clear();
}
