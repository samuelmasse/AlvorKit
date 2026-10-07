namespace AlvorKit;

/// <summary>An individually numbered warmup or retained sample, including its allocation totals.</summary>
public record BenchJsonSample(
    int Number,
    double ElapsedNanoseconds,
    int OperationCount,
    double OperationsPerSecond,
    BenchMemoryUsage? UnmanagedMemory,
    BenchJsonAllocations Allocations);
