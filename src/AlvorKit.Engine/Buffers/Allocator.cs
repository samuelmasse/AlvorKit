namespace AlvorKit.Engine;

public class Allocator
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

    public long Size => size;
    public long Used => used;
    public int FreeBlockCount => freeBlockSizesByIndex.Count;
    public int FreeSizeCount => freeSizeBlocks.Count;
    public int IndexSetPoolCount => indexSetPool.Count;
    public double PackTime => packWatch.Elapsed.TotalMilliseconds;
    public double ResizeTime => resizeWatch.Elapsed.TotalMilliseconds;
    public ReadOnlySpan<Allocation> AllocationSlots => new(allocationSlots, 0, allocationSlotCount);
    public ReadOnlySpan<Allocation> LastAllocationSlots => new(lastAllocationSlots, 0, allocationSlotCount);
    public ReadOnlySpan<int> Allocations => new(allocations, 0, allocationCount);

    public Allocator(Action packCallback, Action<long> resizeCallback)
    {
        this.packCallback = packCallback;
        this.resizeCallback = resizeCallback;
        freeSizeBlocks.Add(size - used, [1]);
        freeBlockSizesByIndex.Add(1, size - used);
    }

    public void Alloc(ref int allocation, int alignement, long allocSize)
    {
        if (allocation != 0)
        {
            var current = allocationSlots[allocation];
            if (current.Alignement == alignement && current.Size >= allocSize)
                return;
            else Free(allocation);
        }

        if (allocSize <= 0)
        {
            allocation = 0;
            return;
        }

        int bestFit;
        do
        {
            bestFit = BinarySearch.FirstGreaterOrEqual(freeSizeBlocks.Keys, allocSize + alignement);

            if (bestFit < 0)
            {
                if (used + allocSize + alignement >= (size / 8) * 7)
                    Resize(NextPowerOfTwo(size + allocSize + alignement));
                else Pack();
            }
        }
        while (bestFit < 0);

        long bestFitSize = freeSizeBlocks.Keys[bestFit];
        long bestFitIndex = freeSizeBlocks.Values[bestFit].Min;

        RemoveFreeSizeBlock(bestFitSize, bestFitIndex);
        freeBlockSizesByIndex.Remove(bestFitIndex);

        if (freeAllocations.Count > 0)
            allocation = freeAllocations.Dequeue();
        else
        {
            if (allocationSlotCount == allocationSlots.Length)
            {
                Array.Resize(ref allocationSlots, allocationSlotCount * 2);
                Array.Resize(ref lastAllocationSlots, allocationSlotCount * 2);
            }

            allocation = allocationSlotCount++;
        }

        if (allocationCount == allocations.Length)
            Array.Resize(ref allocations, allocations.Length * 2);

        allocations[allocationCount] = allocation;
        allocationSlots[allocation] = new()
        {
            Index = bestFitIndex,
            Size = allocSize,
            Alignement = alignement,
            Rank = allocationCount++,
        };

        var newIndex = bestFitIndex + allocSize + alignement;
        var newSize = bestFitSize - (newIndex - bestFitIndex);
        if (newSize > 0)
        {
            AddFreeSizeBlock(newSize, newIndex);
            freeBlockSizesByIndex.Add(newIndex, newSize);
        }

        used += allocSize + alignement;
    }

    public long Addr(int allocation)
    {
        ref var slot = ref allocationSlots[allocation];
        return AlignedAddr(slot.Index, slot.Alignement);
    }

    public long AlignedAddr(long index, int alignement)
    {
        if (alignement == 0)
            return index;

        var rem = index % alignement;
        return index + alignement - rem;
    }

    public void Free(int allocation)
    {
        if (allocation == 0)
            return;

        var allocSlot = allocationSlots[allocation];

        var leftFreeIndex = BinarySearch.LargestStrictlySmaller(freeBlockSizesByIndex.Keys, allocSlot.Index);
        var rightFreeIndex = BinarySearch.SmallestStrictlyLarger(freeBlockSizesByIndex.Keys, allocSlot.Index);

        var leftConnected = leftFreeIndex >= 0 &&
            freeBlockSizesByIndex.Keys[leftFreeIndex] + freeBlockSizesByIndex.Values[leftFreeIndex] == allocSlot.Index;
        var rightConnected = rightFreeIndex >= 0 &&
            freeBlockSizesByIndex.Keys[rightFreeIndex] == allocSlot.Index + allocSlot.Size + allocSlot.Alignement;

        if (leftConnected && rightConnected)
        {
            var leftIndex = freeBlockSizesByIndex.Keys[leftFreeIndex];
            var leftSize = freeBlockSizesByIndex.Values[leftFreeIndex];

            var rightIndex = freeBlockSizesByIndex.Keys[rightFreeIndex];
            var rightSize = freeBlockSizesByIndex.Values[rightFreeIndex];

            freeBlockSizesByIndex.Remove(leftIndex);
            RemoveFreeSizeBlock(leftSize, leftIndex);

            freeBlockSizesByIndex.Remove(rightIndex);
            RemoveFreeSizeBlock(rightSize, rightIndex);

            freeBlockSizesByIndex.Add(leftIndex, leftSize + allocSlot.Size + allocSlot.Alignement + rightSize);
            AddFreeSizeBlock(leftSize + allocSlot.Size + allocSlot.Alignement + rightSize, leftIndex);
        }
        else if (leftConnected)
        {
            var leftIndex = freeBlockSizesByIndex.Keys[leftFreeIndex];
            var leftSize = freeBlockSizesByIndex.Values[leftFreeIndex];

            freeBlockSizesByIndex.Remove(leftIndex);
            RemoveFreeSizeBlock(leftSize, leftIndex);

            freeBlockSizesByIndex.Add(leftIndex, leftSize + allocSlot.Size + allocSlot.Alignement);
            AddFreeSizeBlock(leftSize + allocSlot.Size + allocSlot.Alignement, leftIndex);
        }
        else if (rightConnected)
        {
            var rightIndex = freeBlockSizesByIndex.Keys[rightFreeIndex];
            var rightSize = freeBlockSizesByIndex.Values[rightFreeIndex];

            freeBlockSizesByIndex.Remove(rightIndex);
            RemoveFreeSizeBlock(rightSize, rightIndex);

            freeBlockSizesByIndex.Add(allocSlot.Index, allocSlot.Size + allocSlot.Alignement + rightSize);
            AddFreeSizeBlock(allocSlot.Size + allocSlot.Alignement + rightSize, allocSlot.Index);
        }
        else
        {
            freeBlockSizesByIndex.Add(allocSlot.Index, allocSlot.Size + allocSlot.Alignement);
            AddFreeSizeBlock(allocSlot.Size + allocSlot.Alignement, allocSlot.Index);
        }

        var lastAlloc = allocations[allocationCount - 1];
        ref var lastAllocSlot = ref allocationSlots[lastAlloc];
        allocations[allocSlot.Rank] = lastAlloc;
        lastAllocSlot.Rank = allocSlot.Rank;

        allocationSlots[allocation] = default;
        freeAllocations.Enqueue(allocation);
        allocationCount--;

        used -= allocSlot.Size + allocSlot.Alignement;
    }

    public void Pack()
    {
        packWatch.Restart();
        var index = 1L;
        for (var i = 0; i < allocationCount; i++)
        {
            ref var allocation = ref allocationSlots[allocations[i]];
            lastAllocationSlots[allocations[i]] = allocation;
            allocation.Index = index;
            index += allocation.Size + allocation.Alignement;
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

    private void AddFreeSizeBlock(long newSize, long newIndex)
    {
        if (freeSizeBlocks.TryGetValue(newSize, out var newSizeIndexSet))
            newSizeIndexSet.Add(newIndex);
        else
        {
            var newIndexSet = indexSetPool.Count > 0 ? indexSetPool.Dequeue() : [];
            newIndexSet.Add(newIndex);
            freeSizeBlocks.Add(newSize, newIndexSet);
        }
    }

    private void RemoveFreeSizeBlock(long size, long index)
    {
        var indexSet = freeSizeBlocks[size];
        indexSet.Remove(index);
        if (indexSet.Count == 0)
        {
            freeSizeBlocks.Remove(size);
            indexSetPool.Enqueue(indexSet);
        }
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

    public struct Allocation
    {
        public long Index;
        public long Size;
        public int Alignement;
        public int Rank;
    }
}
