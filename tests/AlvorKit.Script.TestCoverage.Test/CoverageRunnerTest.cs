namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Integration tests for the coverage workflow coordinator.</summary>
[TestClass]
public sealed class CoverageRunnerTest
{
    /// <summary>The runner writes isolated reports through the normal coverage workflow.</summary>
    [TestMethod]
    public async Task RunAsync_FocusedWorkspaceRun_WritesRunScopedReports()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.PathFor("coverage-output");
        var options = CoverageOptions.Parse(
        [
            "--agent",
            "--threshold",
            "0",
            "--max-parallel",
            "1",
            "--source-project",
            "AlvorKit.Script.Workspace",
            "--test-project",
            "AlvorKit.Script.Workspace.Test",
            "--output-root",
            outputRoot,
            "--run-id",
            "nested",
        ]);

        var exitCode = await new CoverageRunner(options).RunAsync();
        var runRoot = Path.Combine(outputRoot, "runs", "nested");

        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "coverage-summary.json")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "coverage-summary.md")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "projects", "AlvorKit.Script.Workspace.Test", "coverage.json")));
    }

    /// <summary>Multiple selected test projects use the prebuild path and can still generate HTML reports.</summary>
    [TestMethod]
    public async Task RunAsync_MultipleProjectsWithHtml_WritesHtmlReport()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.PathFor("coverage-output");
        var options = CoverageOptions.Parse(
        [
            "--threshold",
            "0",
            "--no-lcov",
            "--test-project",
            "AlvorKit.Script.AgentLease.Test",
            "--test-project",
            "AlvorKit.Script.Workspace.Test",
            "--output-root",
            outputRoot,
            "--run-id",
            "prebuild-html",
        ]);

        var exitCode = await new CoverageRunner(options).RunAsync();
        var runRoot = Path.Combine(outputRoot, "runs", "prebuild-html");

        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "html", "index.html")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "projects", "AlvorKit.Script.AgentLease.Test", "dotnet-build.log")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "projects", "AlvorKit.Script.Workspace.Test", "dotnet-build.log")));
    }

    /// <summary>A prebuild failure stops before test execution but still writes diagnostic reports.</summary>
    [TestMethod]
    public async Task RunAsync_FailedPrebuild_WritesFailureReport()
    {
        using var workspace = TempWorkspace.Create();
        var repoRoot = RepositoryPaths.FindRoot();
        var badProjectRoot = Path.Combine(
            repoRoot,
            "tests",
            ".coverage-runner-temp",
            "CoverageRunnerFailure-" + Guid.NewGuid().ToString("N"));
        var badProject = Path.Combine(badProjectRoot, "BrokenCoverageRunner.Test.csproj");
        var outputRoot = workspace.PathFor("coverage-output");

        try
        {
            Directory.CreateDirectory(badProjectRoot);
            File.WriteAllText(badProject, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(badProjectRoot, "Broken.cs"), "public class Broken {");

            var options = CoverageOptions.Parse(
            [
                "--agent",
                "--threshold",
                "0",
                "--test-project",
                RepositoryPaths.Relative(repoRoot, badProject),
                "--test-project",
                "AlvorKit.Script.Workspace.Test",
                "--output-root",
                outputRoot,
                "--run-id",
                "failed-prebuild",
            ]);

            var exitCode = await new CoverageRunner(options).RunAsync();
            var runRoot = Path.Combine(outputRoot, "runs", "failed-prebuild");

            Assert.AreEqual(1, exitCode);
            Assert.IsTrue(File.Exists(Path.Combine(runRoot, "coverage-summary.json")));
            Assert.IsFalse(File.Exists(Path.Combine(runRoot, "projects", "BrokenCoverageRunner.Test", "dotnet-test.log")));
        }
        finally
        {
            if (Directory.Exists(badProjectRoot))
                Directory.Delete(badProjectRoot, recursive: true);
        }
    }
}
