namespace AlvorKit.ECS;

internal static class EntArchStorage<T, N, A>
{
    internal static readonly int ArchFieldId;
    internal static T[][][] Data;

    static EntArchStorage()
    {
        Data = [];
        ArchFieldId = EntArchSet<A>.NewField();
        EntArchSet<A>.Handlers[ArchFieldId] = new Handler();
    }

    internal class Handler : EntArchFieldHandler
    {
        internal override void Resize(int allocatorId, int archId, int capacity)
        {
            if (Data.Length <= allocatorId)
            {
                lock (EntArchSet<A>.Lock)
                {
                    if (Data.Length <= allocatorId)
                        Array.Resize(ref Data, (int)BitOperations.RoundUpToPowerOf2((uint)(allocatorId + 1)));
                }
            }

            if (Data[allocatorId] == null || Data[allocatorId].Length <= archId)
            {
                lock (EntArchSet<A>.Lock)
                {
                    Array.Resize(ref Data[allocatorId], EntArchSet<A>.ArchCapacity);
                }
            }

            ref var archData = ref Data[allocatorId][archId];
            Array.Resize(ref archData, capacity);
        }

        internal override void Move(int allocatorId, int srcArchId, int srcRow, int dstArchId, int dstRow)
        {
            Data[allocatorId][dstArchId][dstRow] = Data[allocatorId][srcArchId][srcRow];
        }

        internal override void Clear(int allocatorId, int archId, int row)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                Data[allocatorId][archId][row] = default!;
        }
    }
}
