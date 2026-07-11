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
        int firstEdgeCapacity = 0;
        var archIds = new int[16];

        for (int mask = 1; mask < archIds.Length; mask++)
        {
            SetSubset(ent, mask, reverse: false);
            int archId = ent.Get<EntArchLoc, SubsetArch>().ArchId;
            archIds[mask] = archId;

            CollectionAssert.AreEqual(ExpectedFieldIds(mask), EntArchGraph<SubsetArch>.FieldIds(archId).ToArray());
            AssertPackedMembership(archId, mask);
            ExitSubset(ent);

            if (firstEdgeCapacity == 0)
            {
                var currentMetrics = EntArchDiagnostics<SubsetArch>.Capture();
                if (currentMetrics.StoredTransitionEdgeCount != 0)
                    firstEdgeCapacity = currentMetrics.TransitionEdgeCapacity;
            }
        }

        ent.SetArchetypal<int, S4, SubsetArch>(4);
        Assert.IsTrue(ent.UnsetArchetypal<int, S4, SubsetArch>());
        ResolveFullArchRemovals(ent, archIds);

        Assert.IsTrue(EntArchGraph<SubsetArch>.ArchCapacity > initialCapacity);
        Assert.AreEqual(15, archIds.Skip(1).Distinct().Count());
        Assert.IsTrue(firstEdgeCapacity > 0);

        var metrics = EntArchDiagnostics<SubsetArch>.Capture();
        Assert.AreEqual(12, Unsafe.SizeOf<EntArchEdge>());
        Assert.AreEqual(16, metrics.SignatureIndexCount);
        Assert.AreEqual(32, metrics.SignatureIndexCapacity);
        Assert.AreEqual(33, metrics.SignatureMembershipCount);
        Assert.AreEqual(metrics.SignatureMembershipCount, metrics.FieldLayoutCount);
        Assert.IsTrue(metrics.FieldLayoutCapacity >= metrics.FieldLayoutCount);
        Assert.IsTrue(metrics.SignatureScratchCapacity >= 4);
        Assert.AreEqual(5, metrics.SingletonArchCount);
        Assert.IsTrue(metrics.SingletonDirectoryCapacity > metrics.RegisteredFieldCount);
        Assert.AreEqual(0L, metrics.TransitionCellCapacity);
        Assert.IsTrue(metrics.StoredTransitionEdgeCount > 0);
        Assert.AreEqual(0, metrics.StoredTransitionEdgeCount % 2);
        Assert.IsTrue(metrics.TransitionEdgeCapacity >= metrics.StoredTransitionEdgeCount + 1);
        Assert.IsTrue(metrics.TransitionEdgeCapacity > firstEdgeCapacity);
        Assert.AreEqual(metrics.ArchCapacity, metrics.EdgeHeadCapacity);
        Assert.AreEqual(
            metrics.StoredTransitionEdgeCount + 2L * metrics.SingletonArchCount,
            metrics.DirectedStructuralEdgeCount);
        AssertSubsetInverseTransitions(archIds);

        int middleFieldId = EntArchColumn<int, S1, SubsetArch>.FieldId;
        Assert.AreEqual(
            EntArchGraph<SubsetArch>.UnresolvedTransitionArchId,
            EntArchGraph<SubsetArch>.GetTransitionArchId(archIds[0b0101], middleFieldId));
        SetSubset(ent, 0b0101, reverse: false);
        ent.SetArchetypal<int, S1, SubsetArch>(1);
        Assert.AreEqual(archIds[0b0111], ent.Get<EntArchLoc, SubsetArch>().ArchId);
        ExitSubset(ent);

        for (int mask = archIds.Length - 1; mask >= 1; mask--)
        {
            SetSubset(ent, mask, reverse: true);
            Assert.AreEqual(archIds[mask], ent.Get<EntArchLoc, SubsetArch>().ArchId);
            CollectionAssert.AreEqual(ExpectedFieldIds(mask), EntArchGraph<SubsetArch>.FieldIds(archIds[mask]).ToArray());
            AssertPackedMembership(archIds[mask], mask);
            ExitSubset(ent);
        }
    }

    /// <summary>Verifies field growth preserves sparse state and lazily populates the singleton directory.</summary>
    [TestMethod]
    public void ArchetypalGraph_FieldCapacityGrowth_PreservesExistingTransitions()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        int firstFieldId = EntArchColumn<int, F00, FieldGrowthArch>.FieldId;

        ent.SetArchetypal<int, F00, FieldGrowthArch>(10);
        int singletonArchId = ent.Get<EntArchLoc, FieldGrowthArch>().ArchId;
        var singletonLayout = EntArchGraph<FieldGrowthArch>.FieldLayouts(singletonArchId).ToArray();
        var beforeFieldGrowth = EntArchDiagnostics<FieldGrowthArch>.Capture();

        RegisterRemainingGrowthFields();
        int lastFieldId = EntArchColumn<int, F15, FieldGrowthArch>.FieldId;
        var afterFieldGrowth = EntArchDiagnostics<FieldGrowthArch>.Capture();

        Assert.IsTrue(afterFieldGrowth.FieldCapacity > beforeFieldGrowth.FieldCapacity);
        Assert.IsTrue(afterFieldGrowth.SingletonDirectoryCapacity > beforeFieldGrowth.SingletonDirectoryCapacity);
        Assert.AreEqual(beforeFieldGrowth.MaterializedArchCount, afterFieldGrowth.MaterializedArchCount);
        Assert.AreEqual(beforeFieldGrowth.ArchCapacity, afterFieldGrowth.ArchCapacity);
        Assert.AreEqual(beforeFieldGrowth.SignatureMembershipCount, afterFieldGrowth.SignatureMembershipCount);
        Assert.AreEqual(beforeFieldGrowth.SignatureIndexCount, afterFieldGrowth.SignatureIndexCount);
        Assert.AreEqual(beforeFieldGrowth.SignatureScratchCapacity, afterFieldGrowth.SignatureScratchCapacity);
        Assert.AreEqual(beforeFieldGrowth.SingletonArchCount, afterFieldGrowth.SingletonArchCount);
        Assert.AreEqual(beforeFieldGrowth.DirectedStructuralEdgeCount, afterFieldGrowth.DirectedStructuralEdgeCount);
        Assert.AreEqual(beforeFieldGrowth.StoredTransitionEdgeCount, afterFieldGrowth.StoredTransitionEdgeCount);
        Assert.AreEqual(beforeFieldGrowth.TransitionEdgeCapacity, afterFieldGrowth.TransitionEdgeCapacity);
        Assert.AreEqual(beforeFieldGrowth.EdgeHeadCapacity, afterFieldGrowth.EdgeHeadCapacity);
        Assert.AreEqual(0L, afterFieldGrowth.TransitionCellCapacity);
        Assert.AreEqual(singletonArchId, EntArchGraph<FieldGrowthArch>.GetSingletonArchId(firstFieldId));
        Assert.AreEqual(EntArchGraph<FieldGrowthArch>.NoArchId, EntArchGraph<FieldGrowthArch>.GetSingletonArchId(lastFieldId));
        Assert.AreEqual(0, EntArchGraph<FieldGrowthArch>.FindFieldOrdinal(singletonArchId, firstFieldId));
        Assert.AreEqual(
            EntArchGraph<FieldGrowthArch>.NoFieldOrdinal,
            EntArchGraph<FieldGrowthArch>.FindFieldOrdinal(singletonArchId, lastFieldId));
        CollectionAssert.AreEqual(singletonLayout, EntArchGraph<FieldGrowthArch>.FieldLayouts(singletonArchId).ToArray());

        ent.SetArchetypal<int, F15, FieldGrowthArch>(15);
        int pairArchId = ent.Get<EntArchLoc, FieldGrowthArch>().ArchId;
        Assert.AreEqual(EntArchGraph<FieldGrowthArch>.NoArchId, EntArchGraph<FieldGrowthArch>.GetSingletonArchId(lastFieldId));
        Assert.AreEqual(10, ent.GetArchetypal<int, F00, FieldGrowthArch>());
        Assert.AreEqual(15, ent.GetArchetypal<int, F15, FieldGrowthArch>());
        Assert.AreEqual(pairArchId, EntArchGraph<FieldGrowthArch>.GetTransitionArchId(singletonArchId, lastFieldId));
        Assert.AreEqual(singletonArchId, EntArchGraph<FieldGrowthArch>.GetTransitionArchId(pairArchId, lastFieldId));
        Assert.AreEqual(0, EntArchGraph<FieldGrowthArch>.FindFieldOrdinal(pairArchId, firstFieldId));
        Assert.AreEqual(1, EntArchGraph<FieldGrowthArch>.FindFieldOrdinal(pairArchId, lastFieldId));
        var pairLayouts = EntArchGraph<FieldGrowthArch>.FieldLayouts(pairArchId);
        Assert.AreEqual(Unsafe.SizeOf<EntMut>(), pairLayouts[0].BytePrefix);
        Assert.AreEqual(Unsafe.SizeOf<EntMut>() + Unsafe.SizeOf<int>(), pairLayouts[1].BytePrefix);

        Assert.IsTrue(ent.UnsetArchetypal<int, F15, FieldGrowthArch>());
        Assert.AreEqual(singletonArchId, ent.Get<EntArchLoc, FieldGrowthArch>().ArchId);
        Assert.IsTrue(ent.UnsetArchetypal<int, F00, FieldGrowthArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, FieldGrowthArch>());

        EntMut lastOnly = arena.Alloc();
        lastOnly.SetArchetypal<int, F15, FieldGrowthArch>(16);
        int lastSingletonArchId = lastOnly.Get<EntArchLoc, FieldGrowthArch>().ArchId;
        Assert.AreEqual(lastSingletonArchId, EntArchGraph<FieldGrowthArch>.GetSingletonArchId(lastFieldId));

        Assert.IsTrue(lastOnly.UnsetArchetypal<int, F15, FieldGrowthArch>());
        lastOnly.SetArchetypal<int, F15, FieldGrowthArch>(17);
        Assert.AreEqual(lastSingletonArchId, lastOnly.Get<EntArchLoc, FieldGrowthArch>().ArchId);
        Assert.IsTrue(lastOnly.UnsetArchetypal<int, F15, FieldGrowthArch>());
    }

    private static void RegisterSubsetFields()
    {
        _ = EntArchColumn<int, S0, SubsetArch>.FieldId;
        _ = EntArchColumn<int, S1, SubsetArch>.FieldId;
        _ = EntArchColumn<int, S2, SubsetArch>.FieldId;
        _ = EntArchColumn<int, S3, SubsetArch>.FieldId;
        _ = EntArchColumn<int, S4, SubsetArch>.FieldId;
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

    private static void ResolveFullArchRemovals(EntMut ent, int[] archIds)
    {
        for (int fieldIndex = 0; fieldIndex < 4; fieldIndex++)
        {
            SetSubset(ent, 0b1111, reverse: false);
            Assert.IsTrue(UnsetSubsetField(ent, fieldIndex));
            Assert.AreEqual(archIds[0b1111 & ~(1 << fieldIndex)], ent.Get<EntArchLoc, SubsetArch>().ArchId);
            ExitSubset(ent);
        }
    }

    private static void AssertSubsetInverseTransitions(int[] archIds)
    {
        int fullArchId = archIds[0b1111];
        int[] fieldIds =
        [
            EntArchColumn<int, S0, SubsetArch>.FieldId,
            EntArchColumn<int, S1, SubsetArch>.FieldId,
            EntArchColumn<int, S2, SubsetArch>.FieldId,
            EntArchColumn<int, S3, SubsetArch>.FieldId,
        ];

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            int dstArchId = archIds[0b1111 & ~(1 << fieldIndex)];
            int fieldId = fieldIds[fieldIndex];
            Assert.AreEqual(dstArchId, EntArchGraph<SubsetArch>.GetTransitionArchId(fullArchId, fieldId));
            Assert.AreEqual(fullArchId, EntArchGraph<SubsetArch>.GetTransitionArchId(dstArchId, fieldId));
        }

        int firstPairFieldId = EntArchColumn<int, S1, SubsetArch>.FieldId;
        Assert.AreEqual(archIds[0b0011], EntArchGraph<SubsetArch>.GetTransitionArchId(archIds[0b0001], firstPairFieldId));
        Assert.AreEqual(archIds[0b0001], EntArchGraph<SubsetArch>.GetTransitionArchId(archIds[0b0011], firstPairFieldId));
    }

    private static bool UnsetSubsetField(EntMut ent, int fieldIndex)
    {
        if (fieldIndex == 0)
            return ent.UnsetArchetypal<int, S0, SubsetArch>();
        if (fieldIndex == 1)
            return ent.UnsetArchetypal<int, S1, SubsetArch>();
        if (fieldIndex == 2)
            return ent.UnsetArchetypal<int, S2, SubsetArch>();

        return ent.UnsetArchetypal<int, S3, SubsetArch>();
    }

    private static void AssertPackedMembership(int archId, int mask)
    {
        int[] expectedFieldIds = ExpectedFieldIds(mask);
        int[] registeredFieldIds =
        [
            EntArchColumn<int, S0, SubsetArch>.FieldId,
            EntArchColumn<int, S1, SubsetArch>.FieldId,
            EntArchColumn<int, S2, SubsetArch>.FieldId,
            EntArchColumn<int, S3, SubsetArch>.FieldId,
        ];
        var fieldIds = EntArchGraph<SubsetArch>.FieldIds(archId);
        var layouts = EntArchGraph<SubsetArch>.FieldLayouts(archId);
        Assert.AreEqual(expectedFieldIds.Length, fieldIds.Length);
        Assert.AreEqual(fieldIds.Length, layouts.Length);

        foreach (int fieldId in registeredFieldIds)
        {
            int expectedOrdinal = Array.IndexOf(expectedFieldIds, fieldId);
            if (expectedOrdinal < 0)
                expectedOrdinal = EntArchGraph<SubsetArch>.NoFieldOrdinal;

            int ordinal = EntArchGraph<SubsetArch>.FindFieldOrdinal(archId, fieldId);
            Assert.AreEqual(expectedOrdinal, ordinal);

            if (ordinal == EntArchGraph<SubsetArch>.NoFieldOrdinal)
                continue;

            Assert.AreEqual(fieldId, fieldIds[ordinal]);
            Assert.IsFalse(layouts[ordinal].ContainsReferences);
            Assert.AreEqual(Unsafe.SizeOf<EntMut>() + ordinal * Unsafe.SizeOf<int>(), layouts[ordinal].BytePrefix);
        }
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
    private readonly record struct S4;
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
