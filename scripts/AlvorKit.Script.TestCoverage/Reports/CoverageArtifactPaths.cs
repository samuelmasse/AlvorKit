namespace AlvorKit.Script.TestCoverage;

/// <summary>Repository-display paths for artifacts produced by one coverage run.</summary>
internal sealed record CoverageArtifactPaths(
    string Agent,
    string Human,
    string? Html,
    string? ReportGeneratorLog,
    string ProjectReports,
    string? ProjectCoberturaReports,
    string? ProjectLcovReports,
    string LatestRun)
{
    /// <summary>Builds artifact paths that match the actual output directories.</summary>
    public static CoverageArtifactPaths Create(string repoRoot, CoverageOutputPaths output, CoverageOptions options)
    {
        var projectsRoot = DisplayPath(repoRoot, output.ProjectsRoot).TrimEnd('/');

        return new(
            DisplayPath(repoRoot, output.AgentReport),
            DisplayPath(repoRoot, output.HumanReport),
            options.GenerateHtmlReport ? DisplayPath(repoRoot, output.HtmlIndexReport) : null,
            options.GenerateHtmlReport ? DisplayPath(repoRoot, output.ReportGeneratorLog) : null,
            $"{projectsRoot}/<test-project>/coverage.json",
            options.GenerateCoberturaReport || options.GenerateHtmlReport ? $"{projectsRoot}/<test-project>/coverage.cobertura.xml" : null,
            options.GenerateLcovReport ? $"{projectsRoot}/<test-project>/coverage.info" : null,
            DisplayPath(repoRoot, output.LatestRunManifest));
    }

    /// <summary>Returns a slash-separated repository-relative path when possible, otherwise an absolute path.</summary>
    private static string DisplayPath(string repoRoot, string path)
    {
        var relative = Path.GetRelativePath(repoRoot, Path.GetFullPath(path));

        return relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative)
            ? Path.GetFullPath(path)
            : relative.Replace('\\', '/');
    }
}
