namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchetypalTest
{
    /// <summary>Verifies a singleton field enters, overwrites in place, and exits its arch group through the public API.</summary>
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

        ent.SetArchetypal<int, C0, SingletonArch>(43);

        Assert.AreEqual(loc, ent.Get<EntArchLoc, SingletonArch>());
        Assert.AreEqual(43, ent.GetArchetypal<int, C0, SingletonArch>());

        Assert.IsTrue(ent.UnsetArchetypal<int, C0, SingletonArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, SingletonArch>());
        Assert.IsFalse(ent.HasArchetypal<int, C0, SingletonArch>());
        Assert.AreEqual(0, ent.GetArchetypal<int, C0, SingletonArch>());
        Assert.IsFalse(ent.UnsetArchetypal<int, C0, SingletonArch>());
    }

    /// <summary>Verifies the column lookup's null slots and exact directory boundaries.</summary>
    [TestMethod]
    public void Archetypal_ValuesAt_UsesDirectoryBoundsAndNullSlots()
    {
        using var firstArena = new EntArena();
        using var secondArena = new EntArena();
        EntMut ent = secondArena.Alloc();

        ent.SetArchetypal<int, C0, ValuesAtBoundsArch>(17);

        var loc = ent.Get<EntArchLoc, ValuesAtBoundsArch>();
        var valuesByAlloc = EntArchColumn<int, C0, ValuesAtBoundsArch>.Values;
        var valuesByArch = valuesByAlloc[loc.AllocId];
        Assert.AreSame(valuesByArch[loc.ArchId], EntArchColumn<int, C0, ValuesAtBoundsArch>.ValuesAt(
            loc.AllocId,
            loc.ArchId));
        Assert.IsNull(EntArchColumn<int, C0, ValuesAtBoundsArch>.ValuesAt(
            loc.AllocId,
            EntArchGraph<ValuesAtBoundsArch>.NoArchId));
        Assert.IsNull(EntArchColumn<int, C0, ValuesAtBoundsArch>.ValuesAt(firstArena.Index, loc.ArchId));
        Assert.IsNull(EntArchColumn<int, C0, ValuesAtBoundsArch>.ValuesAt(valuesByAlloc.Length, loc.ArchId));
        Assert.IsNull(EntArchColumn<int, C0, ValuesAtBoundsArch>.ValuesAt(
            loc.AllocId,
            valuesByArch.Length));

        Assert.IsTrue(ent.UnsetArchetypal<int, C0, ValuesAtBoundsArch>());
    }

    /// <summary>Verifies cold point operations do not register a field until a live structural Set needs it.</summary>
    [TestMethod]
    public void Archetypal_ColdPointOperations_RegisterOnlyFirstLiveSet()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        EntPtr deadPtr = arena.Alloc();
        EntMut deadEnt = deadPtr;
        deadPtr.Dispose();

        var metrics = EntArchDiagnostics<ColdRegistrationArch>.Capture();
        Assert.AreEqual(0, metrics.RegisteredFieldCount);
        Assert.AreEqual(0, metrics.MaterializedArchCount);

        Assert.AreEqual(0, ent.GetArchetypal<int, ColdGetField, ColdRegistrationArch>());
        Assert.IsFalse(ent.HasArchetypal<int, ColdHasField, ColdRegistrationArch>());
        Assert.IsFalse(ent.UnsetArchetypal<int, ColdUnsetField, ColdRegistrationArch>());
        deadEnt.SetArchetypal<int, ColdDeadSetField, ColdRegistrationArch>(4);

        metrics = EntArchDiagnostics<ColdRegistrationArch>.Capture();
        Assert.AreEqual(0, metrics.RegisteredFieldCount);
        Assert.AreEqual(0, metrics.MaterializedArchCount);

        ent.SetArchetypal<int, ColdSetField, ColdRegistrationArch>(5);
        ent.SetArchetypal<int, ColdSetField, ColdRegistrationArch>(6);

        metrics = EntArchDiagnostics<ColdRegistrationArch>.Capture();
        Assert.AreEqual(1, metrics.RegisteredFieldCount);
        Assert.AreEqual(1, metrics.MaterializedArchCount);
        Assert.AreEqual(6, ent.GetArchetypal<int, ColdSetField, ColdRegistrationArch>());
        Assert.IsTrue(ent.HasArchetypal<int, ColdSetField, ColdRegistrationArch>());

        Assert.IsTrue(ent.UnsetArchetypal<int, ColdSetField, ColdRegistrationArch>());
    }

    /// <summary>Verifies Set enters, moves a middle row, repairs compaction, and overwrites in place.</summary>
    [TestMethod]
    public void Archetypal_SetFromMiddleRow_RepairsCompactedLoc()
    {
        using var arena = new EntArena();
        EntMut[] ents = [arena.Alloc(), arena.Alloc(), arena.Alloc(), arena.Alloc()];
        for (int i = 0; i < ents.Length; i++)
            ents[i].SetArchetypal<int, C0, SetMiddleRowArch>(100 + i);

        var srcLoc = ents[1].Get<EntArchLoc, SetMiddleRowArch>();
        var lastLoc = ents[3].Get<EntArchLoc, SetMiddleRowArch>();
        Assert.AreEqual(1, srcLoc.Row);
        Assert.AreEqual(3, lastLoc.Row);
        Assert.AreEqual(srcLoc.ArchId, lastLoc.ArchId);

        ents[1].SetArchetypal<long, C1, SetMiddleRowArch>(200L);

        var dstLoc = ents[1].Get<EntArchLoc, SetMiddleRowArch>();
        var compactedLoc = ents[3].Get<EntArchLoc, SetMiddleRowArch>();
        Assert.AreNotEqual(srcLoc.ArchId, dstLoc.ArchId);
        Assert.AreEqual(0, dstLoc.Row);
        Assert.AreEqual(srcLoc.ArchId, compactedLoc.ArchId);
        Assert.AreEqual(srcLoc.Row, compactedLoc.Row);
        Assert.AreEqual(101, ents[1].GetArchetypal<int, C0, SetMiddleRowArch>());
        Assert.AreEqual(200L, ents[1].GetArchetypal<long, C1, SetMiddleRowArch>());
        Assert.AreEqual(103, ents[3].GetArchetypal<int, C0, SetMiddleRowArch>());

        ents[1].SetArchetypal<long, C1, SetMiddleRowArch>(201L);

        Assert.AreEqual(dstLoc, ents[1].Get<EntArchLoc, SetMiddleRowArch>());
        Assert.AreEqual(201L, ents[1].GetArchetypal<long, C1, SetMiddleRowArch>());

        Assert.IsTrue(ents[1].UnsetArchetypal<long, C1, SetMiddleRowArch>());
        foreach (EntMut ent in ents)
            Assert.IsTrue(ent.UnsetArchetypal<int, C0, SetMiddleRowArch>());
    }

    /// <summary>Verifies point access reads and writes valid nonzero and swap-back-repaired rows.</summary>
    [TestMethod]
    public void Archetypal_PointAccess_ReadsAndWritesValidRepairedRows()
    {
        using var arena = new EntArena();
        EntMut[] ents = [arena.Alloc(), arena.Alloc(), arena.Alloc(), arena.Alloc()];
        var boxes = new RefBox[ents.Length];

        Assert.IsNull(ents[0].GetArchetypal<RefBox, C1, RowAccessArch>());
        for (int i = 0; i < ents.Length; i++)
        {
            boxes[i] = new(i);
            ents[i].SetArchetypal<int, C0, RowAccessArch>(100 + i);
            ents[i].SetArchetypal<RefBox, C1, RowAccessArch>(boxes[i]);
            ents[i].SetArchetypal<RowPayload, C2, RowAccessArch>(new(boxes[i], 200 + i));
        }

        Assert.AreEqual(2, ents[2].Get<EntArchLoc, RowAccessArch>().Row);
        Assert.AreEqual(102, ents[2].GetArchetypal<int, C0, RowAccessArch>());
        Assert.AreSame(boxes[2], ents[2].GetArchetypal<RefBox, C1, RowAccessArch>());
        Assert.AreEqual(new RowPayload(boxes[2], 202),
            ents[2].GetArchetypal<RowPayload, C2, RowAccessArch>());

        var replacement = new RefBox(12);
        ents[2].SetArchetypal<int, C0, RowAccessArch>(112);
        ents[2].SetArchetypal<RefBox, C1, RowAccessArch>(replacement);
        ents[2].SetArchetypal<RowPayload, C2, RowAccessArch>(new(replacement, 212));
        Assert.AreEqual(112, ents[2].GetArchetypal<int, C0, RowAccessArch>());
        Assert.AreSame(replacement, ents[2].GetArchetypal<RefBox, C1, RowAccessArch>());
        Assert.AreEqual(new RowPayload(replacement, 212),
            ents[2].GetArchetypal<RowPayload, C2, RowAccessArch>());

        var middleLoc = ents[1].Get<EntArchLoc, RowAccessArch>();
        Assert.AreEqual(1, middleLoc.Row);
        Assert.AreEqual(3, ents[3].Get<EntArchLoc, RowAccessArch>().Row);
        ents[1].SetArchetypal<long, C3, RowAccessArch>(300L);

        var repairedLoc = ents[3].Get<EntArchLoc, RowAccessArch>();
        Assert.AreEqual(middleLoc.ArchId, repairedLoc.ArchId);
        Assert.AreEqual(middleLoc.Row, repairedLoc.Row);
        Assert.AreEqual(103, ents[3].GetArchetypal<int, C0, RowAccessArch>());
        Assert.AreSame(boxes[3], ents[3].GetArchetypal<RefBox, C1, RowAccessArch>());
        Assert.AreEqual(new RowPayload(boxes[3], 203),
            ents[3].GetArchetypal<RowPayload, C2, RowAccessArch>());

        var movedReplacement = new RefBox(13);
        ents[3].SetArchetypal<int, C0, RowAccessArch>(113);
        ents[3].SetArchetypal<RefBox, C1, RowAccessArch>(movedReplacement);
        ents[3].SetArchetypal<RowPayload, C2, RowAccessArch>(new(movedReplacement, 213));
        Assert.AreEqual(113, ents[3].GetArchetypal<int, C0, RowAccessArch>());
        Assert.AreSame(movedReplacement, ents[3].GetArchetypal<RefBox, C1, RowAccessArch>());
        Assert.AreEqual(new RowPayload(movedReplacement, 213),
            ents[3].GetArchetypal<RowPayload, C2, RowAccessArch>());

        Assert.IsTrue(ents[1].UnsetArchetypal<long, C3, RowAccessArch>());
        foreach (EntMut ent in ents)
        {
            Assert.IsTrue(ent.UnsetArchetypal<RowPayload, C2, RowAccessArch>());
            Assert.IsTrue(ent.UnsetArchetypal<RefBox, C1, RowAccessArch>());
            Assert.IsTrue(ent.UnsetArchetypal<int, C0, RowAccessArch>());
        }
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

    /// <summary>Verifies adding a field between two sorted IDs preserves the canonical signature.</summary>
    [TestMethod]
    public void Archetypal_AddMiddleField_PreservesCanonicalSignature()
    {
        using var arena = new EntArena();
        EntMut first = arena.Alloc();
        EntMut second = arena.Alloc();
        int lowFieldId = EntArchColumn<int, LowField, MiddleInsertionArch>.FieldId;
        int middleFieldId = EntArchColumn<int, MiddleField, MiddleInsertionArch>.FieldId;
        int highFieldId = EntArchColumn<int, HighField, MiddleInsertionArch>.FieldId;

        first.SetArchetypal<int, LowField, MiddleInsertionArch>(10);
        first.SetArchetypal<int, HighField, MiddleInsertionArch>(30);
        first.SetArchetypal<int, MiddleField, MiddleInsertionArch>(20);

        second.SetArchetypal<int, MiddleField, MiddleInsertionArch>(21);
        second.SetArchetypal<int, HighField, MiddleInsertionArch>(31);
        second.SetArchetypal<int, LowField, MiddleInsertionArch>(11);

        int firstArchId = first.Get<EntArchLoc, MiddleInsertionArch>().ArchId;
        int secondArchId = second.Get<EntArchLoc, MiddleInsertionArch>().ArchId;
        CollectionAssert.AreEqual(
            new[] { lowFieldId, middleFieldId, highFieldId },
            EntArchGraph<MiddleInsertionArch>.FieldIds(firstArchId).ToArray());
        Assert.AreEqual(firstArchId, secondArchId);
        Assert.AreEqual(10, first.GetArchetypal<int, LowField, MiddleInsertionArch>());
        Assert.AreEqual(20, first.GetArchetypal<int, MiddleField, MiddleInsertionArch>());
        Assert.AreEqual(30, first.GetArchetypal<int, HighField, MiddleInsertionArch>());

        ExitMiddleInsertionArch(first);
        ExitMiddleInsertionArch(second);
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

        Assert.AreEqual(pairArchId, EntArchGraph<InverseArch>.GetTransitionArchId(singletonArchId, secondFieldId));
        Assert.AreEqual(singletonArchId, EntArchGraph<InverseArch>.GetTransitionArchId(pairArchId, secondFieldId));

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

    private static void ExitMiddleInsertionArch(EntMut ent)
    {
        ent.UnsetArchetypal<int, LowField, MiddleInsertionArch>();
        ent.UnsetArchetypal<int, MiddleField, MiddleInsertionArch>();
        ent.UnsetArchetypal<int, HighField, MiddleInsertionArch>();
        Assert.IsFalse(ent.Has<EntArchLoc, MiddleInsertionArch>());
    }

    private sealed record RefBox(int Value);

    private readonly record struct RowPayload(RefBox Ref, int Value);

    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct C2;
    private readonly record struct C3;
    private readonly record struct LowField;
    private readonly record struct MiddleField;
    private readonly record struct HighField;
    private readonly record struct SingletonArch;
    private readonly record struct ValuesAtBoundsArch;
    private readonly record struct ColdGetField;
    private readonly record struct ColdHasField;
    private readonly record struct ColdUnsetField;
    private readonly record struct ColdDeadSetField;
    private readonly record struct ColdSetField;
    private readonly record struct ColdRegistrationArch;
    private readonly record struct SetMiddleRowArch;
    private readonly record struct ReductionArch;
    private readonly record struct AddOrderArch;
    private readonly record struct MiddleInsertionArch;
    private readonly record struct InverseArch;
    private readonly record struct FirstIndependentArch;
    private readonly record struct SecondIndependentArch;
    private readonly record struct RowAccessArch;
}
