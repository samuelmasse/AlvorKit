namespace AlvorKit.Ranges;

/// <summary>Stores sorted free-block indexes for one free-block size without per-index node allocation.</summary>
internal sealed class RangeFreeBlockIndexSet
{
    private long[] indexes = new long[4];
    private int start;
    private int count;

    /// <summary>Gets the number of indexed free blocks.</summary>
    internal int Count => count;

    /// <summary>Gets the smallest free-block index in this size bucket.</summary>
    internal long Min => ValueAt(0);

    /// <summary>Adds a free-block index while preserving ascending order.</summary>
    internal void Add(long index)
    {
        EnsureCapacity(count + 1);
        if (count == 0)
        {
            AddFirst(index);
            return;
        }

        if (index < ValueAt(0))
        {
            start = Previous(start);
            indexes[start] = index;
            count++;
            return;
        }

        if (index > ValueAt(count - 1))
        {
            SetAt(count, index);
            count++;
            return;
        }

        var insertAt = FirstGreaterOrEqual(index);
        for (var i = count; i > insertAt; i--)
            SetAt(i, ValueAt(i - 1));

        SetAt(insertAt, index);
        count++;
    }

    /// <summary>Clears indexed free blocks while retaining the backing storage for reuse.</summary>
    internal void Clear()
    {
        start = 0;
        count = 0;
    }

    /// <summary>Adds the first index to an empty bucket.</summary>
    internal void AddFirst(long index)
    {
        start = 0;
        indexes[0] = index;
        count = 1;
    }

    /// <summary>Removes a known free-block index.</summary>
    internal void Remove(long index)
    {
        if (count == 1 && indexes[start] == index)
        {
            count = 0;
            start = 0;
            return;
        }

        var removeAt = FirstGreaterOrEqual(index);
        if (removeAt < 0 || ValueAt(removeAt) != index)
            throw new InvalidOperationException("Free block index is missing from the size bucket.");

        if (removeAt == 0)
        {
            start = Next(start);
            count--;
            return;
        }

        count--;
        for (var i = removeAt; i < count; i++)
            SetAt(i, ValueAt(i + 1));
    }

    /// <summary>Returns the first index in this bucket that can fit the requested size and alignment.</summary>
    internal bool TryGetFirstFit(long blockSize, long allocSize, int alignment, out long index, out long padding)
    {
        if (count == 1)
        {
            index = indexes[start];
            padding = alignment <= 1 ? 0 : RangeAddress.Padding(index, alignment);
            if (blockSize >= allocSize + padding)
                return true;

            index = 0;
            padding = 0;
            return false;
        }

        if (alignment <= 1)
        {
            index = indexes[start];
            padding = 0;
            return true;
        }

        for (var i = 0; i < count; i++)
        {
            var candidate = ValueAt(i);
            var candidatePadding = RangeAddress.Padding(candidate, alignment);
            if (blockSize < allocSize + candidatePadding)
                continue;

            index = candidate;
            padding = candidatePadding;
            return true;
        }

        index = 0;
        padding = 0;
        return false;
    }

    /// <summary>Ensures the bucket can hold <paramref name="capacity"/> indexes.</summary>
    private void EnsureCapacity(int capacity)
    {
        if (indexes.Length >= capacity)
            return;

        var newIndexes = new long[indexes.Length * 2];
        for (var i = 0; i < count; i++)
            newIndexes[i] = ValueAt(i);

        indexes = newIndexes;
        start = 0;
    }

    /// <summary>Returns the first logical index whose value is greater than or equal to <paramref name="value"/>.</summary>
    private int FirstGreaterOrEqual(long value)
    {
        if (count == 0)
            return -1;

        var left = 0;
        var right = count - 1;
        var result = -1;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;

            if (ValueAt(mid) >= value)
            {
                result = mid;
                right = mid - 1;
            }
            else
                left = mid + 1;
        }

        return result;
    }

    /// <summary>Returns the physical array index for a logical bucket index.</summary>
    private int PhysicalIndex(int logicalIndex) => (start + logicalIndex) % indexes.Length;

    /// <summary>Returns the previous physical array index.</summary>
    private int Previous(int index) => index == 0 ? indexes.Length - 1 : index - 1;

    /// <summary>Returns the next physical array index.</summary>
    private int Next(int index) => index == indexes.Length - 1 ? 0 : index + 1;

    /// <summary>Sets the value at a logical bucket index.</summary>
    private void SetAt(int logicalIndex, long value) => indexes[PhysicalIndex(logicalIndex)] = value;

    /// <summary>Gets the value at a logical bucket index.</summary>
    private long ValueAt(int logicalIndex) => indexes[PhysicalIndex(logicalIndex)];
}
