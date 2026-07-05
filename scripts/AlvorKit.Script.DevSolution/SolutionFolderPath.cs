namespace AlvorKit.Script.DevSolution;

/// <summary>Parses and formats .slnx solution-folder paths.</summary>
internal static class SolutionFolderPath
{
    /// <summary>Parses a solution folder name into path segments.</summary>
    /// <param name="name">Folder name such as <c>Engine</c> or <c>/Engine/Demos/</c>.</param>
    public static IReadOnlyList<string> ParseSegments(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Solution folder must not be blank.", nameof(name));

        var segments = name
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
            throw new ArgumentException("Solution folder must contain at least one path segment.", nameof(name));

        foreach (var segment in segments)
        {
            if (segment is "." or "..")
                throw new ArgumentException("Solution folder segments must not be traversal markers.", nameof(name));
        }

        return segments;
    }

    /// <summary>Returns true when a folder name is rooted in the solution folder tree.</summary>
    /// <param name="name">Folder name to inspect.</param>
    public static bool IsRootedName(string name)
    {
        var trimmed = name.TrimStart();
        return trimmed.StartsWith("/", StringComparison.Ordinal) || trimmed.StartsWith("\\", StringComparison.Ordinal);
    }

    /// <summary>Combines two segment lists.</summary>
    /// <param name="left">Leading path segments.</param>
    /// <param name="right">Trailing path segments.</param>
    public static IReadOnlyList<string> Combine(IReadOnlyList<string> left, IReadOnlyList<string> right) =>
        [.. left, .. right];

    /// <summary>Formats path segments as a rooted .slnx folder name.</summary>
    /// <param name="segments">Folder path segments.</param>
    public static string FormatName(IReadOnlyList<string> segments) =>
        "/" + string.Join("/", segments) + "/";
}
