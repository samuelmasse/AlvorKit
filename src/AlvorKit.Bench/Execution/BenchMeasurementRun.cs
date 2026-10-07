namespace AlvorKit;

/// <summary>Keeps warmups separate from retained samples and their baseline-relative mean.</summary>
public record BenchMeasurementRun(
    BenchResult[] Warmups,
    BenchResult[] Samples,
    BenchResult Summary,
    double? BaselineMultiplier);
