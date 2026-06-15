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
}
