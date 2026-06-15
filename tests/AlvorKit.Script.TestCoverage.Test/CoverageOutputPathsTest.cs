namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for coverage output directory setup.</summary>
[TestClass]
public sealed class CoverageOutputPathsTest
{
    /// <summary>Creating output paths clears stale artifacts only inside the selected run directory.</summary>
    [TestMethod]
    public void Create_RunScopedOutput_PreservesOtherRunArtifacts()
    {
        using var workspace = TempWorkspace.Create();
        var stale = workspace.Write(Path.Combine("out", "coverage", "runs", "other-run", "coverage-summary.json"), "{}");
        var current = workspace.Write(Path.Combine("out", "coverage", "runs", "current-run", "projects", "Old.Test", "coverage.json"), "{}");
        var options = CoverageOptions.Parse(["--run-id", "current-run"]);

        var output = CoverageOutputPaths.Create(workspace.Root, options, DateTimeOffset.Parse("2026-06-15T00:00:00Z"));

        Assert.IsFalse(File.Exists(current));
        Assert.IsTrue(File.Exists(stale));
        Assert.IsTrue(Directory.Exists(output.Root));
        Assert.IsTrue(Directory.Exists(output.ProjectsRoot));
        Assert.AreEqual(workspace.PathFor(Path.Combine("out", "coverage", "runs", "current-run")), output.Root);
        Assert.AreEqual(workspace.PathFor(Path.Combine("out", "coverage", "runs", "current-run", "html")), output.HtmlReportDirectory);
        Assert.AreEqual(workspace.PathFor(Path.Combine("out", "coverage", "latest-run.json")), output.LatestRunManifest);
    }

    /// <summary>Custom output roots still get isolated run subdirectories.</summary>
    [TestMethod]
    public void Create_CustomOutputRoot_UsesRunDirectoryBelowRoot()
    {
        using var workspace = TempWorkspace.Create();
        var options = CoverageOptions.Parse(["--output-root", "out/coverage/agents/codex", "--run-id", "focused"]);

        var output = CoverageOutputPaths.Create(workspace.Root, options, DateTimeOffset.Parse("2026-06-15T00:00:00Z"));

        Assert.AreEqual(workspace.PathFor(Path.Combine("out", "coverage", "agents", "codex", "runs", "focused")), output.Root);
    }

    /// <summary>Default run IDs include enough context to avoid collisions during concurrent runs.</summary>
    [TestMethod]
    public void Create_DefaultRunId_UsesTimestampProcessAndFilter()
    {
        using var workspace = TempWorkspace.Create();
        var options = CoverageOptions.Parse(["--source-project", "AlvorKit.Script.TestCoverage"]);

        var output = CoverageOutputPaths.Create(workspace.Root, options, DateTimeOffset.Parse("2026-06-15T01:02:03.004Z"));

        StringAssert.StartsWith(output.RunId, "20260615T010203004Z-");
        StringAssert.Contains(output.RunId, "AlvorKit.Script.TestCoverage");
    }
}
