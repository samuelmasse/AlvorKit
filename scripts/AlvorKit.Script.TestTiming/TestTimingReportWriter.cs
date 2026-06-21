namespace AlvorKit.Script.TestTiming;

/// <summary>Writes human-readable and machine-readable unit test timing reports.</summary>
internal sealed class TestTimingReportWriter
{
    /// <summary>Maximum number of slowest tests included in the Markdown summary.</summary>
    private const int SlowestMarkdownCount = 25;

    /// <summary>Writes timing reports for a parsed test run.</summary>
    /// <param name="results">Parsed test case timings.</param>
    /// <param name="maxDuration">Maximum allowed duration for one test case.</param>
    /// <param name="outputDirectory">Directory that receives report files.</param>
    public TestTimingReport Write(IReadOnlyList<TestTimingResult> results, TimeSpan maxDuration, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var ordered = results
            .OrderByDescending(result => result.Duration)
            .ThenBy(result => result.TestName, StringComparer.Ordinal)
            .ToArray();
        var slow = ordered.Where(result => result.Duration > maxDuration).ToArray();
        var markdownPath = Path.Combine(outputDirectory, "slowest-tests.md");
        var csvPath = Path.Combine(outputDirectory, "slowest-tests.csv");

        File.WriteAllText(markdownPath, Markdown(ordered, slow, maxDuration));
        File.WriteAllText(csvPath, Csv(ordered));
        return new TestTimingReport(ordered.Length, slow, markdownPath, csvPath);
    }

    /// <summary>Builds the Markdown timing report body.</summary>
    private static string Markdown(IReadOnlyList<TestTimingResult> ordered, IReadOnlyList<TestTimingResult> slow, TimeSpan maxDuration)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Unit Test Timing Report");
        builder.AppendLine();
        builder.AppendLine($"- Test cases parsed: {ordered.Count}");
        builder.AppendLine($"- Slow-test threshold: {Seconds(maxDuration)}s");
        builder.AppendLine($"- Tests over threshold: {slow.Count}");
        builder.AppendLine();

        AppendTable(builder, "Slow Tests", slow);
        AppendTable(builder, "Slowest Tests", ordered.Take(SlowestMarkdownCount).ToArray());
        return builder.ToString();
    }

    /// <summary>Appends a Markdown table section for test timing results.</summary>
    private static void AppendTable(StringBuilder builder, string title, IReadOnlyList<TestTimingResult> results)
    {
        builder.AppendLine($"## {title}");
        builder.AppendLine();
        if (results.Count == 0)
        {
            builder.AppendLine("None.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Duration | Outcome | Test |");
        builder.AppendLine("| ---: | --- | --- |");
        foreach (var result in results)
            builder.AppendLine($"| {Seconds(result.Duration)}s | {EscapeMarkdown(result.Outcome)} | {EscapeMarkdown(result.TestName)} |");
        builder.AppendLine();
    }

    /// <summary>Builds a CSV body with all timing results.</summary>
    private static string Csv(IReadOnlyList<TestTimingResult> ordered)
    {
        var builder = new StringBuilder();
        builder.AppendLine("durationSeconds,outcome,testName,sourcePath");
        foreach (var result in ordered)
        {
            builder
                .Append(Seconds(result.Duration)).Append(',')
                .Append(EscapeCsv(result.Outcome)).Append(',')
                .Append(EscapeCsv(result.TestName)).Append(',')
                .AppendLine(EscapeCsv(result.SourcePath));
        }

        return builder.ToString();
    }

    /// <summary>Formats a duration in seconds for reports.</summary>
    private static string Seconds(TimeSpan duration) =>
        duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);

    /// <summary>Escapes table separators for Markdown cells.</summary>
    private static string EscapeMarkdown(string value) =>
        value.Replace("|", "\\|", StringComparison.Ordinal);

    /// <summary>Escapes one CSV field.</summary>
    private static string EscapeCsv(string value) =>
        '"' + value.Replace("\"", "\"\"", StringComparison.Ordinal) + '"';
}
