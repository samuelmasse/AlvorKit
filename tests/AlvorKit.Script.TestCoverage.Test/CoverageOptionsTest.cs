namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for coverage tool command-line parsing.</summary>
[TestClass]
public sealed class CoverageOptionsTest
{
    /// <summary>Empty arguments use the standard Debug configuration and strict threshold.</summary>
    [TestMethod]
    public void Parse_EmptyArgs_UsesDefaults()
    {
        var options = CoverageOptions.Parse([]);

        Assert.AreEqual("Debug", options.Configuration);
        Assert.AreEqual(100.0, options.Threshold);
        Assert.AreEqual(0, options.TestProjectFilters.Count);
        Assert.AreEqual(CoverageOptions.DefaultMaxParallel, options.MaxParallel);
        Assert.IsTrue(options.GenerateHtmlReport);
        Assert.IsTrue(options.GenerateCoberturaReport);
        Assert.IsTrue(options.GenerateLcovReport);
    }

    /// <summary>Short options override configuration and threshold.</summary>
    [TestMethod]
    public void Parse_ShortOptions_ReturnsValues()
    {
        var options = CoverageOptions.Parse(["-c", "Release", "-t", "87.5"]);

        Assert.AreEqual("Release", options.Configuration);
        Assert.AreEqual(87.5, options.Threshold);
    }

    /// <summary>Test project filters can be repeated for targeted coverage runs.</summary>
    [TestMethod]
    public void Parse_TestProjectFilters_ReturnsAllValues()
    {
        var options = CoverageOptions.Parse(["--test-project", "One.Test", "--project", "tests/Two.Test/Two.Test.csproj"]);

        CollectionAssert.AreEqual(new[] { "One.Test", "tests/Two.Test/Two.Test.csproj" }, options.TestProjectFilters.ToArray());
    }

    /// <summary>Max parallelism can be configured for hosts with different CPU and IO capacity.</summary>
    [TestMethod]
    public void Parse_MaxParallel_ReturnsValue()
    {
        var options = CoverageOptions.Parse(["--max-parallel", "2"]);

        Assert.AreEqual(2, options.MaxParallel);
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
