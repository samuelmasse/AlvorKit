namespace AlvorKit.Script.LiveWorkspace;

/// <summary>Persists versioned live-workspace manifests and their related JSON artifacts.</summary>
internal sealed class LiveWorkspaceManifestStore
{
    /// <summary>Shared formatting and enum conventions for workspace JSON.</summary>
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>Reads and validates one manifest from its absolute file path.</summary>
    /// <param name="path">Absolute <c>session.json</c> path.</param>
    /// <returns>The validated workspace manifest.</returns>
    internal LiveWorkspaceManifest Read(string path)
    {
        var manifest = JsonSerializer.Deserialize<LiveWorkspaceManifest>(
            File.ReadAllText(path, Encoding.UTF8),
            Json) ?? throw new InvalidOperationException($"Invalid live workspace manifest: {path}");
        if (manifest.SchemaVersion != LiveWorkspaceStore.SchemaVersion)
        {
            throw new InvalidOperationException(
                $"Live workspace schema {manifest.SchemaVersion} is unsupported; " +
                $"expected {LiveWorkspaceStore.SchemaVersion}.");
        }
        return manifest;
    }

    /// <summary>Writes one value with the workspace JSON conventions.</summary>
    /// <typeparam name="T">Serialized value type.</typeparam>
    /// <param name="path">Destination file path.</param>
    /// <param name="value">Value to serialize.</param>
    internal void Write<T>(string path, T value) =>
        File.WriteAllText(path, JsonSerializer.Serialize(value, Json), Encoding.UTF8);

    /// <summary>Atomically replaces the authoritative manifest for one workspace.</summary>
    /// <param name="manifest">Updated manifest value.</param>
    internal void Save(LiveWorkspaceManifest manifest)
    {
        var path = Path.Combine(manifest.WorkspacePath, "session.json");
        var temporary = path + ".tmp";
        Write(temporary, manifest);
        File.Move(temporary, path, overwrite: true);
    }
}
