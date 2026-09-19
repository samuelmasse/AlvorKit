namespace AlvorKit;

/// <summary>Uses the installed SDK to resolve imported and conditional project references without building.</summary>
internal static class SolutionGraph
{
    /// <summary>Configurations supported by the repository development solutions.</summary>
    internal static IReadOnlyList<string> Configurations => ["Debug", "Release"];

    /// <summary>Registers MSBuild before entering any method whose types require its assemblies.</summary>
    public static IReadOnlyList<SolutionProject> Read(string root)
    {
        if (!MSBuildLocator.IsRegistered)
            MSBuildLocator.RegisterDefaults();

        return Evaluate(root);
    }

    /// <summary>Evaluates selected roots and their complete transitive graph for both build configurations.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IReadOnlyList<SolutionProject> Evaluate(string root)
    {
        var paths = RepositoryProjects.Discover(root);

        if (paths.Count == 0)
            throw new InvalidOperationException($"No managed projects found in the source areas of '{root}'.");

        var projects = new Dictionary<string, SolutionProject>(SolutionPaths.Comparer);

        foreach (var configuration in Configurations)
        {
            using var collection = new ProjectCollection();
            var properties = new Dictionary<string, string> { ["Configuration"] = configuration };
            var selected = paths.Where(path =>
                !collection.LoadProject(path, properties, null).GetPropertyValue("WorkspaceProject")
                    .Equals("false", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (selected.Length == 0)
                throw new InvalidOperationException($"No workspace projects are enabled for {configuration} in '{root}'.");

            var graph = new ProjectGraph(selected, properties, collection);

            foreach (var node in graph.ProjectNodes)
            {
                var instance = node.ProjectInstance;
                var path = Path.GetFullPath(instance.FullPath);

                if (!path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Unsupported workspace dependency: {path}");

                var startup = instance.GetPropertyValue("WorkspaceStartup").Equals("true", StringComparison.OrdinalIgnoreCase)
                    && SolutionPaths.IsWithin(root, path);
                var executable = instance.GetPropertyValue("OutputType") is "Exe" or "WinExe";

                if (projects.TryGetValue(path, out var existing))
                {
                    projects[path] = existing with
                    {
                        Startup = existing.Startup || startup,
                        Configurations = existing.Configurations.Append(configuration).ToHashSet(StringComparer.Ordinal),
                    };
                }
                else projects.Add(path, new(path, startup, executable, new HashSet<string> { configuration }));
            }
        }

        var result = projects.Values.OrderBy(project => project.Path, StringComparer.Ordinal).ToArray();
        var startups = result.Where(project => project.Startup).ToArray();

        if (startups.Length > 1 || startups.Any(project => !project.Executable))
            throw new InvalidOperationException("WorkspaceStartup must identify at most one repository executable.");

        if (startups.Length == 0)
        {
            var executables = result.Where(project => project.Executable && SolutionPaths.IsWithin(root, project.Path)).ToArray();

            if (executables.Length == 1)
                result = [.. result.Select(project => project == executables[0] ? project with { Startup = true } : project)];
        }

        return result;
    }
}
