namespace AlvorKit.Script.TestCoverage;

/// <summary>Applies pass/fail rules to test execution and coverage summaries.</summary>
internal static class CoverageGate
{
    /// <summary>Returns true when tests pass, coverage meets thresholds, and measured modules are complete.</summary>
    public static bool Passes(
        IReadOnlyList<TestProjectResult> testResults,
        CoverageSummary summary,
        CoverageThresholds thresholds) =>
        testResults.All(result => result.ExitCode == 0)
        && summary.MeetsThreshold(thresholds)
        && (thresholds.IsDisabled || summary.UnmeasuredModules.Count == 0);
}
