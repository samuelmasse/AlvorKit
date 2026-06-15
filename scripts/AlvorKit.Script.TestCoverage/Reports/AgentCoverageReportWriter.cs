namespace AlvorKit.Script.TestCoverage;

/// <summary>Writes the machine-readable JSON coverage summary for agents.</summary>
internal static class AgentCoverageReportWriter
{
    /// <summary>Serializes a complete coverage result to JSON.</summary>
    public static void Write(
        CoverageOutputPaths output,
        DateTimeOffset started,
        DateTimeOffset generatedAt,
        CoverageOptions options,
        bool passed,
        CoverageSummary summary,
        IReadOnlyList<TestProjectResult> testResults)
    {
        var report = new
        {
            generatedAtUtc = generatedAt,
            durationSeconds = Math.Round((generatedAt - started).TotalSeconds, 3),
            threshold = options.Threshold,
            testProjectFilters = options.TestProjectFilters,
            sourceProjectFilters = options.SourceProjectFilters,
            passed,
            unmeasuredModulesFailGate = options.Threshold > 0,
            totals = summary.Totals,
            modules = summary.Modules,
            unmeasuredModules = summary.UnmeasuredModules,
            files = summary.Files,
            testProjects = testResults,
            artifacts = new
            {
                agent = "out/coverage/coverage-summary.json",
                human = "out/coverage/coverage-summary.md",
                html = options.GenerateHtmlReport ? "out/coverage/html/index.html" : null,
                reportGeneratorLog = options.GenerateHtmlReport ? "out/coverage/reportgenerator.log" : null,
                projectReports = "out/coverage/projects/<test-project>/",
                coverletFormats = options.CoverletOutputFormats(),
            },
        };

        File.WriteAllText(output.AgentReport, JsonSerializer.Serialize(report, CoverageJson.Options));
    }
}
