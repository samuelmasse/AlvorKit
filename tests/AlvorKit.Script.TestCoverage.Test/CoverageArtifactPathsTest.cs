namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for coverage artifact display paths.</summary>
[TestClass]
public sealed class CoverageArtifactPathsTest
{
    /// <summary>Artifact paths are repository-relative when output lives under the repository.</summary>
    [TestMethod]
    public void Create_RepositoryOutput_ReturnsRelativePaths()
    {
        using var workspace = TempWorkspace.Create();
        var output = OutputPaths(workspace.Root, Path.Combine(workspace.Root, "out", "coverage", "runs", "run"));
        var options = CoverageOptions.Parse(["--no-html", "--no-lcov"]);

        var artifacts = CoverageArtifactPaths.Create(workspace.Root, output, options);

        Assert.AreEqual("out/coverage/runs/run/coverage-summary.json", artifacts.Agent);
        Assert.AreEqual("out/coverage/runs/run/projects/<test-project>/coverage.json", artifacts.ProjectReports);
        Assert.IsNull(artifacts.Html);
        Assert.IsNotNull(artifacts.ProjectCoberturaReports);
        Assert.IsNull(artifacts.ProjectLcovReports);
    }

    /// <summary>Artifact paths remain absolute when output lives outside the repository.</summary>
    [TestMethod]
    public void Create_ExternalOutput_ReturnsAbsolutePaths()
    {
        using var workspace = TempWorkspace.Create();
        var outsideRoot = Path.Combine(Path.GetTempPath(), "alvorkit-coverage-outside");
        var output = OutputPaths(workspace.Root, outsideRoot);
        var options = CoverageOptions.Parse(["--agent"]);

        var artifacts = CoverageArtifactPaths.Create(workspace.Root, output, options);

        Assert.AreEqual(Path.Combine(outsideRoot, "coverage-summary.json"), artifacts.Agent);
        Assert.IsTrue(Path.IsPathRooted(artifacts.ProjectReports));
    }

    /// <summary>Builds output paths with a configurable run root.</summary>
    private static CoverageOutputPaths OutputPaths(string repoRoot, string root) =>
        new(
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
