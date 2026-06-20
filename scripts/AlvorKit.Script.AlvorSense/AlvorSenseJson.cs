namespace AlvorKit.Script.AlvorSense;

/// <summary>Reads and writes the small JSON protocol used by the session mailbox.</summary>
internal static class AlvorSenseJson
{
    /// <summary>Shared serializer settings for all protocol files and CLI responses.</summary>
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    /// <summary>Serializes a protocol value to indented JSON.</summary>
    internal static string ToJson<T>(T value) => JsonSerializer.Serialize(value, Options);

    /// <summary>Reads one protocol JSON file.</summary>
    internal static T Load<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), Options) ??
        throw new InvalidOperationException($"Invalid JSON in {path}.");

    /// <summary>Writes one protocol JSON file.</summary>
    internal static void Save<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, ToJson(value), Encoding.UTF8);
    }
}
