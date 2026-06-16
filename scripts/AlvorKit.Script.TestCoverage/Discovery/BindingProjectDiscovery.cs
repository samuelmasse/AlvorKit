namespace AlvorKit.Script.TestCoverage;

/// <summary>Discovers generated binding modules and tests that reference them.</summary>
internal static class BindingProjectDiscovery
{
    /// <summary>Returns API and backend assembly names for selected native library bindings.</summary>
    public static IReadOnlyList<string> SourceAssemblyNames(string repoRoot, IReadOnlyList<string> filters) =>
        [.. BindingProjects(repoRoot, filters).Select(project => project.AssemblyName).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

    /// <summary>Returns tests whose package or project references include one of the selected binding modules.</summary>
    public static IReadOnlyList<string> TestProjectsReferencingBindingModules(
        IReadOnlyList<string> testProjects,
        IReadOnlyCollection<string> sourceModules)
    {
        var selectedModules = new HashSet<string>(sourceModules, StringComparer.OrdinalIgnoreCase);
        return
        [
            .. testProjects
                .Where(project => ProjectReferenceNames(project).Any(selectedModules.Contains))
                .Order(StringComparer.Ordinal)
        ];
    }

    /// <summary>Returns generated API and backend projects described by matching binding configuration files.</summary>
    private static IReadOnlyList<ProjectInfo> BindingProjects(string repoRoot, IReadOnlyList<string> filters)
    {
        var configs = BindingConfigs(repoRoot);
        if (filters.Count > 0)
            configs = [.. configs.Where(config => filters.Any(filter => MatchesFilter(repoRoot, config, filter)))];

        return [.. configs.SelectMany(config => ProjectsFromConfig(repoRoot, config))];
    }

    /// <summary>Finds native library bindgen configuration files under the repository native directory.</summary>
    private static IReadOnlyList<string> BindingConfigs(string repoRoot)
    {
        var nativeRoot = Path.Combine(repoRoot, "native");
        return RepositoryConfigFile.FindAll(nativeRoot, "bindgen");
    }

    /// <summary>Reads generated project paths from one bindgen configuration file.</summary>
    private static IEnumerable<ProjectInfo> ProjectsFromConfig(string repoRoot, string config)
    {
        var bindingConfig = RepositoryConfigFile.ReadPath<BindingProjectConfig>(config);
        foreach (var projectPath in new[] { bindingConfig.ApiProject, bindingConfig.BackendProject })
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                continue;

            var projectDirectory = Path.GetFullPath(Path.Combine(repoRoot, projectPath.Replace('/', Path.DirectorySeparatorChar)));
            var assemblyName = Path.GetFileName(projectDirectory);
            yield return new(Path.Combine(projectDirectory, assemblyName + ".csproj"), assemblyName);
        }
    }

    /// <summary>Returns true when a user filter names the native library, config path, or generated assembly.</summary>
    private static bool MatchesFilter(string repoRoot, string config, string filter)
    {
        var normalizedFilter = filter.Replace('\\', '/');
        var relativeConfig = RepositoryPaths.Relative(repoRoot, config);
        var libraryName = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(config))!);
        var libraryDirectory = $"native/{libraryName}";

        return string.Equals(libraryName, filter, StringComparison.OrdinalIgnoreCase)
            || string.Equals(relativeConfig, normalizedFilter, StringComparison.OrdinalIgnoreCase)
            || string.Equals(libraryDirectory, normalizedFilter, StringComparison.OrdinalIgnoreCase)
            || ProjectsFromConfig(repoRoot, config).Any(project => string.Equals(project.AssemblyName, filter, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Reads package and project reference names from one test project.</summary>
    private static IEnumerable<string> ProjectReferenceNames(string project)
    {
        if (!File.Exists(project))
            yield break;

        var document = XDocument.Load(project);
        foreach (var include in document.Descendants("PackageReference").Select(reference => reference.Attribute("Include")?.Value))
        {
            if (!string.IsNullOrWhiteSpace(include))
                yield return include;
        }

        foreach (var include in document.Descendants("ProjectReference").Select(reference => reference.Attribute("Include")?.Value))
        {
            if (!string.IsNullOrWhiteSpace(include))
                yield return Path.GetFileNameWithoutExtension(include.Replace('\\', '/'));
        }
    }

    /// <summary>Minimal bindgen config values needed to find generated binding projects.</summary>
    private sealed class BindingProjectConfig
    {
        /// <summary>Generated API project path from conf/bindgen.yml.</summary>
        public string? ApiProject { get; init; }

        /// <summary>Generated backend project path from conf/bindgen.yml.</summary>
        public string? BackendProject { get; init; }
    }
}
