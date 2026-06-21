namespace AlvorKit.Script.TestCoverage;

/// <summary>Coordinates project discovery, test execution, coverage aggregation, and report writing.</summary>
/// <param name="options">Validated command-line options for this run.</param>
[ExcludeFromCodeCoverage(Justification = "Coordinates external dotnet and report-generation processes; collaborators are covered by unit tests.")]
internal sealed class CoverageRunner(CoverageOptions options)
{
    /// <summary>Runs the full coverage workflow and returns an executable-style exit code.</summary>
    public async Task<int> RunAsync()
    {
        var repoRoot = RepositoryPaths.FindRoot();
        var started = DateTimeOffset.UtcNow;
        var output = CoverageOutputPaths.Create(repoRoot, options, started);
        var selection = CoverageSelection.FromOptions(repoRoot, options);

        var testRunner = new TestProjectRunner(repoRoot, options, selection.SourceModules);
        var noBuildTests = ShouldPrebuild(selection.TestProjects);
        IReadOnlyList<TestProjectExecution> buildResults = noBuildTests
            ? await BuildTestProjectsAsync(testRunner, selection.TestProjects, output.ProjectsRoot)
            : [];
        var failedBuilds = buildResults.Where(execution => execution.Result.ExitCode != 0).ToArray();
        var coverage = new CoverageAccumulator();

        if (failedBuilds.Length > 0)
        {
            var failedResults = failedBuilds.Select(execution => execution.Result).ToArray();
            return await CompleteAsync(repoRoot, output, started, coverage, selection.SourceModules, failedResults);
        }

        var testExecutions = await RunTestProjectsAsync(testRunner, selection.TestProjects, output.ProjectsRoot, noBuildTests);
        var testResults = testExecutions.Select(execution => execution.Result).ToArray();
        var testTiming = CoverageTestTimingReporter.Write(output, options);

        foreach (var result in testResults)
        {
            if (File.Exists(Path.Combine(repoRoot, result.CoverageJsonPath)))
                coverage.AddCoverletJson(Path.Combine(repoRoot, result.CoverageJsonPath), repoRoot);
        }

        return await CompleteAsync(repoRoot, output, started, coverage, selection.SourceModules, testResults, testTiming);
    }

    /// <summary>Returns true when a separate build lets parallel tests avoid shared file-copy work.</summary>
    private bool ShouldPrebuild(IReadOnlyCollection<string> testProjects) =>
        options.MaxParallel > 1 && testProjects.Count > 1;

    /// <summary>Builds selected test projects sequentially before parallel no-build test execution.</summary>
    private static async Task<IReadOnlyList<TestProjectExecution>> BuildTestProjectsAsync(
        TestProjectRunner testRunner,
        IReadOnlyList<string> testProjects,
        string projectsRoot)
    {
        var results = new List<TestProjectExecution>();

        foreach (var testProject in testProjects)
        {
            var execution = await testRunner.BuildAsync(testProject, projectsRoot);
            Console.Write(execution.Output);
            results.Add(execution);
        }

        return results;
    }

    /// <summary>Runs selected test projects with bounded concurrency and returns results in discovery order.</summary>
    private async Task<IReadOnlyList<TestProjectExecution>> RunTestProjectsAsync(
        TestProjectRunner testRunner,
        IReadOnlyList<string> testProjects,
        string projectsRoot,
        bool noBuild)
    {
        using var semaphore = new SemaphoreSlim(options.MaxParallel);
        var tasks = testProjects
            .Select((testProject, index) => RunIndexedAsync(testRunner, testProject, projectsRoot, noBuild, semaphore, index))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        return
        [
            .. results
                .OrderBy(result => result.Index)
                .Select(result =>
                {
                    Console.Write(result.Execution.Output);
                    return result.Execution;
                })
        ];
    }

    /// <summary>Runs one test project after acquiring a parallelism slot.</summary>
    private static async Task<(int Index, TestProjectExecution Execution)> RunIndexedAsync(
        TestProjectRunner testRunner,
        string testProject,
        string projectsRoot,
        bool noBuild,
        SemaphoreSlim semaphore,
        int index)
    {
        await semaphore.WaitAsync();

        try
        {
            return (index, await testRunner.RunAsync(testProject, projectsRoot, noBuild));
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>Writes final reports, optionally generates HTML, and returns the process exit code.</summary>
    private async Task<int> CompleteAsync(
        string repoRoot,
        CoverageOutputPaths output,
        DateTimeOffset started,
        CoverageAccumulator coverage,
        IReadOnlyCollection<string> sourceModules,
        IReadOnlyList<TestProjectResult> testResults,
        TestTimingSummary? testTiming = null)
    {
        var summary = coverage.BuildSummary(sourceModules);
        var passed = CoverageGate.Passes(testResults, summary, options.Thresholds, testTiming);

        var generatedAt = DateTimeOffset.UtcNow;
        CoverageReportWriter.Write(repoRoot, output, started, generatedAt, options, passed, summary, testResults, testTiming);
        var htmlReportGenerated = false;

        if (options.GenerateHtmlReport)
            htmlReportGenerated = await ReportGeneratorRunner.RunAsync(repoRoot, output, testResults);

        LatestCoverageRunWriter.Write(repoRoot, output, generatedAt, options, passed);
        ConsoleSummary.Write(repoRoot, output, passed, summary, htmlReportGenerated, options.GenerateHtmlReport, testTiming);

        return passed && (!options.GenerateHtmlReport || htmlReportGenerated) ? 0 : 1;
    }

}
