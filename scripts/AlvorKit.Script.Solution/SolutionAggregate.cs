namespace AlvorKit;

/// <summary>Combines evaluated repository graphs into one optional solution with shared projects included once.</summary>
internal static class SolutionAggregate
{
    /// <summary>Unions configuration membership and leaves ambiguous cross-repository startup selection to the IDE.</summary>
    internal static bool Publish(string output, IReadOnlyList<SolutionGeneration> generations, bool check)
    {
        var projects = generations.SelectMany(generation => generation.Projects)
            .GroupBy(project => project.Path, SolutionPaths.Comparer)
            .Select(group => new SolutionProject(group.Key, group.Any(project => project.Startup),
                group.Any(project => project.Executable),
                group.SelectMany(project => project.Configurations).ToHashSet(StringComparer.Ordinal)))
            .ToArray();

        if (projects.Count(project => project.Startup) > 1)
            projects = [.. projects.Select(project => project with { Startup = false })];

        var roots = generations.Select(generation => generation.Root).OrderByDescending(root => root.Length).ToArray();
        var content = SolutionDocument.Format(Path.GetDirectoryName(output)!, projects, path => Folder(roots, path));
        return SolutionOutput.Publish(output, content, projects.Length, check);
    }

    /// <summary>Groups each project beneath its owning checkout, retaining the repository's source-area presentation.</summary>
    private static string Folder(IReadOnlyList<string> roots, string path)
    {
        var owner = roots.FirstOrDefault(root => SolutionPaths.IsWithin(root, path));

        if (owner == null)
            return "Dependencies";

        var name = Path.GetFileName(Path.TrimEndingDirectorySeparator(owner));
        return (name + "/" + SolutionDocument.ProjectDirectory(SolutionPaths.Relative(owner, path))).TrimEnd('/');
    }
}
