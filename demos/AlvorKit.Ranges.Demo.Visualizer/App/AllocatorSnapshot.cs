namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>One live allocation range captured for drawing.</summary>
public readonly record struct AllocatorRangeVisual(
    int Slot,
    int Handle,
    long Index,
    long PayloadIndex,
    long Size,
    long CapacitySize,
    long RetainedExtraSize,
    long ReservedSize,
    long LeadingPadding,
    long TrailingPadding,
    int Alignment);

/// <summary>One free span derived from public allocator state.</summary>
public readonly record struct AllocatorSpanVisual(long Index, long Size);

/// <summary>Allocator state captured after a scenario step.</summary>
public class AllocatorSnapshot(
    AllocatorRangeVisual[] ranges,
    AllocatorSpanVisual[] freeSpans,
    long size,
    long used,
    int freeBlockCount,
    int freeSizeCount,
    int pooledNodeCount,
    int liveCount,
    int packCount,
    int resizeCount,
    double packTime,
    double resizeTime,
    long operationTicks,
    long operationManagedBytes)
{
    /// <summary>An empty snapshot used before the first scenario loads.</summary>
    public static AllocatorSnapshot Empty { get; } = new([], [], 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>Gets live allocation ranges in backing-store order.</summary>
    public AllocatorRangeVisual[] Ranges { get; } = ranges;

    /// <summary>Gets free spans in backing-store order.</summary>
    public AllocatorSpanVisual[] FreeSpans { get; } = freeSpans;

    /// <summary>Gets the backing-store size.</summary>
    public long Size { get; } = size;

    /// <summary>Gets allocator used bytes.</summary>
    public long Used { get; } = used;

    /// <summary>Gets free block count.</summary>
    public int FreeBlockCount { get; } = freeBlockCount;

    /// <summary>Gets distinct free-size count.</summary>
    public int FreeSizeCount { get; } = freeSizeCount;

    /// <summary>Gets reusable free-node count.</summary>
    public int PooledNodeCount { get; } = pooledNodeCount;

    /// <summary>Gets live allocation count.</summary>
    public int LiveCount { get; } = liveCount;

    /// <summary>Gets observed pack callback count.</summary>
    public int PackCount { get; } = packCount;

    /// <summary>Gets observed resize callback count.</summary>
    public int ResizeCount { get; } = resizeCount;

    /// <summary>Gets allocator-reported total pack time.</summary>
    public double PackTime { get; } = packTime;

    /// <summary>Gets allocator-reported total resize time.</summary>
    public double ResizeTime { get; } = resizeTime;

    /// <summary>Gets the latest operation elapsed ticks.</summary>
    public long OperationTicks { get; } = operationTicks;

    /// <summary>Gets managed bytes allocated by the latest allocator operation.</summary>
    public long OperationManagedBytes { get; } = operationManagedBytes;
}
