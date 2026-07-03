namespace AlvorKit.Ranges;

/// <summary>Indexes free range blocks by size and offset for best-fit allocation and coalescing.</summary>
internal sealed class RangeFreeBlockMap
{
    private readonly SortedList<long, RangeFreeBlockIndexSet> blocksBySize = [];
    private readonly SortedList<long, long> sizesByIndex = [];
    private readonly Queue<RangeFreeBlockIndexSet> indexSetPool = [];

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
        if (TryReplaceConnectedSoleBlock(oldSize, diff))
            return;

        var leftFreeIndex = RangeBoundarySearch.LargestStrictlySmaller(sizesByIndex.Keys, newSize);
        var leftConnected = leftFreeIndex >= 0 &&
            sizesByIndex.Keys[leftFreeIndex] + sizesByIndex.Values[leftFreeIndex] == oldSize;

        if (leftConnected)
        {
            var leftIndex = sizesByIndex.Keys[leftFreeIndex];
            var leftSize = sizesByIndex.Values[leftFreeIndex];
            RemoveKnown(leftIndex, leftSize);
            Add(leftIndex, leftSize + diff);
        }
        else
            Add(oldSize, diff);
    }

    /// <summary>Merges a newly freed range with adjacent free blocks.</summary>
    internal void Merge(long index, long size)
    {
        if (TryMergeWithSoleBlock(index, size))
            return;

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
            RemoveKnown(leftIndex, leftSize);
        }

        if (rightConnected)
        {
            size += rightSize;
            RemoveKnown(rightIndex, rightSize);
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

    /// <summary>Removes and returns the smallest free block that can satisfy the requested size and alignment.</summary>
    internal bool TryTakeBestFit(long requiredSize, out long index)
    {
        if (TryTakeSoleBlock(requiredSize, out index))
            return true;

        var bestFit = RangeBoundarySearch.FirstGreaterOrEqual(blocksBySize.Keys, requiredSize);
        if (bestFit >= 0)
        {
            var candidateSize = blocksBySize.Keys[bestFit];
            var indexes = blocksBySize.Values[bestFit];
            index = indexes.Min;
            RemoveKnownSizeAt(bestFit, index);
            sizesByIndex.Remove(index);
            AddRemainder(index, candidateSize, requiredSize);
            return true;
        }

        index = 0;
        return false;
    }

    /// <summary>Adds <paramref name="index"/> to the size-indexed free block map.</summary>
    private void AddSizeBlock(long size, long index)
    {
        if (blocksBySize.TryGetValue(size, out var indexes))
            indexes.Add(index);
        else
        {
            var newIndexes = RentIndexSet();
            newIndexes.AddFirst(index);
            blocksBySize.Add(size, newIndexes);
        }
    }

    /// <summary>Rents a cleared index bucket.</summary>
    private RangeFreeBlockIndexSet RentIndexSet() => indexSetPool.Count > 0 ? indexSetPool.Dequeue() : new();

    /// <summary>Removes a free block when both its offset and size are already known.</summary>
    private void RemoveKnown(long index, long size)
    {
        sizesByIndex.Remove(index);
        RemoveKnownSize(size, index);
    }

    /// <summary>Removes a free block from the size index when the size is already known.</summary>
    private void RemoveKnownSize(long size, long index)
    {
        var sizeIndex = blocksBySize.IndexOfKey(size);
        RemoveKnownSizeAt(sizeIndex, index);
    }

    /// <summary>Removes a free block from a known size-index slot.</summary>
    private void RemoveKnownSizeAt(int sizeIndex, long index)
    {
        var indexes = blocksBySize.Values[sizeIndex];
        if (indexes.Count == 1)
        {
            indexes.Clear();
            blocksBySize.RemoveAt(sizeIndex);
            indexSetPool.Enqueue(indexes);
            return;
        }

        indexes.Remove(index);
        if (indexes.Count != 0)
            return;

        blocksBySize.RemoveAt(sizeIndex);
        indexes.Clear();
        indexSetPool.Enqueue(indexes);
    }

    /// <summary>Fast path for consuming the only free block.</summary>
    private bool TryTakeSoleBlock(long requiredSize, out long index)
    {
        if (sizesByIndex.Count != 1)
        {
            index = 0;
            return false;
        }

        index = sizesByIndex.Keys[0];
        var size = sizesByIndex.Values[0];
        if (size < requiredSize)
            return false;

        AddSoleRemainder(index, size, requiredSize);
        return true;
    }

    /// <summary>Replaces the sole free block with its allocation remainder.</summary>
    private void AddSoleRemainder(long index, long size, long reservedSize)
    {
        var newSize = size - reservedSize;
        if (newSize == 0)
        {
            ClearSoleBlock();
            return;
        }

        SetSoleBlock(index + reservedSize, newSize);
    }

    /// <summary>Adds the free-block remainder left after consuming from a known block.</summary>
    private void AddRemainder(long index, long size, long reservedSize)
    {
        var newSize = size - reservedSize;
        if (newSize > 0)
            Add(index + reservedSize, newSize);
    }

    /// <summary>Fast path for merging a freed block with the only existing free block.</summary>
    private bool TryMergeWithSoleBlock(long index, long size)
    {
        if (sizesByIndex.Count != 1)
            return false;

        var existingIndex = sizesByIndex.Keys[0];
        var existingSize = sizesByIndex.Values[0];
        if (existingIndex + existingSize == index)
        {
            ReplaceSoleBlock(existingIndex, existingSize + size);
            return true;
        }

        if (index + size == existingIndex)
        {
            ReplaceSoleBlock(index, size + existingSize);
            return true;
        }

        return false;
    }

    /// <summary>Fast path for extending the only free block when it touches the old tail.</summary>
    private bool TryReplaceConnectedSoleBlock(long oldSize, long addedSize)
    {
        if (sizesByIndex.Count != 1)
            return false;

        var existingIndex = sizesByIndex.Keys[0];
        var existingSize = sizesByIndex.Values[0];
        if (existingIndex + existingSize != oldSize)
            return false;

        ReplaceSoleBlock(existingIndex, existingSize + addedSize);
        return true;
    }

    /// <summary>Replaces the only free block without boundary searches.</summary>
    private void ReplaceSoleBlock(long index, long size)
    {
        SetSoleBlock(index, size);
    }

    /// <summary>Clears the only free block without a general bucket removal.</summary>
    private void ClearSoleBlock()
    {
        var indexes = blocksBySize.Values[0];
        indexes.Clear();
        blocksBySize.Clear();
        sizesByIndex.Clear();
        indexSetPool.Enqueue(indexes);
    }

    /// <summary>Reuses the only bucket for a new sole free block.</summary>
    private void SetSoleBlock(long index, long size)
    {
        var indexes = blocksBySize.Values[0];
        indexes.Clear();
        blocksBySize.Clear();
        sizesByIndex.Clear();
        indexes.AddFirst(index);
        blocksBySize.Add(size, indexes);
        sizesByIndex.Add(index, size);
    }
}
