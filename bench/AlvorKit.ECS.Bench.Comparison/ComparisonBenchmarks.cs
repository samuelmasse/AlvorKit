namespace AlvorKit;

[Bench]
public class ComparisonBenchmarks : IBenchSuiteProvider
{
    public BenchSuite Create() => new("AlvorKit.ECS.Bench.Comparison",
        [.. ComparisonExperiments.All.GroupBy(entry => entry.Scenario).Select(scenario =>
            BenchNode.Group(scenario.Key, "", [.. scenario.Select(entry =>
                BenchNode.Measure(entry.Id[(entry.Scenario.Length + 1)..], entry.Description, entry.Measure))]))]);
}
