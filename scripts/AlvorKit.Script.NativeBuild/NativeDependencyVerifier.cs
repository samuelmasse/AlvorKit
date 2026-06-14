namespace AlvorKit.Script.NativeBuild;

/// <summary>Parses and validates native library dependency output.</summary>
internal static class NativeDependencyVerifier
{
    /// <summary>Extracts ELF shared library names from readelf -d output.</summary>
    public static IReadOnlyList<string> ElfDependencies(string readElfOutput) =>
        readElfOutput.Split('\n')
            .Select(TryElfDependency)
            .OfType<string>()
            .ToArray();

    /// <summary>Throws when an ELF dependency is outside the platform allow-list.</summary>
    public static void EnsureElfDependenciesAllowed(IEnumerable<string> dependencies, IEnumerable<string> allowedDependencies)
    {
        var allowed = allowedDependencies.ToHashSet(StringComparer.Ordinal);
        foreach (var dependency in dependencies)
        {
            if (dependency.StartsWith("ld-linux", StringComparison.Ordinal))
                continue;
            if (!allowed.Contains(dependency))
                throw new InvalidOperationException($"Unexpected dependency: {dependency}");
        }
    }

    /// <summary>Returns one dependency name from a readelf output line, if present.</summary>
    private static string? TryElfDependency(string line)
    {
        const string marker = "Shared library: [";
        var start = line.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
            return null;
        start += marker.Length;
        var end = line.IndexOf(']', start);
        return end > start ? line[start..end] : null;
    }
}
