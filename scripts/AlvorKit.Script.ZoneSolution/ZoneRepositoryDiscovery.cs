namespace AlvorKit.Script.ZoneSolution;

/// <summary>Discovers sibling repositories that own primary .slnx files.</summary>
internal static class ZoneRepositoryDiscovery
{
    /// <summary>Finds repositories included by the supplied options.</summary>
    /// <param name="options">Validated generator options.</param>
    public static IReadOnlyList<ZoneRepository> Discover(ZoneSolutionOptions options)
    {
        if (!Directory.Exists(options.ZoneRoot))
            throw new DirectoryNotFoundException($"Zone root not found: {options.ZoneRoot}");

        var repositories = Directory.EnumerateDirectories(options.ZoneRoot)
            .Select(path => TryCreateRepository(path, options.OutputPath))
            .OfType<ZoneRepository>()
            .OrderBy(repository => repository.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(repository => repository.Name, StringComparer.Ordinal)
            .ToArray();

        if (options.RepoNames.Count > 0)
            repositories = FilterRepositories(repositories, options.RepoNames, options.ZoneRoot);

        if (repositories.Length == 0)
            throw new InvalidOperationException($"No sibling repositories with primary .slnx files found under '{options.ZoneRoot}'.");

        return repositories;
    }

    /// <summary>Creates a repository descriptor when a directory has exactly one primary .slnx file.</summary>
    private static ZoneRepository? TryCreateRepository(string repositoryRoot, string outputPath)
    {
        var solutions = Directory.GetFiles(repositoryRoot, "*.slnx", SearchOption.TopDirectoryOnly)
            .Where(path => IsPrimarySolution(path, outputPath))
            .Order(StringComparer.Ordinal)
            .ToArray();

        return solutions switch
        {
            [] => null,
            [var solution] => new(Path.GetFileName(repositoryRoot), Path.GetFullPath(repositoryRoot), Path.GetFullPath(solution)),
            _ => throw new InvalidOperationException(
                $"Multiple primary .slnx files found at '{Path.GetFullPath(repositoryRoot)}': {string.Join(", ", solutions.Select(Path.GetFileName))}.")
        };
    }

    /// <summary>Returns true for source solutions, excluding generated outputs.</summary>
    private static bool IsPrimarySolution(string solutionPath, string outputPath)
    {
        var name = Path.GetFileName(solutionPath);
        return !name.EndsWith(SolutionRoot.GeneratedSolutionSuffix, StringComparison.OrdinalIgnoreCase) &&
            !PathText.SamePath(solutionPath, outputPath);
    }

    /// <summary>Restricts discovered repositories to requested names while preserving request order.</summary>
    private static ZoneRepository[] FilterRepositories(
        IReadOnlyList<ZoneRepository> repositories,
        IReadOnlyList<string> repoNames,
        string zoneRoot)
    {
        var byName = repositories.ToDictionary(repository => repository.Name, StringComparer.OrdinalIgnoreCase);
        var filtered = new List<ZoneRepository>();
        foreach (var repoName in repoNames)
        {
            if (!byName.TryGetValue(repoName, out var repository))
                throw new InvalidOperationException($"Repository '{repoName}' was not found under '{zoneRoot}'.");

            filtered.Add(repository);
        }

        return [.. filtered];
    }
}
