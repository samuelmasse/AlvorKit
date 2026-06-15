namespace AlvorKit.Script.TestCoverage;

/// <summary>Top-level aggregated coverage report.</summary>
/// <param name="Totals">Aggregate line, branch, and method metrics.</param>
/// <param name="Modules">Measured source modules.</param>
/// <param name="UnmeasuredModules">Expected source modules that produced no coverage data.</param>
/// <param name="Files">Missing coverage details by source file.</param>
internal sealed record CoverageSummary(
    TotalCoverageSummary Totals,
    IReadOnlyList<ModuleCoverageSummary> Modules,
    IReadOnlyList<string> UnmeasuredModules,
    IReadOnlyList<FileCoverageSummary> Files)
{
    /// <summary>Returns true when every metric meets a percentage threshold.</summary>
    public bool MeetsThreshold(double threshold) =>
        Totals.Line.Percent >= threshold
        && Totals.Branch.Percent >= threshold
        && Totals.Method.Percent >= threshold;
}

/// <summary>Aggregated total coverage metrics.</summary>
/// <param name="Line">Line coverage totals.</param>
/// <param name="Branch">Branch coverage totals.</param>
/// <param name="Method">Method coverage totals.</param>
internal sealed record TotalCoverageSummary(MetricSummary Line, MetricSummary Branch, MetricSummary Method);

/// <summary>Coverage metrics for one measured assembly.</summary>
/// <param name="Name">Assembly name without file extension.</param>
/// <param name="Line">Line coverage for this assembly.</param>
/// <param name="Branch">Branch coverage for this assembly.</param>
/// <param name="Method">Method coverage for this assembly.</param>
internal sealed record ModuleCoverageSummary(string Name, MetricSummary Line, MetricSummary Branch, MetricSummary Method);

/// <summary>Missing coverage details for one source file.</summary>
/// <param name="Path">Repository-relative source path.</param>
/// <param name="MissingLines">Count of uncovered lines.</param>
/// <param name="MissingBranches">Count of uncovered branch paths.</param>
/// <param name="MissingMethods">Count of uncovered methods.</param>
/// <param name="MissingLineNumbers">Uncovered line numbers.</param>
/// <param name="MissingBranchLineNumbers">Line numbers containing uncovered branches.</param>
/// <param name="MissingMethodNames">Uncovered method signatures.</param>
internal sealed record FileCoverageSummary(
    string Path,
    int MissingLines,
    int MissingBranches,
    int MissingMethods,
    IReadOnlyList<int> MissingLineNumbers,
    IReadOnlyList<int> MissingBranchLineNumbers,
    IReadOnlyList<string> MissingMethodNames);

/// <summary>Covered and total count for one coverage metric.</summary>
/// <param name="Covered">Number of covered items.</param>
/// <param name="Total">Total number of coverable items.</param>
internal sealed record MetricSummary(int Covered, int Total)
{
    /// <summary>Number of items not covered.</summary>
    public int Missing => Total - Covered;

    /// <summary>Coverage percentage, treating an empty metric as fully covered.</summary>
    public double Percent => Total == 0 ? 100.0 : Covered * 100.0 / Total;
}

/// <summary>Execution and artifact details for one test project.</summary>
/// <param name="Name">Test project name without file extension.</param>
/// <param name="ProjectPath">Repository-relative project path.</param>
/// <param name="ExitCode">dotnet test exit code.</param>
/// <param name="Duration">Elapsed test project execution time.</param>
/// <param name="LogPath">Repository-relative captured log path.</param>
/// <param name="CoverageJsonPath">Repository-relative Coverlet JSON path.</param>
/// <param name="CoverageCoberturaPath">Repository-relative Cobertura XML path.</param>
/// <param name="CoverageLcovPath">Repository-relative LCOV path.</param>
internal sealed record TestProjectResult(
    string Name,
    string ProjectPath,
    int ExitCode,
    TimeSpan Duration,
    string LogPath,
    string CoverageJsonPath,
    string CoverageCoberturaPath,
    string CoverageLcovPath);
