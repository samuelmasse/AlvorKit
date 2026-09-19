namespace AlvorKit;

/// <summary>Finds checkout roots independently of generated solution files.</summary>
public static class RepositoryRoot
{
    /// <summary>Finds the nearest Git checkout containing a file or directory.</summary>
    public static string FindFrom(string startPath) => FindMarkedFrom(startPath, ".git", false);

    /// <summary>Finds a checkout from the working directory or the calling assembly.</summary>
    public static string FindFromCurrentProcess(Type anchor) =>
        FindFromCandidates(Candidates(anchor), ".git", false);

    /// <summary>Finds a root using an existing repository-owned marker, including source archives.</summary>
    public static string FindMarkedFrom(string startPath, string marker, bool requireResDirectory)
    {
        if (string.IsNullOrWhiteSpace(startPath))
            throw new ArgumentException("Start path must not be blank.", nameof(startPath));

        return FindFromCandidates([startPath], marker, requireResDirectory);
    }

    /// <summary>Finds a marked root from process locations without requiring generated artifacts.</summary>
    public static string FindMarkedFromCurrentProcess(string marker, Type? anchor, bool requireResDirectory) =>
        FindFromCandidates(Candidates(anchor), marker, requireResDirectory);

    /// <summary>Searches process candidates for the requested repository marker.</summary>
    internal static string FindFromCandidates(IEnumerable<string?> paths, string marker, bool requireResDirectory)
    {
        if (string.IsNullOrWhiteSpace(marker) || Path.GetFileName(marker) != marker)
            throw new ArgumentException("Repository marker must be a file or directory name.", nameof(marker));

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            var fullPath = Path.GetFullPath(path);
            var start = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath);

            for (var current = start; current is not null; current = Directory.GetParent(current)?.FullName)
            {
                var candidate = Path.Combine(current, marker);
                var marked = File.Exists(candidate) || Directory.Exists(candidate);

                if (marked && (!requireResDirectory || Directory.Exists(Path.Combine(current, "res"))))
                    return current;
            }
        }

        var description = requireResDirectory ? $"{marker} and res" : marker;
        throw new InvalidOperationException($"{description} not found above the supplied process directories.");
    }

    /// <summary>Enumerates the working directory and assembly locations in search order.</summary>
    private static IEnumerable<string?> Candidates(Type? anchor) =>
    [
        Environment.CurrentDirectory,
        anchor is null ? null : Path.GetDirectoryName(anchor.Assembly.Location),
        AppContext.BaseDirectory,
    ];
}
