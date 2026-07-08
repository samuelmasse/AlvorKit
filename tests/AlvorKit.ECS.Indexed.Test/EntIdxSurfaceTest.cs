namespace AlvorKit.ECS.Indexed.Test;

/// <summary>Verifies the small forwarding surfaces that sit around the indexed core paths.</summary>
[TestClass]
public class EntIdxSurfaceTest
{
    /// <summary>Verifies read-only bag wrappers expose the maintained mutable bag state.</summary>
    [TestMethod]
    public void EntIdxSurface_ReadOnlyBags_DelegateToMutableBags()
    {
        var context = new EntIdxContextBuilder();
        var plainMut = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        var gatedMut = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        var plain = new EntIdxBag<EntIdxTestComponents.IsThing>(plainMut);
        var gated = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(gatedMut);

        context.AddBag(plainMut);
        context.AddGatedBag(gatedMut);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        EntMutIdx mut = entity;

        entity.IsThing = true;

        Assert.AreEqual(1, plain.Count);
        Assert.AreEqual(1, plain.Ents.Length);
        Assert.IsTrue(plain.Contains(mut));
        Assert.AreEqual(0, gated.Count);
        Assert.IsFalse(gated.Contains(mut));

        entity.IsReady = true;

        Assert.AreEqual(1, gated.Count);
        Assert.AreEqual(1, gated.Ents.Length);
        Assert.IsTrue(gated.Contains(mut));
    }

    /// <summary>Verifies indexed handle conversion, unset forwarding, and diagnostic formatting surfaces.</summary>
    [TestMethod]
    public void EntIdxSurface_Handles_ForwardSmallSurface()
    {
        var context = new EntIdxContextBuilder();
        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        EntMutIdx mut = entity;

        entity.Value = 7;

        Ent ptrEnt = entity;
        Ent mutEnt = mut;

        Assert.AreEqual(entity.Handle, ptrEnt.Handle);
        Assert.AreEqual(entity.Handle, mutEnt.Handle);
        Assert.IsTrue(mut.Unset<int, EntIdxTestComponents.Value>());
        Assert.IsTrue(entity.ToString().StartsWith("Ent", StringComparison.Ordinal));
        Assert.IsTrue(mut.ToString().StartsWith("Ent", StringComparison.Ordinal));
    }

    /// <summary>Verifies internal hook key components advertise the hook list value types they store.</summary>
    [TestMethod]
    public void EntIdxSurface_HookKeyMetadata_UsesHookListTypes()
    {
        Assert.AreEqual(
            typeof(ReadOnlyMemory<EntIdxPreHook<int>>),
            EntIdxPre<int, EntIdxTestComponents.Value>.Component.ValueType);
        Assert.AreEqual(
            typeof(ReadOnlyMemory<EntIdxPostHook>),
            EntIdxPost<int, EntIdxTestComponents.Value>.Component.ValueType);
        Assert.AreEqual(
            typeof(ReadOnlyMemory<EntIdxPreDisposeHook>),
            EntIdxPreDispose.Component.ValueType);
    }
}
