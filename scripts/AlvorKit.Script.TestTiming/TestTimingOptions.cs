namespace AlvorKit.Script.TestTiming;

/// <summary>Options that describe one guarded unit test timing run.</summary>
/// <param name="repoRoot">Repository root used as the <c>dotnet test</c> working directory.</param>
/// <param name="resultsDirectory">Directory where TRX files and timing reports are written.</param>
/// <param name="maxDuration">Maximum allowed duration for one test case.</param>
/// <param name="warnOnly">Whether slow tests should warn without changing the exit code.</param>
/// <param name="trxPath">Existing TRX file to inspect instead of running tests.</param>
/// <param name="dotNetTestArguments">Arguments forwarded after <c>dotnet test</c>.</param>
internal sealed class TestTimingOptions(
    string repoRoot,
    string resultsDirectory,
    TimeSpan maxDuration,
    bool warnOnly,
    string? trxPath,
    IEnumerable<string> dotNetTestArguments)
{
    /// <summary>The default maximum allowed duration for one test case.</summary>
    public static readonly TimeSpan DefaultMaxDuration = TimeSpan.FromSeconds(1);

    /// <summary>Returns the default <c>dotnet test</c> arguments for a repository root.</summary>
    /// <param name="repoRoot">Repository root containing the primary solution file.</param>
    public static IReadOnlyList<string> DefaultDotNetTestArguments(string repoRoot) =>
        [SolutionRoot.PrimarySolutionFileName(repoRoot), "--no-restore", "--verbosity", "minimal"];

    /// <summary>Repository root used as the <c>dotnet test</c> working directory.</summary>
    public string RepoRoot { get; } = repoRoot;

    /// <summary>Directory where TRX files and timing reports are written.</summary>
    public string ResultsDirectory { get; } = resultsDirectory;

    /// <summary>Maximum allowed duration for one test case.</summary>
    public TimeSpan MaxDuration { get; } = maxDuration;

    /// <summary>Whether slow tests should warn without changing the exit code.</summary>
    public bool WarnOnly { get; } = warnOnly;

    /// <summary>Existing TRX file to inspect instead of running tests.</summary>
    public string? TrxPath { get; } = trxPath;

    /// <summary>Arguments forwarded after <c>dotnet test</c>.</summary>
    public IReadOnlyList<string> DotNetTestArguments { get; } = dotNetTestArguments.ToArray();
}
