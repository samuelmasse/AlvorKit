namespace AlvorKit;

/// <summary>Deterministic measurements make comparison and export assertions independent of clock noise.</summary>
[Bench]
public class BenchTestSuite : IBenchSuiteProvider
{
    public BenchSuite Create() => new("Contract suite",
    [
        BenchNode.Group("Group", "parent", [
            BenchNode.Compare("Compare", "child", new(new("Base", () => Result(20)), [new("Candidate", () => Result(10))])),
            BenchNode.Measure("Read", "", () => Result(5)),
        ]),
        BenchNode.Measure("Plain", "standalone", () => Result(1)),
    ]);

    internal static BenchResult Result(int ticks) => new(TimeSpan.FromTicks(ticks), 10, "op", "N0")
    {
        WorkloadAllocatedBytes = ticks,
    };
}
