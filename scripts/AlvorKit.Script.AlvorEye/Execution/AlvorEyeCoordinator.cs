namespace AlvorKit.Script.AlvorEye;

/// <summary>Coordinates CLI commands, scenario execution, and session persistence.</summary>
[ExcludeFromCodeCoverage]
internal sealed partial class AlvorEyeCoordinator(IAlvorEyePlatform platform)
{
    /// <summary>Action executor used for timeline commands.</summary>
    private readonly ActionExecutor executor = new();

    /// <summary>Creates a coordinator for the current operating system.</summary>
    public static AlvorEyeCoordinator CreateDefault() =>
        new(OperatingSystem.IsWindowsVersionAtLeast(6, 1)
            ? new WindowsAlvorEyePlatform()
            : new UnsupportedAlvorEyePlatform(RuntimeInformation.OSDescription));

    /// <summary>Executes a parsed command.</summary>
    public async Task<AlvorEyeResult> ExecuteAsync(AlvorEyeCommand command, TextReader input, TextWriter jsonOutput)
    {
        return command.Kind switch
        {
            AlvorEyeCommandKind.Help => AlvorEyeResult.Success(AlvorEyeCommandParser.HelpText),
            AlvorEyeCommandKind.Run => await RunAsync(command),
            AlvorEyeCommandKind.Session => await SessionAsync(command, input, jsonOutput),
            AlvorEyeCommandKind.Handoff => await HandoffAsync(command),
            AlvorEyeCommandKind.Resume => await ResumeAsync(command),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }

    /// <summary>Executes a complete scenario.</summary>
    private async Task<AlvorEyeResult> RunAsync(AlvorEyeCommand command)
    {
        var scenario = AlvorEyeScenarioParser.ParseFile(command.ScenarioPath!);
        var context = await PrepareContextAsync(command.RepoRoot, scenario, createSession: false);
        try
        {
            await executor.ExecuteAsync(context, scenario.Timeline);
            return AlvorEyeResult.Success($"Run artifacts: {context.Manifest.RunDirectory}");
        }
        finally
        {
            await FinishAsync(context, stopOwnedProcess: true);
        }
    }

    /// <summary>Starts a session and consumes JSONL action commands from standard input.</summary>
    private async Task<AlvorEyeResult> SessionAsync(AlvorEyeCommand command, TextReader input, TextWriter jsonOutput)
    {
        var scenario = AlvorEyeScenarioParser.ParseFile(command.ScenarioPath!);
        var context = await PrepareContextAsync(command.RepoRoot, scenario, createSession: true);
        try
        {
            await executor.ExecuteAsync(context, scenario.Timeline);
            await ReadSessionCommandsAsync(context, input, jsonOutput);
            return AlvorEyeResult.Success($"Session {context.Session!.SessionId}: {context.Manifest.RunDirectory}");
        }
        finally
        {
            await FinishAsync(context, stopOwnedProcess: false);
        }
    }

    /// <summary>Freezes an existing session target and captures its frame.</summary>
    private async Task<AlvorEyeResult> HandoffAsync(AlvorEyeCommand command)
    {
        var context = await ReopenSessionAsync(command.RepoRoot, command.SessionId!);
        await executor.ExecuteAsync(context, new AlvorEyeAction { Kind = AlvorEyeActionKind.Handoff, Name = "handoff" });
        WriteManifest(context);
        return AlvorEyeResult.Success($"Session {command.SessionId} frozen.");
    }

    /// <summary>Resumes an existing session target.</summary>
    private async Task<AlvorEyeResult> ResumeAsync(AlvorEyeCommand command)
    {
        var context = await ReopenSessionAsync(command.RepoRoot, command.SessionId!);
        await executor.ExecuteAsync(context, new AlvorEyeAction { Kind = AlvorEyeActionKind.Resume });
        WriteManifest(context);
        return AlvorEyeResult.Success($"Session {command.SessionId} resumed.");
    }

    /// <summary>Reads JSONL session commands until stdin closes.</summary>
    private async Task ReadSessionCommandsAsync(RunContext context, TextReader input, TextWriter jsonOutput)
    {
        for (var line = await input.ReadLineAsync(); line is not null; line = await input.ReadLineAsync())
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var actions = AlvorEyeActionParser.ParseJsonLine(line);
            await executor.ExecuteAsync(context, actions);
            context.SessionStore.Save(context.Session!);
            await jsonOutput.WriteLineAsync(JsonSerializer.Serialize(new
            {
                status = "ok",
                frozen = context.Session!.Frozen,
                queued = context.Session.QueuedActions.Count,
                lastFrame = context.LastFramePath
            }, ScenarioJson.Options));
        }
    }

    /// <summary>Writes the run manifest and optionally stops an owned process.</summary>
    private static async Task FinishAsync(RunContext context, bool stopOwnedProcess)
    {
        WriteManifest(context);
        if (stopOwnedProcess && context.Process is { HasExited: false } process)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }

        if (context.LogCopyTasks.Count > 0)
            await Task.WhenAll(context.LogCopyTasks);
    }

    /// <summary>Writes the run manifest to disk.</summary>
    private static void WriteManifest(RunContext context)
    {
        var path = Path.Combine(context.Manifest.RunDirectory, "manifest.json");
        File.WriteAllText(path, JsonSerializer.Serialize(context.Manifest, ScenarioJson.Options), Encoding.UTF8);
    }
}
