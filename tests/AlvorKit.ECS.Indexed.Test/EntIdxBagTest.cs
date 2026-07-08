namespace AlvorKit.ECS.Indexed.Test;

[TestClass]
public class EntIdxBagTest
{
    /// <summary>Verifies plain bag membership follows the marker and ignores unrelated gates.</summary>
    [TestMethod]
    public void EntIdxBag_PlainMembership_FollowsMarkerOnly()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context.Ent);
        var first = arena.Alloc();
        var second = arena.Alloc();
        EntMutIdx firstMut = first;
        EntMutIdx secondMut = second;

        first.IsReady = true;
        Assert.AreEqual(0, bag.Count);

        first.IsThing = true;
        second.IsThing = true;
        Assert.AreEqual(2, bag.Count);
        Assert.IsTrue(bag.Contains(firstMut));
        Assert.IsTrue(bag.Contains(secondMut));

        first.IsThing = false;
        Assert.AreEqual(1, bag.Count);
        Assert.IsFalse(bag.Contains(firstMut));
        Assert.AreEqual(second.Handle, bag.Ents[0].Handle);
    }

    /// <summary>Verifies gated bag membership follows the marker and gate transition matrix.</summary>
    [TestMethod]
    public void EntIdxBag_GatedMembership_FollowsMarkerAndGateTransitions()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(bag);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        EntMutIdx mut = entity;

        AssertBagState(bag, mut, false);

        entity.IsThing = true;
        AssertBagState(bag, mut, false);

        entity.IsReady = true;
        AssertBagState(bag, mut, true);

        entity.IsThing = false;
        AssertBagState(bag, mut, false);

        entity.IsThing = true;
        AssertBagState(bag, mut, true);

        entity.IsReady = false;
        AssertBagState(bag, mut, false);

        entity.IsReady = true;
        AssertBagState(bag, mut, true);

        Assert.IsTrue(entity.UnsetIsThing());
        AssertBagState(bag, mut, false);

        entity.IsReady = false;
        entity.IsReady = true;
        AssertBagState(bag, mut, false);

        entity.IsThing = true;
        AssertBagState(bag, mut, true);

        Assert.IsTrue(entity.UnsetIsReady());
        AssertBagState(bag, mut, false);
    }

    /// <summary>Verifies setting the gate before the marker still admits the entity once both are true.</summary>
    [TestMethod]
    public void EntIdxBag_GatedMembership_AllowsGateBeforeMarker()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(bag);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        EntMutIdx mut = entity;

        entity.IsReady = true;
        AssertBagState(bag, mut, false);

        entity.IsThing = true;
        AssertBagState(bag, mut, true);
    }

    /// <summary>Verifies different gates over the same marker and a plain bag can coexist.</summary>
    [TestMethod]
    public void EntIdxBag_DifferentGatesOverSameMarker_Coexist()
    {
        var context = new EntIdxContextBuilder();
        var plain = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        var gateA = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateA>();
        var gateB = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateB>();

        context.AddBag(plain);
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateA>(gateA);
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateB>(gateB);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        EntMutIdx mut = entity;

        entity.IsThing = true;
        entity.IsGateA = true;

        AssertBagState(plain, mut, true);
        AssertBagState(gateA, mut, true);
        AssertBagState(gateB, mut, false);

        entity.IsGateB = true;
        AssertBagState(gateB, mut, true);

        entity.IsGateA = false;
        AssertBagState(plain, mut, true);
        AssertBagState(gateA, mut, false);
        AssertBagState(gateB, mut, true);
    }

    /// <summary>Verifies disposing an entity removes it when marker unset runs before bag index unset.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_RemovesWhenMarkerUnsetsBeforeIndex()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        entity.IsThing = true;
        Assert.AreEqual(1, bag.Count);

        entity.Dispose();

        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Verifies manual clear removes bag entries through indexed unsets.</summary>
    [TestMethod]
    public void EntIdxBag_Clear_RemovesBagEntryThroughUnsetPipeline()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        EntMutIdx mut = entity;

        entity.IsThing = true;
        AssertBagState(bag, mut, true);

        entity.Clear();

        AssertBagState(bag, mut, false);
        Assert.IsTrue(entity.IsAlive);
    }

    /// <summary>Verifies disposing an entity removes it when bag index unset runs before marker unset.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_RemovesWhenIndexUnsetsBeforeMarker()
    {
        var context = new EntIdxContextBuilder();
        using var arena = new EntIdxArena(context.Ent);
        var dummy = arena.Alloc();
        dummy.Set<int, EntIdxBagIndex<EntIdxTestComponents.IsThing>>(42);

        var bag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        var entity = arena.Alloc();
        entity.IsThing = true;
        Assert.AreEqual(1, bag.Count);

        entity.Dispose();

        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Verifies the index backstop also cleans up a gated bag during dispose.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_RemovesGatedBagWhenIndexUnsetsBeforeMarker()
    {
        var context = new EntIdxContextBuilder();
        using var arena = new EntIdxArena(context.Ent);
        var dummy = arena.Alloc();
        dummy.Set<int, EntIdxGatedBagIndex<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>>(42);

        var bag = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(bag);

        var entity = arena.Alloc();
        entity.IsThing = true;
        entity.IsReady = true;
        Assert.AreEqual(1, bag.Count);

        entity.Dispose();

        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Verifies swap-removing a disposed entity leaves the survivor indexed correctly.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_SwapRemoveKeepsSurvivorIndexed()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context.Ent);
        var first = arena.Alloc();
        var second = arena.Alloc();
        EntMutIdx firstMut = first;
        EntMutIdx secondMut = second;

        first.IsThing = true;
        second.IsThing = true;
        first.Dispose();

        Assert.AreEqual(1, bag.Count);
        Assert.IsFalse(bag.Contains(firstMut));
        Assert.IsTrue(bag.Contains(secondMut));
        Assert.AreEqual(second.Handle, bag.Ents[0].Handle);
    }

    /// <summary>Verifies arena dispose bulk-invalidates entities but leaves indexed bag views stale by contract.</summary>
    [TestMethod]
    public void EntIdxBag_ArenaDispose_LeavesStaleInvalidBagView()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxBagMut<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        entity.IsThing = true;
        Assert.AreEqual(1, bag.Count);

        arena.Dispose();

        Assert.IsFalse(arena.IsAlive);
        Assert.AreEqual(1, bag.Count);
        Assert.AreEqual(1, bag.Ents.Length);
        Assert.IsFalse(bag.Ents[0].IsAlive);
    }

    private static void AssertBagState<N>(EntIdxBagMut<N> bag, EntMutIdx entity, bool expected)
        where N : IComponent
    {
        Assert.AreEqual(expected ? 1 : 0, bag.Count);
        Assert.AreEqual(expected, bag.Contains(entity));
        Assert.AreEqual(expected, bag.Ents.Length == 1);
    }

    private static void AssertBagState<N, TGate>(EntIdxGatedBagMut<N, TGate> bag, EntMutIdx entity, bool expected)
        where N : IComponent
        where TGate : IComponent
    {
        Assert.AreEqual(expected ? 1 : 0, bag.Count);
        Assert.AreEqual(expected, bag.Contains(entity));
        Assert.AreEqual(expected, bag.Ents.Length == 1);
    }
}
