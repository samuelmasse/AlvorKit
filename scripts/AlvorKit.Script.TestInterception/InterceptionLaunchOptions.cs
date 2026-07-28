namespace AlvorKit.Script.TestInterception;

/// <summary>Validated inputs for one isolated profiler-enabled child launch.</summary>
internal sealed record InterceptionLaunchOptions(
    string RepositoryRoot,
    string? TestProject,
    string? ExecutableProject,
    string Configuration,
    string? Filter,
    string? ProfilerPath,
    IReadOnlyList<string> Modules,
    TimeSpan Timeout,
    IReadOnlyList<string> ChildArguments)
{
    /// <summary>Gets whether the child is launched through <c>dotnet test</c>.</summary>
    internal bool IsTest => TestProject is not null;

    /// <summary>Gets the selected project path.</summary>
    internal string Project => TestProject ?? ExecutableProject!;
}
