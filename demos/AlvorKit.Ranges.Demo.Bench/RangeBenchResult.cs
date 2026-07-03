namespace AlvorKit.Ranges.Demo.Bench;

/// <summary>Stores one measured range allocator benchmark result.</summary>
public sealed record RangeBenchResult(
    string Name,
    string Unit,
    long Units,
    double BestUnitsPerSecond,
    double MeanUnitsPerSecond,
    double ManagedBytesPerUnit,
    double MeanPackCount,
    double MeanPackTimeMilliseconds,
    double MeanResizeCount,
    double MeanResizeTimeMilliseconds,
    int FinalFreeBlockCount,
    int FinalFreeSizeCount,
    int LiveRangeCount,
    long ReservedBytes,
    long RequestedBytes,
    long EstimatedPaddingBytes);
