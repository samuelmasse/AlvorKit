namespace AlvorKit.Script.ZoneSolution;

/// <summary>Result of an AlvorZone solution generation run.</summary>
/// <param name="OutputPath">Generated solution path.</param>
/// <param name="Changed">True when the output file was written.</param>
/// <param name="Repositories">Repositories included in the generated solution.</param>
internal sealed record ZoneSolutionResult(
    string OutputPath,
    bool Changed,
    IReadOnlyList<ZoneRepositoryResult> Repositories)
{
    /// <summary>Number of included repositories.</summary>
    public int RepositoryCount => Repositories.Count;

    /// <summary>Number of source projects read from included repository solutions.</summary>
    public int ProjectCount => Repositories.Sum(repository => repository.ProjectCount);
}

/// <summary>Projects read from one repository solution.</summary>
/// <param name="Name">Repository name.</param>
/// <param name="SolutionPath">Repository solution path.</param>
/// <param name="ProjectCount">Number of projects read from the solution.</param>
internal sealed record ZoneRepositoryResult(string Name, string SolutionPath, int ProjectCount);
