namespace AlvorKit.Script.TestCoverage;

/// <summary>Writes the machine-readable JSON coverage summary for agents.</summary>
internal static class AgentCoverageReportWriter
{
    /// <summary>Serializes a complete coverage result to JSON.</summary>
    public static void Write(
        string repoRoot,
        CoverageOutputPaths output,
        DateTimeOffset started,
        DateTimeOffset generatedAt,
        CoverageOptions options,
        bool passed,
        CoverageSummary summary,
        IReadOnlyList<TestProjectResult> testResults,
        TestTimingSummary? testTiming = null)
    {
        var artifacts = CoverageArtifactPaths.Create(repoRoot, output, options);
        var report = new
        {
            generatedAtUtc = generatedAt,
            durationSeconds = Math.Round((generatedAt - started).TotalSeconds, 3),
            runId = output.RunId,
            thresholds = options.Thresholds,
            testProjectFilters = options.TestProjectFilters,
            sourceProjectFilters = options.SourceProjectFilters,
            bindingFilters = options.BindingFilters,
            passed,
            unmeasuredModulesFailGate = !options.Thresholds.IsDisabled,
            totals = summary.Totals,
            modules = summary.Modules,
            unmeasuredModules = summary.UnmeasuredModules,
            files = summary.Files,
            testProjects = testResults,
            testTiming = TestTimingJson(repoRoot, testTiming),
            artifacts = new
            {
                agent = artifacts.Agent,
                human = artifacts.Human,
                html = artifacts.Html,
                reportGeneratorLog = artifacts.ReportGeneratorLog,
                projectReports = artifacts.ProjectReports,
                projectCoberturaReports = artifacts.ProjectCoberturaReports,
                projectLcovReports = artifacts.ProjectLcovReports,
                latestRun = artifacts.LatestRun,
                coverletFormats = options.CoverletOutputFormats(),
            },
        };

        File.WriteAllText(output.AgentReport, JsonSerializer.Serialize(report, CoverageJson.Options));
    }

    /// <summary>Builds the agent-readable test timing section.</summary>
    private static object? TestTimingJson(string repoRoot, TestTimingSummary? testTiming)
    {
        if (testTiming is null)
            return null;

        return new
        {
            complete = testTiming.IsComplete,
            warnOnly = testTiming.WarnOnly,
            maxDurationSeconds = Math.Round(testTiming.MaxDuration.TotalSeconds, 3),
            totalCount = testTiming.TotalCount,
            slowCount = testTiming.SlowResults.Count,
            markdown = RepositoryPaths.Relative(repoRoot, testTiming.MarkdownPath),
            csv = RepositoryPaths.Relative(repoRoot, testTiming.CsvPath),
            slowTests = testTiming.SlowResults.Select(result => new
            {
                durationSeconds = Math.Round(result.Duration.TotalSeconds, 3),
                result.Outcome,
                result.TestName,
                sourcePath = RepositoryPaths.Relative(repoRoot, result.SourcePath),
            }),
        };
    }
}
