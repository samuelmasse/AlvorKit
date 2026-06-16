namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for coverage tool command-line parsing.</summary>
[TestClass]
public sealed class CoverageOptionsTest
{
    /// <summary>Empty arguments use the standard Debug configuration and repository threshold.</summary>
    [TestMethod]
    public void Parse_EmptyArgs_UsesDefaults()
    {
        var options = CoverageOptions.Parse([]);

        Assert.AreEqual("Debug", options.Configuration);
        Assert.AreEqual(95.0, options.Thresholds.Line);
        Assert.AreEqual(85.0, options.Thresholds.Branch);
        Assert.AreEqual(95.0, options.Thresholds.Method);
        Assert.AreEqual(0, options.TestProjectFilters.Count);
        Assert.AreEqual(0, options.SourceProjectFilters.Count);
        Assert.AreEqual(0, options.BindingFilters.Count);
        Assert.AreEqual(CoverageOptions.DefaultMaxParallel, options.MaxParallel);
        Assert.IsTrue(options.GenerateHtmlReport);
        Assert.IsTrue(options.GenerateCoberturaReport);
        Assert.IsTrue(options.GenerateLcovReport);
        Assert.IsNull(options.OutputRoot);
        Assert.IsNull(options.RunId);
    }

    /// <summary>Short options override configuration and all metric thresholds.</summary>
    [TestMethod]
    public void Parse_ShortOptions_ReturnsValues()
    {
        var options = CoverageOptions.Parse(["-c", "Release", "-t", "87.5"]);

        Assert.AreEqual("Release", options.Configuration);
        Assert.AreEqual(new CoverageThresholds(87.5, 87.5, 87.5), options.Thresholds);
    }

    /// <summary>Long and alias options parse through the same validation path.</summary>
    [TestMethod]
    public void Parse_LongAndAliasOptions_ReturnsValues()
    {
        var options = CoverageOptions.Parse(["--configuration", "Release", "--threshold", "50", "-m", "1", "--agent-fast"]);

        Assert.AreEqual("Release", options.Configuration);
        Assert.AreEqual(new CoverageThresholds(50.0, 50.0, 50.0), options.Thresholds);
        Assert.AreEqual(1, options.MaxParallel);
        Assert.IsFalse(options.GenerateHtmlReport);
    }

    /// <summary>Metric-specific thresholds override only their selected coverage metric.</summary>
    [TestMethod]
    public void Parse_MetricThresholdOptions_ReturnsValues()
    {
        var options = CoverageOptions.Parse(["--line-threshold", "96", "--branch-threshold", "86.5", "--method-threshold", "97"]);

        Assert.AreEqual(new CoverageThresholds(96.0, 86.5, 97.0), options.Thresholds);
    }

    /// <summary>Test project filters can be repeated for targeted coverage runs.</summary>
    [TestMethod]
    public void Parse_TestProjectFilters_ReturnsAllValues()
    {
        var options = CoverageOptions.Parse(["--test-project", "One.Test", "--project", "tests/Two.Test/Two.Test.csproj"]);

        CollectionAssert.AreEqual(new[] { "One.Test", "tests/Two.Test/Two.Test.csproj" }, options.TestProjectFilters.ToArray());
    }

    /// <summary>Source project filters can be repeated for source-scoped coverage gates.</summary>
    [TestMethod]
    public void Parse_SourceProjectFilters_ReturnsAllValues()
    {
        var options = CoverageOptions.Parse(["--source-project", "Tool", "--source", "scripts/Other/Other.csproj"]);

        CollectionAssert.AreEqual(new[] { "Tool", "scripts/Other/Other.csproj" }, options.SourceProjectFilters.ToArray());
    }

    /// <summary>Binding filters can be repeated for generated binding coverage gates.</summary>
    [TestMethod]
    public void Parse_BindingFilters_ReturnsAllValues()
    {
        var options = CoverageOptions.Parse(["--binding", "xxhash", "--binding", "native/freetype/conf/bindgen.yml"]);

        CollectionAssert.AreEqual(new[] { "xxhash", "native/freetype/conf/bindgen.yml" }, options.BindingFilters.ToArray());
    }

    /// <summary>Max parallelism can be configured for hosts with different CPU and IO capacity.</summary>
    [TestMethod]
    public void Parse_MaxParallel_ReturnsValue()
    {
        var options = CoverageOptions.Parse(["--max-parallel", "2"]);

        Assert.AreEqual(2, options.MaxParallel);
    }

    /// <summary>Output routing options select a parent directory and stable run directory name.</summary>
    [TestMethod]
    public void Parse_OutputRootAndRunId_ReturnsValues()
    {
        var options = CoverageOptions.Parse(["--output-root", "out/coverage/agents/codex", "--run-id", "focused-run"]);

        Assert.AreEqual("out/coverage/agents/codex", options.OutputRoot);
        Assert.AreEqual("focused-run", options.RunId);
    }

    /// <summary>Agent mode keeps only the artifacts needed for automated coverage decisions.</summary>
    [TestMethod]
    public void Parse_AgentMode_UsesJsonOnlyReports()
    {
        var options = CoverageOptions.Parse(["--agent"]);

        Assert.IsFalse(options.GenerateHtmlReport);
        Assert.IsFalse(options.GenerateCoberturaReport);
        Assert.IsFalse(options.GenerateLcovReport);
        CollectionAssert.AreEqual(new[] { "json" }, options.CoverletOutputFormats().ToArray());
    }

    /// <summary>Individual raw report options can be disabled without entering agent mode.</summary>
    [TestMethod]
    public void Parse_DisableReportOptions_ReturnsValues()
    {
        var options = CoverageOptions.Parse(["--no-html", "--no-lcov"]);

        Assert.IsFalse(options.GenerateHtmlReport);
        Assert.IsTrue(options.GenerateCoberturaReport);
        Assert.IsFalse(options.GenerateLcovReport);
    }

    /// <summary>Full report mode includes every existing raw report format.</summary>
    [TestMethod]
    public void CoverletOutputFormats_Defaults_ReturnsAllFormats()
    {
        var options = CoverageOptions.Parse([]);

        CollectionAssert.AreEqual(new[] { "json", "cobertura", "lcov" }, options.CoverletOutputFormats().ToArray());
    }

    /// <summary>Unknown arguments are rejected so agents do not assume unsupported behavior.</summary>
    [TestMethod]
    public void Parse_UnknownArg_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CoverageOptions.Parse(["--json"]));
    }

    /// <summary>Missing option values produce a targeted argument error.</summary>
    [TestMethod]
    public void Parse_MissingValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CoverageOptions.Parse(["--threshold"]));
    }

    /// <summary>Output roots must name a real parent directory.</summary>
    [TestMethod]
    public void Parse_EmptyOutputRoot_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CoverageOptions.Parse(["--output-root", ""]));
    }

    /// <summary>Run IDs must not be empty.</summary>
    [TestMethod]
    public void Parse_EmptyRunId_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CoverageOptions.Parse(["--run-id", ""]));
    }

    /// <summary>Run IDs cannot contain path separators.</summary>
    [TestMethod]
    public void Parse_PathSeparatorRunId_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CoverageOptions.Parse(["--run-id", "agent/run"]));
        Assert.ThrowsExactly<ArgumentException>(() => CoverageOptions.Parse(["--run-id", @"agent\run"]));
    }

    /// <summary>Run IDs cannot contain invalid filename characters.</summary>
    [TestMethod]
    public void Parse_InvalidFilenameRunId_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CoverageOptions.Parse(["--run-id", "agent:run"]));
    }

    /// <summary>Thresholds outside percentage bounds are rejected.</summary>
    [TestMethod]
    public void Parse_OutOfRangeThreshold_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CoverageOptions.Parse(["--threshold", "101"]));
    }

    /// <summary>Parallelism must be positive so the scheduler cannot deadlock.</summary>
    [TestMethod]
    public void Parse_OutOfRangeMaxParallel_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CoverageOptions.Parse(["--max-parallel", "0"]));
    }
}
