namespace AlvorKit.Hashing.Test;

[TestClass]
public class EpochIndex32Test
{
    [TestMethod]
    public void GetOrAdd_StoresOneSlotAndKeepsItForDuplicates()
    {
        var index = new EpochIndex32(0);

        var first = index.GetOrAdd(-17, 41, out var firstAdded);
        var duplicate = index.GetOrAdd(-17, 99, out var duplicateAdded);

        Assert.IsTrue(firstAdded);
        Assert.IsFalse(duplicateAdded);
        Assert.AreEqual(41, first);
        Assert.AreEqual(41, duplicate);
        Assert.AreEqual(1, index.Count);
    }

    [TestMethod]
    public void TryGet_SupportsEveryKeyAndSlotBoundary()
    {
        var index = new EpochIndex32(4);
        index.GetOrAdd(0, -1, out _);
        index.GetOrAdd(-1, int.MinValue, out _);
        index.GetOrAdd(int.MinValue, 0, out _);
        index.GetOrAdd(int.MaxValue, int.MaxValue, out _);

        Assert.IsTrue(index.TryGet(0, out var zeroSlot));
        Assert.IsTrue(index.TryGet(-1, out var negativeSlot));
        Assert.IsTrue(index.TryGet(int.MinValue, out var minimumSlot));
        Assert.IsTrue(index.TryGet(int.MaxValue, out var maximumSlot));
        Assert.AreEqual(-1, zeroSlot);
        Assert.AreEqual(int.MinValue, negativeSlot);
        Assert.AreEqual(0, minimumSlot);
        Assert.AreEqual(int.MaxValue, maximumSlot);
    }

    [TestMethod]
    public void TryGet_WhenMissing_WritesNegativeOne()
    {
        var index = new EpochIndex32(0);

        var found = index.TryGet(8, out var slot);

        Assert.IsFalse(found);
        Assert.AreEqual(-1, slot);
    }

    [TestMethod]
    public void Begin_RemovesMappingsAndAcceptsNewOnes()
    {
        var index = new EpochIndex32(8);
        index.GetOrAdd(3, 30, out _);
        index.GetOrAdd(4, 40, out _);

        index.Begin();

        Assert.AreEqual(0, index.Count);
        Assert.IsFalse(index.TryGet(3, out _));
        Assert.AreEqual(50, index.GetOrAdd(5, 50, out var added));
        Assert.IsTrue(added);
    }

    [TestMethod]
    public void Growth_PreservesAllActiveMappings()
    {
        var index = new EpochIndex32(0);
        for (var key = -512; key < 512; key++)
            index.GetOrAdd(key, key * 3, out _);

        index.EnsureCapacity(4_096);

        Assert.AreEqual(1_024, index.Count);
        for (var key = -512; key < 512; key++)
        {
            Assert.IsTrue(index.TryGet(key, out var slot));
            Assert.AreEqual(key * 3, slot);
        }
    }

    [TestMethod]
    public void WarmedOperations_AllocateNothing()
    {
        var index = new EpochIndex32(256);
        index.GetOrAdd(0, 0, out _);
        index.Begin();
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var iteration = 0; iteration < 100; iteration++)
        {
            index.Begin();
            for (var key = 0; key < 128; key++)
                index.GetOrAdd(key, key, out _);
            for (var key = 0; key < 128; key++)
                index.TryGet(key, out _);
        }

        Assert.AreEqual(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
