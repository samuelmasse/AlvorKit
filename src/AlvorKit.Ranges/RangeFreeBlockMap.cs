namespace AlvorKit.Ranges;

/// <summary>Indexes free range blocks by size and offset for best-fit allocation and coalescing.</summary>
internal sealed class RangeFreeBlockMap
{
    private readonly SortedList<long, SortedSet<long>> blocksBySize = [];
    private readonly SortedList<long, long> sizesByIndex = [];
    private readonly Queue<SortedSet<long>> indexSetPool = [];

    /// <summary>Creates a free block map with one initial block.</summary>
    internal RangeFreeBlockMap(long index, long size) => Add(index, size);

    /// <summary>Gets the number of free blocks indexed by offset.</summary>
    internal int BlockCount => sizesByIndex.Count;

    /// <summary>Gets the number of distinct free block sizes.</summary>
    internal int SizeCount => blocksBySize.Count;

    /// <summary>Gets the number of pooled index sets available for reuse.</summary>
    internal int PooledSetCount => indexSetPool.Count;

    /// <summary>Adds a free block at <paramref name="index"/> with <paramref name="size"/> bytes.</summary>
    internal void Add(long index, long size)
    {
        sizesByIndex.Add(index, size);
        AddSizeBlock(size, index);
    }

    /// <summary>Extends the final free block when adjacent, otherwise adds a new tail block.</summary>
    internal void Extend(long oldSize, long newSize)
    {
        var diff = newSize - oldSize;
        var leftFreeIndex = RangeBoundarySearch.LargestStrictlySmaller(sizesByIndex.Keys, newSize);
        var leftConnected = leftFreeIndex >= 0 &&
            sizesByIndex.Keys[leftFreeIndex] + sizesByIndex.Values[leftFreeIndex] == oldSize;

        if (leftConnected)
        {
            var leftIndex = sizesByIndex.Keys[leftFreeIndex];
            var leftSize = sizesByIndex.Values[leftFreeIndex];
            Remove(leftIndex);
            Add(leftIndex, leftSize + diff);
        }
        else
            Add(oldSize, diff);
    }

    /// <summary>Finds the smallest free block that can satisfy <paramref name="requiredSize"/>.</summary>
    internal int FindBestFit(long requiredSize) => RangeBoundarySearch.FirstGreaterOrEqual(blocksBySize.Keys, requiredSize);

    /// <summary>Merges a newly freed range with adjacent free blocks.</summary>
    internal void Merge(long index, long size)
    {
        var leftFreeIndex = RangeBoundarySearch.LargestStrictlySmaller(sizesByIndex.Keys, index);
        var rightFreeIndex = RangeBoundarySearch.SmallestStrictlyLarger(sizesByIndex.Keys, index);
        var leftConnected = leftFreeIndex >= 0 && sizesByIndex.Keys[leftFreeIndex] + sizesByIndex.Values[leftFreeIndex] == index;
        var rightConnected = rightFreeIndex >= 0 && sizesByIndex.Keys[rightFreeIndex] == index + size;
        var leftIndex = leftConnected ? sizesByIndex.Keys[leftFreeIndex] : 0;
        var leftSize = leftConnected ? sizesByIndex.Values[leftFreeIndex] : 0;
        var rightIndex = rightConnected ? sizesByIndex.Keys[rightFreeIndex] : 0;
        var rightSize = rightConnected ? sizesByIndex.Values[rightFreeIndex] : 0;

        if (leftConnected)
        {
            index = leftIndex;
            size += leftSize;
            Remove(leftIndex);
        }

        if (rightConnected)
        {
            size += rightSize;
            Remove(rightIndex);
        }

        Add(index, size);
    }

    /// <summary>Replaces every free block with one block, usually after compaction.</summary>
    internal void Reset(long index, long size)
    {
        foreach (var item in blocksBySize.Values)
        {
            item.Clear();
            indexSetPool.Enqueue(item);
        }

        blocksBySize.Clear();
        sizesByIndex.Clear();
        Add(index, size);
    }

    /// <summary>Removes and returns the free block at a known best-fit size index.</summary>
    internal (long Index, long Size) TakeBestFit(int bestFit)
    {
        var size = blocksBySize.Keys[bestFit];
        var index = blocksBySize.Values[bestFit].Min;
        RemoveSizeBlock(size, index);
        sizesByIndex.Remove(index);
        return (index, size);
    }

    /// <summary>Adds <paramref name="index"/> to the size-indexed free block map.</summary>
    private void AddSizeBlock(long size, long index)
    {
        if (blocksBySize.TryGetValue(size, out var indexes))
            indexes.Add(index);
        else
        {
            var newIndexes = indexSetPool.Count > 0 ? indexSetPool.Dequeue() : [];
            newIndexes.Add(index);
            blocksBySize.Add(size, newIndexes);
        }
    }

    /// <summary>Removes a free block by offset from both indexes.</summary>
    private void Remove(long index)
    {
        var size = sizesByIndex[index];
        sizesByIndex.Remove(index);
        RemoveSizeBlock(size, index);
    }

    /// <summary>Removes one offset from a size-indexed free block set.</summary>
    private void RemoveSizeBlock(long size, long index)
    {
        var indexes = blocksBySize[size];
        indexes.Remove(index);
        if (indexes.Count == 0)
        {
            blocksBySize.Remove(size);
            indexSetPool.Enqueue(indexes);
        }
    }
}
