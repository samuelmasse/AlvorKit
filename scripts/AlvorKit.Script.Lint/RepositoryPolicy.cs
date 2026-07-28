namespace AlvorKit.Script.Lint;

/// <summary>Checks repository files against policies shared by AlvorKit and its game repositories.</summary>
internal static class RepositoryPolicy
{
    /// <summary>Directory names excluded from repository policy discovery.</summary>
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        "bin",
        "dist",
        "node_modules",
        "obj",
        "out",
        "packages",
        "tmp",
    };

    /// <summary>Returns hand-authored assembly metadata files prohibited by the shared repository policy.</summary>
    public static IReadOnlyList<string> FindAssemblyInfoFiles(string repoRoot, LintScope? scope = null)
    {
        var files = scope is null
            ? EnumerateFiles(Path.GetFullPath(repoRoot))
            : scope.AllFiles;
        return files
            .Where(file => string.Equals(Path.GetFileName(file), "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>Enumerates repository-relative files while skipping generated, dependency, and tool-output directories.</summary>
    private static IEnumerable<string> EnumerateFiles(string repoRoot)
    {
        var directories = new Stack<string>();
        directories.Push(repoRoot);

        while (directories.Count > 0)
        {
            var directory = directories.Pop();
            foreach (var childDirectory in Directory.EnumerateDirectories(directory).Where(ShouldEnterDirectory))
                directories.Push(childDirectory);
            foreach (var file in Directory.EnumerateFiles(directory))
                yield return GlobPattern.NormalizePath(Path.GetRelativePath(repoRoot, file));
        }
    }

    /// <summary>Returns true when repository policy discovery should enter a directory.</summary>
    private static bool ShouldEnterDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        return !ExcludedDirectories.Contains(Path.GetFileName(path))
            && !attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}
