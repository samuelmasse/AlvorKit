namespace AlvorKit.ECS;

internal sealed class EntArchColumnOps<T, N, A> : EntArchColumnOps
{
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
}
