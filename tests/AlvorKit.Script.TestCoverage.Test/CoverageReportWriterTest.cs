using System.Text.Json;

namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for writing all coverage summary formats together.</summary>
[TestClass]
public sealed class CoverageReportWriterTest
{
    /// <summary>The combined writer creates matching JSON and Markdown summaries.</summary>
    [TestMethod]
    public void Write_CreatesAgentAndHumanReports()
    {
        using var workspace = TempWorkspace.Create();
        var output = OutputPaths(workspace.Root);
        Directory.CreateDirectory(output.Root);
        var options = CoverageOptions.Parse(["--agent"]);
        var summary = new CoverageSummary(new(new(1, 1), new(1, 1), new(1, 1)), [], [], []);

        CoverageReportWriter.Write(
            workspace.Root,
            output,
            DateTimeOffset.Parse("2026-06-15T00:00:00Z"),
            DateTimeOffset.Parse("2026-06-15T00:00:01Z"),
            options,
            passed: true,
            summary,
            []);

        using var document = JsonDocument.Parse(File.ReadAllText(output.AgentReport));
        var markdown = File.ReadAllText(output.HumanReport);

        Assert.IsTrue(document.RootElement.GetProperty("passed").GetBoolean());
        StringAssert.Contains(markdown, "Status: PASS");
    }

    /// <summary>Builds output paths for the combined report writer.</summary>
    private static CoverageOutputPaths OutputPaths(string repoRoot)
    {
        var root = Path.Combine(repoRoot, "out", "coverage", "runs", "run");
        return new(
            root,
            Path.Combine(root, "projects"),
            Path.Combine(root, "coverage-summary.json"),
            Path.Combine(root, "coverage-summary.md"),
            Path.Combine(root, "html"),
            Path.Combine(root, "html", "index.html"),
            Path.Combine(root, "reportgenerator.log"),
            Path.Combine(repoRoot, "out", "coverage", "latest-run.json"),
            "run");
    }
}
