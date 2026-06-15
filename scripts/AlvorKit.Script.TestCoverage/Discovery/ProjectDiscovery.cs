namespace AlvorKit.Script.TestCoverage;

/// <summary>Discovers source and test projects that participate in coverage measurement.</summary>
internal static class ProjectDiscovery
{
    /// <summary>Returns all test project paths under the repository tests directory.</summary>
    public static IReadOnlyList<string> TestProjects(string repoRoot, IReadOnlyList<string> filters)
    {
        var projects = FindProjects(Path.Combine(repoRoot, "tests"));

        return filters.Count == 0
            ? projects
            : [.. projects.Where(project => filters.Any(filter => MatchesFilter(repoRoot, project, filter)))];
    }

    /// <summary>Returns source assembly names under src and scripts.</summary>
    public static IReadOnlyList<string> SourceAssemblyNames(string repoRoot)
    {
        string[] sourceRoots = ["src", "scripts"];

        return sourceRoots
            .Select(path => Path.Combine(repoRoot, path))
            .Where(Directory.Exists)
            .SelectMany(root => Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray()!;
    }

    /// <summary>Returns source project paths and assembly names under src and scripts.</summary>
    public static IReadOnlyList<ProjectInfo> SourceProjects(string repoRoot)
    {
        string[] sourceRoots = ["src", "scripts"];

        return
        [
            .. sourceRoots
                .Select(path => Path.Combine(repoRoot, path))
                .Where(Directory.Exists)
                .SelectMany(root => Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories))
                .Select(project => new ProjectInfo(Path.GetFullPath(project), Path.GetFileNameWithoutExtension(project)))
                .OrderBy(project => project.Path, StringComparer.Ordinal)
        ];
    }

    /// <summary>Finds project files under a root, or returns an empty set when the root is absent.</summary>
    private static IReadOnlyList<string> FindProjects(string root) =>
        Directory.Exists(root)
            ? [.. Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories).Order(StringComparer.Ordinal)]
            : [];

    /// <summary>Returns true when a project matches a user supplied name or path filter.</summary>
    private static bool MatchesFilter(string repoRoot, string project, string filter)
    {
        var fullPath = Path.GetFullPath(Path.Combine(repoRoot, filter));
        var relativePath = RepositoryPaths.Relative(repoRoot, project);
        var normalizedFilter = filter.Replace('\\', '/');

        return string.Equals(project, fullPath, StringComparison.OrdinalIgnoreCase)
            || string.Equals(relativePath, normalizedFilter, StringComparison.OrdinalIgnoreCase)
            || string.Equals(Path.GetFileName(project), filter, StringComparison.OrdinalIgnoreCase)
            || string.Equals(Path.GetFileNameWithoutExtension(project), filter, StringComparison.OrdinalIgnoreCase);
    }
}
