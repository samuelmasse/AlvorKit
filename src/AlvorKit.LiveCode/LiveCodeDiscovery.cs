namespace AlvorKit;

/// <summary>Finds running loopback LiveCode sessions through same-user discovery manifests.</summary>
public sealed class LiveCodeDiscovery(string? directory = null)
{
    /// <summary>Gets the default per-user discovery directory.</summary>
    public static string DefaultDirectory =>
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AlvorKit", "LiveCode");

    /// <summary>Gets the directory searched by this discovery instance.</summary>
    public string Directory { get; } = directory ?? DefaultDirectory;

    /// <summary>Lists manifests whose owning process is still running.</summary>
    public LiveCodeSessionManifest[] List()
    {
        if (!System.IO.Directory.Exists(Directory))
            return [];

        var sessions = new List<LiveCodeSessionManifest>();
        foreach (var path in System.IO.Directory.GetFiles(Directory, "*.json"))
        {
            var session = Read(path);
            if (session is not null && IsAlive(session.ProcessId))
                sessions.Add(session);
        }

        return [.. sessions.OrderByDescending(x => x.StartedUtc)];
    }

    /// <summary>Finds the newest running session matching an exact session id or display name.</summary>
    public LiveCodeSessionManifest Find(string selector)
    {
        foreach (var session in List())
        {
            if (session.SessionId == selector || session.Name == selector)
                return session;
        }

        throw new InvalidOperationException($"No running LiveCode session matches '{selector}'.");
    }

    private static LiveCodeSessionManifest? Read(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<LiveCodeSessionManifest>(
                File.ReadAllText(path),
                LiveCodeJson.Options);
        }
        catch (Exception exception) when (
            exception is IOException
            or JsonException
            or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static bool IsAlive(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
