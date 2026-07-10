namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchGraphTest
{
    /// <summary>Verifies all non-empty four-field subsets survive catalog growth and reuse canonical IDs.</summary>
    [TestMethod]
    public void ArchetypalGraph_AllFourFieldSubsets_SurviveCatalogGrowth()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        RegisterSubsetFields();
        int initialCapacity = EntArchGraph<SubsetArch>.ArchCapacity;
        var archIds = new int[16];

        for (int mask = 1; mask < archIds.Length; mask++)
        {
            SetSubset(ent, mask, reverse: false);
            int archId = ent.Get<EntArchLoc, SubsetArch>().ArchId;
            archIds[mask] = archId;

            CollectionAssert.AreEqual(ExpectedFieldIds(mask), EntArchGraph<SubsetArch>.FieldIds(archId).ToArray());
            ExitSubset(ent);
        }

        Assert.IsTrue(EntArchGraph<SubsetArch>.ArchCapacity > initialCapacity);
        Assert.AreEqual(15, archIds.Skip(1).Distinct().Count());

        var metrics = EntArchDiagnostics<SubsetArch>.Capture();
        Assert.AreEqual(15, metrics.SignatureIndexCount);
        Assert.AreEqual(32, metrics.SignatureIndexCapacity);

        int middleFieldId = EntArchColumn<int, S1, SubsetArch>.FieldId;
        Assert.AreEqual(
            EntArchGraph<SubsetArch>.UnresolvedTransitionArchId,
            EntArchGraph<SubsetArch>.GetAddArchId(archIds[0b0101], middleFieldId));
        SetSubset(ent, 0b0101, reverse: false);
        ent.SetArchetypal<int, S1, SubsetArch>(1);
        Assert.AreEqual(archIds[0b0111], ent.Get<EntArchLoc, SubsetArch>().ArchId);
        ExitSubset(ent);

        for (int mask = archIds.Length - 1; mask >= 1; mask--)
        {
            SetSubset(ent, mask, reverse: true);
            Assert.AreEqual(archIds[mask], ent.Get<EntArchLoc, SubsetArch>().ArchId);
            CollectionAssert.AreEqual(ExpectedFieldIds(mask), EntArchGraph<SubsetArch>.FieldIds(archIds[mask]).ToArray());
            ExitSubset(ent);
        }
    }

    /// <summary>Verifies field-capacity growth preserves an existing singleton and its later inverse transition.</summary>
    [TestMethod]
    public void ArchetypalGraph_FieldCapacityGrowth_PreservesExistingTransitions()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        int firstFieldId = EntArchColumn<int, F00, FieldGrowthArch>.FieldId;

        ent.SetArchetypal<int, F00, FieldGrowthArch>(10);
        int singletonArchId = ent.Get<EntArchLoc, FieldGrowthArch>().ArchId;

        RegisterRemainingGrowthFields();
        int lastFieldId = EntArchColumn<int, F15, FieldGrowthArch>.FieldId;

        Assert.AreEqual(singletonArchId, EntArchGraph<FieldGrowthArch>.GetSingletonArchId(firstFieldId));
        Assert.IsTrue(EntArchGraph<FieldGrowthArch>.ContainsField(singletonArchId, firstFieldId));

        ent.SetArchetypal<int, F15, FieldGrowthArch>(15);
        int pairArchId = ent.Get<EntArchLoc, FieldGrowthArch>().ArchId;
        Assert.AreEqual(10, ent.GetArchetypal<int, F00, FieldGrowthArch>());
        Assert.AreEqual(15, ent.GetArchetypal<int, F15, FieldGrowthArch>());
        Assert.AreEqual(pairArchId, EntArchGraph<FieldGrowthArch>.GetAddArchId(singletonArchId, lastFieldId));
        Assert.AreEqual(singletonArchId, EntArchGraph<FieldGrowthArch>.GetRemoveArchId(pairArchId, lastFieldId));

        Assert.IsTrue(ent.UnsetArchetypal<int, F15, FieldGrowthArch>());
        Assert.AreEqual(singletonArchId, ent.Get<EntArchLoc, FieldGrowthArch>().ArchId);
        Assert.IsTrue(ent.UnsetArchetypal<int, F00, FieldGrowthArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, FieldGrowthArch>());
    }

    private static void RegisterSubsetFields()
    {
        _ = EntArchColumn<int, S0, SubsetArch>.FieldId;
        _ = EntArchColumn<int, S1, SubsetArch>.FieldId;
        _ = EntArchColumn<int, S2, SubsetArch>.FieldId;
        _ = EntArchColumn<int, S3, SubsetArch>.FieldId;
    }

    private static void SetSubset(EntMut ent, int mask, bool reverse)
    {
        if (reverse)
        {
            if ((mask & 0b1000) != 0) ent.SetArchetypal<int, S3, SubsetArch>(3);
            if ((mask & 0b0100) != 0) ent.SetArchetypal<int, S2, SubsetArch>(2);
            if ((mask & 0b0010) != 0) ent.SetArchetypal<int, S1, SubsetArch>(1);
            if ((mask & 0b0001) != 0) ent.SetArchetypal<int, S0, SubsetArch>(0);
            return;
        }

        if ((mask & 0b0001) != 0) ent.SetArchetypal<int, S0, SubsetArch>(0);
        if ((mask & 0b0010) != 0) ent.SetArchetypal<int, S1, SubsetArch>(1);
        if ((mask & 0b0100) != 0) ent.SetArchetypal<int, S2, SubsetArch>(2);
        if ((mask & 0b1000) != 0) ent.SetArchetypal<int, S3, SubsetArch>(3);
    }

    private static int[] ExpectedFieldIds(int mask)
    {
        var fieldIds = new List<int>(4);
        if ((mask & 0b0001) != 0) fieldIds.Add(EntArchColumn<int, S0, SubsetArch>.FieldId);
        if ((mask & 0b0010) != 0) fieldIds.Add(EntArchColumn<int, S1, SubsetArch>.FieldId);
        if ((mask & 0b0100) != 0) fieldIds.Add(EntArchColumn<int, S2, SubsetArch>.FieldId);
        if ((mask & 0b1000) != 0) fieldIds.Add(EntArchColumn<int, S3, SubsetArch>.FieldId);
        fieldIds.Sort();
        return [.. fieldIds];
    }

    private static void ExitSubset(EntMut ent)
    {
        ent.UnsetArchetypal<int, S0, SubsetArch>();
        ent.UnsetArchetypal<int, S1, SubsetArch>();
        ent.UnsetArchetypal<int, S2, SubsetArch>();
        ent.UnsetArchetypal<int, S3, SubsetArch>();
        Assert.IsFalse(ent.Has<EntArchLoc, SubsetArch>());
    }

    private static void RegisterRemainingGrowthFields()
    {
        _ = EntArchColumn<int, F01, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F02, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F03, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F04, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F05, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F06, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F07, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F08, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F09, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F10, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F11, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F12, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F13, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F14, FieldGrowthArch>.FieldId;
        _ = EntArchColumn<int, F15, FieldGrowthArch>.FieldId;
    }

    private readonly record struct S0;
    private readonly record struct S1;
    private readonly record struct S2;
    private readonly record struct S3;
    private readonly record struct SubsetArch;
    private readonly record struct F00;
    private readonly record struct F01;
    private readonly record struct F02;
    private readonly record struct F03;
    private readonly record struct F04;
    private readonly record struct F05;
    private readonly record struct F06;
    private readonly record struct F07;
    private readonly record struct F08;
    private readonly record struct F09;
    private readonly record struct F10;
    private readonly record struct F11;
    private readonly record struct F12;
    private readonly record struct F13;
    private readonly record struct F14;
    private readonly record struct F15;
    private readonly record struct FieldGrowthArch;
}
