namespace AlvorKit.ECS;

/// <summary>Performs cold structural operations and precise registration for one exact field.</summary>
internal sealed class EntArchColumnOps<T, N, A> : EntArchColumnOps
{
    /// <summary>The graph field ID assigned when structural code first initializes this exact field.</summary>
    internal static readonly int FieldId;

    /// <summary>Registers the field without imposing a precise initializer on the hot values holder.</summary>
    static EntArchColumnOps()
    {
        // Keep registration precise and cold while EntArchColumn remains eligible for beforefieldinit hot access.
        FieldId = EntArchGraph<A>.RegisterField(
            new EntArchColumnOps<T, N, A>(),
            Unsafe.SizeOf<T>(),
            EntArchStorageClass<T, A>.Id);
    }

    internal override void Resize(int allocId, int archId, int capacity)
    {
        if (EntArchColumn<T, N, A>.Values.Length <= allocId)
        {
            lock (EntArchGraph<A>.Sync)
            {
                if (EntArchColumn<T, N, A>.Values.Length <= allocId)
                {
                    Array.Resize(
                        ref EntArchColumn<T, N, A>.Values,
                        (int)BitOperations.RoundUpToPowerOf2((uint)(allocId + 1)));
                }
            }
        }

        if (EntArchColumn<T, N, A>.Values[allocId] == null ||
            EntArchColumn<T, N, A>.Values[allocId].Length <= archId)
        {
            lock (EntArchGraph<A>.Sync)
            {
                Array.Resize(
                    ref EntArchColumn<T, N, A>.Values[allocId],
                    EntArchGraph<A>.ArchCapacity);
            }
        }

        ref var values = ref EntArchColumn<T, N, A>.Values[allocId][archId];
        Array.Resize(ref values, capacity);
    }

    internal override void Copy(int allocId, int srcArchId, int srcRow, int dstArchId, int dstRow)
    {
        var valuesByArch = EntArchColumn<T, N, A>.Values[allocId];
        valuesByArch[dstArchId][dstRow] = valuesByArch[srcArchId][srcRow];
    }

    internal override void Clear(int allocId, int archId, int row)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            EntArchColumn<T, N, A>.Values[allocId][archId][row] = default!;
    }

    internal override void AccumulateMetrics(ref EntArchMetrics metrics)
    {
        var valuesByAlloc = EntArchColumn<T, N, A>.Values;
        metrics.AddColumnArray(valuesByAlloc);

        for (int allocId = 0; allocId < valuesByAlloc.Length; allocId++)
        {
            var valuesByArch = valuesByAlloc[allocId];
            if (valuesByArch == null)
                continue;

            metrics.AddColumnArray(valuesByArch);

            for (int archId = 0; archId < valuesByArch.Length; archId++)
            {
                var values = valuesByArch[archId];
                if (values == null || values.Length == 0)
                    continue;

                metrics.ComponentBufferCount++;
                metrics.ComponentCapacity += values.LongLength;
                metrics.AddComponentArray(values, EntArchRows<A>.CountAt(allocId, archId));
            }
        }
    }
}
