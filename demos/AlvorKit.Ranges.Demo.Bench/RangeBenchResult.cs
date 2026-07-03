namespace AlvorKit.Ranges.Demo.Bench;

/// <summary>Stores one measured range allocator benchmark result.</summary>
public sealed record RangeBenchResult(
    string Name,
    int Operations,
    double BestAllocationsPerSecond,
    double MeanAllocationsPerSecond,
    double ManagedBytesPerAllocation);
