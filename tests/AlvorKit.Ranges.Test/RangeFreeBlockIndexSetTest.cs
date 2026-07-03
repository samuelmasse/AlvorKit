namespace AlvorKit.Ranges.Test;

[TestClass]
public sealed class RangeFreeBlockIndexSetTest
{
    /// <summary>Adding indexes keeps the bucket sorted and grows storage without changing the minimum index.</summary>
    [TestMethod]
    public void Add_KeepsIndexesSortedAndGrowsStorage()
    {
        var indexes = new RangeFreeBlockIndexSet();

        indexes.Add(30);
        indexes.Add(10);
        indexes.Add(20);
        indexes.Add(50);
        indexes.Add(40);

        Assert.AreEqual(5, indexes.Count);
        Assert.AreEqual(10, indexes.Min);
        Assert.IsTrue(indexes.TryGetFirstFit(4, 1, 0, out var index, out var padding));
        Assert.AreEqual(10, index);
        Assert.AreEqual(0, padding);
    }

    /// <summary>Removing an index preserves sorted lookup and missing removals fail clearly.</summary>
    [TestMethod]
    public void Remove_PreservesSortedLookupAndRejectsMissingIndex()
    {
        var indexes = new RangeFreeBlockIndexSet();
        indexes.Add(30);
        indexes.Add(10);
        indexes.Add(20);

        indexes.Remove(10);

        Assert.AreEqual(2, indexes.Count);
        Assert.AreEqual(20, indexes.Min);
        Assert.ThrowsExactly<InvalidOperationException>(() => indexes.Remove(10));
    }

    /// <summary>Removing the only index clears the bucket.</summary>
    [TestMethod]
    public void Remove_WithSingleIndex_ClearsBucket()
    {
        var indexes = new RangeFreeBlockIndexSet();
        indexes.Add(10);

        indexes.Remove(10);

        Assert.AreEqual(0, indexes.Count);
    }

    /// <summary>Removing a middle index shifts later indexes into sorted position.</summary>
    [TestMethod]
    public void Remove_WithMiddleIndex_ShiftsLaterIndexes()
    {
        var indexes = new RangeFreeBlockIndexSet();
        indexes.Add(10);
        indexes.Add(20);
        indexes.Add(30);

        indexes.Remove(20);

        Assert.AreEqual(2, indexes.Count);
        Assert.AreEqual(10, indexes.Min);
        indexes.Remove(10);
        Assert.AreEqual(30, indexes.Min);
    }

    /// <summary>Removing from an empty bucket fails clearly.</summary>
    [TestMethod]
    public void Remove_WhenEmpty_Throws()
    {
        var indexes = new RangeFreeBlockIndexSet();

        Assert.ThrowsExactly<InvalidOperationException>(() => indexes.Remove(1));
    }

    /// <summary>Fit lookup skips indexes whose alignment padding would exceed the block size.</summary>
    [TestMethod]
    public void TryGetFirstFit_WithAlignment_ReturnsFirstIndexThatActuallyFits()
    {
        var indexes = new RangeFreeBlockIndexSet();
        indexes.Add(1);
        indexes.Add(8);

        var found = indexes.TryGetFirstFit(8, 8, 8, out var index, out var padding);

        Assert.IsTrue(found);
        Assert.AreEqual(8, index);
        Assert.AreEqual(0, padding);
    }

    /// <summary>Fit lookup reports no match when every indexed block needs too much alignment padding.</summary>
    [TestMethod]
    public void TryGetFirstFit_WhenNoIndexFits_ReturnsFalse()
    {
        var indexes = new RangeFreeBlockIndexSet();
        indexes.Add(1);

        var found = indexes.TryGetFirstFit(8, 8, 8, out var index, out var padding);

        Assert.IsFalse(found);
        Assert.AreEqual(0, index);
        Assert.AreEqual(0, padding);
    }

    /// <summary>Fit lookup reports no match when every index in a multi-index bucket needs too much padding.</summary>
    [TestMethod]
    public void TryGetFirstFit_WhenNoMultiIndexFits_ReturnsFalse()
    {
        var indexes = new RangeFreeBlockIndexSet();
        indexes.Add(1);
        indexes.Add(9);

        var found = indexes.TryGetFirstFit(8, 8, 8, out var index, out var padding);

        Assert.IsFalse(found);
        Assert.AreEqual(0, index);
        Assert.AreEqual(0, padding);
    }

    /// <summary>Clearing a bucket drops the logical count while keeping the bucket reusable.</summary>
    [TestMethod]
    public void Clear_DropsLogicalCount()
    {
        var indexes = new RangeFreeBlockIndexSet();
        indexes.Add(1);

        indexes.Clear();

        Assert.AreEqual(0, indexes.Count);
    }
}
