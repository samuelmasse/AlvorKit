namespace AlvorKit.Script.TestCoverage;

/// <summary>Absolute paths for generated coverage artifacts.</summary>
/// <param name="Root">Coverage output root directory.</param>
/// <param name="ProjectsRoot">Directory containing per-test-project artifacts.</param>
/// <param name="AgentReport">Machine-readable summary path.</param>
/// <param name="HumanReport">Human-readable summary path.</param>
/// <param name="HtmlReportDirectory">ReportGenerator HTML output directory.</param>
/// <param name="HtmlIndexReport">ReportGenerator browser entry point.</param>
/// <param name="ReportGeneratorLog">Captured ReportGenerator restore and execution log.</param>
internal sealed record CoverageOutputPaths(
    string Root,
    string ProjectsRoot,
    string AgentReport,
    string HumanReport,
    string HtmlReportDirectory,
    string HtmlIndexReport,
    string ReportGeneratorLog)
{
    /// <summary>Creates the output directory shape under out/coverage.</summary>
    public static CoverageOutputPaths Create(string repoRoot)
    {
        var root = Path.Combine(repoRoot, "out", "coverage");
        var projectsRoot = Path.Combine(root, "projects");
        var htmlRoot = Path.Combine(root, "html");
        ResetDirectory(root);
        Directory.CreateDirectory(projectsRoot);

        return new(
            root,
            projectsRoot,
            Path.Combine(root, "coverage-summary.json"),
            Path.Combine(root, "coverage-summary.md"),
            htmlRoot,
            Path.Combine(htmlRoot, "index.html"),
            Path.Combine(root, "reportgenerator.log"));
    }

    /// <summary>Recreates a directory so stale artifacts from previous runs cannot survive.</summary>
    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }
}
