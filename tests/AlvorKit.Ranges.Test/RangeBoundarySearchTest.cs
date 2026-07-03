namespace AlvorKit.Ranges.Test;

[TestClass]
public sealed class RangeBoundarySearchTest
{
    /// <summary>Boundary searches report no match for empty lists.</summary>
    [TestMethod]
    public void Searches_WithEmptyList_ReturnNoMatch()
    {
        var values = Array.Empty<int>();

        Assert.AreEqual(-1, RangeBoundarySearch.FirstGreaterOrEqual(values, 1));
        Assert.AreEqual(-1, RangeBoundarySearch.SmallestStrictlyLarger(values, 1));
        Assert.AreEqual(-1, RangeBoundarySearch.LargestStrictlySmaller(values, 1));
    }

    /// <summary>Boundary searches return expected sorted-list positions.</summary>
    [TestMethod]
    public void Searches_WithValues_ReturnBoundaryIndexes()
    {
        var values = new[] { 1, 3, 5, 7 };

        Assert.AreEqual(1, RangeBoundarySearch.FirstGreaterOrEqual(values, 2));
        Assert.AreEqual(-1, RangeBoundarySearch.FirstGreaterOrEqual(values, 8));
        Assert.AreEqual(2, RangeBoundarySearch.SmallestStrictlyLarger(values, 3));
        Assert.AreEqual(-1, RangeBoundarySearch.SmallestStrictlyLarger(values, 7));
        Assert.AreEqual(1, RangeBoundarySearch.LargestStrictlySmaller(values, 5));
        Assert.AreEqual(-1, RangeBoundarySearch.LargestStrictlySmaller(values, 1));
    }

    /// <summary>Long-specialized searches return expected sorted-list positions.</summary>
    [TestMethod]
    public void LongSearches_WithValues_ReturnBoundaryIndexes()
    {
        var values = new List<long> { 1, 3, 5, 7 };

        Assert.AreEqual(1, RangeBoundarySearch.FirstGreaterOrEqual(values, 2));
        Assert.AreEqual(-1, RangeBoundarySearch.FirstGreaterOrEqual(values, 8));
        Assert.AreEqual(2, RangeBoundarySearch.SmallestStrictlyLarger(values, 3));
        Assert.AreEqual(-1, RangeBoundarySearch.SmallestStrictlyLarger(values, 7));
        Assert.AreEqual(1, RangeBoundarySearch.LargestStrictlySmaller(values, 5));
        Assert.AreEqual(-1, RangeBoundarySearch.LargestStrictlySmaller(values, 1));
    }

    /// <summary>Span searches return expected positions without requiring a list wrapper.</summary>
    [TestMethod]
    public void SpanSearch_WithValues_ReturnsBoundaryIndexes()
    {
        ReadOnlySpan<long> values = [1, 3, 5, 7];
        ReadOnlySpan<long> empty = [];

        Assert.AreEqual(-1, RangeBoundarySearch.FirstGreaterOrEqual(empty, 1));
        Assert.AreEqual(1, RangeBoundarySearch.FirstGreaterOrEqual(values, 2));
        Assert.AreEqual(-1, RangeBoundarySearch.FirstGreaterOrEqual(values, 8));
    }
}
