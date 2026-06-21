namespace AlvorKit.Script.TestCoverage;

/// <summary>Writes a compact console summary after report generation.</summary>
internal static class ConsoleSummary
{
    /// <summary>Prints status, aggregate percentages, and report paths.</summary>
    public static void Write(
        string repoRoot,
        CoverageOutputPaths output,
        bool passed,
        CoverageSummary summary,
        bool htmlReportGenerated,
        bool htmlReportRequested,
        TestTimingSummary? testTiming = null)
    {
        Console.WriteLine();
        Console.WriteLine($"Coverage status: {(passed ? "PASS" : "FAIL")}");
        Console.WriteLine(
            $"Line: {Percent(summary.Totals.Line.Percent)}; Branch: {Percent(summary.Totals.Branch.Percent)}; Method: {Percent(summary.Totals.Method.Percent)}");
        WriteTestTiming(repoRoot, testTiming);
        Console.WriteLine($"Agent report: {RepositoryPaths.Relative(repoRoot, output.AgentReport)}");
        Console.WriteLine($"Human report: {RepositoryPaths.Relative(repoRoot, output.HumanReport)}");

        if (!htmlReportRequested)
        {
            Console.WriteLine("HTML report: skipped");
            return;
        }

        Console.WriteLine(
            htmlReportGenerated
                ? $"HTML report: {ClickableFileUri(output.HtmlIndexReport)}"
                : $"HTML report generation failed; see {ClickableFileUri(output.ReportGeneratorLog)}");
    }

    /// <summary>Formats an absolute file URI that VS Code terminals can open as a clickable link.</summary>
    public static string ClickableFileUri(string path) =>
        new Uri(Path.GetFullPath(path)).AbsoluteUri;

    /// <summary>Formats a percentage using invariant culture.</summary>
    private static string Percent(double percent) => percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";

    /// <summary>Prints per-test timing warnings collected from TRX output.</summary>
    private static void WriteTestTiming(string repoRoot, TestTimingSummary? testTiming)
    {
        if (testTiming is null)
            return;

        Console.WriteLine(
            $"Test timing: {testTiming.TotalCount} tests, {testTiming.SlowResults.Count} over {Seconds(testTiming.MaxDuration)}s.");
        if (!testTiming.IsComplete)
            Console.WriteLine("WARNING AVKTESTTIMING: no TRX timing data was found.");

        foreach (var result in testTiming.SlowResults.Take(10))
            Console.WriteLine($"WARNING AVKTESTTIMING: {Seconds(result.Duration)}s {result.TestName}");

        if (testTiming.SlowResults.Count > 10)
            Console.WriteLine($"WARNING AVKTESTTIMING: {testTiming.SlowResults.Count - 10} more slow tests in the timing report.");

        Console.WriteLine($"Timing report: {RepositoryPaths.Relative(repoRoot, testTiming.MarkdownPath)}");
        Console.WriteLine($"Timing CSV: {RepositoryPaths.Relative(repoRoot, testTiming.CsvPath)}");
    }

    /// <summary>Formats a duration in seconds using invariant culture.</summary>
    private static string Seconds(TimeSpan duration) => duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
}
