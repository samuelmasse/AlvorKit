namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchetypalTest
{
    /// <summary>Verifies a singleton field enters and exits its arch group through the public API.</summary>
    [TestMethod]
    public void Archetypal_SingletonEntryAndExit_UpdatesPresenceAndLoc()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();

        Assert.IsFalse(ent.HasArchetypal<int, C0, SingletonArch>());
        Assert.AreEqual(0, ent.GetArchetypal<int, C0, SingletonArch>());
        Assert.IsFalse(ent.UnsetArchetypal<int, C0, SingletonArch>());

        ent.SetArchetypal<int, C0, SingletonArch>(42);

        var loc = ent.Get<EntArchLoc, SingletonArch>();
        Assert.IsTrue(ent.HasArchetypal<int, C0, SingletonArch>());
        Assert.AreEqual(42, ent.GetArchetypal<int, C0, SingletonArch>());
        Assert.AreEqual(arena.Index, loc.AllocId);
        Assert.IsTrue(loc.ArchId > EntArchGraph<SingletonArch>.NoArchId);
        Assert.AreEqual(0, loc.Row);

        Assert.IsTrue(ent.UnsetArchetypal<int, C0, SingletonArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, SingletonArch>());
        Assert.IsFalse(ent.HasArchetypal<int, C0, SingletonArch>());
        Assert.AreEqual(0, ent.GetArchetypal<int, C0, SingletonArch>());
        Assert.IsFalse(ent.UnsetArchetypal<int, C0, SingletonArch>());
    }

    /// <summary>Verifies reduction retains every dst field and re-adding a removed field preserves those values.</summary>
    [TestMethod]
    public void Archetypal_Reduction_PreservesRetainedFields()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        var box = new RefBox(30);

        ent.SetArchetypal<int, C0, ReductionArch>(10);
        ent.SetArchetypal<long, C1, ReductionArch>(20L);
        ent.SetArchetypal<RefBox, C2, ReductionArch>(box);

        Assert.IsTrue(ent.UnsetArchetypal<long, C1, ReductionArch>());
        Assert.AreEqual(10, ent.GetArchetypal<int, C0, ReductionArch>());
        Assert.AreSame(box, ent.GetArchetypal<RefBox, C2, ReductionArch>());
        Assert.IsFalse(ent.HasArchetypal<long, C1, ReductionArch>());

        ent.SetArchetypal<long, C1, ReductionArch>(21L);
        Assert.AreEqual(10, ent.GetArchetypal<int, C0, ReductionArch>());
        Assert.AreEqual(21L, ent.GetArchetypal<long, C1, ReductionArch>());
        Assert.AreSame(box, ent.GetArchetypal<RefBox, C2, ReductionArch>());

        Assert.IsTrue(ent.UnsetArchetypal<int, C0, ReductionArch>());
        Assert.AreEqual(21L, ent.GetArchetypal<long, C1, ReductionArch>());
        Assert.AreSame(box, ent.GetArchetypal<RefBox, C2, ReductionArch>());

        Assert.IsTrue(ent.UnsetArchetypal<long, C1, ReductionArch>());
        Assert.IsTrue(ent.UnsetArchetypal<RefBox, C2, ReductionArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, ReductionArch>());
    }

    /// <summary>Verifies different field-add orders intern one exact canonical arch signature.</summary>
    [TestMethod]
    public void Archetypal_DifferentAddOrders_InternSameSignature()
    {
        using var arena = new EntArena();
        EntMut first = arena.Alloc();
        EntMut second = arena.Alloc();

        first.SetArchetypal<int, C0, AddOrderArch>(10);
        first.SetArchetypal<long, C1, AddOrderArch>(11L);
        first.SetArchetypal<short, C2, AddOrderArch>(12);
        first.SetArchetypal<byte, C3, AddOrderArch>(13);

        second.SetArchetypal<byte, C3, AddOrderArch>(23);
        second.SetArchetypal<short, C2, AddOrderArch>(22);
        second.SetArchetypal<long, C1, AddOrderArch>(21L);
        second.SetArchetypal<int, C0, AddOrderArch>(20);

        int firstArchId = first.Get<EntArchLoc, AddOrderArch>().ArchId;
        int secondArchId = second.Get<EntArchLoc, AddOrderArch>().ArchId;
        var expectedFieldIds = new[]
        {
            EntArchColumn<int, C0, AddOrderArch>.FieldId,
            EntArchColumn<long, C1, AddOrderArch>.FieldId,
            EntArchColumn<short, C2, AddOrderArch>.FieldId,
            EntArchColumn<byte, C3, AddOrderArch>.FieldId,
        };
        Array.Sort(expectedFieldIds);

        Assert.AreEqual(firstArchId, secondArchId);
        CollectionAssert.AreEqual(expectedFieldIds, EntArchGraph<AddOrderArch>.FieldIds(firstArchId).ToArray());
        Assert.AreEqual(10, first.GetArchetypal<int, C0, AddOrderArch>());
        Assert.AreEqual(13, first.GetArchetypal<byte, C3, AddOrderArch>());
        Assert.AreEqual(20, second.GetArchetypal<int, C0, AddOrderArch>());
        Assert.AreEqual(23, second.GetArchetypal<byte, C3, AddOrderArch>());

        ExitAddOrderArch(first);
        ExitAddOrderArch(second);
    }

    /// <summary>Verifies a resolved add and remove relationship caches both inverse transition directions.</summary>
    [TestMethod]
    public void Archetypal_ResolvedRelationship_ReusesInverseTransitions()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        int secondFieldId = EntArchColumn<long, C1, InverseArch>.FieldId;

        ent.SetArchetypal<int, C0, InverseArch>(10);
        int singletonArchId = ent.Get<EntArchLoc, InverseArch>().ArchId;
        ent.SetArchetypal<long, C1, InverseArch>(20L);
        int pairArchId = ent.Get<EntArchLoc, InverseArch>().ArchId;

        Assert.AreEqual(pairArchId, EntArchGraph<InverseArch>.GetAddArchId(singletonArchId, secondFieldId));
        Assert.AreEqual(singletonArchId, EntArchGraph<InverseArch>.GetRemoveArchId(pairArchId, secondFieldId));

        Assert.IsTrue(ent.UnsetArchetypal<long, C1, InverseArch>());
        Assert.AreEqual(singletonArchId, ent.Get<EntArchLoc, InverseArch>().ArchId);
        ent.SetArchetypal<long, C1, InverseArch>(21L);
        Assert.AreEqual(pairArchId, ent.Get<EntArchLoc, InverseArch>().ArchId);
        Assert.AreEqual(21L, ent.GetArchetypal<long, C1, InverseArch>());

        Assert.IsTrue(ent.UnsetArchetypal<long, C1, InverseArch>());
        Assert.IsTrue(ent.UnsetArchetypal<int, C0, InverseArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, InverseArch>());
    }

    /// <summary>Verifies two arch groups store and remove the same named field independently on one Ent.</summary>
    [TestMethod]
    public void Archetypal_IndependentGroups_DoNotShareState()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();

        ent.SetArchetypal<int, C0, FirstIndependentArch>(11);
        ent.SetArchetypal<int, C0, SecondIndependentArch>(22);

        Assert.AreEqual(11, ent.GetArchetypal<int, C0, FirstIndependentArch>());
        Assert.AreEqual(22, ent.GetArchetypal<int, C0, SecondIndependentArch>());
        Assert.IsTrue(ent.Has<EntArchLoc, FirstIndependentArch>());
        Assert.IsTrue(ent.Has<EntArchLoc, SecondIndependentArch>());

        Assert.IsTrue(ent.UnsetArchetypal<int, C0, FirstIndependentArch>());
        Assert.IsFalse(ent.HasArchetypal<int, C0, FirstIndependentArch>());
        Assert.AreEqual(22, ent.GetArchetypal<int, C0, SecondIndependentArch>());
        Assert.IsTrue(ent.Has<EntArchLoc, SecondIndependentArch>());

        Assert.IsTrue(ent.UnsetArchetypal<int, C0, SecondIndependentArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, SecondIndependentArch>());
    }

    private static void ExitAddOrderArch(EntMut ent)
    {
        ent.UnsetArchetypal<int, C0, AddOrderArch>();
        ent.UnsetArchetypal<long, C1, AddOrderArch>();
        ent.UnsetArchetypal<short, C2, AddOrderArch>();
        ent.UnsetArchetypal<byte, C3, AddOrderArch>();
        Assert.IsFalse(ent.Has<EntArchLoc, AddOrderArch>());
    }

    private sealed record RefBox(int Value);

    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct C2;
    private readonly record struct C3;
    private readonly record struct SingletonArch;
    private readonly record struct ReductionArch;
    private readonly record struct AddOrderArch;
    private readonly record struct InverseArch;
    private readonly record struct FirstIndependentArch;
    private readonly record struct SecondIndependentArch;
}
