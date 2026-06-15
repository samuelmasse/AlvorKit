namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for generated coverage run identifiers.</summary>
[TestClass]
public sealed class CoverageRunIdentityTest
{
    /// <summary>Default run IDs include timestamp, process, and all-project filter context.</summary>
    [TestMethod]
    public void Create_NoFilters_UsesAllSlug()
    {
        var options = CoverageOptions.Parse([]);

        var runId = CoverageRunIdentity.Create(DateTimeOffset.Parse("2026-06-15T01:02:03.004Z"), options);

        StringAssert.StartsWith(runId, "20260615T010203004Z-");
        StringAssert.EndsWith(runId, "-all");
    }

    /// <summary>Source filters take precedence and multiple values are reflected in the slug.</summary>
    [TestMethod]
    public void Create_SourceFilters_UsesFirstSourceSlug()
    {
        var options = CoverageOptions.Parse(["--source-project", "scripts/One.Tool.csproj", "--source-project", "Two.Tool"]);

        var runId = CoverageRunIdentity.Create(DateTimeOffset.Parse("2026-06-15T01:02:03Z"), options);

        StringAssert.Contains(runId, "One.Tool-plus-1");
    }

    /// <summary>Binding filters are used when no source filters are present.</summary>
    [TestMethod]
    public void Create_BindingFilter_UsesBindingSlug()
    {
        var options = CoverageOptions.Parse(["--binding", "native/xxhash/conf/bindgen.json"]);

        var runId = CoverageRunIdentity.Create(DateTimeOffset.Parse("2026-06-15T01:02:03Z"), options);

        StringAssert.Contains(runId, "bindgen");
    }

    /// <summary>Test filters are used when no source or binding filters are present.</summary>
    [TestMethod]
    public void Create_TestFilter_UsesTestSlug()
    {
        var options = CoverageOptions.Parse(["--test-project", "tests/AlvorKit.Script.TestCoverage.Test"]);

        var runId = CoverageRunIdentity.Create(DateTimeOffset.Parse("2026-06-15T01:02:03Z"), options);

        StringAssert.Contains(runId, "AlvorKit.Script.TestCoverage.Test");
    }

    /// <summary>Unsafe and overlong filter text is normalized into a bounded directory segment.</summary>
    [TestMethod]
    public void Create_UnsafeLongFilter_NormalizesSlug()
    {
        var options = CoverageOptions.Parse(["--source-project", "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"]);

        var runId = CoverageRunIdentity.Create(DateTimeOffset.Parse("2026-06-15T01:02:03Z"), options);

        Assert.IsTrue(runId.Length < 90);
        StringAssert.EndsWith(runId, "-all");
    }

    /// <summary>Long safe filter text is truncated to keep path lengths manageable.</summary>
    [TestMethod]
    public void Create_LongSafeFilter_TruncatesSlug()
    {
        var options = CoverageOptions.Parse(["--source-project", "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz"]);

        var runId = CoverageRunIdentity.Create(DateTimeOffset.Parse("2026-06-15T01:02:03Z"), options);

        Assert.IsTrue(runId.EndsWith("-abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuv", StringComparison.Ordinal));
    }

    /// <summary>Digits, underscores, and hyphens are preserved in generated slugs.</summary>
    [TestMethod]
    public void Create_SafePunctuation_PreservesSlug()
    {
        var options = CoverageOptions.Parse(["--source-project", "Tool_1-Backend"]);

        var runId = CoverageRunIdentity.Create(DateTimeOffset.Parse("2026-06-15T01:02:03Z"), options);

        StringAssert.EndsWith(runId, "-Tool_1-Backend");
    }
}
