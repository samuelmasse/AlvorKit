namespace AlvorKit.Script.TestCoverage;

/// <summary>Finds source modules reachable from selected test projects through project references.</summary>
internal static class ProjectReferenceDiscovery
{
    /// <summary>Returns source assembly names referenced by selected tests, including transitive references.</summary>
    public static IReadOnlyList<string> SourceAssemblyNamesForTests(string repoRoot, IReadOnlyList<string> testProjects)
    {
        var sourceProjects = ProjectDiscovery.SourceProjects(repoRoot)
            .ToDictionary(project => project.Path, project => project.AssemblyName, StringComparer.OrdinalIgnoreCase);
        var modules = new SortedSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var testProject in testProjects)
            Visit(testProject, sourceProjects, modules, visited);

        return [.. modules];
    }

    /// <summary>Returns test projects that reference at least one selected source module.</summary>
    public static IReadOnlyList<string> TestProjectsReferencingSourceModules(
        string repoRoot,
        IReadOnlyList<string> testProjects,
        IReadOnlyCollection<string> sourceModules)
    {
        var selectedModules = new HashSet<string>(sourceModules, StringComparer.Ordinal);
        return
        [
            .. testProjects
                .Where(testProject => SourceAssemblyNamesForTests(repoRoot, [testProject]).Any(selectedModules.Contains))
        ];
    }

    /// <summary>Walks a project reference graph and records source modules.</summary>
    private static void Visit(
        string project,
        IReadOnlyDictionary<string, string> sourceProjects,
        ISet<string> modules,
        ISet<string> visited)
    {
        var fullProject = Path.GetFullPath(project);
        if (!visited.Add(fullProject) || !File.Exists(fullProject))
            return;

        foreach (var reference in ProjectReferences(fullProject))
        {
            if (sourceProjects.TryGetValue(reference, out var module))
                modules.Add(module);

            Visit(reference, sourceProjects, modules, visited);
        }
    }

    /// <summary>Reads normalized project reference paths from an SDK-style project file.</summary>
    private static IEnumerable<string> ProjectReferences(string project)
    {
        var directory = Path.GetDirectoryName(project)!;
        var document = XDocument.Load(project);

        return document.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFullPath(Path.Combine(directory, NormalizeProjectPath(include!))));
    }

    /// <summary>Normalizes MSBuild project reference separators before resolving them on the current host.</summary>
    private static string NormalizeProjectPath(string path) =>
        path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
}
