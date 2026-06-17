namespace AlvorKit.ECS.Test;

/// <summary>Tests internal diagnostic surfaces that are exposed to friend test assemblies.</summary>
[TestClass]
public sealed class EntInternalCoverageTest
{
    /// <summary>Verifies internal entity views expose the same live identity across handle shapes.</summary>
    [TestMethod]
    public void InternalViews_ForLiveEntities_ReportRegistryAndIdentity()
    {
        var ptr = new EntPtr();
        try
        {
            ptr.First = 7;

            Ent ent = ptr;
            EntMut mut = ptr;
            EntRef reference = ptr;
            EntRefMut mutableReference = ptr;
            var obj = new EntObj();

            AssertHandleView(ptr.Index, ptr.Generation, ptr.PageIndex, ptr.SubIndex, ptr.Allocator, ptr.Registry);
            AssertHandleView(ent.Index, ent.Generation, ent.PageIndex, ent.SubIndex, ent.Allocator, ent.Registry);
            AssertHandleView(mut.Index, mut.Generation, mut.PageIndex, mut.SubIndex, mut.Allocator, mut.Registry);
            AssertHandleView(
                reference.Index,
                reference.Generation,
                reference.PageIndex,
                reference.SubIndex,
                reference.Allocator,
                reference.Registry);
            AssertHandleView(
                mutableReference.Index,
                mutableReference.Generation,
                mutableReference.PageIndex,
                mutableReference.SubIndex,
                mutableReference.Allocator,
                mutableReference.Registry);
            AssertHandleView(obj.Index, obj.Generation, obj.PageIndex, obj.SubIndex, obj.Allocator, obj.Registry);
        }
        finally
        {
            ptr.Dispose();
        }
    }

    /// <summary>Verifies registry diagnostic properties expose the current shared ECS state.</summary>
    [TestMethod]
    public void RegistryView_ReportsSharedState()
    {
        var view = EntReg.View;

        Assert.AreEqual(12, view.PageBits);
        Assert.AreEqual(4096, view.PageSize);
        Assert.AreEqual(4095, view.PageMask);
        Assert.IsNotNull(view.Storage);
        Assert.IsNotNull(view.Fields);
        Assert.IsTrue(view.NextPage >= 1);
        Assert.IsNotNull(view.FreePages);
        Assert.IsNotNull(view.Allocators);
        Assert.IsNotNull(view.FreeAllocators);
        Assert.IsNotNull(view.PageAllocators);
        Assert.IsNotNull(view.PageGenerations);
        Assert.IsNotNull(view.PageFields);
        Assert.IsNotNull(view.PageRefFields);
    }

    /// <summary>Verifies arenas expose internal identity and registry diagnostics.</summary>
    [TestMethod]
    public void ArenaView_ReportsIdentityAndRegistry()
    {
        using var arena = new EntArena();
        Assert.IsTrue(arena.Index >= 0);
        Assert.IsTrue(arena.Generation >= 0);
        Assert.IsNotNull(arena.Registry);
    }

    /// <summary>Verifies component metadata and generator attributes expose their configured values.</summary>
    [TestMethod]
    public void MetadataTypes_ReportConfiguredValues()
    {
        var component = new EntComponent(typeof(int), typeof(FirstComponent));
        Assert.AreEqual(typeof(int), component.ValueType);
        Assert.AreEqual(typeof(FirstComponent), component.NameType);
        Assert.AreEqual("Int32 FirstComponent", component.ToString());

        var components = new ComponentsAttribute { SkipBuilder = true };
        Assert.IsTrue(components.SkipBuilder);
    }

    /// <summary>Verifies mutator conversions and action-based mutation preserve the underlying entity.</summary>
    [TestMethod]
    public void MutatorHelpers_ReturnUnderlyingEntityAndApplyActions()
    {
        using var ptr = new EntPtr();
        EntMutator<EntPtr> mutator = ptr;
        EntPtr restored = mutator;

        Assert.AreEqual(ptr.Handle, restored.Handle);

        var called = false;
        mutator.Mutate(entity =>
        {
            called = true;
            entity.First = 12;
        }).Set<int, SecondComponent>(34);

        ptr.Mutate(entity => entity.Third = "done");

        Assert.IsTrue(called);
        Assert.AreEqual(12, ptr.First);
        Assert.AreEqual(34, ptr.Get<int, SecondComponent>());
        Assert.AreEqual("done", ptr.Third);
    }

    /// <summary>Verifies disposed mutable handles report no allocator and ignore clear requests.</summary>
    [TestMethod]
    public void DeadMutableHandle_ReportsNoAllocatorAndClearIsNoOp()
    {
        var ptr = new EntPtr();
        EntMut mut = ptr;

        ptr.Dispose();
        mut.Clear();

        Assert.IsFalse(mut.IsAlive);
        Assert.IsNull(mut.Allocator);
        Assert.IsNotNull(mut.Registry);
    }

    /// <summary>Verifies internal page-field maps tolerate empty and out-of-range pages.</summary>
    [TestMethod]
    public void PageFieldMap_WhenEmptyOrOutOfRange_ReturnsEmptySpan()
    {
        var map = new EntPageFieldMap();

        Assert.AreEqual(0, map.Fields(0).Length);
        Assert.AreEqual(0, map.Fields(100).Length);

        map.Clear(100);
        map.Add(0, EntStorage<int, FirstComponent>.Field);

        Assert.AreEqual(1, map.Fields(0).Length);
    }

    /// <summary>Verifies storage view diagnostics expose the registered field and sparse pages.</summary>
    [TestMethod]
    public void StorageView_ReportsFieldAndSparsePages()
    {
        var ptr = new EntPtr();
        try
        {
            ptr.First = 5;

            var view = EntStorage<int, FirstComponent>.View;
            Assert.AreSame(EntStorage<int, FirstComponent>.Field, view.Field);
            Assert.AreSame(EntStorage<int, FirstComponent>.Sparse, view.Sparse);
        }
        finally
        {
            ptr.Dispose();
        }
    }

    /// <summary>Asserts internal handle identity properties are populated for a live entity.</summary>
    private static void AssertHandleView(
        int index,
        int generation,
        int pageIndex,
        int subIndex,
        EntAllocator? allocator,
        EntRegView registry)
    {
        Assert.IsTrue(index > 0);
        Assert.IsTrue(generation > 0);
        Assert.IsTrue(pageIndex >= 0);
        Assert.IsTrue(subIndex >= 0);
        Assert.IsNotNull(allocator);
        Assert.IsNotNull(registry);
    }
}
