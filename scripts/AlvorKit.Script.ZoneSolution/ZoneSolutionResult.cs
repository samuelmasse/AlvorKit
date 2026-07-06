namespace AlvorKit.Script.ZoneSolution;

/// <summary>Result of an AlvorZone solution generation run.</summary>
/// <param name="OutputPath">Generated solution path.</param>
/// <param name="CodeWorkspacePath">Generated VS Code workspace path.</param>
/// <param name="SolutionChanged">True when the generated solution file was written.</param>
/// <param name="CodeWorkspaceChanged">True when the generated VS Code workspace file was written.</param>
/// <param name="Repositories">Repositories included in the generated solution.</param>
/// <param name="DevSolutions">Development solutions generated for consumer repositories.</param>
internal sealed record ZoneSolutionResult(
    string OutputPath,
    string CodeWorkspacePath,
    bool SolutionChanged,
    bool CodeWorkspaceChanged,
    IReadOnlyList<ZoneRepositoryResult> Repositories,
    IReadOnlyList<ZoneDevSolutionResult> DevSolutions)
{
    /// <summary>True when any generated file was written.</summary>
    public bool Changed => SolutionChanged || CodeWorkspaceChanged || DevSolutions.Any(solution => solution.Changed);

    /// <summary>Number of included repositories.</summary>
    public int RepositoryCount => Repositories.Count;

    /// <summary>Number of source projects read from included repository solutions.</summary>
    public int ProjectCount => Repositories.Sum(repository => repository.ProjectCount);

    /// <summary>Number of generated development solutions.</summary>
    public int DevSolutionCount => DevSolutions.Count;

    /// <summary>Number of development solutions written during this run.</summary>
    public int ChangedDevSolutionCount => DevSolutions.Count(solution => solution.Changed);
}

/// <summary>Projects read from one repository solution.</summary>
/// <param name="Name">Repository name.</param>
/// <param name="SolutionPath">Repository solution path.</param>
/// <param name="ProjectCount">Number of projects read from the solution.</param>
internal sealed record ZoneRepositoryResult(string Name, string SolutionPath, int ProjectCount);

/// <summary>Generated development solution for one consumer repository.</summary>
/// <param name="RepositoryName">Consumer repository name.</param>
/// <param name="OutputPath">Generated development solution path.</param>
/// <param name="Changed">True when the generated development solution file was written.</param>
/// <param name="ConsumerProjectCount">Number of projects read from the consumer solution.</param>
/// <param name="EngineProjectCount">Number of projects read from the AlvorKit solution.</param>
internal sealed record ZoneDevSolutionResult(
    string RepositoryName,
    string OutputPath,
    bool Changed,
    int ConsumerProjectCount,
    int EngineProjectCount);
