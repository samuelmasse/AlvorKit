namespace AlvorKit.Script.AlvorSense;

/// <summary>Reads persisted AlvorSense session directories for recovery, status checks, and handoffs.</summary>
internal static class AlvorSenseSessionRegistry
{
    /// <summary>Returns summaries for all known session directories, newest first.</summary>
    internal static AlvorSenseSessionInfo[] List()
    {
        if (!Directory.Exists(AlvorSensePaths.Root))
            return [];

        return [.. Directory.GetDirectories(AlvorSensePaths.Root)
            .Select(Read)
            .OrderByDescending(static session => session.LastWriteTimeUtc)
            .ThenBy(static session => session.Id, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>Returns a summary for one session id.</summary>
    /// <param name="id">Session id to inspect.</param>
    internal static AlvorSenseSessionInfo Get(string id)
    {
        var sessionDir = AlvorSensePaths.SessionDir(id);
        if (!Directory.Exists(sessionDir))
            throw new InvalidOperationException($"Session does not exist: {id}");
        return Read(sessionDir);
    }

    /// <summary>Reads one session directory and counts its mailbox files.</summary>
    /// <param name="sessionDir">Directory to inspect.</param>
    private static AlvorSenseSessionInfo Read(string sessionDir)
    {
        var readyPath = AlvorSensePaths.Ready(sessionDir);
        return new(
            Path.GetFileName(sessionDir),
            sessionDir,
            File.Exists(readyPath),
            ReadProcessId(readyPath),
            CountFiles(Path.Combine(sessionDir, "requests")),
            CountFiles(Path.Combine(sessionDir, "responses")),
            Directory.GetLastWriteTimeUtc(sessionDir));
    }

    /// <summary>Counts committed JSON files in a mailbox directory.</summary>
    /// <param name="directory">Mailbox directory to inspect.</param>
    private static int CountFiles(string directory) =>
        Directory.Exists(directory) ? Directory.GetFiles(directory, "*.json").Length : 0;

    /// <summary>Reads the optional host process id from the ready marker.</summary>
    /// <param name="readyPath">Ready marker path.</param>
    private static int? ReadProcessId(string readyPath)
    {
        if (!File.Exists(readyPath))
            return null;

        using var document = JsonDocument.Parse(File.ReadAllText(readyPath, Encoding.UTF8));
        return document.RootElement.TryGetProperty("processId", out var processId) && processId.TryGetInt32(out var value) ? value : null;
    }
}
