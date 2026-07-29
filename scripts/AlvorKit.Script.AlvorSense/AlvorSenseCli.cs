namespace AlvorKit.Script.AlvorSense;

/// <summary>Coordinates the foreground command-line surface for AlvorSense sessions.</summary>
[ExcludeFromCodeCoverage(Justification = "Coordinates external host processes and filesystem mailbox waits.")]
internal static class AlvorSenseCli
{
    private static readonly LiveWorkspaceStore Workspaces = new(Directory.GetCurrentDirectory());

    /// <summary>Runs the AlvorSense session command line.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    /// <param name="input">Input stream used for send commands.</param>
    /// <param name="output">Output stream receiving command results.</param>
    /// <param name="error">Error stream receiving command failures.</param>
    /// <returns>The process exit code for the command.</returns>
    internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error)
    {
        try
        {
            return RunCore(AlvorSenseCommandLine.Parse(args, input), output);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or TimeoutException)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>Dispatches one parsed command.</summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="output">Output stream receiving command results.</param>
    /// <returns>The process exit code for the command.</returns>
    private static int RunCore(AlvorSenseCommand command, TextWriter output) =>
        command switch
        {
            AlvorSenseStartCommand start => Start(start, output),
            AlvorSenseSendCommand send => Send(send, output),
            AlvorSenseStopCommand stop => Stop(stop, output),
            AlvorSenseListCommand => List(output),
            AlvorSenseStatusCommand status => Status(status, output),
            AlvorSenseHelpCommand help => Help(help, output),
            AlvorSenseHostCommand host => new AlvorSenseHost(host.SessionDir).Run(),
            _ => throw new ArgumentException("Unknown command.")
        };

    /// <summary>Creates a session directory, persists its manifest, and starts the detached host.</summary>
    /// <param name="command">Parsed start command.</param>
    /// <param name="output">Output stream receiving the session id and directory.</param>
    /// <returns>The command exit code.</returns>
    private static int Start(AlvorSenseStartCommand command, TextWriter output)
    {
        var sessionDir = AlvorSensePaths.SessionDir(command.Id);
        if (Directory.Exists(sessionDir))
            throw new InvalidOperationException($"Session already exists: {command.Id}");

        Directory.CreateDirectory(sessionDir);
        Directory.CreateDirectory(Path.Combine(sessionDir, "requests"));
        Directory.CreateDirectory(Path.Combine(sessionDir, "responses"));
        var manifest = command.ToManifest();
        if (command.EditableProject is not null)
            manifest = AlvorSenseEditableProject.Prepare(sessionDir, manifest);
        AlvorSenseJson.Save(AlvorSensePaths.Manifest(sessionDir), manifest);
        using var hostProcess = AlvorSenseHostProcess.Start(sessionDir);
        AlvorSenseHostProcess.WaitReady(sessionDir, command.Timeout);
        var status = AlvorSenseSessionRegistry.Get(command.Id);
        output.WriteLine(AlvorSenseJson.ToJson(new AlvorSenseStartResult(
            command.Id,
            sessionDir,
            status.Ready,
            status.ProcessId,
            manifest.EditableLaunchManifestPath)));
        output.Flush();
        return 0;
    }

    /// <summary>Sends command text to a running session and writes the JSON response.</summary>
    /// <param name="command">Parsed send command.</param>
    /// <param name="output">Output stream receiving the JSON response.</param>
    /// <returns>The command exit code.</returns>
    private static int Send(AlvorSenseSendCommand command, TextWriter output)
    {
        var sessionDir = AlvorSensePaths.SessionDir(command.Id);
        var request = new AlvorSenseRequest(Guid.NewGuid().ToString("N"), command.Commands, Stop: false, AppendState: true);
        var response = AlvorSenseRequestStore.Send(sessionDir, request, command.Timeout);
        var result = AlvorSenseForegroundResponses.Result(response, command, sessionDir);
        Record(
            command.Workspace,
            command.Id,
            "alvorsense-send",
            new
            {
                sessionId = command.Id,
                commands = command.Commands,
                timeoutSeconds = command.Timeout.TotalSeconds,
                command.StderrTailLines
            },
            result);
        WriteResult(result, output);
        return response.Ok ? 0 : 1;
    }

    /// <summary>Requests a running session to terminate and writes the JSON response.</summary>
    /// <param name="command">Parsed stop command.</param>
    /// <param name="output">Output stream receiving the JSON response.</param>
    /// <returns>The command exit code.</returns>
    private static int Stop(AlvorSenseStopCommand command, TextWriter output)
    {
        var sessionDir = AlvorSensePaths.SessionDir(command.Id);
        var request = new AlvorSenseRequest(Guid.NewGuid().ToString("N"), [], Stop: true, AppendState: false);
        var response = AlvorSenseRequestStore.Send(sessionDir, request, command.Timeout);
        Record(
            command.Workspace,
            command.Id,
            "alvorsense-stop",
            new
            {
                sessionId = command.Id,
                timeoutSeconds = command.Timeout.TotalSeconds
            },
            response);
        WriteResult(response, output);
        return response.Ok ? 0 : 1;
    }

    /// <summary>Writes known session directories as JSON.</summary>
    /// <param name="output">Output stream receiving session summaries.</param>
    /// <returns>The command exit code.</returns>
    private static int List(TextWriter output)
    {
        output.WriteLine(AlvorSenseJson.ToJson(AlvorSenseSessionRegistry.List()));
        output.Flush();
        return 0;
    }

    /// <summary>Writes one session summary as JSON.</summary>
    /// <param name="command">Parsed status command.</param>
    /// <param name="output">Output stream receiving the session summary.</param>
    /// <returns>The command exit code.</returns>
    private static int Status(AlvorSenseStatusCommand command, TextWriter output)
    {
        var result = AlvorSenseSessionRegistry.Get(command.Id);
        Record(
            command.Workspace,
            command.Id,
            "alvorsense-status",
            new { sessionId = command.Id },
            result);
        WriteResult(result, output);
        return 0;
    }

    /// <summary>Writes generated CLI help without requiring a running session.</summary>
    /// <param name="command">Parsed help request containing contextual help arguments.</param>
    /// <param name="output">Output stream receiving usage text.</param>
    /// <returns>The command exit code.</returns>
    private static int Help(AlvorSenseHelpCommand command, TextWriter output)
    {
        AlvorSenseCommandLine.WriteHelp(command.Args, output);
        output.Flush();
        return 0;
    }

    /// <summary>Writes one protocol response as JSON.</summary>
    /// <param name="response">Response to serialize.</param>
    /// <param name="output">Output stream receiving the JSON response.</param>
    private static void WriteResult<T>(T response, TextWriter output)
    {
        output.WriteLine(AlvorSenseJson.ToJson(response));
        output.Flush();
    }

    /// <summary>Records one exact AlvorSense request after verifying its workspace association.</summary>
    private static void Record<TRequest, TResult>(
        string? workspace,
        string sessionId,
        string operation,
        TRequest request,
        TResult result)
    {
        if (workspace is null)
            return;
        var manifest = Workspaces.Read(workspace);
        if (manifest.AlvorSenseSessionId != sessionId)
        {
            throw new InvalidOperationException(
                $"Live workspace '{manifest.Id}' is associated with AlvorSense session " +
                $"'{manifest.AlvorSenseSessionId ?? "none"}', not '{sessionId}'.");
        }
        Workspaces.Record(workspace, operation, request, result);
    }
}
