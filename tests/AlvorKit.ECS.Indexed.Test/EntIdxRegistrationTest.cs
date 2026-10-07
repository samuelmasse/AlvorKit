namespace AlvorKit;

[TestClass]
public class EntIdxRegistrationTest
{
    /// <summary>Reaction and index registration validate the component's declared value type.</summary>
    [TestMethod]
    public void Registration_RejectsMismatchedValueTypes()
    {
        using var context = new EntIdxContext();
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.OnWrite<string, EntIdxTestComponents.Value>(ent => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.OnChange<string, EntIdxTestComponents.Value>((ent, in change) => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddIndex(ent => { }).OnWrite<string, EntIdxTestComponents.Value>(ent => { }));
    }

    /// <summary>Bag markers and gates must be sparse booleans.</summary>
    [TestMethod]
    public void Registration_RejectsNonBooleanBags()
    {
        using var context = new EntIdxContext();
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddBag(new EntIdxBag<EntIdxTestComponents.Value>()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddGatedBag(new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.Value>()));
    }

    /// <summary>One bag identity per context and one context per bag instance prevent shared back-index corruption.</summary>
    [TestMethod]
    public void Registration_RejectsDuplicateBagsAndSharedInstances()
    {
        using var first = new EntIdxContext();
        using var second = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        first.AddBag(bag);
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            first.AddBag(new EntIdxBag<EntIdxTestComponents.IsThing>()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() => second.AddBag(bag));

        var gated = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        first.AddGatedBag(gated);
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            first.AddGatedBag(new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>()));
    }

    /// <summary>Registration stays open during arena construction and closes permanently on the first allocation.</summary>
    [TestMethod]
    public void Registration_FreezesOnFirstAllocation()
    {
        using var context = new EntIdxContext();
        using var arena = new EntIdxArena(context);
        var index = context.AddIndex(ent => { });
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => { });
        context.OnClearing(ent => { });
        context.OnDisposing(ent => { });
        var ent = arena.Alloc();
        ent.Dispose();

        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.OnWrite<int, EntIdxTestComponents.Value>(target => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            index.OnWrite<int, EntIdxTestComponents.Value>(target => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.AddIndex(target => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.OnClearing(target => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.OnDisposing(target => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.AddBag(new EntIdxBag<EntIdxTestComponents.IsThing>()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddGatedBag(new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>()));
    }

    /// <summary>An index cannot accidentally register the same input twice.</summary>
    [TestMethod]
    public void Registration_RejectsDuplicateIndexInput()
    {
        using var context = new EntIdxContext();
        var index = context.AddIndex(ent => { }).OnWrite<int, EntIdxTestComponents.Value>(ent => { });
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            index.OnWrite<int, EntIdxTestComponents.Value>(ent => { }));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            index.OnChange<int, EntIdxTestComponents.Value>((ent, in change) => { }));
    }

    /// <summary>A gate identical to the marker is registered once and behaves like a plain bag.</summary>
    [TestMethod]
    public void GatedBag_AllowsIdenticalMarkerAndGate()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsThing>();
        context.AddGatedBag(bag);
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.IsThing = true;
        Assert.AreEqual(1, bag.Count);
        ent.Dispose();
        Assert.AreEqual(0, bag.Count);
    }

    /// <summary>A failed attempt to borrow an already-bound bag leaves the other context's identity available.</summary>
    [TestMethod]
    public void Registration_FailedBindDoesNotReserveIdentity()
    {
        using var first = new EntIdxContext();
        using var second = new EntIdxContext();
        var shared = new EntIdxBag<EntIdxTestComponents.IsThing>();
        first.AddBag(shared);
        Assert.ThrowsExactly<EntIdxRegistrationException>(() => second.AddBag(shared));
        var own = new EntIdxBag<EntIdxTestComponents.IsThing>();
        second.AddBag(own);
        using var arena = new EntIdxArena(second);
        arena.Alloc().Mutate().IsThing(true);
        Assert.AreEqual(1, own.Count);
        Assert.AreEqual(0, shared.Count);
    }
}
