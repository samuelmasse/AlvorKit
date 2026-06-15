namespace AlvorKit.Script.TestCoverage;

/// <summary>Applies pass/fail rules to test execution and coverage summaries.</summary>
internal static class CoverageGate
{
    /// <summary>Returns true when tests pass, coverage meets the threshold, and measured modules are complete.</summary>
    public static bool Passes(
        IReadOnlyList<TestProjectResult> testResults,
        CoverageSummary summary,
        double threshold) =>
        testResults.All(result => result.ExitCode == 0)
        && summary.MeetsThreshold(threshold)
        && (threshold <= 0 || summary.UnmeasuredModules.Count == 0);
}
