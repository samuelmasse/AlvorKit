namespace AlvorKit.Script.TestCoverage;

/// <summary>Normalizes document paths stored in coverage artifacts.</summary>
internal static class CoveragePath
{
    /// <summary>Converts absolute repository paths and path separators to report-friendly relative paths.</summary>
    public static string NormalizeDocument(string path, string repoRoot)
    {
        var normalizedPath = path.Replace('\\', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(normalizedPath) && normalizedPath.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase))
            normalizedPath = Path.GetRelativePath(repoRoot, normalizedPath);

        return normalizedPath.Replace('\\', '/');
    }
}
