using System.Globalization;

namespace AlvorKit.Script.TestCoverage;

/// <summary>Writes the human-readable Markdown coverage summary.</summary>
internal static class MarkdownCoverageReportWriter
{
    /// <summary>Writes a Markdown coverage summary to a file.</summary>
    public static void Write(
        string path,
        DateTimeOffset generatedAt,
        CoverageOptions options,
        bool passed,
        CoverageSummary summary,
        IReadOnlyList<TestProjectResult> testResults)
    {
        var builder = new StringBuilder();
        WriteHeader(builder, generatedAt, options, passed);
        WriteTotals(builder, summary.Totals);
        WriteModules(builder, summary);
        WriteTestProjects(builder, testResults);
        WriteFiles(builder, summary.Files);
        WriteArtifacts(builder, options);
        File.WriteAllText(path, builder.ToString());
    }

    /// <summary>Writes report title and run metadata.</summary>
    private static void WriteHeader(StringBuilder builder, DateTimeOffset generatedAt, CoverageOptions options, bool passed)
    {
        builder.AppendLine("# Coverage Summary");
        builder.AppendLine();
        builder.AppendLine($"Generated: {generatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        builder.AppendLine($"Status: {(passed ? "PASS" : "FAIL")}");
        builder.AppendLine($"Threshold: {options.Threshold.ToString(CultureInfo.InvariantCulture)}% line, branch, and method coverage");
        builder.AppendLine($"Test project filter: {FilterText(options.TestProjectFilters)}");
        builder.AppendLine($"Source project filter: {FilterText(options.SourceProjectFilters)}");
    }

    /// <summary>Writes aggregate metric totals.</summary>
    private static void WriteTotals(StringBuilder builder, TotalCoverageSummary totals)
    {
        builder.AppendLine();
        builder.AppendLine("## Totals");
        builder.AppendLine();
        builder.AppendLine("| Metric | Covered | Total | Percent | Missing |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: |");
        WriteMetricRow(builder, "Line", totals.Line);
        WriteMetricRow(builder, "Branch", totals.Branch);
        WriteMetricRow(builder, "Method", totals.Method);
    }

    /// <summary>Writes module metric rows and unmeasured source modules.</summary>
    private static void WriteModules(StringBuilder builder, CoverageSummary summary)
    {
        builder.AppendLine();
        builder.AppendLine("## Modules");
        builder.AppendLine();
        builder.AppendLine("| Module | Line | Branch | Method |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var module in summary.Modules)
            builder.AppendLine($"| {Escape(module.Name)} | {Percent(module.Line.Percent)} | {Percent(module.Branch.Percent)} | {Percent(module.Method.Percent)} |");

        if (summary.UnmeasuredModules.Count == 0)
            return;

        builder.AppendLine();
        builder.AppendLine("## Unmeasured Source Modules");
        builder.AppendLine();

        foreach (var module in summary.UnmeasuredModules)
            builder.AppendLine($"- `{module}`");
    }

    /// <summary>Writes test project status rows.</summary>
    private static void WriteTestProjects(StringBuilder builder, IReadOnlyList<TestProjectResult> testResults)
    {
        builder.AppendLine();
        builder.AppendLine("## Test Projects");
        builder.AppendLine();
        builder.AppendLine("| Project | Status | Duration | Log |");
        builder.AppendLine("| --- | --- | ---: | --- |");

        foreach (var result in testResults)
        {
            var status = result.ExitCode == 0 ? "PASS" : $"FAIL ({result.ExitCode})";
            var duration = result.Duration.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture);
            builder.AppendLine($"| {Escape(result.Name)} | {status} | {duration}s | `{result.LogPath}` |");
        }
    }

    /// <summary>Writes the top files with missing coverage.</summary>
    private static void WriteFiles(StringBuilder builder, IReadOnlyList<FileCoverageSummary> files)
    {
        builder.AppendLine();
        builder.AppendLine("## Files Needing Coverage");
        builder.AppendLine();
        builder.AppendLine("| File | Missing Lines | Missing Branches | Missing Methods |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var file in files.Where(file => file.MissingLines > 0 || file.MissingBranches > 0 || file.MissingMethods > 0).Take(50))
            builder.AppendLine($"| `{file.Path}` | {file.MissingLines} | {file.MissingBranches} | {file.MissingMethods} |");
    }

    /// <summary>Writes the fixed artifact path reference block.</summary>
    private static void WriteArtifacts(StringBuilder builder, CoverageOptions options)
    {
        builder.AppendLine();
        builder.AppendLine("## Artifacts");
        builder.AppendLine();
        builder.AppendLine("- Agent JSON: `out/coverage/coverage-summary.json`");
        builder.AppendLine("- Human summary: `out/coverage/coverage-summary.md`");

        if (options.GenerateHtmlReport)
        {
            builder.AppendLine("- HTML summary: `out/coverage/html/index.html`");
            builder.AppendLine("- ReportGenerator log: `out/coverage/reportgenerator.log`");
        }

        builder.AppendLine("- Per-project JSON reports: `out/coverage/projects/<test-project>/coverage.json`");

        if (options.GenerateCoberturaReport || options.GenerateHtmlReport)
            builder.AppendLine("- Per-project Cobertura reports: `out/coverage/projects/<test-project>/coverage.cobertura.xml`");
        if (options.GenerateLcovReport)
            builder.AppendLine("- Per-project LCOV reports: `out/coverage/projects/<test-project>/coverage.info`");
    }

    /// <summary>Writes one metric row.</summary>
    private static void WriteMetricRow(StringBuilder builder, string name, MetricSummary metric) =>
        builder.AppendLine($"| {name} | {metric.Covered} | {metric.Total} | {Percent(metric.Percent)} | {metric.Missing} |");

    /// <summary>Formats a percentage using invariant culture.</summary>
    private static string Percent(double percent) => percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";

    /// <summary>Formats selected project filters for the report header.</summary>
    private static string FilterText(IReadOnlyList<string> filters) =>
        filters.Count == 0 ? "all" : string.Join(", ", filters.Select(filter => $"`{filter}`"));

    /// <summary>Escapes table delimiter characters in Markdown cell text.</summary>
    private static string Escape(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);
}
