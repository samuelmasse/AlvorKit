namespace AlvorKit;

/// <summary>Exports retained means and signed elapsed-time differences relative to the baseline.</summary>
public record BenchJsonSummary(
    double MeanElapsedNanoseconds,
    int OperationCount,
    double OperationsPerSecond,
    double? BaselineMultiplier,
    double? BaselinePercentDifference,
    BenchMemoryUsage? MeanUnmanagedMemory,
    BenchJsonAllocations MeanAllocations);
