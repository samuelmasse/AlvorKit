namespace AlvorKit.Script.TestCoverage;

/// <summary>Writes all coverage report formats owned by the script.</summary>
internal static class CoverageReportWriter
{
    /// <summary>Writes agent-readable and human-readable coverage summaries.</summary>
    public static void Write(
        string repoRoot,
        CoverageOutputPaths output,
        DateTimeOffset started,
        DateTimeOffset generatedAt,
        CoverageOptions options,
        bool passed,
        CoverageSummary summary,
        IReadOnlyList<TestProjectResult> testResults)
    {
        AgentCoverageReportWriter.Write(repoRoot, output, started, generatedAt, options, passed, summary, testResults);
        MarkdownCoverageReportWriter.Write(repoRoot, output, generatedAt, options, passed, summary, testResults);
    }
}
