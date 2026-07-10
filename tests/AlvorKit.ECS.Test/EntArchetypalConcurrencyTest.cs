namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchetypalConcurrencyTest
{
    /// <summary>Verifies cold resolution from different alloc owners interns one exact global arch.</summary>
    [TestMethod]
    public void Archetypal_ConcurrentColdResolution_DifferentAllocsInternSameArch()
    {
        const int ownerCount = 2;
        _ = EntArchColumn<int, C0, ColdResolutionArch>.FieldId;
        _ = EntArchColumn<int, C1, ColdResolutionArch>.FieldId;
        using var barrier = new Barrier(ownerCount);
        var allocIds = new int[ownerCount];
        var archIds = new int[ownerCount];
        var firstValues = new int[ownerCount];
        var secondValues = new int[ownerCount];

        Parallel.Invoke(
            () => ResolveColdArch(0, barrier, allocIds, archIds, firstValues, secondValues),
            () => ResolveColdArch(1, barrier, allocIds, archIds, firstValues, secondValues));

        Assert.AreNotEqual(allocIds[0], allocIds[1]);
        Assert.AreEqual(archIds[0], archIds[1]);
        Assert.AreEqual(10, firstValues[0]);
        Assert.AreEqual(11, secondValues[0]);
        Assert.AreEqual(20, firstValues[1]);
        Assert.AreEqual(21, secondValues[1]);
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

    private static void ResolveColdArch(
        int owner,
        Barrier barrier,
        int[] allocIds,
        int[] archIds,
        int[] firstValues,
        int[] secondValues)
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

        var loc = ent.Get<EntArchLoc, ColdResolutionArch>();
        allocIds[owner] = loc.AllocId;
        archIds[owner] = loc.ArchId;
        firstValues[owner] = ent.GetArchetypal<int, C0, ColdResolutionArch>();
        secondValues[owner] = ent.GetArchetypal<int, C1, ColdResolutionArch>();

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

    private static void ExitWarmArch(EntMut ent)
    {
        ent.UnsetArchetypal<int, C0, WarmAccessArch>();
        ent.UnsetArchetypal<long, C1, WarmAccessArch>();
    }

    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct ColdResolutionArch;
    private readonly record struct WarmAccessArch;
}
