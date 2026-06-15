namespace AlvorKit.Script.TestCoverage;

/// <summary>Selected test projects and source modules for one coverage run.</summary>
/// <param name="TestProjects">Runnable test projects selected for execution.</param>
/// <param name="SourceModules">Source modules selected for coverage gating.</param>
internal sealed record CoverageSelection(
    IReadOnlyList<string> TestProjects,
    IReadOnlyList<string> SourceModules)
{
    /// <summary>Builds a focused or full coverage selection from user options.</summary>
    public static CoverageSelection FromOptions(string repoRoot, CoverageOptions options)
    {
        var sourceModules = SelectSourceModules(repoRoot, options);
        var testProjects = SelectTestProjects(repoRoot, options, sourceModules);

        if (testProjects.Count == 0)
            throw new InvalidOperationException("No test projects matched the coverage filters.");
        if (sourceModules.Count == 0)
            throw new InvalidOperationException("No source projects matched the coverage filters.");

        return new(testProjects, sourceModules);
    }

    /// <summary>Returns the source modules to gate for the selected coverage run.</summary>
    private static IReadOnlyList<string> SelectSourceModules(string repoRoot, CoverageOptions options)
    {
        if (options.SourceProjectFilters.Count > 0 || options.BindingFilters.Count > 0)
        {
            var modules = new SortedSet<string>(StringComparer.Ordinal);
            if (options.SourceProjectFilters.Count > 0)
            {
                foreach (var module in ProjectDiscovery.SourceAssemblyNames(repoRoot, options.SourceProjectFilters))
                    modules.Add(module);
            }

            if (options.BindingFilters.Count > 0)
            {
                foreach (var module in BindingProjectDiscovery.SourceAssemblyNames(repoRoot, options.BindingFilters))
                    modules.Add(module);
            }

            return [.. modules];
        }

        return options.TestProjectFilters.Count == 0
            ? ProjectDiscovery.SourceAssemblyNames(repoRoot)
            : ProjectReferenceDiscovery.SourceAssemblyNamesForTests(repoRoot, ProjectDiscovery.TestProjects(repoRoot, options.TestProjectFilters));
    }

    /// <summary>Returns the test projects to execute for the selected coverage run.</summary>
    private static IReadOnlyList<string> SelectTestProjects(
        string repoRoot,
        CoverageOptions options,
        IReadOnlyList<string> sourceModules)
    {
        if (options.TestProjectFilters.Count > 0)
            return ProjectDiscovery.TestProjects(repoRoot, options.TestProjectFilters);

        var allTests = ProjectDiscovery.TestProjects(repoRoot, []);
        if (options.SourceProjectFilters.Count == 0 && options.BindingFilters.Count == 0)
            return allTests;

        var selected = new SortedSet<string>(StringComparer.Ordinal);
        if (options.SourceProjectFilters.Count > 0)
        {
            foreach (var testProject in ProjectReferenceDiscovery.TestProjectsReferencingSourceModules(repoRoot, allTests, sourceModules))
                selected.Add(testProject);
        }

        if (options.BindingFilters.Count > 0)
        {
            foreach (var testProject in BindingProjectDiscovery.TestProjectsReferencingBindingModules(allTests, sourceModules))
                selected.Add(testProject);
        }

        return [.. selected];
    }
}
