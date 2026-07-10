namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchRowsTest
{
    /// <summary>Verifies first activation reserves four rows and parallel buffers grow together from four to eight.</summary>
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
                    rowSlack: 3,
                    componentBufferCount: 1,
                    componentCapacity: 4);
                var singletonLoc = ents[i].Get<EntArchLoc, CapacityArch>();
                Assert.AreEqual(
                    4,
                    EntArchColumn<int, FirstField, CapacityArch>
                        .Values[singletonLoc.AllocId][singletonLoc.ArchId].Length);
            }

            ents[i].SetArchetypal<int, SecondField, CapacityArch>(200 + i);

            if (i == 3)
            {
                AssertMetrics(
                    retainedStateCount: 2,
                    activeStateCount: 1,
                    activeRowCount: 4,
                    rowCapacity: 8,
                    rowSlack: 4,
                    componentBufferCount: 3,
                    componentCapacity: 12);
                AssertPairBufferCapacity(ents[i], 4);
            }
            else if (i == 4)
            {
                AssertMetrics(
                    retainedStateCount: 2,
                    activeStateCount: 1,
                    activeRowCount: 5,
                    rowCapacity: 12,
                    rowSlack: 7,
                    componentBufferCount: 3,
                    componentCapacity: 20);
                AssertPairBufferCapacity(ents[i], 8);
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
    }

    private static void AssertMetrics(
        long retainedStateCount,
        long activeStateCount,
        long activeRowCount,
        long rowCapacity,
        long rowSlack,
        long componentBufferCount,
        long componentCapacity)
    {
        var metrics = EntArchDiagnostics<CapacityArch>.Capture();

        Assert.AreEqual(retainedStateCount, metrics.RetainedStateCount);
        Assert.AreEqual(activeStateCount, metrics.ActiveStateCount);
        Assert.AreEqual(activeRowCount, metrics.ActiveRowCount);
        Assert.AreEqual(rowCapacity, metrics.RowCapacity);
        Assert.AreEqual(rowSlack, metrics.RowSlack);
        Assert.AreEqual(componentBufferCount, metrics.ComponentBufferCount);
        Assert.AreEqual(componentCapacity, metrics.ComponentCapacity);
    }

    private static void AssertPairBufferCapacity(EntMut ent, int capacity)
    {
        var loc = ent.Get<EntArchLoc, CapacityArch>();

        Assert.AreEqual(capacity, EntArchColumn<int, FirstField, CapacityArch>.Values[loc.AllocId][loc.ArchId].Length);
        Assert.AreEqual(capacity, EntArchColumn<int, SecondField, CapacityArch>.Values[loc.AllocId][loc.ArchId].Length);
    }

    private readonly record struct FirstField;
    private readonly record struct SecondField;
    private readonly record struct CapacityArch;
}
