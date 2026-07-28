namespace AlvorKit.LiveCode;

/// <summary>Collects structured output and caller-owned artifacts from a predefined bridge invocation.</summary>
public sealed class LiveCodeBridgeContext
{
    private const int MaximumArtifacts = 32;
    private const int MaximumArtifactBytes = 40 * 1024 * 1024;
    private readonly List<string> lines = [];
    private readonly Dictionary<string, JsonElement> values = [];
    private readonly List<LiveCodeBridgeArtifact> artifacts = [];

    /// <summary>Appends one explanatory output line.</summary>
    public void WriteLine(string line) => lines.Add(line);

    /// <summary>Records or replaces one JSON-serializable named value.</summary>
    public void Value<T>(string name, T value) =>
        values[name] = JsonSerializer.SerializeToElement(value, LiveCodeJson.Options);

    /// <summary>Adds binary output that the client can persist or display.</summary>
    public void Artifact(string name, string contentType, byte[] data)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bridge artifact name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Bridge artifact content type cannot be empty.", nameof(contentType));
        ArgumentNullException.ThrowIfNull(data);
        if (artifacts.Count >= MaximumArtifacts)
            throw new InvalidOperationException($"A bridge invocation cannot return more than {MaximumArtifacts} artifacts.");
        if (data.Length > MaximumArtifactBytes - artifacts.Sum(static x => x.Data.Length))
            throw new InvalidOperationException($"Bridge artifacts exceed {MaximumArtifactBytes} total bytes.");

        artifacts.Add(new(name, contentType, data));
    }

    internal string[] Lines() => [.. lines];

    internal Dictionary<string, JsonElement> Values() => new(values);

    internal LiveCodeBridgeArtifact[] Artifacts() => [.. artifacts];
}
