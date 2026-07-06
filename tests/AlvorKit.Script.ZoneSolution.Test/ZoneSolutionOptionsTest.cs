namespace AlvorKit.Script.ZoneSolution.Test;

/// <summary>Tests command-line option parsing for the AlvorZone solution generator.</summary>
[TestClass]
public sealed class ZoneSolutionOptionsTest
{
    /// <summary>Uses the inferred zone root and derives the generated output path.</summary>
    [TestMethod]
    public void ParseUsesDefaults()
    {
        var zoneRoot = Path.Combine(Path.GetTempPath(), "AlvorZone");

        var options = ZoneSolutionOptions.Parse([], () => zoneRoot);

        Assert.AreEqual(Path.GetFullPath(zoneRoot), options.ZoneRoot);
        Assert.AreEqual(Path.GetFullPath(Path.Combine(zoneRoot, "AlvorZone.slnx")), options.OutputPath);
        Assert.AreEqual(0, options.RepoNames.Count);
        Assert.IsFalse(options.ListOnly);
    }

    /// <summary>Parses explicit paths, repo filters, and preview mode.</summary>
    [TestMethod]
    public void ParseUsesExplicitOptions()
    {
        var options = ZoneSolutionOptions.Parse(
            [
                "--zone-root",
                "Repos",
                "--output",
                "Repos/All.slnx",
                "--repo",
                "Craftdig",
                "--repo",
                "Rombadil",
                "--list-only"
            ],
            () => "unused-zone");

        Assert.AreEqual(Path.GetFullPath("Repos"), options.ZoneRoot);
        Assert.AreEqual(Path.GetFullPath("Repos/All.slnx"), options.OutputPath);
        CollectionAssert.AreEqual(new[] { "Craftdig", "Rombadil" }, options.RepoNames.ToArray());
        Assert.IsTrue(options.ListOnly);
    }

    /// <summary>Rejects path-shaped repository filters.</summary>
    [TestMethod]
    public void ParseRejectsPathShapedRepoFilter()
    {
        Assert.ThrowsException<ArgumentException>(() => ZoneSolutionOptions.Parse(["--repo", "../Craftdig"], () => "Repos"));
    }

    /// <summary>Generated help is handled by the command tree rather than parsed generator options.</summary>
    [TestMethod]
    public void ParseRejectsHelpAsExecutionOptions()
    {
        Assert.ThrowsException<ArgumentException>(() => ZoneSolutionOptions.Parse(["--help"], () => "Repos"));
    }
}
