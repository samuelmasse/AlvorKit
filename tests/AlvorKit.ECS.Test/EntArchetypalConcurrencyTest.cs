namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchetypalConcurrencyTest
{
    /// <summary>Verifies cold alloc owners intern one arch and observe the same immutable catalog metadata.</summary>
    [TestMethod]
    public void Archetypal_ConcurrentColdResolution_DifferentAllocsInternSameArch()
    {
        const int ownerCount = 2;
        _ = EntArchColumn<int, C0, ColdResolutionArch>.FieldId;
        _ = EntArchColumn<int, C1, ColdResolutionArch>.FieldId;
        _ = EntArchColumn<int, C2, ColdResolutionArch>.FieldId;
        using var barrier = new Barrier(ownerCount);
        var allocIds = new int[ownerCount];
        var archIds = new int[ownerCount];
        var firstValues = new int[ownerCount];
        var secondValues = new int[ownerCount];
        var layoutSnapshots = new EntArchFieldLayout[ownerCount][];
        var ordinalSnapshots = new int[ownerCount][];
        var transitionSnapshots = new int[ownerCount][];

        Parallel.Invoke(
            () => ResolveColdArch(
                0,
                barrier,
                allocIds,
                archIds,
                firstValues,
                secondValues,
                layoutSnapshots,
                ordinalSnapshots,
                transitionSnapshots),
            () => ResolveColdArch(
                1,
                barrier,
                allocIds,
                archIds,
                firstValues,
                secondValues,
                layoutSnapshots,
                ordinalSnapshots,
                transitionSnapshots));

        Assert.AreNotEqual(allocIds[0], allocIds[1]);
        Assert.AreEqual(archIds[0], archIds[1]);
        Assert.AreEqual(10, firstValues[0]);
        Assert.AreEqual(11, secondValues[0]);
        Assert.AreEqual(20, firstValues[1]);
        Assert.AreEqual(21, secondValues[1]);
        CollectionAssert.AreEqual(layoutSnapshots[0], layoutSnapshots[1]);
        Assert.AreEqual(2, layoutSnapshots[0].Length);
        Assert.AreEqual(Unsafe.SizeOf<EntMut>(), layoutSnapshots[0][0].BytePrefix);
        Assert.AreEqual(Unsafe.SizeOf<EntMut>() + Unsafe.SizeOf<int>(), layoutSnapshots[0][1].BytePrefix);
        CollectionAssert.AreEqual(ordinalSnapshots[0], ordinalSnapshots[1]);
        CollectionAssert.AreEqual(
            new[] { 0, 1, EntArchGraph<ColdResolutionArch>.NoFieldOrdinal },
            ordinalSnapshots[0]);
        CollectionAssert.AreEqual(transitionSnapshots[0], transitionSnapshots[1]);

        int pairArchId = archIds[0];
        int firstSingletonArchId = transitionSnapshots[0][0];
        int secondSingletonArchId = transitionSnapshots[0][1];
        Assert.AreEqual(EntArchGraph<ColdResolutionArch>.NoArchId, transitionSnapshots[0][2]);
        Assert.AreEqual(pairArchId, transitionSnapshots[0][3]);
        Assert.AreEqual(firstSingletonArchId, transitionSnapshots[0][4]);
        Assert.AreEqual(pairArchId, transitionSnapshots[0][5]);
        Assert.AreEqual(secondSingletonArchId, transitionSnapshots[0][6]);

        var metrics = EntArchDiagnostics<ColdResolutionArch>.Capture();
        Assert.AreEqual(0L, metrics.TransitionCellCapacity);
        Assert.AreEqual(4, metrics.StoredTransitionEdgeCount);
    }

    /// <summary>Verifies concurrent owners can repeatedly read and write one warm arch through different allocs.</summary>
    [TestMethod]
    public void Archetypal_ConcurrentWarmAccess_DifferentAllocsRemainIsolated()
    {
        const int ownerCount = 4;
        const int entsPerOwner = 32;
        int warmArchId;

        using (var warmArena = new EntArena())
        {
            EntMut warmEnt = warmArena.Alloc();
            warmEnt.SetArchetypal<int, C0, WarmAccessArch>(1);
            warmEnt.SetArchetypal<long, C1, WarmAccessArch>(2L);
            warmArchId = warmEnt.Get<EntArchLoc, WarmAccessArch>().ArchId;
            ExitWarmArch(warmEnt);
        }

        var allocIds = new int[ownerCount];
        var archIds = new int[ownerCount];
        var valid = new bool[ownerCount];

        Parallel.For(
            0,
            ownerCount,
            new ParallelOptions { MaxDegreeOfParallelism = ownerCount },
            owner => ExerciseWarmArch(owner, entsPerOwner, allocIds, archIds, valid));

        Assert.AreEqual(ownerCount, allocIds.Distinct().Count());
        for (int owner = 0; owner < ownerCount; owner++)
        {
            Assert.AreEqual(warmArchId, archIds[owner]);
            Assert.IsTrue(valid[owner]);
        }
    }

    /// <summary>Verifies concurrent first Sets register one field and share one singleton across different allocs.</summary>
    [TestMethod]
    public void Archetypal_ConcurrentFirstSet_DifferentAllocsRegistersOnce()
    {
        const int ownerCount = 2;
        var before = EntArchDiagnostics<ConcurrentFirstSetArch>.Capture();
        Assert.AreEqual(0, before.RegisteredFieldCount);
        Assert.AreEqual(0, before.MaterializedArchCount);

        using var barrier = new Barrier(ownerCount);
        var allocIds = new int[ownerCount];
        var archIds = new int[ownerCount];
        var values = new int[ownerCount];
        var columns = new int[ownerCount][];

        Parallel.Invoke(
            () => FirstSet(0, barrier, allocIds, archIds, values, columns),
            () => FirstSet(1, barrier, allocIds, archIds, values, columns));

        var metrics = EntArchDiagnostics<ConcurrentFirstSetArch>.Capture();
        Assert.AreEqual(1, metrics.RegisteredFieldCount);
        Assert.AreEqual(1, metrics.MaterializedArchCount);
        Assert.AreEqual(1, metrics.SingletonArchCount);
        Assert.AreNotEqual(allocIds[0], allocIds[1]);
        Assert.AreEqual(archIds[0], archIds[1]);
        Assert.AreEqual(10, values[0]);
        Assert.AreEqual(20, values[1]);
        for (int owner = 0; owner < ownerCount; owner++)
        {
            Assert.IsNotNull(columns[owner]);
            Assert.AreSame(
                columns[owner],
                EntArchColumn<int, ConcurrentFirstSetField, ConcurrentFirstSetArch>
                    .Values[allocIds[owner]][archIds[owner]]);
        }
    }

    private static void ResolveColdArch(
        int owner,
        Barrier barrier,
        int[] allocIds,
        int[] archIds,
        int[] firstValues,
        int[] secondValues,
        EntArchFieldLayout[][] layoutSnapshots,
        int[][] ordinalSnapshots,
        int[][] transitionSnapshots)
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();

        if (owner == 0)
            ent.SetArchetypal<int, C0, ColdResolutionArch>(10);
        else ent.SetArchetypal<int, C1, ColdResolutionArch>(21);

        barrier.SignalAndWait();

        if (owner == 0)
            ent.SetArchetypal<int, C1, ColdResolutionArch>(11);
        else ent.SetArchetypal<int, C0, ColdResolutionArch>(20);

        barrier.SignalAndWait();

        var loc = ent.Get<EntArchLoc, ColdResolutionArch>();
        allocIds[owner] = loc.AllocId;
        archIds[owner] = loc.ArchId;
        firstValues[owner] = ent.GetArchetypal<int, C0, ColdResolutionArch>();
        secondValues[owner] = ent.GetArchetypal<int, C1, ColdResolutionArch>();
        layoutSnapshots[owner] = EntArchGraph<ColdResolutionArch>.FieldLayouts(loc.ArchId).ToArray();
        ordinalSnapshots[owner] =
        [
            EntArchGraph<ColdResolutionArch>.FindFieldOrdinal(
                loc.ArchId,
                EntArchColumn<int, C0, ColdResolutionArch>.FieldId),
            EntArchGraph<ColdResolutionArch>.FindFieldOrdinal(
                loc.ArchId,
                EntArchColumn<int, C1, ColdResolutionArch>.FieldId),
            EntArchGraph<ColdResolutionArch>.FindFieldOrdinal(
                loc.ArchId,
                EntArchColumn<int, C2, ColdResolutionArch>.FieldId),
        ];
        int firstFieldId = EntArchColumn<int, C0, ColdResolutionArch>.FieldId;
        int secondFieldId = EntArchColumn<int, C1, ColdResolutionArch>.FieldId;
        int firstSingletonArchId = EntArchGraph<ColdResolutionArch>.GetSingletonArchId(firstFieldId);
        int secondSingletonArchId = EntArchGraph<ColdResolutionArch>.GetSingletonArchId(secondFieldId);
        transitionSnapshots[owner] =
        [
            firstSingletonArchId,
            secondSingletonArchId,
            EntArchGraph<ColdResolutionArch>.GetSingletonArchId(
                EntArchColumn<int, C2, ColdResolutionArch>.FieldId),
            EntArchGraph<ColdResolutionArch>.GetTransitionArchId(firstSingletonArchId, secondFieldId),
            EntArchGraph<ColdResolutionArch>.GetTransitionArchId(loc.ArchId, secondFieldId),
            EntArchGraph<ColdResolutionArch>.GetTransitionArchId(secondSingletonArchId, firstFieldId),
            EntArchGraph<ColdResolutionArch>.GetTransitionArchId(loc.ArchId, firstFieldId),
        ];

        ent.UnsetArchetypal<int, C0, ColdResolutionArch>();
        ent.UnsetArchetypal<int, C1, ColdResolutionArch>();
    }

    private static void ExerciseWarmArch(
        int owner,
        int entsPerOwner,
        int[] allocIds,
        int[] archIds,
        bool[] valid)
    {
        using var arena = new EntArena();
        var ents = new EntMut[entsPerOwner];
        bool ownerValid = true;

        for (int row = 0; row < ents.Length; row++)
        {
            ents[row] = arena.Alloc();
            ents[row].SetArchetypal<int, C0, WarmAccessArch>(owner * 1000 + row);
            ents[row].SetArchetypal<long, C1, WarmAccessArch>(owner * 10000L + row);
        }

        var firstLoc = ents[0].Get<EntArchLoc, WarmAccessArch>();
        allocIds[owner] = firstLoc.AllocId;
        archIds[owner] = firstLoc.ArchId;

        for (int iteration = 0; iteration < 64; iteration++)
        {
            for (int row = 0; row < ents.Length; row++)
            {
                int intValue = owner * 100000 + iteration * ents.Length + row;
                long longValue = owner * 1000000L + iteration * ents.Length + row;
                ents[row].SetArchetypal<int, C0, WarmAccessArch>(intValue);
                ents[row].SetArchetypal<long, C1, WarmAccessArch>(longValue);

                ownerValid &= ents[row].HasArchetypal<int, C0, WarmAccessArch>();
                ownerValid &= ents[row].HasArchetypal<long, C1, WarmAccessArch>();
                ownerValid &= ents[row].GetArchetypal<int, C0, WarmAccessArch>() == intValue;
                ownerValid &= ents[row].GetArchetypal<long, C1, WarmAccessArch>() == longValue;
            }
        }

        foreach (var ent in ents)
            ExitWarmArch(ent);

        valid[owner] = ownerValid;
    }

    private static void FirstSet(
        int owner,
        Barrier barrier,
        int[] allocIds,
        int[] archIds,
        int[] values,
        int[][] columns)
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        barrier.SignalAndWait();

        int value = (owner + 1) * 10;
        ent.SetArchetypal<int, ConcurrentFirstSetField, ConcurrentFirstSetArch>(value);
        var loc = ent.Get<EntArchLoc, ConcurrentFirstSetArch>();
        allocIds[owner] = loc.AllocId;
        archIds[owner] = loc.ArchId;
        values[owner] = ent.GetArchetypal<int, ConcurrentFirstSetField, ConcurrentFirstSetArch>();
        columns[owner] = EntArchColumn<int, ConcurrentFirstSetField, ConcurrentFirstSetArch>
            .ValuesAt(loc.AllocId, loc.ArchId)!;

        Assert.IsTrue(ent.UnsetArchetypal<int, ConcurrentFirstSetField, ConcurrentFirstSetArch>());
    }

    private static void ExitWarmArch(EntMut ent)
    {
        ent.UnsetArchetypal<int, C0, WarmAccessArch>();
        ent.UnsetArchetypal<long, C1, WarmAccessArch>();
    }

    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct C2;
    private readonly record struct ColdResolutionArch;
    private readonly record struct WarmAccessArch;
    private readonly record struct ConcurrentFirstSetField;
    private readonly record struct ConcurrentFirstSetArch;
}
