namespace AlvorKit.Script.TestCoverage;

/// <summary>Coordinates project discovery, test execution, coverage aggregation, and report writing.</summary>
/// <param name="options">Validated command-line options for this run.</param>
internal sealed class CoverageRunner(CoverageOptions options)
{
    /// <summary>Runs the full coverage workflow and returns an executable-style exit code.</summary>
    public async Task<int> RunAsync()
    {
        var repoRoot = RepositoryPaths.FindRoot();
        var started = DateTimeOffset.UtcNow;
        var output = CoverageOutputPaths.Create(repoRoot);
        var testProjects = ProjectDiscovery.TestProjects(repoRoot, options.TestProjectFilters);
        var sourceModules = options.TestProjectFilters.Count == 0
            ? ProjectDiscovery.SourceAssemblyNames(repoRoot)
            : ProjectReferenceDiscovery.SourceAssemblyNamesForTests(repoRoot, testProjects);

        if (testProjects.Count == 0)
            throw new InvalidOperationException("No test projects found under tests.");
        if (sourceModules.Count == 0)
            throw new InvalidOperationException("No source projects found under src or scripts.");

        var testRunner = new TestProjectRunner(repoRoot, options, sourceModules);
        var testResults = new List<TestProjectResult>();
        var coverage = new CoverageAccumulator();

        foreach (var testProject in testProjects)
        {
            var result = await testRunner.RunAsync(testProject, output.ProjectsRoot);
            testResults.Add(result);

            if (File.Exists(Path.Combine(repoRoot, result.CoverageJsonPath)))
                coverage.AddCoverletJson(Path.Combine(repoRoot, result.CoverageJsonPath), repoRoot);
        }

        var summary = coverage.BuildSummary(sourceModules);
        var passed = CoverageGate.Passes(testResults, summary, options.Threshold);

        CoverageReportWriter.Write(output, started, DateTimeOffset.UtcNow, options, passed, summary, testResults);
        var htmlReportGenerated = await ReportGeneratorRunner.RunAsync(repoRoot, output, testResults);
        ConsoleSummary.Write(repoRoot, output, passed, summary, htmlReportGenerated);

        return passed && htmlReportGenerated ? 0 : 1;
    }
}
