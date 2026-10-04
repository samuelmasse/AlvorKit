namespace AlvorKit;

/// <summary>Verifies Indexed registration rejects archetypal storage while mixed Ents remain usable.</summary>
[TestClass]
public class EntIdxArchetypalRegistrationTest
{
    /// <summary>Rejects pre and post hooks before either hook is stored on the context.</summary>
    [TestMethod]
    public void ArchetypalHooks_ThrowBeforeRegistration()
    {
        using var context = new EntIdxContextBuilder();
        var pre = Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddPre<int, EntIdxTestComponents.ArchValue>((ent, in value) => Assert.Fail()));
        var post = Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddPost<int, EntIdxTestComponents.ArchValue>(ent => Assert.Fail()));

        StringAssert.Contains(pre.Message, typeof(EntIdxTestComponents.ArchValue).FullName!);
        StringAssert.Contains(post.Message, "Archetypal");
        Assert.IsFalse(context.Ent.Has<ReadOnlyMemory<EntIdxPreHook<int>>, EntIdxPre<int, EntIdxTestComponents.ArchValue>>());
        Assert.IsFalse(context.Ent.Has<ReadOnlyMemory<EntIdxPostHook>, EntIdxPost<int, EntIdxTestComponents.ArchValue>>());
    }

    /// <summary>Rejects archetypal plain bag markers before their membership hooks are installed.</summary>
    [TestMethod]
    public void ArchetypalBagMarker_ThrowsBeforeRegistration()
    {
        using var context = new EntIdxContextBuilder();
        var bag = new EntIdxBagMut<EntIdxTestComponents.IsArchThing>();

        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.AddBag(bag));
        Assert.IsFalse(context.Ent.Has<ReadOnlyMemory<EntIdxPostHook>, EntIdxPost<bool, EntIdxTestComponents.IsArchThing>>());
        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Rejects an archetypal gated bag marker before registering the sparse gate's hook.</summary>
    [TestMethod]
    public void ArchetypalGatedBagMarker_ThrowsBeforeRegistration()
    {
        using var context = new EntIdxContextBuilder();
        var bag = new EntIdxGatedBagMut<EntIdxTestComponents.IsArchThing, EntIdxTestComponents.IsReady>();

        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.AddGatedBag(bag));
        Assert.IsFalse(context.Ent.Has<ReadOnlyMemory<EntIdxPostHook>, EntIdxPost<bool, EntIdxTestComponents.IsReady>>());
        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Rejects an archetypal gate before registering the sparse marker's hook.</summary>
    [TestMethod]
    public void ArchetypalBagGate_ThrowsBeforeRegistration()
    {
        using var context = new EntIdxContextBuilder();
        var bag = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsArchThing>();

        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.AddGatedBag(bag));
        Assert.IsFalse(context.Ent.Has<ReadOnlyMemory<EntIdxPostHook>, EntIdxPost<bool, EntIdxTestComponents.IsThing>>());
        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Preserves unobserved archetypal access, sparse hooks, and whole-Ent disposal on mixed Indexed Ents.</summary>
    [TestMethod]
    public void MixedEnt_SparseHooksAndWholeEntDisposalRemainSupported()
    {
        using var context = new EntIdxContextBuilder();
        int sparseWrites = 0;
        int valueAtDisposal = 0;
        context.AddPost<int, EntIdxTestComponents.Value>(ent => sparseWrites++);
        context.AddPreDispose(ent => valueAtDisposal = ent.ArchValue);

        using var arena = new EntIdxArena(context.Ent);
        EntPtrIdx ptr = arena.Alloc();
        EntMutIdx ent = ptr;
        ent.ArchValue = 10;
        Assert.IsTrue(ent.HasArchValue);
        Assert.AreEqual(10, ent.ArchValue);
        Assert.AreEqual(0, sparseWrites);
        Assert.IsTrue(ent.UnsetArchValue());
        Assert.IsFalse(ent.HasArchValue);

        ent.ArchValue = 20;
        ent.Value = 5;
        Assert.AreEqual(1, sparseWrites);

        ptr.Dispose();
        Assert.AreEqual(20, valueAtDisposal);
        Assert.AreEqual(2, sparseWrites);
        Assert.IsFalse(ent.IsAlive);
        Assert.IsFalse(ent.HasArchValue);
    }
}
