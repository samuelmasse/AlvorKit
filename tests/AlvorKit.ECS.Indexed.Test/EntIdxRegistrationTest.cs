namespace AlvorKit.ECS.Indexed.Test;

[TestClass]
public class EntIdxRegistrationTest
{
    /// <summary>Verifies hook registration rejects mismatched component value and marker types.</summary>
    [TestMethod]
    public void EntIdxRegistration_MismatchedHookType_Throws()
    {
        var context = new EntIdxContextBuilder();

        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddPre<string, EntIdxTestComponents.Value>(
                (ent, in value) => _ = value));
        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddPost<string, EntIdxTestComponents.Value>(
                ent => _ = ent));
    }

    /// <summary>Verifies bag registration rejects non-bool markers and gates.</summary>
    [TestMethod]
    public void EntIdxRegistration_NonBoolBagMarkerOrGate_Throws()
    {
        var context = new EntIdxContextBuilder();

        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddBag(new EntIdxBagMut<EntIdxTestComponents.Value>()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.Value>(
                new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.Value>()));
    }

    /// <summary>Verifies duplicate plain bag registration on one context is rejected.</summary>
    [TestMethod]
    public void EntIdxRegistration_DuplicatePlainBagOnOneContext_Throws()
    {
        var context = new EntIdxContextBuilder();

        context.AddBag(new EntIdxBagMut<EntIdxTestComponents.IsThing>());

        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddBag(new EntIdxBagMut<EntIdxTestComponents.IsThing>()));
    }

    /// <summary>Verifies duplicate marker and gate bag registration on one context is rejected.</summary>
    [TestMethod]
    public void EntIdxRegistration_DuplicateGatedBagOnOneContext_Throws()
    {
        var context = new EntIdxContextBuilder();

        context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(
            new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>());

        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(
                new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>()));
    }

    /// <summary>Verifies the same marker and gate bag identity can be registered on separate contexts.</summary>
    [TestMethod]
    public void EntIdxRegistration_SameBagIdentityOnSeparateContexts_Works()
    {
        var first = new EntIdxContextBuilder();
        var second = new EntIdxContextBuilder();

        first.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(
            new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>());
        second.AddGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>(
            new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>());
    }

    /// <summary>Verifies a gated bag uses both its marker and gate components.</summary>
    [TestMethod]
    public void EntIdxRegistration_GatedBag_UsesMarkerAndGate()
    {
        var context = new EntIdxContextBuilder();
        var bag = new EntIdxGatedBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddGatedBag(bag);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        entity.IsThing = true;
        Assert.AreEqual(0, bag.Count);

        entity.IsReady = true;
        Assert.AreEqual(1, bag.Count);
    }
}
