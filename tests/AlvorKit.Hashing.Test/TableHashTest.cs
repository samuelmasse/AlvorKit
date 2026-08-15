namespace AlvorKit;

[TestClass]
public class TableHashTest
{
    [TestMethod]
    public void Index_AllKeyShapes_StayInsideMask()
    {
        ReadOnlySpan<int> masks = [0, 1, 3, 15, 63, 255];
        foreach (var mask in masks)
        {
            AssertIndex(TableHash.Index(0, mask), mask);
            AssertIndex(TableHash.Index(-1, mask), mask);
            AssertIndex(TableHash.Index(int.MinValue, int.MaxValue, mask), mask);
            AssertIndex(TableHash.Index(long.MinValue, mask), mask);
            AssertIndex(TableHash.Index(long.MaxValue, mask), mask);
            AssertIndex(TableHash.Index(ulong.MinValue, mask), mask);
            AssertIndex(TableHash.Index(ulong.MaxValue, mask), mask);
            AssertIndex(TableHash.Index(ulong.MaxValue, int.MinValue, mask), mask);
        }
    }

    [TestMethod]
    public void Index_RepeatedCalls_AreDeterministic()
    {
        Assert.AreEqual(TableHash.Index(-17, 255), TableHash.Index(-17, 255));
        Assert.AreEqual(TableHash.Index(-17, 91, 255), TableHash.Index(-17, 91, 255));
        Assert.AreEqual(TableHash.Index(long.MinValue, 255), TableHash.Index(long.MinValue, 255));
        Assert.AreEqual(TableHash.Index(ulong.MaxValue, 255), TableHash.Index(ulong.MaxValue, 255));
        Assert.AreEqual(TableHash.Index(ulong.MaxValue, -91, 255), TableHash.Index(ulong.MaxValue, -91, 255));
    }

    [TestMethod]
    public void Index_SequentialIntKeys_UseEveryBucketOnce()
    {
        const int bucketCount = 256;
        Span<bool> occupied = stackalloc bool[bucketCount];
        for (var key = 0; key < bucketCount; key++)
            occupied[TableHash.Index(key, bucketCount - 1)] = true;

        foreach (var present in occupied)
            Assert.IsTrue(present);
    }

    [TestMethod]
    public void Index_SequentialUlongKeys_DistributeAcrossMostBuckets()
    {
        const int bucketCount = 8_192;
        Span<bool> occupied = stackalloc bool[bucketCount];
        for (ulong key = 0; key < 4_096; key++)
            occupied[TableHash.Index(key, bucketCount - 1)] = true;

        var used = 0;
        foreach (var present in occupied)
            used += present ? 1 : 0;

        Assert.IsTrue(used > 3_000, $"Expected broad bucket use but observed {used} buckets.");
    }

    [TestMethod]
    public void Index_WarmedCalls_AllocateNothing()
    {
        TableHash.Index(1, 255);
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var key = 0; key < 10_000; key++)
        {
            TableHash.Index(key, 255);
            TableHash.Index(key, -key, 255);
            TableHash.Index((long)key << 32, 255);
            TableHash.Index((ulong)(uint)key << 32, 255);
            TableHash.Index((ulong)(uint)key << 32, -key, 255);
        }

        Assert.AreEqual(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    private static void AssertIndex(int index, int mask)
    {
        Assert.IsTrue(index >= 0);
        Assert.IsTrue(index <= mask);
    }
}
