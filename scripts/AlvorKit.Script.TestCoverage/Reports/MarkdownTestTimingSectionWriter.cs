namespace AlvorKit.Script.TestCoverage;

/// <summary>Writes per-test timing budget results into the coverage Markdown report.</summary>
internal static class MarkdownTestTimingSectionWriter
{
    /// <summary>Appends a test timing section when timing data is available for the report.</summary>
    /// <param name="builder">Markdown builder receiving the section.</param>
    /// <param name="repoRoot">Repository root used for relative artifact paths.</param>
    /// <param name="testTiming">Optional timing summary from the coverage run.</param>
    public static void Write(StringBuilder builder, string repoRoot, TestTimingSummary? testTiming)
    {
        if (testTiming is null)
            return;

        builder.AppendLine();
        builder.AppendLine("## Test Timing");
        builder.AppendLine();
        builder.AppendLine($"- Threshold: {Seconds(testTiming.MaxDuration)}s");
        builder.AppendLine($"- Test cases parsed: {testTiming.TotalCount}");
        builder.AppendLine($"- Tests over threshold: {testTiming.SlowResults.Count}");
        builder.AppendLine($"- Timing report: `{RepositoryPaths.Relative(repoRoot, testTiming.MarkdownPath)}`");
        builder.AppendLine($"- Timing CSV: `{RepositoryPaths.Relative(repoRoot, testTiming.CsvPath)}`");

        if (!testTiming.IsComplete)
        {
            builder.AppendLine();
            builder.AppendLine("No TRX timing data was found for this run.");
            return;
        }

        if (testTiming.SlowResults.Count > 0)
            WriteSlowTests(builder, testTiming);
    }

    /// <summary>Writes the slow-test timing table.</summary>
    private static void WriteSlowTests(StringBuilder builder, TestTimingSummary testTiming)
    {
        builder.AppendLine();
        builder.AppendLine("| Duration | Outcome | Test |");
        builder.AppendLine("| ---: | --- | --- |");
        foreach (var result in testTiming.SlowResults.Take(25))
            builder.AppendLine($"| {Seconds(result.Duration)}s | {Escape(result.Outcome)} | {Escape(result.TestName)} |");
    }

    /// <summary>Formats a duration in seconds using invariant culture.</summary>
    private static string Seconds(TimeSpan duration) => duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);

    /// <summary>Escapes table delimiter characters in Markdown cell text.</summary>
    private static string Escape(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);
}
