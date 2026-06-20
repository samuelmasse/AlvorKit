namespace AlvorKit.Script.AlvorSense;

/// <summary>Runs the background request loop for one persistent agent-controlled game session.</summary>
/// <param name="sessionDir">Session directory containing the manifest, request mailbox, and logs.</param>
[ExcludeFromCodeCoverage(Justification = "Coordinates a persistent external game process and filesystem mailbox.")]
internal sealed class AlvorSenseHost(string sessionDir)
{
    /// <summary>Prefix written by the in-process command runner for recoverable command errors.</summary>
    private const string CommandErrorPrefix = "ALVORSENSE_COMMAND_ERROR ";

    /// <summary>Manifest describing the hosted game process.</summary>
    private readonly AlvorSenseSessionManifest manifest = AlvorSenseJson.Load<AlvorSenseSessionManifest>(AlvorSensePaths.Manifest(sessionDir));

    /// <summary>Runs the host request loop until the target exits or a stop request arrives.</summary>
    internal int Run()
    {
        using var target = AlvorSenseTarget.Start(sessionDir, manifest);
        if (!target.WaitForReady(TimeSpan.FromSeconds(30)))
            return 1;

        AlvorSenseJson.Save(AlvorSensePaths.Ready(sessionDir), new { processId = Environment.ProcessId });

        while (!target.HasExited)
        {
            var requestPath = Directory.GetFiles(Path.Combine(sessionDir, "requests"), "*.json").OrderBy(static x => x).FirstOrDefault();
            if (requestPath is null)
            {
                Thread.Sleep(50);
                continue;
            }

            var request = AlvorSenseJson.Load<AlvorSenseRequest>(requestPath);
            File.Delete(requestPath);
            var response = Handle(target, request);
            AlvorSenseJson.Save(AlvorSensePaths.Response(sessionDir, request.Id), response);
            if (request.Stop)
                break;
        }

        return 0;
    }

    /// <summary>Handles one foreground request and captures the target output produced during it.</summary>
    /// <param name="target">Hosted game process wrapper.</param>
    /// <param name="request">Request loaded from the mailbox.</param>
    /// <returns>The response to persist for the foreground CLI.</returns>
    private static AlvorSenseResponse Handle(AlvorSenseTarget target, AlvorSenseRequest request)
    {
        try
        {
            var start = target.LineCount;
            if (request.Stop)
                return Stop(target, request.Id, start);

            var exits = request.Commands.Any(IsExit);
            target.Send(request.Commands);
            if (request.AppendState && !exits)
                target.Send(["state"]);

            var observed = exits ? target.WaitExit(TimeSpan.FromSeconds(30)) : target.WaitForState(start, TimeSpan.FromSeconds(30));
            var lines = target.LinesSince(start);
            var commandError = CommandError(lines);
            var ok = observed && commandError is null;
            return new(
                request.Id,
                ok,
                request.Commands.Length,
                StateLine(lines),
                lines,
                target.HasExited,
                target.ExitCode,
                commandError ?? (observed ? null : "Timed out waiting for target."));
        }
        catch (Exception ex)
        {
            return new(request.Id, false, request.Commands.Length, null, [], target.HasExited, target.ExitCode, ex.Message);
        }
    }

    /// <summary>Stops the hosted game process and captures any final output.</summary>
    /// <param name="target">Hosted game process wrapper.</param>
    /// <param name="id">Request id to copy into the response.</param>
    /// <param name="start">Output line count before the stop request was sent.</param>
    /// <returns>A stop response.</returns>
    private static AlvorSenseResponse Stop(AlvorSenseTarget target, string id, int start)
    {
        target.Send(["quit"]);
        if (!target.WaitExit(TimeSpan.FromSeconds(5)))
            target.Kill();
        return new(id, true, 0, null, target.LinesSince(start), true, target.ExitCode, null);
    }

    /// <summary>Finds the first recoverable command error reported by the hosted target.</summary>
    /// <param name="lines">Output lines captured while handling a request.</param>
    /// <returns>The error message without its protocol prefix, or <see langword="null" />.</returns>
    private static string? CommandError(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.StartsWith(CommandErrorPrefix, StringComparison.Ordinal))
                return line[CommandErrorPrefix.Length..];
        }
        return null;
    }

    /// <summary>Finds the latest state line captured while handling a request.</summary>
    /// <param name="lines">Output lines captured while handling a request.</param>
    private static string? StateLine(IEnumerable<string> lines) =>
        lines.LastOrDefault(static line => line.StartsWith("time=", StringComparison.Ordinal));

    /// <summary>Checks whether a command batch asks the hosted process to exit itself.</summary>
    /// <param name="command">One command line.</param>
    /// <returns><see langword="true" /> when the command is <c>quit</c> or <c>exit</c>.</returns>
    private static bool IsExit(string command)
    {
        var trimmed = command.Trim();
        return trimmed.Equals("quit", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("exit", StringComparison.OrdinalIgnoreCase);
    }
}
