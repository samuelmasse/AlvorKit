namespace AlvorKit.Ranges;

/// <summary>Allocates aligned logical ranges inside a compactable backing store.</summary>
public class RangeAllocator
{
    /// <summary>The default backing-store size used when no initial size is supplied.</summary>
    public const long DefaultInitialSize = 1 << 16;

    private const long FirstUsableIndex = 1;
    private readonly Action? packCallback;
    private readonly Action<long>? resizeCallback;
    private readonly RangeAllocationList allocations = new();
    private readonly RangeFreeBlockMap freeBlocks;
    private readonly Stopwatch packWatch = new();
    private readonly Stopwatch resizeWatch = new();
    private long size;
    private long used = FirstUsableIndex;

    /// <summary>Creates a range allocator with optional pack and resize callbacks.</summary>
    public RangeAllocator(Action? packCallback = null, Action<long>? resizeCallback = null, long initialSize = DefaultInitialSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(initialSize, FirstUsableIndex);
        this.packCallback = packCallback;
        this.resizeCallback = resizeCallback;
        size = initialSize;
        freeBlocks = new(used, size - used);
    }

    /// <summary>Gets the current backing-store size in bytes.</summary>
    public long Size => size;

    /// <summary>Gets the reserved byte count, including the sentinel byte and alignment padding.</summary>
    public long Used => used;

    /// <summary>Gets the number of free blocks indexed by offset.</summary>
    public int FreeBlockCount => freeBlocks.BlockCount;

    /// <summary>Gets the number of distinct free block sizes.</summary>
    public int FreeSizeCount => freeBlocks.SizeCount;

    /// <summary>Gets the number of pooled free-block index nodes available for reuse.</summary>
    public int IndexSetPoolCount => freeBlocks.PooledSetCount;

    /// <summary>Gets the total time spent packing, in milliseconds.</summary>
    public double PackTime => packWatch.Elapsed.TotalMilliseconds;

    /// <summary>Gets the total time spent resizing, in milliseconds.</summary>
    public double ResizeTime => resizeWatch.Elapsed.TotalMilliseconds;

    /// <summary>Gets allocation slots by handle index.</summary>
    public ReadOnlySpan<RangeAllocation> AllocationSlots => allocations.Slots;

    /// <summary>Gets allocation slots as they were before the latest pack operation.</summary>
    public ReadOnlySpan<RangeAllocation> LastAllocationSlots => allocations.LastSlots;

    /// <summary>Gets the dense list of live allocation handles.</summary>
    public ReadOnlySpan<int> Allocations => allocations.Handles;

    /// <summary>Allocates or resizes a logical range handle.</summary>
    public void Alloc(ref int allocation, int alignment, long allocSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(alignment);
        ArgumentOutOfRangeException.ThrowIfNegative(allocSize);

        if (allocSize == 0)
        {
            Clear(ref allocation);
            return;
        }

        if (ReuseExistingOrFree(ref allocation, alignment, allocSize))
            return;

        var reservedSize = allocSize + MaxPadding(alignment);
        var index = TakeBlock(reservedSize);
        allocation = allocations.Add(index, allocSize, alignment);
        used += reservedSize;
    }

    /// <summary>Returns the aligned backing-store address for a logical allocation.</summary>
    public long Addr(int allocation)
    {
        ref var slot = ref allocations.Slot(allocation);
        return AlignedAddr(slot.Index, slot.Alignment);
    }

    /// <summary>Returns the aligned address at or after <paramref name="index"/>.</summary>
    public long AlignedAddr(long index, int alignment) => RangeAddress.Align(index, alignment);

    /// <summary>Frees a logical allocation handle.</summary>
    public void Free(int allocation)
    {
        if (allocation == 0)
            return;

        var slot = allocations.Remove(allocation);
        var allocationSize = ReservedSize(slot);
        freeBlocks.Merge(slot.Index, allocationSize);
        used -= allocationSize;
    }

    /// <summary>Packs live allocations toward the start of the backing store and invokes the pack callback.</summary>
    public void Pack()
    {
        packWatch.Restart();
        var index = FirstUsableIndex;
        for (var i = 0; i < allocations.Count; i++)
        {
            var handle = allocations.HandleAt(i);
            ref var allocation = ref allocations.Slot(handle);
            allocations.CaptureLast(handle);
            allocation.Index = index;
            allocation.CapacitySize = allocation.Size;
            index += ReservedSize(allocation);
        }

        used = index;
        freeBlocks.Reset(used, size - used);
        packCallback?.Invoke();
        packWatch.Stop();
    }

    /// <summary>Clears an allocation handle for a zero-byte allocation request.</summary>
    private void Clear(ref int allocation)
    {
        if (allocation != 0)
            Free(allocation);
        allocation = 0;
    }

    /// <summary>Extends the backing store and invokes the resize callback.</summary>
    private void Resize(long newSize)
    {
        resizeWatch.Restart();
        freeBlocks.Extend(size, newSize);
        size = newSize;
        resizeCallback?.Invoke(newSize);
        resizeWatch.Stop();
    }

    /// <summary>Returns a best-fit free block, packing or resizing until one exists.</summary>
    private long TakeBlock(long requiredSize)
    {
        do
        {
            if (freeBlocks.TryTakeBestFit(requiredSize, out var index))
                return index;

            if (used + requiredSize >= (size / 8) * 7)
                Resize(NextPowerOfTwo(size + requiredSize));
            else
                Pack();
        }
        while (true);
    }

    /// <summary>Returns the reserved byte count for a live allocation slot.</summary>
    private static long ReservedSize(RangeAllocation allocation) => allocation.CapacitySize + MaxPadding(allocation.Alignment);

    /// <summary>Returns the maximum padding any index can require for <paramref name="alignment"/>.</summary>
    private static long MaxPadding(int alignment) => alignment <= 1 ? 0 : alignment - 1L;

    /// <summary>Frees an existing handle when it cannot satisfy the new request.</summary>
    private bool ReuseExistingOrFree(ref int allocation, int alignment, long allocSize)
    {
        if (allocation == 0)
            return false;

        ref var current = ref allocations.Slot(allocation);
        if (current.Alignment == alignment && current.CapacitySize >= allocSize)
        {
            current.Size = allocSize;
            return true;
        }

        Free(allocation);
        return false;
    }

    /// <summary>Returns the next power of two that can hold <paramref name="value"/>.</summary>
    private static long NextPowerOfTwo(long value) => value <= 1 ? 1 : 1L << (64 - BitOperations.LeadingZeroCount((ulong)(value - 1)));
}
