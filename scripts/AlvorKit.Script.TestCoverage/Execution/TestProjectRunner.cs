namespace AlvorKit.Script.TestCoverage;

/// <summary>Runs one test project with Coverlet instrumentation enabled.</summary>
/// <param name="repoRoot">Repository root used as the process working directory.</param>
/// <param name="options">Validated command-line options for dotnet test.</param>
/// <param name="sourceModules">Source assembly names included in coverage measurement.</param>
internal sealed class TestProjectRunner(string repoRoot, CoverageOptions options, IReadOnlyList<string> sourceModules)
{
    /// <summary>Builds one test project before a later no-build coverage run.</summary>
    public async Task<TestProjectExecution> BuildAsync(string testProject, string projectsRoot)
    {
        var projectName = Path.GetFileNameWithoutExtension(testProject);
        var projectCoverageRoot = PrepareProjectDirectory(projectsRoot, projectName);
        var logPath = Path.Combine(projectCoverageRoot, "dotnet-build.log");
        var started = DateTimeOffset.UtcNow;
        var result = await DotNetProcess.RunAsync(repoRoot, BuildBuildArguments(testProject));

        await File.WriteAllTextAsync(logPath, result.Output);

        return new(
            Result(projectName, testProject, result.ExitCode, DateTimeOffset.UtcNow - started, logPath, projectCoverageRoot),
            result.Output);
    }

    /// <summary>Runs dotnet test and records logs plus raw coverage artifact paths.</summary>
    public async Task<TestProjectExecution> RunAsync(string testProject, string projectsRoot, bool noBuild)
    {
        var projectName = Path.GetFileNameWithoutExtension(testProject);
        var projectCoverageRoot = noBuild
            ? Path.Combine(projectsRoot, projectName)
            : PrepareProjectDirectory(projectsRoot, projectName);

        var outputPrefix = Path.Combine(projectCoverageRoot, "coverage");
        var logPath = Path.Combine(projectCoverageRoot, "dotnet-test.log");
        var started = DateTimeOffset.UtcNow;
        var result = await DotNetProcess.RunAsync(repoRoot, BuildTestArguments(testProject, outputPrefix, noBuild));

        await File.WriteAllTextAsync(logPath, result.Output);

        return new(
            Result(projectName, testProject, result.ExitCode, DateTimeOffset.UtcNow - started, logPath, projectCoverageRoot),
            result.Output);
    }

    /// <summary>Builds the dotnet build command-line for one test project.</summary>
    private IReadOnlyList<string> BuildBuildArguments(string testProject) =>
    [
        "build",
        testProject,
        "--configuration",
        options.Configuration,
        "--verbosity",
        "minimal",
    ];

    /// <summary>Builds the dotnet test command-line for one project.</summary>
    private IReadOnlyList<string> BuildTestArguments(string testProject, string outputPrefix, bool noBuild)
    {
        var arguments = new List<string>
        {
            "test",
            testProject,
            "--configuration",
            options.Configuration,
            "--verbosity",
            "minimal",
        };

        if (noBuild)
        {
            arguments.Add("--no-build");
            arguments.Add("--no-restore");
        }

        arguments.AddRange(
        [
            "/p:CollectCoverage=true",
            $"/p:CoverletOutput={outputPrefix}",
            $"/p:CoverletOutputFormat={string.Join("%2c", options.CoverletOutputFormats())}",
            $"/p:Include={string.Join("%2c", sourceModules.Select(name => $"[{name}]*"))}",
            "/p:Exclude=[*.Test]*",
        ]);

        return arguments;
    }

    /// <summary>Builds a stable result object using the standard per-project artifact paths.</summary>
    private TestProjectResult Result(
        string projectName,
        string testProject,
        int exitCode,
        TimeSpan duration,
        string logPath,
        string projectCoverageRoot)
    {
        var outputPrefix = Path.Combine(projectCoverageRoot, "coverage");

        return new(
            projectName,
            RepositoryPaths.Relative(repoRoot, testProject),
            exitCode,
            duration,
            RepositoryPaths.Relative(repoRoot, logPath),
            RepositoryPaths.Relative(repoRoot, outputPrefix + ".json"),
            RepositoryPaths.Relative(repoRoot, outputPrefix + ".cobertura.xml"),
            RepositoryPaths.Relative(repoRoot, outputPrefix + ".info"));
    }

    /// <summary>Recreates and returns one project's coverage directory.</summary>
    private static string PrepareProjectDirectory(string projectsRoot, string projectName)
    {
        var projectCoverageRoot = Path.Combine(projectsRoot, projectName);
        ResetDirectory(projectCoverageRoot);
        return projectCoverageRoot;
    }

    /// <summary>Recreates a directory so stale coverage files cannot affect a later run.</summary>
    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }
}
