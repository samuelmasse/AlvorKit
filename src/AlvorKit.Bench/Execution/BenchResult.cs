namespace AlvorKit;

/// <summary>A measurement with an explicit operation count and separate fixture and workload allocation totals.</summary>
public readonly record struct BenchResult(
    TimeSpan Elapsed,
    int OperationCount,
    string Unit,
    string RateFormat)
{
    /// <summary>Throughput derived from the complete workload operation count.</summary>
    public double OperationsPerSecond => OperationCount / Elapsed.TotalSeconds;
    /// <summary>Total captured native bytes, or null when the measurement supplies no snapshot.</summary>
    public long? UnmanagedBytes => UnmanagedMemory == null ? null : (long)UnmanagedMemory.TotalBytes;
    /// <summary>Current-thread bytes allocated by the entire measurement, including its fixture.</summary>
    public long AllocatedBytes { get; init; }
    /// <summary>Whole-measurement object count when native allocation profiling is enabled.</summary>
    public ulong? AllocatedObjects { get; init; }
    /// <summary>Optional owned native-memory tree captured by the measurement.</summary>
    public BenchMemoryUsage? UnmanagedMemory { get; init; }
    /// <summary>Managed bytes in the timed workload, excluding fixture preparation and disposal; null when not measured.</summary>
    public long? WorkloadAllocatedBytes { get; init; }

    /// <summary>Attaches a snapshot while preserving timing and managed allocation fields.</summary>
    public BenchResult WithUnmanagedMemory(BenchMemoryUsage memory)
    {
        return this with { UnmanagedMemory = memory };
    }
}
