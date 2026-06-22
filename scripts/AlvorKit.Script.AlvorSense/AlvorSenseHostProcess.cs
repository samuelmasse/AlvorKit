namespace AlvorKit.Script.AlvorSense;

/// <summary>Starts and waits for the detached host process that owns one interactive game session.</summary>
[ExcludeFromCodeCoverage(Justification = "Starts detached helper processes for interactive agent sessions.")]
internal static class AlvorSenseHostProcess
{
    /// <summary>Starts the background host that owns the target game process.</summary>
    /// <param name="sessionDir">Session directory containing the manifest and mailbox.</param>
    /// <returns>The started host process, kept alive by the caller until foreground startup output is written.</returns>
    internal static Process Start(string sessionDir)
    {
        var start = CreateStartInfo(sessionDir, typeof(AlvorSenseCli).Assembly.Location);
        return Process.Start(start) ?? throw new InvalidOperationException("Failed to start AlvorSense host.");
    }

    /// <summary>Creates the detached host process start information from a session-local runtime copy.</summary>
    /// <param name="sessionDir">Session directory containing the manifest and mailbox.</param>
    /// <param name="assemblyPath">Foreground AlvorSense assembly path to copy for detached execution.</param>
    /// <returns>Configured host process start information.</returns>
    internal static ProcessStartInfo CreateStartInfo(string sessionDir, string assemblyPath)
    {
        var assembly = AlvorSenseHostRuntime.CopyForSession(sessionDir, assemblyPath);
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };
        start.ArgumentList.Add(assembly);
        start.ArgumentList.Add("host");
        start.ArgumentList.Add("--session-dir");
        start.ArgumentList.Add(Path.GetFullPath(sessionDir));
        return start;
    }

    /// <summary>Waits for the host ready marker.</summary>
    /// <param name="sessionDir">Session directory containing the ready marker.</param>
    /// <param name="timeout">Maximum time to wait for readiness.</param>
    internal static void WaitReady(string sessionDir, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (File.Exists(AlvorSensePaths.Ready(sessionDir)))
                return;
            Thread.Sleep(50);
        }

        throw new TimeoutException("Timed out waiting for AlvorSense host.");
    }
}
