namespace AlvorKit.Script.TestCoverage;

/// <summary>Command-line options controlling coverage collection.</summary>
/// <param name="Configuration">Build configuration passed to dotnet test.</param>
/// <param name="Thresholds">Required percentages for line, branch, and method coverage.</param>
/// <param name="TestProjectFilters">Optional test project names or paths to run.</param>
/// <param name="SourceProjectFilters">Optional source project names or paths to measure.</param>
/// <param name="BindingFilters">Optional native library binding names to measure under out/bindgen.</param>
/// <param name="MaxParallel">Maximum number of coverage-enabled test projects to run concurrently.</param>
/// <param name="GenerateHtmlReport">Whether to generate the browser-readable ReportGenerator output.</param>
/// <param name="GenerateCoberturaReport">Whether to emit raw Cobertura XML reports.</param>
/// <param name="GenerateLcovReport">Whether to emit raw LCOV reports.</param>
/// <param name="OutputRoot">Optional parent directory for run-scoped coverage output.</param>
/// <param name="RunId">Optional directory name for this coverage run.</param>
/// <param name="MaxTestDurationMilliseconds">Maximum allowed duration for one test case.</param>
/// <param name="TestTimingWarnOnly">Whether slow tests should warn without failing coverage.</param>
/// <param name="RepoRoot">Optional repository root to measure instead of the script repository.</param>
internal sealed record CoverageOptions(
    string Configuration,
    CoverageThresholds Thresholds,
    IReadOnlyList<string> TestProjectFilters,
    IReadOnlyList<string> SourceProjectFilters,
    IReadOnlyList<string> BindingFilters,
    int MaxParallel,
    bool GenerateHtmlReport,
    bool GenerateCoberturaReport,
    bool GenerateLcovReport,
    string? OutputRoot = null,
    string? RunId = null,
    double MaxTestDurationMilliseconds = 1000,
    bool TestTimingWarnOnly = false,
    string? RepoRoot = null)
{
    /// <summary>Default bounded parallelism that avoids overwhelming local build and test infrastructure.</summary>
    public static int DefaultMaxParallel => Math.Min(4, Math.Max(1, Environment.ProcessorCount));

    /// <summary>Maximum allowed duration for one test case.</summary>
    public TimeSpan MaxTestDuration => TimeSpan.FromMilliseconds(MaxTestDurationMilliseconds);

    /// <summary>Returns the Coverlet output formats required by the selected report mode.</summary>
    public IReadOnlyList<string> CoverletOutputFormats()
    {
        var formats = new List<string> { "json" };

        if (GenerateHtmlReport || GenerateCoberturaReport)
            formats.Add("cobertura");
        if (GenerateLcovReport)
            formats.Add("lcov");

        return formats;
    }

    /// <summary>Parses command-line arguments into validated coverage options.</summary>
    public static CoverageOptions Parse(IReadOnlyList<string> args) =>
        new CoverageOptionsParser().Parse(args);
}
