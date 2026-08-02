namespace AlvorKit.Hashing.Test;

[TestClass]
public class EpochIndex64Test
{
    [TestMethod]
    public void GetOrAdd_StoresOneSlotAndKeepsItForDuplicates()
    {
        var index = new EpochIndex64(0);

        var first = index.GetOrAdd(ulong.MaxValue, 41, out var firstAdded);
        var duplicate = index.GetOrAdd(ulong.MaxValue, 99, out var duplicateAdded);

        Assert.IsTrue(firstAdded);
        Assert.IsFalse(duplicateAdded);
        Assert.AreEqual(41, first);
        Assert.AreEqual(41, duplicate);
        Assert.AreEqual(1, index.Count);
    }

    [TestMethod]
    public void TryGet_SupportsEveryKeyBoundary()
    {
        var index = new EpochIndex64(3);
        index.GetOrAdd(0, -1, out _);
        index.GetOrAdd(1UL << 63, 63, out _);
        index.GetOrAdd(ulong.MaxValue, int.MaxValue, out _);

        Assert.IsTrue(index.TryGet(0, out var zeroSlot));
        Assert.IsTrue(index.TryGet(1UL << 63, out var highSlot));
        Assert.IsTrue(index.TryGet(ulong.MaxValue, out var maximumSlot));
        Assert.AreEqual(-1, zeroSlot);
        Assert.AreEqual(63, highSlot);
        Assert.AreEqual(int.MaxValue, maximumSlot);
    }

    [TestMethod]
    public void TryGet_WhenMissing_WritesNegativeOne()
    {
        var index = new EpochIndex64(0);

        var found = index.TryGet(8, out var slot);

        Assert.IsFalse(found);
        Assert.AreEqual(-1, slot);
    }

    [TestMethod]
    public void Begin_RemovesMappingsAndAcceptsNewOnes()
    {
        var index = new EpochIndex64(8);
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
        var index = new EpochIndex64(0);
        for (ulong key = 0; key < 1_024; key++)
            index.GetOrAdd((key << 40) | key, (int)key, out _);

        index.EnsureCapacity(4_096);

        Assert.AreEqual(1_024, index.Count);
        for (ulong key = 0; key < 1_024; key++)
        {
            Assert.IsTrue(index.TryGet((key << 40) | key, out var slot));
            Assert.AreEqual((int)key, slot);
        }
    }

    [TestMethod]
    public void WarmedOperations_AllocateNothing()
    {
        var index = new EpochIndex64(256);
        index.GetOrAdd(0, 0, out _);
        index.Begin();
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var iteration = 0; iteration < 100; iteration++)
        {
            index.Begin();
            for (ulong key = 0; key < 128; key++)
                index.GetOrAdd(key << 32, (int)key, out _);
            for (ulong key = 0; key < 128; key++)
                index.TryGet(key << 32, out _);
        }

        Assert.AreEqual(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
