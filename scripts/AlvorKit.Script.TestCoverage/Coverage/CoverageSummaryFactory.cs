namespace AlvorKit.Script.TestCoverage;

/// <summary>Creates immutable report summaries from accumulated coverage items.</summary>
internal static class CoverageSummaryFactory
{
    /// <summary>Builds the coverage totals across all measured modules.</summary>
    public static TotalCoverageSummary BuildTotals(
        IEnumerable<LineCoverage> lineCoverage,
        IEnumerable<BranchCoverage> branchCoverage,
        IEnumerable<MethodCoverage> methodCoverage) =>
        new(
            BuildMetric(lineCoverage, line => line.Hits),
            BuildMetric(branchCoverage, branch => branch.Hits),
            BuildMetric(methodCoverage, method => method.Hits));

    /// <summary>Builds the coverage summary for one module.</summary>
    public static ModuleCoverageSummary BuildModule(
        string module,
        IEnumerable<LineCoverage> lineCoverage,
        IEnumerable<BranchCoverage> branchCoverage,
        IEnumerable<MethodCoverage> methodCoverage) =>
        new(
            module,
            BuildMetric(lineCoverage.Where(line => line.Module == module), line => line.Hits),
            BuildMetric(branchCoverage.Where(branch => branch.Module == module), branch => branch.Hits),
            BuildMetric(methodCoverage.Where(method => method.Module == module), method => method.Hits));

    /// <summary>Builds missing coverage details for one source document.</summary>
    public static FileCoverageSummary BuildFile(
        string document,
        IEnumerable<LineCoverage> lineCoverage,
        IEnumerable<BranchCoverage> branchCoverage,
        IEnumerable<MethodCoverage> methodCoverage)
    {
        var missingLineNumbers = lineCoverage
            .Where(line => line.Document == document && line.Hits == 0)
            .Select(line => line.Line)
            .Distinct()
            .Order()
            .ToArray();
        var missingBranchLines = branchCoverage
            .Where(branch => branch.Document == document && branch.Hits == 0)
            .Select(branch => branch.Line)
            .Distinct()
            .Order()
            .ToArray();
        var missingMethods = methodCoverage
            .Where(method => method.Document == document && method.Hits == 0)
            .Select(method => method.Method)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new(
            document,
            missingLineNumbers.Length,
            branchCoverage.Count(branch => branch.Document == document && branch.Hits == 0),
            missingMethods.Length,
            missingLineNumbers,
            missingBranchLines,
            missingMethods);
    }

    /// <summary>Builds a covered and total count for any coverage item sequence.</summary>
    private static MetricSummary BuildMetric<T>(IEnumerable<T> items, Func<T, int> hits)
    {
        var total = 0;
        var covered = 0;

        foreach (var item in items)
        {
            total++;
            if (hits(item) > 0)
                covered++;
        }

        return new(covered, total);
    }
}
