namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for the human-readable Markdown coverage report.</summary>
[TestClass]
public sealed class MarkdownCoverageReportWriterTest
{
    /// <summary>The report includes totals, project failures, unmeasured modules, and artifact paths.</summary>
    [TestMethod]
    public void Write_Report_IncludesHumanReadableSections()
    {
        using var workspace = TempWorkspace.Create();
        var path = workspace.PathFor("coverage-summary.md");
        var summary = new CoverageSummary(
            new(new(1, 2), new(0, 1), new(1, 1)),
            [new("Tool", new(1, 2), new(0, 1), new(1, 1))],
            ["MissingTool"],
            [new("scripts/Tool/Source.cs", 1, 1, 0, [11], [10], [])]);
        var results = new[]
        {
            new TestProjectResult("Tool.Test", "tests/Tool.Test/Tool.Test.csproj", 1, TimeSpan.FromSeconds(1.25), "log", "json", "xml", "info")
        };

        MarkdownCoverageReportWriter.Write(path, DateTimeOffset.Parse("2026-06-15T00:00:00Z"), new("Debug", 100, ["Tool.Test"]), false, summary, results);
        var markdown = File.ReadAllText(path);

        StringAssert.Contains(markdown, "## Totals");
        StringAssert.Contains(markdown, "Test project filter: `Tool.Test`");
        StringAssert.Contains(markdown, "MissingTool");
        StringAssert.Contains(markdown, "FAIL (1)");
        StringAssert.Contains(markdown, "out/coverage/coverage-summary.json");
        StringAssert.Contains(markdown, "out/coverage/html/index.html");
    }
}
