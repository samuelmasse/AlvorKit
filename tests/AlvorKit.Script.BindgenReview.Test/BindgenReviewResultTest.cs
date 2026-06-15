namespace AlvorKit.Script.BindgenReview.Test;

/// <summary>Tests small bindgen review result helpers.</summary>
[TestClass]
public sealed class BindgenReviewResultTest
{
    /// <summary>Prepend accepts an empty line set and preserves existing output.</summary>
    [TestMethod]
    public void Prepend_EmptyLines_PreservesExistingLines()
    {
        var result = BindgenReviewResult.Success("tail").Prepend();

        CollectionAssert.AreEqual(new[] { "tail" }, result.Lines.ToArray());
    }
}
