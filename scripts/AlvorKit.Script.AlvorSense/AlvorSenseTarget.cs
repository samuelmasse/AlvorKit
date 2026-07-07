namespace AlvorKit.Script.AlvorSense;

/// <summary>Owns the hosted game process and captures stdout for interactive request responses.</summary>
[ExcludeFromCodeCoverage(Justification = "Wraps a long-running child process with redirected streams.")]
internal sealed class AlvorSenseTarget : IDisposable
{
    /// <summary>Synchronizes access to captured stdout lines.</summary>
    private readonly Lock gate = new();

    /// <summary>All captured stdout lines in arrival order.</summary>
    private readonly List<string> lines = [];

    /// <summary>Persistent stdout log for the target process.</summary>
    private readonly StreamWriter stdoutLog;

    /// <summary>Persistent stderr log for the target process.</summary>
    private readonly StreamWriter stderrLog;

    /// <summary>Hosted game process.</summary>
    private readonly Process process;

    /// <summary>Creates a target wrapper and starts asynchronous stream capture.</summary>
    /// <param name="sessionDir">Session directory where logs are written.</param>
    /// <param name="process">Started game process.</param>
    private AlvorSenseTarget(string sessionDir, Process process)
    {
        this.process = process;
        stdoutLog = File.AppendText(AlvorSensePaths.Stdout(sessionDir));
        stderrLog = File.AppendText(AlvorSensePaths.Stderr(sessionDir));
        process.OutputDataReceived += (_, e) => AcceptStdout(e.Data);
        process.ErrorDataReceived += (_, e) => AcceptStderr(e.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    /// <summary>Gets whether the target process has exited.</summary>
    internal bool HasExited => process.HasExited;

    /// <summary>Gets the target process exit code when available.</summary>
    internal int? ExitCode => process.HasExited ? process.ExitCode : null;

    /// <summary>Gets the number of stdout lines captured so far.</summary>
    internal int LineCount { get { lock (gate) return lines.Count; } }

    /// <summary>Starts the target project with the AlvorSense environment enabled.</summary>
    /// <param name="sessionDir">Session directory where target logs are written.</param>
    /// <param name="manifest">Session manifest describing the target process.</param>
    /// <returns>A started target process wrapper.</returns>
    internal static AlvorSenseTarget Start(string sessionDir, AlvorSenseSessionManifest manifest)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = manifest.WorkingDirectory
        };
        start.Environment["ALVORKIT_WINDOWING_AGENT"] = "1";
        start.Environment[AlvorSenseEnvironment.AudioSilentVariable] = AlvorSenseEnvironment.EnabledValue;
        foreach (var pair in manifest.Environment)
            start.Environment[pair.Key] = pair.Value;
        start.ArgumentList.Add("run");
        start.ArgumentList.Add("--project");
        start.ArgumentList.Add(manifest.Project);
        return new(sessionDir, Process.Start(start) ?? throw new InvalidOperationException("Failed to start target process."));
    }

    /// <summary>Sends command lines to the target stdin.</summary>
    /// <param name="commands">Command lines to write to the hosted game process.</param>
    internal void Send(IReadOnlyList<string> commands)
    {
        foreach (var command in commands)
            process.StandardInput.WriteLine(command);
        process.StandardInput.Flush();
    }

    /// <summary>Returns stdout lines captured after the given count.</summary>
    /// <param name="start">Previously observed stdout line count.</param>
    /// <returns>Captured lines after <paramref name="start" />.</returns>
    internal string[] LinesSince(int start)
    {
        lock (gate)
            return [.. lines.Skip(Math.Min(start, lines.Count))];
    }

    /// <summary>Waits for a state line to appear after the supplied line count.</summary>
    /// <param name="start">Previously observed stdout line count.</param>
    /// <param name="timeout">Maximum time to wait for a state response.</param>
    /// <returns><see langword="true" /> when a state line was observed.</returns>
    internal bool WaitForState(int start, TimeSpan timeout)
    {
        return WaitForLine(start, timeout, static line => line.StartsWith("time=", StringComparison.Ordinal));
    }

    /// <summary>Waits until the target prints its interactive command prompt.</summary>
    /// <param name="timeout">Maximum time to wait for the command prompt.</param>
    /// <returns><see langword="true" /> when the target is ready to accept agent commands.</returns>
    internal bool WaitForReady(TimeSpan timeout)
    {
        return WaitForLine(0, timeout, static line => line.StartsWith("Usage:", StringComparison.Ordinal));
    }

    /// <summary>Waits for a captured stdout line matching the supplied predicate.</summary>
    /// <param name="start">Previously observed stdout line count.</param>
    /// <param name="timeout">Maximum time to wait for the line.</param>
    /// <param name="predicate">Predicate used to identify the expected line.</param>
    /// <returns><see langword="true" /> when a matching line was observed.</returns>
    private bool WaitForLine(int start, TimeSpan timeout, Func<string, bool> predicate)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline && !process.HasExited)
        {
            if (LinesSince(start).Any(predicate))
                return true;
            Thread.Sleep(25);
        }
        return false;
    }

    /// <summary>Waits for the target process to exit.</summary>
    /// <param name="timeout">Maximum time to wait for process exit.</param>
    /// <returns><see langword="true" /> when the target exited in time.</returns>
    internal bool WaitExit(TimeSpan timeout) => process.WaitForExit(timeout);

    /// <summary>Kills the target process if it is still running.</summary>
    internal void Kill() { if (!process.HasExited) process.Kill(entireProcessTree: true); }

    /// <summary>Disposes process and log resources.</summary>
    public void Dispose()
    {
        stdoutLog.Dispose();
        stderrLog.Dispose();
        process.Dispose();
    }

    /// <summary>Records one stdout line from the hosted process.</summary>
    /// <param name="line">Line captured by the process stream callback.</param>
    private void AcceptStdout(string? line)
    {
        if (line is null)
            return;
        lock (gate) lines.Add(line);
        stdoutLog.WriteLine(line);
        stdoutLog.Flush();
    }

    /// <summary>Records one stderr line from the hosted process.</summary>
    /// <param name="line">Line captured by the process stream callback.</param>
    private void AcceptStderr(string? line)
    {
        if (line is null)
            return;
        stderrLog.WriteLine(line);
        stderrLog.Flush();
    }
}
