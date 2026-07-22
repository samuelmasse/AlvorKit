namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchRowsTest
{
    /// <summary>Verifies logical and physical capacity grow together from four to eight.</summary>
    [TestMethod]
    public void ArchetypalRows_InitialAndFirstGrowth_KeepParallelCapacitiesAligned()
    {
        using var arena = new EntArena();
        var ents = new EntMut[5];

        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = arena.Alloc();
            ents[i].SetArchetypal<int, FirstField, CapacityArch>(100 + i);

            if (i == 0)
            {
                AssertMetrics(
                    retainedStateCount: 1,
                    activeStateCount: 1,
                    activeRowCount: 1,
                    rowCapacity: 4,
                    componentBufferCount: 1,
                    componentCapacity: 4);
                var singletonLoc = ents[i].Get<EntArchLoc, CapacityArch>();
                Assert.AreEqual(4, EntArchRows<CapacityArch>.CapacityAt(singletonLoc.RowSetId));
                Assert.AreEqual(
                    4,
                    EntArchColumn<int, FirstField, CapacityArch>
                        .Values[singletonLoc.RowSetId].Length);
            }

            ents[i].SetArchetypal<int, SecondField, CapacityArch>(200 + i);

            if (i == 3)
            {
                AssertMetrics(
                    retainedStateCount: 1,
                    activeStateCount: 1,
                    activeRowCount: 4,
                    rowCapacity: 4,
                    componentBufferCount: 2,
                    componentCapacity: 8);
                AssertPairCapacity(ents[i], 4);
            }
            else if (i == 4)
            {
                AssertMetrics(
                    retainedStateCount: 1,
                    activeStateCount: 1,
                    activeRowCount: 5,
                    rowCapacity: 8,
                    componentBufferCount: 2,
                    componentCapacity: 16);
                AssertPairCapacity(ents[i], 8);
            }
        }

        var firstLoc = ents[0].Get<EntArchLoc, CapacityArch>();
        int pairArchId = firstLoc.ArchId;

        for (int i = 0; i < ents.Length; i++)
        {
            Assert.AreEqual(100 + i, ents[i].GetArchetypal<int, FirstField, CapacityArch>());
            Assert.AreEqual(200 + i, ents[i].GetArchetypal<int, SecondField, CapacityArch>());

            var loc = ents[i].Get<EntArchLoc, CapacityArch>();
            Assert.AreEqual(pairArchId, loc.ArchId);
            Assert.AreEqual(i, loc.Row);
        }

        foreach (EntMut ent in ents)
        {
            Assert.IsTrue(ent.UnsetArchetypal<int, SecondField, CapacityArch>());
            Assert.IsTrue(ent.UnsetArchetypal<int, FirstField, CapacityArch>());
        }

        AssertMetrics(
            retainedStateCount: 0,
            activeStateCount: 0,
            activeRowCount: 0,
            rowCapacity: 0,
            componentBufferCount: 0,
            componentCapacity: 0);
    }

    /// <summary>Verifies the exact pool returns and reuses the requested power-of-two length.</summary>
    [TestMethod]
    public void ArchetypalArrayPool_PowerOfTwoCapacity_IsExactAndReusable()
    {
        PoolValue[] first = EntArchArrayPool<PoolValue>.Rent(4);
        Assert.AreEqual(4, first.Length);
        EntArchArrayPool<PoolValue>.Return(first);

        PoolValue[] reused = EntArchArrayPool<PoolValue>.Rent(4);
        Assert.AreSame(first, reused);
        EntArchArrayPool<PoolValue>.Return(reused);

        PoolValue[] larger = EntArchArrayPool<PoolValue>.Rent(8);
        Assert.AreEqual(8, larger.Length);
        EntArchArrayPool<PoolValue>.Return(larger);
    }

    /// <summary>Verifies exact-bucket retention follows observed demand rather than processor count.</summary>
    [TestMethod]
    public void ArchetypalArrayPool_ExactBucket_CachesBeyondProcessorCount()
    {
        var returned = new MultiPoolValue[Environment.ProcessorCount + 1][];
        for (int i = 0; i < returned.Length; i++)
            returned[i] = EntArchArrayPool<MultiPoolValue>.Rent(4);

        foreach (MultiPoolValue[] values in returned)
            EntArchArrayPool<MultiPoolValue>.Return(values);

        for (int i = returned.Length - 1; i >= 0; i--)
            Assert.AreSame(returned[i], EntArchArrayPool<MultiPoolValue>.Rent(4));
    }

    /// <summary>Verifies fields and groups with the same exact component type share one archetypal pool.</summary>
    [TestMethod]
    public void ArchetypalArrayPool_ExactType_IsSharedAcrossFieldsAndGroups()
    {
        using var arena = new EntArena();
        EntMut first = arena.Alloc();
        first.SetArchetypal<SharedPoolValue, FirstField, FirstSharedPoolArch>(new(1));
        var firstLoc = first.Get<EntArchLoc, FirstSharedPoolArch>();
        SharedPoolValue[] firstValues = EntArchColumn<SharedPoolValue, FirstField, FirstSharedPoolArch>
            .Values[firstLoc.RowSetId];
        Assert.IsTrue(first.UnsetArchetypal<SharedPoolValue, FirstField, FirstSharedPoolArch>());

        EntMut second = arena.Alloc();
        second.SetArchetypal<SharedPoolValue, SecondField, SecondSharedPoolArch>(new(2));
        var secondLoc = second.Get<EntArchLoc, SecondSharedPoolArch>();
        SharedPoolValue[] secondValues = EntArchColumn<SharedPoolValue, SecondField, SecondSharedPoolArch>
            .Values[secondLoc.RowSetId];
        Assert.AreSame(firstValues, secondValues);
        Assert.AreEqual(2, second.GetArchetypal<SharedPoolValue, SecondField, SecondSharedPoolArch>().Value);
        Assert.IsTrue(second.UnsetArchetypal<SharedPoolValue, SecondField, SecondSharedPoolArch>());
    }

    /// <summary>Verifies exact 25% occupancy is retained, lower occupancy halves capacity, and zero returns every buffer.</summary>
    [TestMethod]
    public void ArchetypalRows_SparseUsage_HalvesCapacityAndReturnsEmptyBuffers()
    {
        using var arena = new EntArena();
        var ents = new EntMut[17];
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = arena.Alloc();
            ents[i].SetArchetypal<int, FirstField, ShrinkArch>(100 + i);
            ents[i].SetArchetypal<int, SecondField, ShrinkArch>(200 + i);
        }

        var pairLoc = ents[0].Get<EntArchLoc, ShrinkArch>();
        int rowSetId = pairLoc.RowSetId;
        int archId = pairLoc.ArchId;
        Assert.AreEqual(32, EntArchRows<ShrinkArch>.CapacityAt(rowSetId));

        for (int i = 0; i < 9; i++)
            Assert.IsTrue(ents[i].UnsetArchetypal<int, SecondField, ShrinkArch>());

        Assert.AreEqual(8, EntArchRows<ShrinkArch>.CountAt(rowSetId));
        Assert.AreEqual(32, EntArchRows<ShrinkArch>.CapacityAt(rowSetId));
        int[] firstBeforeShrink = EntArchColumn<int, FirstField, ShrinkArch>.Values[rowSetId];
        int[] secondBeforeShrink = EntArchColumn<int, SecondField, ShrinkArch>.Values[rowSetId];

        Assert.IsTrue(ents[9].UnsetArchetypal<int, SecondField, ShrinkArch>());

        Assert.AreEqual(7, EntArchRows<ShrinkArch>.CountAt(rowSetId));
        Assert.AreEqual(16, EntArchRows<ShrinkArch>.CapacityAt(rowSetId));
        int[] firstAfterShrink = EntArchColumn<int, FirstField, ShrinkArch>.Values[rowSetId];
        int[] secondAfterShrink = EntArchColumn<int, SecondField, ShrinkArch>.Values[rowSetId];
        Assert.AreNotSame(firstBeforeShrink, firstAfterShrink);
        Assert.AreNotSame(secondBeforeShrink, secondAfterShrink);
        Assert.IsTrue(firstAfterShrink.Length < firstBeforeShrink.Length);
        Assert.IsTrue(secondAfterShrink.Length < secondBeforeShrink.Length);

        for (int i = 10; i < ents.Length; i++)
            Assert.IsTrue(ents[i].UnsetArchetypal<int, SecondField, ShrinkArch>());

        Assert.AreEqual(0, EntArchRows<ShrinkArch>.CountAt(rowSetId));
        Assert.AreEqual(0, EntArchRows<ShrinkArch>.CapacityAt(rowSetId));
        Assert.IsNull(EntArchColumn<int, FirstField, ShrinkArch>.Values[rowSetId]);
        Assert.IsNull(EntArchColumn<int, SecondField, ShrinkArch>.Values[rowSetId]);

        EntMut reactivated = arena.Alloc();
        reactivated.SetArchetypal<int, FirstField, ShrinkArch>(300);
        reactivated.SetArchetypal<int, SecondField, ShrinkArch>(400);
        var reactivatedLoc = reactivated.Get<EntArchLoc, ShrinkArch>();
        Assert.AreEqual(archId, reactivatedLoc.ArchId);
        Assert.AreEqual(0, reactivatedLoc.Row);
        Assert.AreEqual(rowSetId, reactivatedLoc.RowSetId);
        Assert.AreEqual(4, EntArchRows<ShrinkArch>.CapacityAt(rowSetId));
        Assert.AreEqual(300, reactivated.GetArchetypal<int, FirstField, ShrinkArch>());
        Assert.AreEqual(400, reactivated.GetArchetypal<int, SecondField, ShrinkArch>());
        Assert.IsTrue(reactivated.UnsetArchetypal<int, SecondField, ShrinkArch>());
        Assert.IsTrue(reactivated.UnsetArchetypal<int, FirstField, ShrinkArch>());

        foreach (EntMut ent in ents)
            Assert.IsTrue(ent.UnsetArchetypal<int, FirstField, ShrinkArch>());
    }

    /// <summary>Verifies an empty reference column is cleared before its exact buffer is reused.</summary>
    [TestMethod]
    public void ArchetypalRows_EmptyReferenceColumn_ClearsBufferBeforeReuse()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        ent.SetArchetypal<PooledBox, FirstField, ReferencePoolArch>(new());

        var loc = ent.Get<EntArchLoc, ReferencePoolArch>();

        Assert.IsTrue(ent.UnsetArchetypal<PooledBox, FirstField, ReferencePoolArch>());
        Assert.IsNull(EntArchColumn<PooledBox, FirstField, ReferencePoolArch>.Values[loc.RowSetId]);

        EntMut reactivated = arena.Alloc();
        reactivated.SetArchetypal<PooledBox, FirstField, ReferencePoolArch>(new());
        var reactivatedLoc = reactivated.Get<EntArchLoc, ReferencePoolArch>();
        PooledBox[] values = EntArchColumn<PooledBox, FirstField, ReferencePoolArch>
            .Values[reactivatedLoc.RowSetId];
        Assert.AreEqual(4, values.Length);
        Assert.IsNotNull(values[0]);
        for (int i = 1; i < values.Length; i++)
            Assert.IsNull(values[i]);
        Assert.IsTrue(reactivated.UnsetArchetypal<PooledBox, FirstField, ReferencePoolArch>());
    }

    private static void AssertMetrics(
        long retainedStateCount,
        long activeStateCount,
        long activeRowCount,
        long rowCapacity,
        long componentBufferCount,
        long componentCapacity)
    {
        var metrics = EntArchDiagnostics<CapacityArch>.Capture();

        Assert.AreEqual(retainedStateCount, metrics.RetainedStateCount);
        Assert.AreEqual(activeStateCount, metrics.ActiveStateCount);
        Assert.AreEqual(activeRowCount, metrics.ActiveRowCount);
        Assert.AreEqual(rowCapacity, metrics.RowCapacity);
        Assert.AreEqual(metrics.RowCapacity - activeRowCount, metrics.RowSlack);
        Assert.AreEqual(componentBufferCount, metrics.ComponentBufferCount);
        Assert.AreEqual(componentCapacity, metrics.ComponentCapacity);
    }

    private static void AssertPairCapacity(EntMut ent, int capacity)
    {
        var loc = ent.Get<EntArchLoc, CapacityArch>();

        Assert.AreEqual(capacity, EntArchRows<CapacityArch>.CapacityAt(loc.RowSetId));
        Assert.AreEqual(capacity, EntArchColumn<int, FirstField, CapacityArch>.Values[loc.RowSetId].Length);
        Assert.AreEqual(capacity, EntArchColumn<int, SecondField, CapacityArch>.Values[loc.RowSetId].Length);
    }

    private readonly record struct FirstField;
    private readonly record struct SecondField;
    private readonly record struct CapacityArch;
    private readonly record struct ShrinkArch;
    private readonly record struct ReferencePoolArch;
    private readonly record struct PoolValue(int Value);
    private readonly record struct MultiPoolValue(int Value);
    private readonly record struct SharedPoolValue(int Value);
    private readonly record struct FirstSharedPoolArch;
    private readonly record struct SecondSharedPoolArch;
    private sealed class PooledBox;
}
