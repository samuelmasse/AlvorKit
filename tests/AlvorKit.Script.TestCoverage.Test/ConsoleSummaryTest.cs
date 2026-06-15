namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for console summary link formatting.</summary>
[TestClass]
public sealed class ConsoleSummaryTest
{
    /// <summary>HTML report paths are printed as absolute file URIs so VS Code terminals can linkify them.</summary>
    [TestMethod]
    public void ClickableFileUri_RelativePath_ReturnsAbsoluteFileUri()
    {
        var path = Path.Combine("out", "coverage", "html", "index.html");

        var uri = ConsoleSummary.ClickableFileUri(path);
        var parsed = new Uri(uri);

        Assert.AreEqual(Uri.UriSchemeFile, parsed.Scheme);
        Assert.IsTrue(parsed.IsAbsoluteUri);
        Assert.AreEqual(Path.GetFullPath(path), parsed.LocalPath);
    }
}
