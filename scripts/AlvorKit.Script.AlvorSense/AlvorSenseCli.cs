namespace AlvorKit.Script.AlvorSense;

/// <summary>Coordinates the foreground command-line surface for AlvorSense sessions.</summary>
[ExcludeFromCodeCoverage(Justification = "Coordinates external host processes and filesystem mailbox waits.")]
internal static class AlvorSenseCli
{
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
        AlvorSenseJson.Save(AlvorSensePaths.Manifest(sessionDir), command.ToManifest());
        using var hostProcess = AlvorSenseHostProcess.Start(sessionDir);
        AlvorSenseHostProcess.WaitReady(sessionDir, command.Timeout);
        var status = AlvorSenseSessionRegistry.Get(command.Id);
        output.WriteLine(AlvorSenseJson.ToJson(new AlvorSenseStartResult(command.Id, sessionDir, status.Ready, status.ProcessId)));
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
        AlvorSenseForegroundResponses.WriteSend(response, command, sessionDir, output);
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
        WriteResponse(response, output);
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
        output.WriteLine(AlvorSenseJson.ToJson(AlvorSenseSessionRegistry.Get(command.Id)));
        output.Flush();
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
    private static void WriteResponse(AlvorSenseResponse response, TextWriter output)
    {
        output.WriteLine(AlvorSenseJson.ToJson(response));
        output.Flush();
    }
}
