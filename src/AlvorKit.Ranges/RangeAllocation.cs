namespace AlvorKit.Ranges;

/// <summary>Describes one live range allocation in a <see cref="RangeAllocator"/>.</summary>
public struct RangeAllocation
{
    /// <summary>Gets or sets the unaligned backing-store index reserved for this allocation.</summary>
    public long Index;

    /// <summary>Gets or sets the latest requested payload size in bytes.</summary>
    public long Size;

    /// <summary>Gets or sets the retained payload capacity in bytes.</summary>
    public long CapacitySize;

    /// <summary>Gets or sets the requested byte alignment for this allocation.</summary>
    public int Alignment;

    /// <summary>Gets or sets the dense live-allocation rank used by the allocator.</summary>
    public int Rank;
}
