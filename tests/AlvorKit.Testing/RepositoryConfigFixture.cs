namespace AlvorKit.Testing;

/// <summary>Writes simple repository config fixtures used by tests.</summary>
public static class RepositoryConfigFixture
{
    /// <summary>Writes a flat YAML mapping with single-quoted scalar values.</summary>
    public static void WriteYamlMapping(string path, params (string Key, string? Value)[] entries) =>
        File.WriteAllText(path, YamlMapping(entries));

    /// <summary>Formats a flat YAML mapping with single-quoted scalar values.</summary>
    public static string YamlMapping(params (string Key, string? Value)[] entries) =>
        string.Join(Environment.NewLine, entries.Select(entry => $"{entry.Key}: {YamlScalar(entry.Value)}")) + Environment.NewLine;

    /// <summary>Formats a YAML scalar without treating Windows path separators as escapes.</summary>
    public static string YamlScalar(string? value) =>
        value is null ? "null" : "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";
}
