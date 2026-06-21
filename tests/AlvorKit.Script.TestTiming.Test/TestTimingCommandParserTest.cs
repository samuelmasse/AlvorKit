namespace AlvorKit.Script.TestTiming.Test;

/// <summary>Tests command-line parsing for the unit test timing guard.</summary>
[TestClass]
public sealed class TestTimingCommandParserTest
{
    /// <summary>Help requests do not require a test target.</summary>
    [TestMethod]
    public void Parse_Help_ReturnsHelpOptions()
    {
        var options = new TestTimingCommandParser().Parse(["--help"], "C:/repo");

        Assert.IsTrue(options.IsHelp);
    }

    /// <summary>The public parser can return help without a discovered test target.</summary>
    [TestMethod]
    public void Parse_PublicNoArgs_ReturnsHelpOptions()
    {
        var options = new TestTimingCommandParser().Parse([]);

        Assert.IsTrue(options.IsHelp);
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
            "C:/repo");

        Assert.AreEqual(TimeSpan.FromMilliseconds(25.5), options.MaxDuration);
        Assert.IsTrue(options.WarnOnly);
        Assert.AreEqual(Path.GetFullPath("out/timing", "C:/repo"), options.ResultsDirectory);
        CollectionAssert.AreEqual(new[] { "AlvorKit.slnx", "--no-build" }, options.DotNetTestArguments.ToArray());
    }

    /// <summary>Repository roots and separator-delimited dotnet arguments are parsed explicitly.</summary>
    [TestMethod]
    public void Parse_RepoRootAndSeparator_ReturnsForwardedArguments()
    {
        var options = new TestTimingCommandParser().Parse(["--repo-root", "C:/other", "--", "--filter", "Fast"], "C:/repo");

        Assert.AreEqual(Path.GetFullPath("C:/other"), options.RepoRoot);
        CollectionAssert.AreEqual(new[] { "AlvorKit.slnx", "--filter", "Fast" }, options.DotNetTestArguments.ToArray());
    }

    /// <summary>Forwarded dotnet test options receive the repository solution when no target is supplied.</summary>
    [TestMethod]
    public void Parse_DotNetOptionsOnly_PrependsSolution()
    {
        var options = new TestTimingCommandParser().Parse(["--no-build", "--filter", "Fast"], "C:/repo");

        CollectionAssert.AreEqual(new[] { "AlvorKit.slnx", "--no-build", "--filter", "Fast" }, options.DotNetTestArguments.ToArray());
    }

    /// <summary>TRX parsing mode resolves the input path relative to the repository root.</summary>
    [TestMethod]
    public void Parse_Trx_ReturnsParseOnlyPath()
    {
        var options = new TestTimingCommandParser().Parse(["--trx", "out/run.trx"], "C:/repo");

        Assert.AreEqual(Path.GetFullPath("out/run.trx", "C:/repo"), options.TrxPath);
    }

    /// <summary>Non-positive timing budgets are rejected.</summary>
    [TestMethod]
    public void Parse_NonPositiveMaxDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestTimingCommandParser().Parse(["--max-duration-ms", "0"], "C:/repo"));
    }

    /// <summary>Missing option values are rejected with a targeted parse error.</summary>
    [TestMethod]
    public void Parse_MissingValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestTimingCommandParser().Parse(["--trx"], "C:/repo"));
    }

    /// <summary>Non-numeric timing budgets are rejected.</summary>
    [TestMethod]
    public void Parse_NonNumericMaxDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TestTimingCommandParser().Parse(["--max-duration-ms", "fast"], "C:/repo"));
    }
}
