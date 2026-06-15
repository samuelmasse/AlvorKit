using System.Text.Json;

namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for the machine-readable agent coverage report.</summary>
[TestClass]
public sealed class AgentCoverageReportWriterTest
{
    /// <summary>The JSON report includes test and source filters for focused-run auditing.</summary>
    [TestMethod]
    public void Write_IncludesFocusedCoverageFilters()
    {
        using var workspace = TempWorkspace.Create();
        var output = OutputPaths(workspace.Root);
        Directory.CreateDirectory(output.Root);
        var options = new CoverageOptions("Debug", 100, ["Tool.Test"], ["Tool"], 1, false, false, false);
        var summary = new CoverageSummary(new(new(1, 1), new(1, 1), new(1, 1)), [], [], []);

        AgentCoverageReportWriter.Write(
            output,
            DateTimeOffset.Parse("2026-06-15T00:00:00Z"),
            DateTimeOffset.Parse("2026-06-15T00:00:01Z"),
            options,
            passed: true,
            summary,
            []);

        using var document = JsonDocument.Parse(File.ReadAllText(output.AgentReport));
        var root = document.RootElement;

        Assert.AreEqual("Tool.Test", root.GetProperty("testProjectFilters")[0].GetString());
        Assert.AreEqual("Tool", root.GetProperty("sourceProjectFilters")[0].GetString());
        Assert.AreEqual("out/coverage/coverage-summary.json", root.GetProperty("artifacts").GetProperty("agent").GetString());
    }

    /// <summary>The JSON report includes browser artifact paths when HTML generation is enabled.</summary>
    [TestMethod]
    public void Write_WithHtmlReport_IncludesHtmlArtifacts()
    {
        using var workspace = TempWorkspace.Create();
        var output = OutputPaths(workspace.Root);
        Directory.CreateDirectory(output.Root);
        var options = new CoverageOptions("Debug", 100, [], [], 1, true, true, true);
        var summary = new CoverageSummary(new(new(1, 1), new(1, 1), new(1, 1)), [], [], []);

        AgentCoverageReportWriter.Write(
            output,
            DateTimeOffset.Parse("2026-06-15T00:00:00Z"),
            DateTimeOffset.Parse("2026-06-15T00:00:01Z"),
            options,
            passed: true,
            summary,
            []);

        using var document = JsonDocument.Parse(File.ReadAllText(output.AgentReport));
        var artifacts = document.RootElement.GetProperty("artifacts");

        Assert.AreEqual("out/coverage/html/index.html", artifacts.GetProperty("html").GetString());
        Assert.AreEqual("out/coverage/reportgenerator.log", artifacts.GetProperty("reportGeneratorLog").GetString());
    }

    /// <summary>Builds output paths inside a temporary workspace.</summary>
    private static CoverageOutputPaths OutputPaths(string root)
    {
        var coverageRoot = Path.Combine(root, "coverage");
        return new(
            coverageRoot,
            Path.Combine(coverageRoot, "projects"),
            Path.Combine(coverageRoot, "coverage-summary.json"),
            Path.Combine(coverageRoot, "coverage-summary.md"),
            Path.Combine(coverageRoot, "html"),
            Path.Combine(coverageRoot, "html", "index.html"),
            Path.Combine(coverageRoot, "reportgenerator.log"));
    }
}
