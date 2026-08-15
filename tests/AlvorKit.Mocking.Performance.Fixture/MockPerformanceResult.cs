namespace AlvorKit;

/// <summary>Stores robust timing and current-thread allocation statistics for one case.</summary>
internal sealed record MockPerformanceResult(
    string Name,
    string Unit,
    int OperationsPerRun,
    double MedianNanosecondsPerOperation,
    double MinimumNanosecondsPerOperation,
    double MaximumNanosecondsPerOperation,
    double SpreadNanosecondsPerOperation,
    double? MedianAllocatedBytesPerOperation,
    string Notes);
