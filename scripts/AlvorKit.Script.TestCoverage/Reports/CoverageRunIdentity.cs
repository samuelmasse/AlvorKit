namespace AlvorKit.Script.TestCoverage;

/// <summary>Creates stable, filesystem-safe identifiers for coverage runs.</summary>
internal static class CoverageRunIdentity
{
    /// <summary>Creates a timestamped run ID with the process ID and the primary coverage filter.</summary>
    public static string Create(DateTimeOffset started, CoverageOptions options)
    {
        var timestamp = started.UtcDateTime.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture);
        return $"{timestamp}-{Environment.ProcessId}-{FilterSlug(options)}";
    }

    /// <summary>Returns a short slug describing the coverage selection.</summary>
    private static string FilterSlug(CoverageOptions options)
    {
        var filter = FirstFilter(options.SourceProjectFilters)
            ?? FirstFilter(options.BindingFilters)
            ?? FirstFilter(options.TestProjectFilters)
            ?? "all";
        var slug = Sanitize(filter).Trim('-');

        return slug.Length switch
        {
            0 => "all",
            > 48 => slug[..48].Trim('-'),
            _ => slug,
        };
    }

    /// <summary>Returns the first configured filter value, including a count suffix when more are present.</summary>
    private static string? FirstFilter(IReadOnlyList<string> filters) =>
        filters.Count switch
        {
            0 => null,
            1 => FilterName(filters[0]),
            _ => $"{FilterName(filters[0])}-plus-{filters.Count - 1}",
        };

    /// <summary>Returns a readable filter name without treating dotted project names as extensions.</summary>
    private static string FilterName(string value)
    {
        var normalized = value.Replace('\\', '/');
        var fileName = Path.GetFileName(normalized);

        if (IsBindgenConfig(fileName) && LibraryNameFromConfigPath(normalized) is { } libraryName)
            return libraryName;

        return Path.GetExtension(fileName) is ".csproj" or ".json" or ".yml" or ".yaml"
            ? Path.GetFileNameWithoutExtension(fileName)
            : fileName;
    }

    /// <summary>Returns whether the filter names a bindgen config file.</summary>
    private static bool IsBindgenConfig(string fileName) =>
        Path.GetFileNameWithoutExtension(fileName).Equals("bindgen", StringComparison.OrdinalIgnoreCase)
        && Path.GetExtension(fileName) is ".json" or ".yml" or ".yaml";

    /// <summary>Returns the native library name from a conventional native library config path.</summary>
    private static string? LibraryNameFromConfigPath(string normalized)
    {
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 3 && parts[^2].Equals("conf", StringComparison.OrdinalIgnoreCase)
            ? parts[^3]
            : null;
    }

    /// <summary>Replaces characters that are inconvenient or unsafe inside a directory name.</summary>
    private static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
            builder.Append(IsSafeSlugCharacter(character) ? character : '-');

        return builder.ToString();
    }

    /// <summary>Returns true for the small ASCII set allowed in generated run IDs.</summary>
    private static bool IsSafeSlugCharacter(char value) =>
        value is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') or '.' or '_' or '-';
}
