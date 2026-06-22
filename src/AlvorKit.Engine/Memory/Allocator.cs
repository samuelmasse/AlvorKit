namespace AlvorKit.Engine;

/// <summary>Coalescing range allocator used by shared GPU buffers.</summary>
[ExcludeFromCodeCoverage(Justification = "Faithful old-engine allocator port; it needs a dedicated fragmentation test sweep.")]
public sealed class Allocator
{
    private readonly Action packCallback;
    private readonly Action<long> resizeCallback;
    private readonly Queue<int> freeAllocations = [];
    private readonly SortedList<long, SortedSet<long>> freeSizeBlocks = [];
    private readonly SortedList<long, long> freeBlockSizesByIndex = [];
    private readonly Queue<SortedSet<long>> indexSetPool = [];
    private readonly Stopwatch packWatch = new();
    private readonly Stopwatch resizeWatch = new();
    private Allocation[] allocationSlots = [default];
    private Allocation[] lastAllocationSlots = [default];
    private int allocationSlotCount = 1;
    private int[] allocations = new int[4];
    private int allocationCount;
    private long size = 1 << 16;
    private long used = 1;

    /// <summary>Creates an allocator with callbacks for compaction and backing-store resize.</summary>
    public Allocator(Action packCallback, Action<long> resizeCallback)
    {
        this.packCallback = packCallback;
        this.resizeCallback = resizeCallback;
        freeSizeBlocks.Add(size - used, [1]);
        freeBlockSizesByIndex.Add(1, size - used);
    }

    /// <summary>Gets the backing capacity.</summary>
    public long Size => size;

    /// <summary>Gets the currently used backing size, including alignment padding.</summary>
    public long Used => used;

    /// <summary>Gets the number of free blocks indexed by position.</summary>
    public int FreeBlockCount => freeBlockSizesByIndex.Count;

    /// <summary>Gets the number of distinct free block sizes.</summary>
    public int FreeSizeCount => freeSizeBlocks.Count;

    /// <summary>Gets the number of pooled free-index sets.</summary>
    public int IndexSetPoolCount => indexSetPool.Count;

    /// <summary>Gets the latest compaction duration in milliseconds.</summary>
    public double PackTime => packWatch.Elapsed.TotalMilliseconds;

    /// <summary>Gets the latest resize duration in milliseconds.</summary>
    public double ResizeTime => resizeWatch.Elapsed.TotalMilliseconds;

    /// <summary>Gets live and free allocation slots.</summary>
    public ReadOnlySpan<Allocation> AllocationSlots => new(allocationSlots, 0, allocationSlotCount);

    /// <summary>Gets slot positions captured before the latest compaction.</summary>
    public ReadOnlySpan<Allocation> LastAllocationSlots => new(lastAllocationSlots, 0, allocationSlotCount);

    /// <summary>Gets dense live allocation handles.</summary>
    public ReadOnlySpan<int> Allocations => new(allocations, 0, allocationCount);

    /// <summary>Allocates or resizes an allocation handle.</summary>
    public void Alloc(ref int allocation, int alignment, long allocSize)
    {
        if (allocation != 0)
        {
            var current = allocationSlots[allocation];
            if (current.Alignment == alignment && current.Size >= allocSize)
                return;
            Free(allocation);
        }

        if (allocSize <= 0)
        {
            allocation = 0;
            return;
        }

        var bestFit = FindBestFit(allocSize + alignment);
        var bestFitSize = freeSizeBlocks.Keys[bestFit];
        var bestFitIndex = freeSizeBlocks.Values[bestFit].Min;
        RemoveFreeSizeBlock(bestFitSize, bestFitIndex);
        freeBlockSizesByIndex.Remove(bestFitIndex);

        allocation = GetAllocationSlot();
        allocationSlots[allocation] = new() { Index = bestFitIndex, Size = allocSize, Alignment = alignment, Rank = allocationCount };
        allocations[allocationCount++] = allocation;

        var newIndex = bestFitIndex + allocSize + alignment;
        var newSize = bestFitSize - (newIndex - bestFitIndex);
        if (newSize > 0)
        {
            AddFreeSizeBlock(newSize, newIndex);
            freeBlockSizesByIndex.Add(newIndex, newSize);
        }

        used += allocSize + alignment;
    }

    /// <summary>Returns the aligned address for an allocation handle.</summary>
    public long Addr(int allocation)
    {
        ref var slot = ref allocationSlots[allocation];
        return AlignedAddr(slot.Index, slot.Alignment);
    }

    /// <summary>Returns <paramref name="index"/> aligned to <paramref name="alignment"/>.</summary>
    public long AlignedAddr(long index, int alignment)
    {
        if (alignment == 0)
            return index;

        var remainder = index % alignment;
        return remainder == 0 ? index : index + alignment - remainder;
    }

    /// <summary>Frees an allocation handle when it is live.</summary>
    public void Free(int allocation)
    {
        if (allocation == 0)
            return;

        var slot = allocationSlots[allocation];
        var leftFreeIndex = BinarySearch.LargestStrictlySmaller(freeBlockSizesByIndex.Keys, slot.Index);
        var rightFreeIndex = BinarySearch.SmallestStrictlyLarger(freeBlockSizesByIndex.Keys, slot.Index);
        var leftConnected = leftFreeIndex >= 0 && freeBlockSizesByIndex.Keys[leftFreeIndex] + freeBlockSizesByIndex.Values[leftFreeIndex] == slot.Index;
        var rightConnected = rightFreeIndex >= 0 && freeBlockSizesByIndex.Keys[rightFreeIndex] == slot.Index + slot.Size + slot.Alignment;

        var freeIndex = slot.Index;
        var freeSize = slot.Size + slot.Alignment;
        if (leftConnected)
            RemoveFreeBlock(freeBlockSizesByIndex.Keys[leftFreeIndex], ref freeIndex, ref freeSize);
        if (rightConnected)
            RemoveFreeBlock(freeBlockSizesByIndex.Keys[rightFreeIndex], ref freeIndex, ref freeSize);

        freeBlockSizesByIndex.Add(freeIndex, freeSize);
        AddFreeSizeBlock(freeSize, freeIndex);
        RemoveAllocationSlot(allocation, slot);
        used -= slot.Size + slot.Alignment;
    }

    /// <summary>Compacts live allocations toward the start of the backing store.</summary>
    public void Pack()
    {
        packWatch.Restart();
        var index = 1L;
        for (var i = 0; i < allocationCount; i++)
        {
            ref var allocation = ref allocationSlots[allocations[i]];
            lastAllocationSlots[allocations[i]] = allocation;
            allocation.Index = index;
            index += allocation.Size + allocation.Alignment;
        }

        used = index;
        foreach (var item in freeSizeBlocks.Values)
        {
            item.Clear();
            indexSetPool.Enqueue(item);
        }

        var set = indexSetPool.Count > 0 ? indexSetPool.Dequeue() : [];
        set.Add(used);
        freeSizeBlocks.Clear();
        freeSizeBlocks.Add(size - used, set);
        freeBlockSizesByIndex.Clear();
        freeBlockSizesByIndex.Add(used, size - used);
        packCallback();
        packWatch.Stop();
    }

    private int FindBestFit(long sizeNeeded)
    {
        int bestFit;
        do
        {
            bestFit = BinarySearch.FirstGreaterOrEqual(freeSizeBlocks.Keys, sizeNeeded);
            if (bestFit < 0)
            {
                if (used + sizeNeeded >= size / 8 * 7)
                    Resize(NextPowerOfTwo(size + sizeNeeded));
                else
                    Pack();
            }
        }
        while (bestFit < 0);
        return bestFit;
    }

    private int GetAllocationSlot()
    {
        if (freeAllocations.Count > 0)
            return freeAllocations.Dequeue();

        if (allocationSlotCount == allocationSlots.Length)
        {
            Array.Resize(ref allocationSlots, allocationSlotCount * 2);
            Array.Resize(ref lastAllocationSlots, allocationSlotCount * 2);
        }

        if (allocationCount == allocations.Length)
            Array.Resize(ref allocations, allocations.Length * 2);
        return allocationSlotCount++;
    }

    private void RemoveAllocationSlot(int allocation, Allocation slot)
    {
        var lastAllocation = allocations[allocationCount - 1];
        ref var lastSlot = ref allocationSlots[lastAllocation];
        allocations[slot.Rank] = lastAllocation;
        lastSlot.Rank = slot.Rank;
        allocationSlots[allocation] = default;
        freeAllocations.Enqueue(allocation);
        allocationCount--;
    }

    private void RemoveFreeBlock(long index, ref long freeIndex, ref long freeSize)
    {
        var size = freeBlockSizesByIndex[index];
        freeBlockSizesByIndex.Remove(index);
        RemoveFreeSizeBlock(size, index);
        freeIndex = Math.Min(freeIndex, index);
        freeSize += size;
    }

    private void AddFreeSizeBlock(long newSize, long newIndex)
    {
        if (!freeSizeBlocks.TryGetValue(newSize, out var set))
        {
            set = indexSetPool.Count > 0 ? indexSetPool.Dequeue() : [];
            freeSizeBlocks.Add(newSize, set);
        }

        set.Add(newIndex);
    }

    private void RemoveFreeSizeBlock(long size, long index)
    {
        var set = freeSizeBlocks[size];
        set.Remove(index);
        if (set.Count != 0)
            return;

        freeSizeBlocks.Remove(size);
        indexSetPool.Enqueue(set);
    }

    private void Resize(long newSize)
    {
        resizeWatch.Restart();
        var diff = newSize - size;
        var leftFreeIndex = BinarySearch.LargestStrictlySmaller(freeBlockSizesByIndex.Keys, newSize);
        var leftConnected = leftFreeIndex >= 0 &&
            freeBlockSizesByIndex.Keys[leftFreeIndex] + freeBlockSizesByIndex.Values[leftFreeIndex] == size;

        if (leftConnected)
        {
            var leftIndex = freeBlockSizesByIndex.Keys[leftFreeIndex];
            var leftSize = freeBlockSizesByIndex.Values[leftFreeIndex];
            freeBlockSizesByIndex.Remove(leftIndex);
            RemoveFreeSizeBlock(leftSize, leftIndex);
            freeBlockSizesByIndex.Add(leftIndex, leftSize + diff);
            AddFreeSizeBlock(leftSize + diff, leftIndex);
        }
        else
        {
            freeBlockSizesByIndex.Add(size, diff);
            AddFreeSizeBlock(diff, size);
        }

        size = newSize;
        resizeCallback(newSize);
        resizeWatch.Stop();
    }

    private static long NextPowerOfTwo(long value) => value <= 1 ? 1 : 1L << (64 - BitOperations.LeadingZeroCount((ulong)(value - 1)));
}
