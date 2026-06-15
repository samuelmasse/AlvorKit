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
        bool htmlReportRequested)
    {
        Console.WriteLine();
        Console.WriteLine($"Coverage status: {(passed ? "PASS" : "FAIL")}");
        Console.WriteLine(
            $"Line: {Percent(summary.Totals.Line.Percent)}; Branch: {Percent(summary.Totals.Branch.Percent)}; Method: {Percent(summary.Totals.Method.Percent)}");
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
}
