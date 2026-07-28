namespace AlvorKit.Script.TestInterception;

/// <summary>Builds a private profiler-enabled environment for one selected child.</summary>
internal sealed class InterceptionLauncher
{
    /// <summary>Launches the configured test or executable project.</summary>
    internal async Task<int> RunAsync(
        InterceptionLaunchOptions options,
        CancellationToken cancellationToken = default)
    {
        var profilerPath = InterceptionProfilerAsset.Resolve(
            options.RepositoryRoot,
            options.ProfilerPath);
        var projectPath = Path.GetFullPath(
            options.Project,
            options.RepositoryRoot);
        if (!File.Exists(projectPath))
            throw new FileNotFoundException("The selected project does not exist.", projectPath);

        var modules = options.Modules.Count == 0
            ? new[] { Path.GetFileNameWithoutExtension(projectPath) }
            : options.Modules;
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            "AlvorKit",
            "TestInterception",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);

        try
        {
            var startInfo = options.IsTest
                ? TestStartInfo(options, projectPath, profilerPath, modules, temporaryRoot)
                : ExecutableStartInfo(options, projectPath, profilerPath, modules);
            Console.WriteLine(
                $"Interception child: dotnet {string.Join(" ", startInfo.ArgumentList)}");
            Console.WriteLine($"Profiler: {profilerPath}");
            Console.WriteLine($"Modules: {string.Join(";", modules)}");
            return await InterceptionChildProcess.RunAsync(
                startInfo,
                options.Timeout,
                cancellationToken);
        }
        finally
        {
            if (Directory.Exists(temporaryRoot))
                Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    private static ProcessStartInfo TestStartInfo(
        InterceptionLaunchOptions options,
        string projectPath,
        string profilerPath,
        IReadOnlyList<string> modules,
        string temporaryRoot)
    {
        var settingsPath = InterceptionRunSettings.Write(
            temporaryRoot,
            profilerPath,
            modules);
        var startInfo = BaseStartInfo(options.RepositoryRoot);
        Add(
            startInfo,
            "test",
            projectPath,
            "--configuration",
            options.Configuration,
            "--settings",
            settingsPath);
        if (options.Filter is not null)
            Add(startInfo, "--filter", options.Filter);
        Add(startInfo, options.ChildArguments);
        return startInfo;
    }

    private static ProcessStartInfo ExecutableStartInfo(
        InterceptionLaunchOptions options,
        string projectPath,
        string profilerPath,
        IReadOnlyList<string> modules)
    {
        var startInfo = BaseStartInfo(options.RepositoryRoot);
        Add(
            startInfo,
            "run",
            "--project",
            projectPath,
            "--configuration",
            options.Configuration);
        if (options.ChildArguments.Count > 0)
        {
            Add(startInfo, "--");
            Add(startInfo, options.ChildArguments);
        }

        SetProfilerEnvironment(startInfo, profilerPath, modules);
        return startInfo;
    }

    private static ProcessStartInfo BaseStartInfo(string repositoryRoot)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        InterceptionProfilerEnvironment.Clear(startInfo);
        return startInfo;
    }

    private static void SetProfilerEnvironment(
        ProcessStartInfo startInfo,
        string profilerPath,
        IReadOnlyList<string> modules)
    {
        startInfo.Environment["CORECLR_ENABLE_PROFILING"] = "1";
        startInfo.Environment["CORECLR_PROFILER"] =
            InterceptionRunSettings.ProfilerClsid;
        startInfo.Environment["CORECLR_PROFILER_PATH"] = profilerPath;
        startInfo.Environment["CORECLR_PROFILER_PATH_64"] = profilerPath;
        startInfo.Environment[
            InterceptionProfilerAsset.PathVariable] = profilerPath;
        startInfo.Environment["DOTNET_ReadyToRun"] = "0";
        startInfo.Environment[
            InterceptionRunSettings.ModulesVariable] = string.Join(";", modules);
    }

    private static void Add(
        ProcessStartInfo startInfo,
        params IEnumerable<string> arguments)
    {
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);
    }
}
