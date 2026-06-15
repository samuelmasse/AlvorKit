namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for console summary link formatting.</summary>
[TestClass]
public sealed class ConsoleSummaryTest
{
    /// <summary>Skipped HTML is reported distinctly from failed HTML generation.</summary>
    [TestMethod]
    public void Write_HtmlSkipped_PrintsSkippedMessage()
    {
        using var writer = new StringWriter();
        var previous = Console.Out;
        var summary = new CoverageSummary(new(new(1, 1), new(1, 1), new(1, 1)), [], [], []);
        var output = new CoverageOutputPaths("out/coverage", "projects", "agent.json", "human.md", "html", "html/index.html", "log", "latest.json", "run");

        try
        {
            Console.SetOut(writer);
            ConsoleSummary.Write(Environment.CurrentDirectory, output, true, summary, htmlReportGenerated: false, htmlReportRequested: false);
        }
        finally
        {
            Console.SetOut(previous);
        }

        StringAssert.Contains(writer.ToString(), "HTML report: skipped");
    }

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
