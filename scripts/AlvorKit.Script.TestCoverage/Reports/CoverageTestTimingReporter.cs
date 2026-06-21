namespace AlvorKit.Script.TestCoverage;

/// <summary>Parses coverage-run TRX files and writes the per-test timing reports.</summary>
internal static class CoverageTestTimingReporter
{
    /// <summary>Writes timing reports for TRX files emitted by the coverage test run.</summary>
    /// <param name="output">Coverage output paths for the current run.</param>
    /// <param name="options">Coverage options containing the timing budget.</param>
    public static TestTimingSummary Write(CoverageOutputPaths output, CoverageOptions options)
    {
        var trxPaths = Directory.Exists(output.ProjectsRoot)
            ? Directory.GetFiles(output.ProjectsRoot, "*.trx", SearchOption.AllDirectories)
            : [];
        var results = trxPaths.Length == 0 ? [] : new TrxTimingReader().ReadFiles(trxPaths);
        var report = new TestTimingReportWriter().Write(results, options.MaxTestDuration, output.Root);

        return new(
            options.MaxTestDuration,
            options.TestTimingWarnOnly,
            trxPaths.Length > 0,
            report.TotalCount,
            report.SlowResults,
            report.MarkdownPath,
            report.CsvPath);
    }
}
