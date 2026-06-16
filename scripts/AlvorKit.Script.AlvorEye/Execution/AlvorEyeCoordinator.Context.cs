namespace AlvorKit.Script.AlvorEye;

internal sealed partial class AlvorEyeCoordinator
{
    /// <summary>Starts or attaches to a scenario target and creates a run context.</summary>
    private async Task<RunContext> PrepareContextAsync(string repoRoot, AlvorEyeScenario scenario, bool createSession)
    {
        var (runId, runDirectory, framesDirectory, logsDirectory) = OutputPaths.Create(repoRoot, scenario.Output);
        var manifest = new RunManifest { RunId = runId, RunDirectory = runDirectory };
        var store = new SessionStore(repoRoot);
        var context = new RunContext(repoRoot, scenario, platform, store, manifest, framesDirectory);
        if (StartProcess(scenario.Run, logsDirectory) is { } launched)
        {
            var (process, stdoutCopy, stderrCopy) = launched;
            context.Process = process;
            context.LogCopyTasks.Add(stdoutCopy);
            context.LogCopyTasks.Add(stderrCopy);
        }

        try
        {
            context.Target = await platform.WaitForWindowAsync(scenario.Window, CancellationToken.None);
            platform.PlaceWindow(context.Target, scenario.Window);
        }
        catch
        {
            if (context.Process is { HasExited: false } process)
                process.Kill(entireProcessTree: true);
            throw;
        }

        if (createSession)
            CreateSessionState(context, scenario);

        return context;
    }

    /// <summary>Reopens persistent state for an existing session.</summary>
    private async Task<RunContext> ReopenSessionAsync(string repoRoot, string sessionId)
    {
        var store = new SessionStore(repoRoot);
        var state = store.Load(sessionId);
        var scenario = new AlvorEyeScenario
        {
            Window = new() { Title = state.WindowTitle, Exact = state.ExactTitle },
            Output = new() { Directory = state.RunDirectory }
        };
        var manifest = LoadManifest(state.RunDirectory, sessionId);
        var framesDirectory = Path.Combine(state.RunDirectory, "frames");
        var context = new RunContext(repoRoot, scenario, platform, store, manifest, framesDirectory)
        {
            Session = state,
            Target = await platform.WaitForWindowAsync(scenario.Window, CancellationToken.None)
        };
        return context;
    }

    /// <summary>Starts the configured child process and redirects logs to disk.</summary>
    private static LaunchedProcess? StartProcess(ScenarioRun? run, string logsDirectory)
    {
        if (run is null)
            return null;

        var start = new ProcessStartInfo(run.Executable)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = run.WorkingDirectory ?? Environment.CurrentDirectory
        };
        foreach (var arg in run.Args)
            start.ArgumentList.Add(arg);
        foreach (var item in run.Environment)
            start.Environment[item.Key] = item.Value;

        var process = Process.Start(start) ?? throw new InvalidOperationException("Failed to start target process.");
        return new LaunchedProcess(
            process,
            CopyLogAsync(process.StandardOutput, Path.Combine(logsDirectory, "stdout.log")),
            CopyLogAsync(process.StandardError, Path.Combine(logsDirectory, "stderr.log")));
    }

    /// <summary>Copies redirected process output to a log file.</summary>
    private static async Task CopyLogAsync(TextReader reader, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(await reader.ReadToEndAsync());
    }

    /// <summary>Loads a previous manifest for resumed session commands.</summary>
    private static RunManifest LoadManifest(string runDirectory, string sessionId)
    {
        var path = Path.Combine(runDirectory, "manifest.json");
        if (File.Exists(path))
        {
            var manifest = JsonSerializer.Deserialize<RunManifest>(File.ReadAllText(path, Encoding.UTF8), ScenarioJson.Options);
            if (manifest is not null)
                return manifest;
        }

        return new RunManifest { RunId = sessionId, SessionId = sessionId, RunDirectory = runDirectory };
    }

    /// <summary>Creates and saves persistent session state for a context.</summary>
    private static void CreateSessionState(RunContext context, AlvorEyeScenario scenario)
    {
        var sessionId = context.Manifest.RunId;
        var state = new SessionState
        {
            SessionId = sessionId,
            RepoRoot = context.RepoRoot,
            RunDirectory = context.Manifest.RunDirectory,
            WindowTitle = scenario.Window.Title,
            ExactTitle = scenario.Window.Exact,
            ProcessId = context.Target.ProcessId
        };
        context.Session = state;
        context.Manifest.SessionId = sessionId;
        context.SessionStore.Save(state);
    }

    /// <summary>Started process with asynchronous log-copy tasks.</summary>
    private readonly record struct LaunchedProcess(Process Process, Task StdoutCopy, Task StderrCopy);
}
