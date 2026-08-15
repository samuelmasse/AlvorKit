namespace AlvorKit;

/// <summary>Validated inputs for one isolated profiler-enabled child launch.</summary>
/// <param name="RepositoryRoot">Repository used as the child working directory.</param>
/// <param name="TestProject">Optional project launched through <c>dotnet test</c>.</param>
/// <param name="ExecutableProject">Optional project launched through <c>dotnet run</c>.</param>
/// <param name="Configuration">Child build configuration.</param>
/// <param name="Filter">Optional VSTest filter.</param>
/// <param name="ProfilerPath">Optional explicit native profiler path.</param>
/// <param name="Modules">Managed module allowlist.</param>
/// <param name="AllocationProfiling">Whether startup enables allocation callbacks and stack snapshots.</param>
/// <param name="Timeout">Hard child-process timeout.</param>
/// <param name="ChildArguments">Arguments forwarded to the selected child command.</param>
internal record InterceptionLaunchOptions(
    string RepositoryRoot,
    string? TestProject,
    string? ExecutableProject,
    string Configuration,
    string? Filter,
    string? ProfilerPath,
    IReadOnlyList<string> Modules,
    bool AllocationProfiling,
    TimeSpan Timeout,
    IReadOnlyList<string> ChildArguments)
{
    /// <summary>Gets whether the child is launched through <c>dotnet test</c>.</summary>
    internal bool IsTest => TestProject is not null;

    /// <summary>Gets the selected project path.</summary>
    internal string Project => TestProject ?? ExecutableProject!;
}
