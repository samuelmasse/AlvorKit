namespace AlvorKit;

/// <summary>Executes selected cases in order and compares candidates to measured baseline means.</summary>
[Bench]
public class BenchRunner(
    BenchCommand command,
    BenchConsole output,
    BenchMeasurementRunner measurements)
{
    /// <summary>Measures each selected case once through its configured sampling runner.</summary>
    public BenchExecution Run(BenchCatalog catalog, BenchCase[] benchCases)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;
        var duration = Stopwatch.StartNew();

        if (!command.Display.Quiet)
            output.WriteSuite(catalog.SuiteName);

        CollectGarbage();
        Dictionary<string, TimeSpan> baselineMeans = [];
        var caseRuns = new BenchCaseRun[benchCases.Length];
        string[] previousSegments = [];

        for (var index = 0; index < benchCases.Length; index++)
        {
            var benchCase = benchCases[index];
            var baseline = FindBaseline(benchCase, baselineMeans);
            var caseDepth = 0;

            if (!command.Display.Quiet)
                caseDepth = output.WriteCase(benchCase, previousSegments, out previousSegments);

            var measurement = measurements.Run(benchCase.Measurement, caseDepth + 1, baseline);
            caseRuns[index] = new(benchCase, measurement);

            if (benchCase.Role == BenchCaseRole.Baseline)
                baselineMeans.Add(benchCase.Id, measurement.Summary.Elapsed);

            if (!command.Display.Quiet && EndsBenchmark(benchCases, index))
                output.WriteBlankLine();
        }

        duration.Stop();
        return new(startedAtUtc, duration.Elapsed, caseRuns);
    }

    /// <summary>Uses only a baseline already measured in this invocation.</summary>
    private TimeSpan? FindBaseline(BenchCase benchCase, Dictionary<string, TimeSpan> baselineMeans) =>
        benchCase.BaselineId is not null && baselineMeans.TryGetValue(benchCase.BaselineId, out var baseline)
            ? baseline
            : null;

    /// <summary>Separates standalone cases and comparison groups in console output.</summary>
    private bool EndsBenchmark(BenchCase[] benchCases, int index)
    {
        if (index == benchCases.Length - 1)
            return true;

        var current = benchCases[index];
        var next = benchCases[index + 1];

        if (current.Role == BenchCaseRole.Measurement || next.Role == BenchCaseRole.Measurement)
            return true;

        return ParentId(current.Id) != ParentId(next.Id);
    }

    /// <summary>Returns the group path containing a comparison leaf.</summary>
    private string ParentId(string id)
    {
        var separatorIndex = id.LastIndexOf('/');
        return separatorIndex < 0 ? "" : id[..separatorIndex];
    }

    /// <summary>Stabilizes initial GC state before measuring the selected suite.</summary>
    private void CollectGarbage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
