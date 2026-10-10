namespace AlvorKit;

/// <summary>Performs cold structural operations and precise registration for one exact field.</summary>
internal class EntArchColumnOps<T, N, A> : EntArchColumnOps
{
    private static readonly EntArchColumnOps<T, N, A> Instance = new();

    /// <summary>The graph field ID assigned when structural code first initializes this exact field.</summary>
    internal static readonly int FieldId;

    /// <summary>Registers the field without imposing a precise initializer on the hot values holder.</summary>
    static EntArchColumnOps()
    {
        // Keep registration precise and cold while EntArchColumn remains eligible for beforefieldinit hot access.
        FieldId = EntArchGraph<A>.RegisterField(Instance);
    }

    internal override Type NameType() => typeof(N);

    internal override Type ValueType() => typeof(T);

    internal override Type ArchGroupType() => typeof(A);

    internal override bool Has(Ent ent) =>
        new EntMut(ent.Index, ent.Generation).HasArchetypal<T, N, A>();

    internal override object? Get(Ent ent) =>
        new EntMut(ent.Index, ent.Generation).GetArchetypal<T, N, A>();

    internal override void EnsureCapacity(int rowSetId, int capacity, int count)
    {
        lock (this)
        {
            EnsureDirectoryCapacity(rowSetId);

            ref var values = ref EntArchColumn<T, N, A>.Values[rowSetId];
            values = values == null
                ? EntArchArrayPool<T>.Rent(capacity)
                : EntArchArrayPool<T>.Grow(values, capacity, count);
        }
    }

    internal override void ReduceCapacity(int rowSetId, int capacity, int count)
    {
        lock (this)
        {
            ref var values = ref EntArchColumn<T, N, A>.Values[rowSetId];
            if (capacity == 0)
            {
                EntArchArrayPool<T>.Return(values);
                values = null!;
                return;
            }

            values = EntArchArrayPool<T>.Reduce(values, capacity, count);
        }
    }

    private static void EnsureDirectoryCapacity(int rowSetId)
    {
        if (EntArchColumn<T, N, A>.Values.Length > rowSetId)
            return;

        Array.Resize(
            ref EntArchColumn<T, N, A>.Values,
            (int)BitOperations.RoundUpToPowerOf2((uint)(rowSetId + 1)));
    }

    /// <summary>Copies a retained field before compacting and clearing its source column.</summary>
    internal override void Move(int srcRowSetId, int srcRow, int dstRowSetId, int dstRow, int lastRow)
    {
        var valuesByRowSet = EntArchColumn<T, N, A>.Values;
        var source = valuesByRowSet[srcRowSetId];
        var destination = (uint)dstRowSetId < (uint)valuesByRowSet.Length ? valuesByRowSet[dstRowSetId] : null;

        if (destination != null)
            destination[dstRow] = source[srcRow];

        if (srcRow != lastRow)
            source[srcRow] = source[lastRow];

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            source[lastRow] = default!;
    }

    /// <summary>Compacts and clears a removed value with one directory lookup.</summary>
    internal override void Remove(int rowSetId, int row, int lastRow)
    {
        var values = EntArchColumn<T, N, A>.Values[rowSetId];

        if (row != lastRow)
            values[row] = values[lastRow];

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            values[lastRow] = default!;
    }

    internal override void ClearRowSet(int rowSetId)
    {
        lock (this)
        {
            var valuesByRowSet = EntArchColumn<T, N, A>.Values;
            if ((uint)rowSetId >= (uint)valuesByRowSet.Length)
                return;

            var values = valuesByRowSet[rowSetId];
            if (values != null)
                EntArchArrayPool<T>.Return(values);
            valuesByRowSet[rowSetId] = null!;
        }
    }

    internal override void AccumulateMetrics(ref EntArchMetrics metrics)
    {
        var valuesByRowSet = EntArchColumn<T, N, A>.Values;
        metrics.AddColumnArray(valuesByRowSet);

        for (int rowSetId = EntArchRows<A>.FirstRowSetId; rowSetId < valuesByRowSet.Length; rowSetId++)
        {
            var values = valuesByRowSet[rowSetId];
            if (values == null || values.Length == 0)
                continue;

            metrics.ComponentBufferCount++;
            metrics.ComponentCapacity += values.LongLength;
            metrics.AddComponentArray(values, EntArchRows<A>.CountAt(rowSetId));
        }
    }
}
