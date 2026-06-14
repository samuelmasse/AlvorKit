using System.Text.Json;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Reads repository JSON files with the relaxed settings used by manifests.</summary>
internal static class JsonFile
{
    /// <summary>Shared serializer options for repository-owned config files.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>Reads a JSON file and deserializes it to the requested type.</summary>
    public static T Read<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options)
        ?? throw new InvalidOperationException($"Could not read {path}.");
}
