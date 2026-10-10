namespace AlvorKit;

/// <summary>One explicitly selected case in an isolated child; no unrelated fixtures are constructed.</summary>
[Bench]
public class ComparisonWorkerBenchmarks : IBenchSuiteProvider
{
    private static ComparisonExperiment? selected;

    internal static void Select(ComparisonExperiment experiment) => selected = experiment;

    public BenchSuite Create()
    {
        var experiment = selected ?? throw new InvalidOperationException("A worker must select its case before running.");
        return new("AlvorKit.ECS.Bench.Comparison", [BenchNode.Measure(experiment.Id, experiment.Description, experiment.Measure)]);
    }
}
