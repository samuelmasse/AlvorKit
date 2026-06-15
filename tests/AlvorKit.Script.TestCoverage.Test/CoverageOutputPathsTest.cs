namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for coverage output directory setup.</summary>
[TestClass]
public sealed class CoverageOutputPathsTest
{
    /// <summary>Creating output paths clears stale artifacts from previous coverage runs.</summary>
    [TestMethod]
    public void Create_RemovesStaleCoverageArtifacts()
    {
        using var workspace = TempWorkspace.Create();
        var stale = workspace.Write(Path.Combine("out", "coverage", "projects", "Old.Test", "coverage.json"), "{}");

        var output = CoverageOutputPaths.Create(workspace.Root);

        Assert.IsFalse(File.Exists(stale));
        Assert.IsTrue(Directory.Exists(output.Root));
        Assert.IsTrue(Directory.Exists(output.ProjectsRoot));
    }
}
