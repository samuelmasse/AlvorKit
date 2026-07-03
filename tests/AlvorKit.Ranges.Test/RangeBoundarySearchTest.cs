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
}
