namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>One live allocation range captured for drawing.</summary>
internal readonly record struct AllocatorRangeVisual(
    int Slot,
    int Handle,
    long Index,
    long LastIndex,
    long PayloadIndex,
    long Size,
    long ReservedSize,
    long LeadingPadding,
    long TrailingPadding,
    int Alignment,
    bool Moved);

/// <summary>One free span derived from public allocator state.</summary>
internal readonly record struct AllocatorSpanVisual(long Index, long Size);

/// <summary>Allocator state captured after a scenario step.</summary>
internal sealed class AllocatorSnapshot(
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
    internal static AllocatorSnapshot Empty { get; } = new([], [], 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>Gets live allocation ranges in backing-store order.</summary>
    internal AllocatorRangeVisual[] Ranges { get; } = ranges;

    /// <summary>Gets free spans in backing-store order.</summary>
    internal AllocatorSpanVisual[] FreeSpans { get; } = freeSpans;

    /// <summary>Gets the backing-store size.</summary>
    internal long Size { get; } = size;

    /// <summary>Gets allocator used bytes.</summary>
    internal long Used { get; } = used;

    /// <summary>Gets free block count.</summary>
    internal int FreeBlockCount { get; } = freeBlockCount;

    /// <summary>Gets distinct free-size count.</summary>
    internal int FreeSizeCount { get; } = freeSizeCount;

    /// <summary>Gets reusable free-node count.</summary>
    internal int PooledNodeCount { get; } = pooledNodeCount;

    /// <summary>Gets live allocation count.</summary>
    internal int LiveCount { get; } = liveCount;

    /// <summary>Gets observed pack callback count.</summary>
    internal int PackCount { get; } = packCount;

    /// <summary>Gets observed resize callback count.</summary>
    internal int ResizeCount { get; } = resizeCount;

    /// <summary>Gets allocator-reported total pack time.</summary>
    internal double PackTime { get; } = packTime;

    /// <summary>Gets allocator-reported total resize time.</summary>
    internal double ResizeTime { get; } = resizeTime;

    /// <summary>Gets the latest operation elapsed ticks.</summary>
    internal long OperationTicks { get; } = operationTicks;

    /// <summary>Gets managed bytes allocated by the latest allocator operation.</summary>
    internal long OperationManagedBytes { get; } = operationManagedBytes;
}
