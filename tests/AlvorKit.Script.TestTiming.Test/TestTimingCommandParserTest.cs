namespace AlvorKit.Script.TestTiming.Test;

/// <summary>Tests command-line parsing for the unit test timing guard.</summary>
[TestClass]
public sealed class TestTimingCommandParserTest
{
    /// <summary>Portable absolute repository root fixture used by parser tests.</summary>
    private static readonly string RepoRoot = Path.Combine(Path.GetTempPath(), "alvorkit-testtiming-repo");

    /// <summary>Portable absolute alternate repository root fixture used by parser tests.</summary>
    private static readonly string OtherRepoRoot = Path.Combine(Path.GetTempPath(), "alvorkit-testtiming-other-repo");

    /// <summary>Creates reusable repository roots used by parse-only tests.</summary>
    [TestInitialize]
    public void Initialize()
    {
        Directory.CreateDirectory(RepoRoot);
        Directory.CreateDirectory(OtherRepoRoot);
        File.WriteAllText(Path.Combine(RepoRoot, "AlvorKit.slnx"), "<Solution />");
        File.WriteAllText(Path.Combine(OtherRepoRoot, "Rombadil.slnx"), "<Solution />");
        File.WriteAllText(Path.Combine(OtherRepoRoot, "Rombadil.Dev.slnx"), "<Solution />");
    }

    /// <summary>Parse-only calls leave generated help to the command-line app.</summary>
    [TestMethod]
    public void Parse_Help_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestTimingCommandParser().Parse(["--help"], RepoRoot));
    }

    /// <summary>No-argument parse-only calls use the default test target.</summary>
    [TestMethod]
    public void Parse_NoArgs_ReturnsDefaultTarget()
    {
        var options = new TestTimingCommandParser().Parse([], RepoRoot);

        CollectionAssert.AreEqual(TestTimingOptions.DefaultDotNetTestArguments(RepoRoot).ToArray(), options.DotNetTestArguments.ToArray());
    }

    /// <summary>Timing options parse before forwarded dotnet test arguments.</summary>
    [TestMethod]
    public void Parse_WithOptions_ReturnsTimingValues()
    {
        var options = new TestTimingCommandParser().Parse(
            [
                "--max-duration-ms",
                "25.5",
                "--warn-only",
                "--results-directory",
                "out/timing",
                "AlvorKit.slnx",
                "--no-build"
            ],
            RepoRoot);

        Assert.AreEqual(TimeSpan.FromMilliseconds(25.5), options.MaxDuration);
        Assert.IsTrue(options.WarnOnly);
        Assert.AreEqual(Path.GetFullPath("out/timing", RepoRoot), options.ResultsDirectory);
        CollectionAssert.AreEqual(new[] { "AlvorKit.slnx", "--no-build" }, options.DotNetTestArguments.ToArray());
    }

    /// <summary>Repository roots and separator-delimited dotnet arguments are parsed explicitly.</summary>
    [TestMethod]
    public void Parse_RepoRootAndSeparator_ReturnsForwardedArguments()
    {
        var options = new TestTimingCommandParser().Parse(["--repo-root", OtherRepoRoot, "--", "--filter", "Fast"], RepoRoot);

        Assert.AreEqual(Path.GetFullPath(OtherRepoRoot), options.RepoRoot);
        CollectionAssert.AreEqual(new[] { "Rombadil.slnx", "--filter", "Fast" }, options.DotNetTestArguments.ToArray());
    }

    /// <summary>Forwarded dotnet test options receive the repository solution when no target is supplied.</summary>
    [TestMethod]
    public void Parse_DotNetOptionsOnly_PrependsSolution()
    {
        var options = new TestTimingCommandParser().Parse(["--no-build", "--filter", "Fast"], RepoRoot);

        CollectionAssert.AreEqual(new[] { "AlvorKit.slnx", "--no-build", "--filter", "Fast" }, options.DotNetTestArguments.ToArray());
    }

    /// <summary>TRX parsing mode resolves the input path relative to the repository root.</summary>
    [TestMethod]
    public void Parse_Trx_ReturnsParseOnlyPath()
    {
        var options = new TestTimingCommandParser().Parse(["--trx", "out/run.trx"], RepoRoot);

        Assert.AreEqual(Path.GetFullPath("out/run.trx", RepoRoot), options.TrxPath);
    }

    /// <summary>Non-positive timing budgets are rejected.</summary>
    [TestMethod]
    public void Parse_NonPositiveMaxDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestTimingCommandParser().Parse(["--max-duration-ms", "0"], RepoRoot));
    }

    /// <summary>Missing option values are rejected with a targeted parse error.</summary>
    [TestMethod]
    public void Parse_MissingValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestTimingCommandParser().Parse(["--trx"], RepoRoot));
    }

    /// <summary>Non-numeric timing budgets are rejected.</summary>
    [TestMethod]
    public void Parse_NonNumericMaxDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestTimingCommandParser().Parse(["--max-duration-ms", "fast"], RepoRoot));
    }
}
