namespace AlvorKit.Script.AlvorSense;

/// <summary>Starts and waits for the detached host process that owns one interactive game session.</summary>
[ExcludeFromCodeCoverage(Justification = "Starts detached helper processes for interactive agent sessions.")]
internal static class AlvorSenseHostProcess
{
    /// <summary>Starts the background host that owns the target game process.</summary>
    /// <param name="sessionDir">Session directory containing the manifest and mailbox.</param>
    internal static void Start(string sessionDir)
    {
        var assembly = typeof(AlvorSenseCli).Assembly.Location;
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };
        start.ArgumentList.Add(assembly);
        start.ArgumentList.Add("host");
        start.ArgumentList.Add("--session-dir");
        start.ArgumentList.Add(Path.GetFullPath(sessionDir));
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Failed to start AlvorSense host.");
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
