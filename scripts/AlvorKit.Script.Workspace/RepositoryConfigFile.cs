using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AlvorKit.Script.Workspace;

/// <summary>Finds and reads repository-owned configuration files written as YAML or transitional JSON.</summary>
public static class RepositoryConfigFile
{
    /// <summary>Supported config file extensions in preferred order.</summary>
    private static readonly string[] Extensions = [".yml", ".yaml", ".json"];

    /// <summary>Shared serializer options for transitional repository-owned JSON config files.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>Shared deserializer for repository-owned YAML config files.</summary>
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithCaseInsensitivePropertyMatching()
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>Finds a config file by directory and stem, returning null when no supported file exists.</summary>
    public static string? Find(string directory, string stem)
    {
        var matches = Extensions
            .Select(extension => Path.Combine(directory, stem + extension))
            .Where(File.Exists)
            .ToArray();

        return SingleMatch(directory, stem, matches);
    }

    /// <summary>Finds every supported config file with the requested stem under a root directory.</summary>
    public static IReadOnlyList<string> FindAll(string root, string stem)
    {
        if (!Directory.Exists(root))
            return [];

        return
        [
            .. Directory
                .EnumerateFiles(root, stem + ".*", SearchOption.AllDirectories)
                .Where(path => IsSupportedConfig(path, stem))
                .GroupBy(path => Path.GetDirectoryName(path)!, StringComparer.OrdinalIgnoreCase)
                .Select(group => SingleMatch(group.Key, stem, group.Order(StringComparer.Ordinal).ToArray())!)
                .Order(StringComparer.Ordinal)
        ];
    }

    /// <summary>Reads a supported config file found by directory and stem.</summary>
    public static T Read<T>(string directory, string stem) =>
        ReadPath<T>(Find(directory, stem) ?? throw new FileNotFoundException($"Could not find {stem}.yml under {directory}."));

    /// <summary>Reads a supported config file from an explicit path.</summary>
    public static T ReadPath<T>(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".yml" or ".yaml" => Required(YamlDeserializer.Deserialize<T>(File.ReadAllText(path)), path),
            ".json" => Required(JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions), path),
            _ => throw new InvalidOperationException($"Unsupported repository config file '{path}'.")
        };

    /// <summary>Returns whether a path has the requested stem and a supported extension.</summary>
    private static bool IsSupportedConfig(string path, string stem) =>
        string.Equals(Path.GetFileNameWithoutExtension(path), stem, StringComparison.OrdinalIgnoreCase)
        && Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the single matching config path, or fails when a directory has competing formats.</summary>
    private static string? SingleMatch(string directory, string stem, IReadOnlyList<string> matches) =>
        matches.Count switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidOperationException(
                $"Multiple config files for {stem} under {directory}: {string.Join(", ", matches.Select(Path.GetFileName))}. Keep only one.")
        };

    /// <summary>Rejects null config documents with the same clear error shape as the previous JSON reader.</summary>
    private static T Required<T>(T? value, string path) =>
        value ?? throw new InvalidOperationException($"Could not read {path}.");
}
