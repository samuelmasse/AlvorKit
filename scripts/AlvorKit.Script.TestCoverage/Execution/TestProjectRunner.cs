namespace AlvorKit.Script.TestCoverage;

/// <summary>Runs one test project with Coverlet instrumentation enabled.</summary>
/// <param name="repoRoot">Repository root used as the process working directory.</param>
/// <param name="options">Validated command-line options for dotnet test.</param>
/// <param name="sourceModules">Source assembly names included in coverage measurement.</param>
internal sealed class TestProjectRunner(string repoRoot, CoverageOptions options, IReadOnlyList<string> sourceModules)
{
    /// <summary>Runs dotnet test and records logs plus raw coverage artifact paths.</summary>
    public async Task<TestProjectResult> RunAsync(string testProject, string projectsRoot)
    {
        var projectName = Path.GetFileNameWithoutExtension(testProject);
        var projectCoverageRoot = Path.Combine(projectsRoot, projectName);
        ResetDirectory(projectCoverageRoot);

        var outputPrefix = Path.Combine(projectCoverageRoot, "coverage");
        var logPath = Path.Combine(projectCoverageRoot, "dotnet-test.log");
        var started = DateTimeOffset.UtcNow;
        var result = await DotNetProcess.RunAsync(repoRoot, BuildArguments(testProject, outputPrefix));

        await File.WriteAllTextAsync(logPath, result.Output);
        Console.Write(result.Output);

        return new(
            projectName,
            RepositoryPaths.Relative(repoRoot, testProject),
            result.ExitCode,
            DateTimeOffset.UtcNow - started,
            RepositoryPaths.Relative(repoRoot, logPath),
            RepositoryPaths.Relative(repoRoot, outputPrefix + ".json"),
            RepositoryPaths.Relative(repoRoot, outputPrefix + ".cobertura.xml"),
            RepositoryPaths.Relative(repoRoot, outputPrefix + ".info"));
    }

    /// <summary>Builds the dotnet test command-line for one project.</summary>
    private IReadOnlyList<string> BuildArguments(string testProject, string outputPrefix) =>
    [
        "test",
        testProject,
        "--configuration",
        options.Configuration,
        "--verbosity",
        "minimal",
        "/p:CollectCoverage=true",
        $"/p:CoverletOutput={outputPrefix}",
        "/p:CoverletOutputFormat=json%2ccobertura%2clcov",
        $"/p:Include={string.Join("%2c", sourceModules.Select(name => $"[{name}]*"))}",
        "/p:Exclude=[*.Test]*",
    ];

    /// <summary>Recreates a directory so stale coverage files cannot affect a later run.</summary>
    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }
}
