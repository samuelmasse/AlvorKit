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
            new EntArchColumnOps<T, N, A>());
    }

    internal override Type NameType() => typeof(N);

    internal override Type ValueType() => typeof(T);

    internal override Type ArchGroupType() => typeof(A);

    internal override bool Has(Ent ent) =>
        new EntMut(ent.Index, ent.Generation).HasArchetypal<T, N, A>();

    internal override object? Get(Ent ent) =>
        new EntMut(ent.Index, ent.Generation).GetArchetypal<T, N, A>();

    internal override void EnsureCapacity(int allocId, int archId, int capacity, int count)
    {
        EnsureDirectoryCapacity(allocId, archId);

        ref var values = ref EntArchColumn<T, N, A>.Values[allocId][archId];
        values = values == null
            ? EntArchArrayPool<T>.Rent(capacity)
            : EntArchArrayPool<T>.Grow(values, capacity, count);
    }

    internal override void ReduceCapacity(int allocId, int archId, int capacity, int count)
    {
        ref var values = ref EntArchColumn<T, N, A>.Values[allocId][archId];
        if (capacity == 0)
        {
            EntArchArrayPool<T>.Return(values);
            values = null!;
            return;
        }

        values = EntArchArrayPool<T>.Reduce(values, capacity, count);
    }

    private static void EnsureDirectoryCapacity(int allocId, int archId)
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

    internal override void ClearAlloc(int allocId)
    {
        var valuesByAlloc = EntArchColumn<T, N, A>.Values;
        if ((uint)allocId >= (uint)valuesByAlloc.Length)
            return;

        var valuesByArch = valuesByAlloc[allocId];
        if (valuesByArch == null)
            return;

        foreach (var values in valuesByArch)
        {
            if (values != null)
                EntArchArrayPool<T>.Return(values);
        }

        valuesByAlloc[allocId] = null!;
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
