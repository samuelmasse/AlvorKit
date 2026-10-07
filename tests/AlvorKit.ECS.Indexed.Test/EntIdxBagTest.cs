namespace AlvorKit;

[TestClass]
public class EntIdxBagTest
{
    /// <summary>Verifies plain bag membership follows the marker and ignores unrelated gates.</summary>
    [TestMethod]
    public void EntIdxBag_PlainMembership_FollowsMarkerOnly()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context);
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
        using var context = new EntIdxContext();
        var bag = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(bag);

        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        EntMutIdx mut = ent;

        AssertBagState(bag, mut, false);

        ent.IsThing = true;
        AssertBagState(bag, mut, false);

        ent.IsReady = true;
        AssertBagState(bag, mut, true);

        ent.IsThing = false;
        AssertBagState(bag, mut, false);

        ent.IsThing = true;
        AssertBagState(bag, mut, true);

        ent.IsReady = false;
        AssertBagState(bag, mut, false);

        ent.IsReady = true;
        AssertBagState(bag, mut, true);

        Assert.IsTrue(ent.UnsetIsThing());
        AssertBagState(bag, mut, false);

        ent.IsReady = false;
        ent.IsReady = true;
        AssertBagState(bag, mut, false);

        ent.IsThing = true;
        AssertBagState(bag, mut, true);

        Assert.IsTrue(ent.UnsetIsReady());
        AssertBagState(bag, mut, false);
    }

    /// <summary>Verifies setting the gate before the marker still admits the ent once both are true.</summary>
    [TestMethod]
    public void EntIdxBag_GatedMembership_AllowsGateBeforeMarker()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(bag);

        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        EntMutIdx mut = ent;

        ent.IsReady = true;
        AssertBagState(bag, mut, false);

        ent.IsThing = true;
        AssertBagState(bag, mut, true);
    }

    /// <summary>Verifies different gates over the same marker and a plain bag can coexist.</summary>
    [TestMethod]
    public void EntIdxBag_DifferentGatesOverSameMarker_Coexist()
    {
        using var context = new EntIdxContext();
        var plain = new EntIdxBag<EntIdxTestComponents.IsThing>();
        var gateA = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateA>();
        var gateB = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateB>();

        context.AddBag(plain);
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateA>(gateA);
        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsGateB>(gateB);

        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        EntMutIdx mut = ent;

        ent.IsThing = true;
        ent.IsGateA = true;

        AssertBagState(plain, mut, true);
        AssertBagState(gateA, mut, true);
        AssertBagState(gateB, mut, false);

        ent.IsGateB = true;
        AssertBagState(gateB, mut, true);

        ent.IsGateA = false;
        AssertBagState(plain, mut, true);
        AssertBagState(gateA, mut, false);
        AssertBagState(gateB, mut, true);
    }

    /// <summary>Individual disposal detaches a bag whose marker storage was created first.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_RemovesWhenMarkerStorageWasCreatedFirst()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();

        ent.IsThing = true;
        Assert.AreEqual(1, bag.Count);

        ent.Dispose();

        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Manual Clear detaches the bag and preserves the allocation.</summary>
    [TestMethod]
    public void EntIdxBag_Clear_RemovesBagEntry()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        EntMutIdx mut = ent;

        ent.IsThing = true;
        AssertBagState(bag, mut, true);

        ent.Clear();

        AssertBagState(bag, mut, false);
        Assert.IsTrue(ent.IsAlive);
    }

    /// <summary>Individual disposal detaches a bag whose private index storage was created first.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_RemovesWithPrewarmedIndexStorage()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);
        using var arena = new EntIdxArena(context);
        var dummy = arena.Alloc();
        dummy.Set<int, EntIdxBagIndex<EntIdxTestComponents.IsThing>>(42);

        var ent = arena.Alloc();
        ent.IsThing = true;
        Assert.AreEqual(1, bag.Count);

        ent.Dispose();

        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Gated bag removal is independent of private index storage creation order.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_RemovesGatedBagWithPrewarmedIndexStorage()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddGatedBag(bag);
        using var arena = new EntIdxArena(context);
        var dummy = arena.Alloc();
        dummy.Set<int, EntIdxGatedBagIndex<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>>(42);

        var ent = arena.Alloc();
        ent.IsThing = true;
        ent.IsReady = true;
        Assert.AreEqual(1, bag.Count);

        ent.Dispose();

        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>Verifies swap-removing a disposed ent leaves the survivor indexed correctly.</summary>
    [TestMethod]
    public void EntIdxBag_Dispose_SwapRemoveKeepsSurvivorIndexed()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        using var arena = new EntIdxArena(context);
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

    /// <summary>Verifies arena dispose bulk-invalidates Ents but leaves indexed bag views stale by contract.</summary>
    [TestMethod]
    public void EntIdxBag_ArenaDispose_LeavesStaleInvalidBagView()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);

        var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.IsThing = true;
        Assert.AreEqual(1, bag.Count);

        arena.Dispose();

        Assert.IsFalse(arena.IsAlive);
        Assert.AreEqual(1, bag.Count);
        Assert.AreEqual(1, bag.Ents.Length);
        Assert.IsFalse(bag.Ents[0].IsAlive);
    }

    private static void AssertBagState<N>(EntIdxBag<N> bag, EntMutIdx ent, bool expected)
        where N : IComponent
    {
        Assert.AreEqual(expected ? 1 : 0, bag.Count);
        Assert.AreEqual(expected, bag.Contains(ent));
        Assert.AreEqual(expected, bag.Ents.Length == 1);
    }

    private static void AssertBagState<N, TGate>(EntIdxGatedBag<N, TGate> bag, EntMutIdx ent, bool expected)
        where N : IComponent
        where TGate : IComponent
    {
        Assert.AreEqual(expected ? 1 : 0, bag.Count);
        Assert.AreEqual(expected, bag.Contains(ent));
        Assert.AreEqual(expected, bag.Ents.Length == 1);
    }
}
