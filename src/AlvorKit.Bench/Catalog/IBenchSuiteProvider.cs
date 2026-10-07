namespace AlvorKit;

/// <summary>Supplies suite metadata and measurement delegates before execution begins.</summary>
[Bench]
public interface IBenchSuiteProvider
{
    /// <summary>Builds the suite tree without running timed workloads.</summary>
    BenchSuite Create();
}
