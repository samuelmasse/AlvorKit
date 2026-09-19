namespace AlvorKit;

/// <summary>Normalizes project identity and repository boundaries on the current filesystem.</summary>
internal static class SolutionPaths
{
    /// <summary>Windows paths are case insensitive; Unix paths retain their case.</summary>
    public static StringComparer Comparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    /// <summary>Checks containment by path components, not a common textual prefix.</summary>
    public static bool IsWithin(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return !Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar);
    }

    /// <summary>Formats paths consistently in generated XML on every operating system.</summary>
    public static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
}
