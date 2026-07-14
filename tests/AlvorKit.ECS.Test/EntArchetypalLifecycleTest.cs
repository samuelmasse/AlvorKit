namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchetypalLifecycleTest
{
    /// <summary>Clear removes sparse and archetypal components, compacts peers, and leaves the Ent reusable.</summary>
    [TestMethod]
    public void ArchetypalLifecycle_Clear_RemovesEveryStorageKindAndKeepsEntAlive()
    {
        using var arena = new EntArena();
        EntMut first = arena.Alloc();
        EntMut second = arena.Alloc();
        first.Set<int, SparseField>(1);
        first.SetArchetypal<int, FirstField, ClearArch>(10);
        first.SetArchetypal<string, SecondField, ClearArch>("first");
        first.SetArchetypal<long, FirstField, OtherClearArch>(100);
        second.SetArchetypal<int, FirstField, ClearArch>(20);
        second.SetArchetypal<string, SecondField, ClearArch>("second");

        first.Clear();

        Assert.IsTrue(first.IsAlive);
        Assert.IsFalse(first.Has<int, SparseField>());
        Assert.IsFalse(first.Has<EntArchLoc, ClearArch>());
        Assert.IsFalse(first.Has<EntArchLoc, OtherClearArch>());
        Assert.IsFalse(first.HasArchetypal<int, FirstField, ClearArch>());
        Assert.AreEqual(20, second.GetArchetypal<int, FirstField, ClearArch>());
        Assert.AreEqual("second", second.GetArchetypal<string, SecondField, ClearArch>());
        Assert.AreEqual(0, second.Get<EntArchLoc, ClearArch>().Row);

        first.SetArchetypal<int, FirstField, ClearArch>(30);
        Assert.AreEqual(30, first.GetArchetypal<int, FirstField, ClearArch>());
        first.Clear();
        second.Clear();
    }

    /// <summary>EntPtr disposal removes its dense row before the index is returned and preserves compacted peers.</summary>
    [TestMethod]
    public void ArchetypalLifecycle_EntPtrDispose_RemovesRowBeforeIndexReuse()
    {
        using var arena = new EntArena();
        EntPtr firstPtr = arena.Alloc();
        EntPtr secondPtr = arena.Alloc();
        EntMut first = firstPtr;
        EntMut second = secondPtr;
        first.SetArchetypal<int, FirstField, PtrDisposeArch>(10);
        second.SetArchetypal<int, FirstField, PtrDisposeArch>(20);

        firstPtr.Dispose();

        Assert.IsFalse(firstPtr.IsAlive);
        Assert.AreEqual(20, second.GetArchetypal<int, FirstField, PtrDisposeArch>());
        Assert.AreEqual(0, second.Get<EntArchLoc, PtrDisposeArch>().Row);

        EntPtr replacementPtr = arena.Alloc();
        EntMut replacement = replacementPtr;
        replacement.SetArchetypal<int, FirstField, PtrDisposeArch>(30);
        Assert.AreEqual(30, replacement.GetArchetypal<int, FirstField, PtrDisposeArch>());
        Assert.AreEqual(2, EntArchDiagnostics<PtrDisposeArch>.Capture().ActiveRowCount);

        secondPtr.Dispose();
        replacementPtr.Dispose();
        Assert.AreEqual(0, EntArchDiagnostics<PtrDisposeArch>.Capture().ActiveRowCount);
    }

    /// <summary>Arena disposal bulk-releases every alloc-local archetypal row and column before allocator reuse.</summary>
    [TestMethod]
    public void ArchetypalLifecycle_ArenaDispose_ReleasesAllocStateBeforeReuse()
    {
        var arena = new EntArena();
        int allocId = arena.Index;
        EntMut ent = arena.Alloc();
        ent.SetArchetypal<int, FirstField, ArenaDisposeArch>(10);
        ent.SetArchetypal<PooledReference, SecondField, ArenaDisposeArch>(new("old"));
        int archId = ent.Get<EntArchLoc, ArenaDisposeArch>().ArchId;

        arena.Dispose();

        var afterDispose = EntArchDiagnostics<ArenaDisposeArch>.Capture();
        Assert.AreEqual(0, afterDispose.ActiveRowCount);
        Assert.AreEqual(0, afterDispose.RetainedStateCount);
        Assert.IsNull(EntArchColumn<int, FirstField, ArenaDisposeArch>.Values[allocId]);
        Assert.IsNull(EntArchColumn<PooledReference, SecondField, ArenaDisposeArch>.Values[allocId]);

        using var replacementArena = new EntArena();
        Assert.AreEqual(allocId, replacementArena.Index);
        EntMut replacement = replacementArena.Alloc();
        replacement.SetArchetypal<int, FirstField, ArenaDisposeArch>(20);
        replacement.SetArchetypal<PooledReference, SecondField, ArenaDisposeArch>(new("new"));
        Assert.AreEqual(archId, replacement.Get<EntArchLoc, ArenaDisposeArch>().ArchId);
        Assert.AreEqual(20, replacement.GetArchetypal<int, FirstField, ArenaDisposeArch>());
        Assert.AreEqual("new", replacement.GetArchetypal<PooledReference, SecondField, ArenaDisposeArch>()?.Value);
    }

    /// <summary>EntObj finalization defers row compaction until the alloc owner performs structural work.</summary>
    [TestMethod]
    public void ArchetypalLifecycle_EntObjFinalizer_DefersCleanupToAllocOwner()
    {
        WeakReference doomed = CreateFinalizableEnt(out EntObj survivor);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(doomed.IsAlive);

        EntMut survivorMut = (EntMut)survivor;
        survivorMut.SetArchetypal<long, SecondField, ObjFinalizeArch>(30);

        Assert.AreEqual(1, EntArchDiagnostics<ObjFinalizeArch>.Capture().ActiveRowCount);
        Assert.AreEqual(20, survivorMut.GetArchetypal<int, FirstField, ObjFinalizeArch>());
        Assert.AreEqual(30L, survivorMut.GetArchetypal<long, SecondField, ObjFinalizeArch>());
        survivor.Clear();
        GC.KeepAlive(survivor);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateFinalizableEnt(out EntObj survivor)
    {
        var doomed = new EntObj();
        survivor = new EntObj();
        ((EntMut)doomed).SetArchetypal<int, FirstField, ObjFinalizeArch>(10);
        ((EntMut)survivor).SetArchetypal<int, FirstField, ObjFinalizeArch>(20);
        return new(doomed);
    }

    private readonly record struct SparseField;
    private readonly record struct FirstField;
    private readonly record struct SecondField;
    private readonly record struct ClearArch;
    private readonly record struct OtherClearArch;
    private readonly record struct PtrDisposeArch;
    private readonly record struct ArenaDisposeArch;
    private readonly record struct ObjFinalizeArch;
    private sealed record PooledReference(string Value);
}
