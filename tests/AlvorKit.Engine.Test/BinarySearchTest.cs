namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class BinarySearchTest
{
    /// <summary>Range search returns the inclusive span of matching sorted values.</summary>
    [TestMethod]
    public void FindRange_ReturnsInclusiveMatchingRange()
    {
        var values = new[] { 1, 3, 5, 7, 9 };

        Assert.AreEqual((1, 3, 3), BinarySearch.FindRange(values, 2, 7));
        Assert.AreEqual((-1, -1, 0), BinarySearch.FindRange(values, 10, 12));
        Assert.AreEqual((-1, -1, 0), BinarySearch.FindRange(Array.Empty<int>(), 1, 2));
    }

    /// <summary>Boundary searches return nearest sorted positions or -1 when no value matches.</summary>
    [TestMethod]
    public void BoundarySearches_ReturnExpectedIndexes()
    {
        var values = new[] { 1, 3, 5, 7, 9 };

        Assert.AreEqual(2, BinarySearch.FirstGreaterOrEqual(values, 4));
        Assert.AreEqual(2, BinarySearch.LastLessOrEqual(values, 5));
        Assert.AreEqual(3, BinarySearch.SmallestStrictlyLarger(values, 5));
        Assert.AreEqual(1, BinarySearch.LargestStrictlySmaller(values, 5));
        Assert.AreEqual(-1, BinarySearch.FirstGreaterOrEqual(values, 10));
        Assert.AreEqual(-1, BinarySearch.LastLessOrEqual(values, 0));
        Assert.AreEqual(-1, BinarySearch.SmallestStrictlyLarger(values, 9));
        Assert.AreEqual(-1, BinarySearch.LargestStrictlySmaller(values, 1));
    }
}
