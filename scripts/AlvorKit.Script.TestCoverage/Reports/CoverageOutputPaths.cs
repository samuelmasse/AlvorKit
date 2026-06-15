namespace AlvorKit.Script.TestCoverage;

/// <summary>Absolute paths for generated coverage artifacts.</summary>
/// <param name="Root">Coverage output root directory.</param>
/// <param name="ProjectsRoot">Directory containing per-test-project artifacts.</param>
/// <param name="AgentReport">Machine-readable summary path.</param>
/// <param name="HumanReport">Human-readable summary path.</param>
/// <param name="HtmlReportDirectory">ReportGenerator HTML output directory.</param>
/// <param name="HtmlIndexReport">ReportGenerator browser entry point.</param>
/// <param name="ReportGeneratorLog">Captured ReportGenerator restore and execution log.</param>
/// <param name="LatestRunManifest">Non-authoritative pointer to the most recent run.</param>
/// <param name="RunId">Directory name identifying this coverage run.</param>
internal sealed record CoverageOutputPaths(
    string Root,
    string ProjectsRoot,
    string AgentReport,
    string HumanReport,
    string HtmlReportDirectory,
    string HtmlIndexReport,
    string ReportGeneratorLog,
    string LatestRunManifest,
    string RunId)
{
    /// <summary>Creates the output directory shape for one isolated coverage run.</summary>
    public static CoverageOutputPaths Create(string repoRoot, CoverageOptions options, DateTimeOffset started)
    {
        var runId = options.RunId ?? CoverageRunIdentity.Create(started, options);
        var coverageRoot = Path.Combine(repoRoot, "out", "coverage");
        var outputRoot = ResolveOutputRoot(repoRoot, options.OutputRoot);
        var root = Path.Combine(outputRoot, "runs", runId);
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
            Path.Combine(root, "reportgenerator.log"),
            Path.Combine(coverageRoot, "latest-run.json"),
            runId);
    }

    /// <summary>Resolves a user-provided output parent or the default coverage directory.</summary>
    private static string ResolveOutputRoot(string repoRoot, string? outputRoot) =>
        outputRoot is null
            ? Path.Combine(repoRoot, "out", "coverage")
            : Path.GetFullPath(outputRoot, repoRoot);

    /// <summary>Recreates a directory so stale artifacts from previous runs cannot survive.</summary>
    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }
}
