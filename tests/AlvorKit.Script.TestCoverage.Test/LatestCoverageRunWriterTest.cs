using System.Text.Json;

namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for the latest coverage run manifest.</summary>
[TestClass]
public sealed class LatestCoverageRunWriterTest
{
    /// <summary>The manifest points to the immutable artifacts for the completed run.</summary>
    [TestMethod]
    public void Write_WritesLatestRunManifest()
    {
        using var workspace = TempWorkspace.Create();
        var output = OutputPaths(workspace.Root);
        var options = CoverageOptions.Parse(["--agent"]);

        LatestCoverageRunWriter.Write(workspace.Root, output, DateTimeOffset.Parse("2026-06-15T00:00:00Z"), options, passed: true);

        using var document = JsonDocument.Parse(File.ReadAllText(output.LatestRunManifest));
        var root = document.RootElement;

        Assert.AreEqual("run", root.GetProperty("runId").GetString());
        Assert.IsTrue(root.GetProperty("passed").GetBoolean());
        Assert.AreEqual("out/coverage/runs/run/coverage-summary.json", root.GetProperty("artifacts").GetProperty("agent").GetString());
    }

    /// <summary>Repeated writes replace the pointer without disturbing the run output directory.</summary>
    [TestMethod]
    public void Write_ExistingManifest_ReplacesPointer()
    {
        using var workspace = TempWorkspace.Create();
        var output = OutputPaths(workspace.Root);
        var options = CoverageOptions.Parse(["--agent"]);

        LatestCoverageRunWriter.Write(workspace.Root, output, DateTimeOffset.Parse("2026-06-15T00:00:00Z"), options, passed: true);
        LatestCoverageRunWriter.Write(workspace.Root, output, DateTimeOffset.Parse("2026-06-15T00:00:01Z"), options, passed: false);

        using var document = JsonDocument.Parse(File.ReadAllText(output.LatestRunManifest));

        Assert.IsFalse(document.RootElement.GetProperty("passed").GetBoolean());
    }

    /// <summary>Builds output paths for the latest-run writer.</summary>
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
