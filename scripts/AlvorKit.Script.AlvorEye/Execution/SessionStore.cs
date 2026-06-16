namespace AlvorKit.Script.AlvorEye;

/// <summary>Stores persistent session state under <c>out/alvoreye/sessions</c>.</summary>
internal sealed class SessionStore(string repoRoot)
{
    /// <summary>Directory containing persistent session JSON files.</summary>
    public string SessionsDirectory { get; } = Path.Combine(repoRoot, "out", "alvoreye", "sessions");

    /// <summary>Saves a session state file.</summary>
    public void Save(SessionState state)
    {
        Directory.CreateDirectory(SessionsDirectory);
        File.WriteAllText(PathFor(state.SessionId), JsonSerializer.Serialize(state, ScenarioJson.Options), Encoding.UTF8);
    }

    /// <summary>Loads a session state file.</summary>
    public SessionState Load(string sessionId)
    {
        var path = PathFor(sessionId);
        if (!File.Exists(path))
            throw new FileNotFoundException($"AlvorEye session '{sessionId}' was not found.", path);
        return JsonSerializer.Deserialize<SessionState>(File.ReadAllText(path, Encoding.UTF8), ScenarioJson.Options)
            ?? throw new InvalidOperationException($"AlvorEye session '{sessionId}' could not be read.");
    }

    /// <summary>Builds the path for a session id.</summary>
    private string PathFor(string sessionId) => Path.Combine(SessionsDirectory, $"{sessionId}.json");
}
