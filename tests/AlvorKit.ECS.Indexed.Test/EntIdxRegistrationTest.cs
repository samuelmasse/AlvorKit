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
                (EntMutIdx ent, in string value) => _ = value));
        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddPost<string, EntIdxTestComponents.Value>(
                ent => _ = ent));
    }

    /// <summary>Verifies loaded builders require a bool loaded gate component.</summary>
    [TestMethod]
    public void EntIdxRegistration_NonBoolLoadedGate_Throws()
    {
        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => _ = new EntIdxContextBuilder<EntIdxTestComponents.Value>());
    }

    /// <summary>Verifies bag registration rejects non-bool markers and gates.</summary>
    [TestMethod]
    public void EntIdxRegistration_NonBoolBagMarkerOrGate_Throws()
    {
        var context = new EntIdxContextBuilder();

        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddBag(new EntIdxBagMut<EntIdxTestComponents.Value>()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.Value>(
                new EntIdxBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.Value>()));
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

        context.AddBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>(
            new EntIdxBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>());

        Assert.ThrowsExactly<EntIdxRegistrationException>(
            () => context.AddBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>(
                new EntIdxBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>()));
    }

    /// <summary>Verifies the same marker and gate bag identity can be registered on separate contexts.</summary>
    [TestMethod]
    public void EntIdxRegistration_SameBagIdentityOnSeparateContexts_Works()
    {
        var first = new EntIdxContextBuilder();
        var second = new EntIdxContextBuilder();

        first.AddBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>(
            new EntIdxBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>());
        second.AddBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>(
            new EntIdxBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>());
    }

    /// <summary>Verifies the generic loaded builder registers bags with its loaded gate.</summary>
    [TestMethod]
    public void EntIdxRegistration_LoadedBuilder_AddBagLoadedUsesLoadedGate()
    {
        var context = new EntIdxContextBuilder<EntIdxTestComponents.IsLoaded>();
        var bag = new EntIdxBagMut<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsLoaded>();
        context.AddBagLoaded(bag);

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        entity.IsThing = true;
        Assert.AreEqual(0, bag.Count);

        entity.IsLoaded = true;
        Assert.AreEqual(1, bag.Count);
    }
}

