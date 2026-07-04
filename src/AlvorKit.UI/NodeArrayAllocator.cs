namespace AlvorKit.UI;

internal class NodeArrayAllocator
{
    private const int MaxRank = 20;

    private readonly EntMut[][] data = new EntMut[MaxRank][];
    private readonly int[] counts = new int[MaxRank];
    private readonly Stack<int>[] free = new Stack<int>[MaxRank];

    public NodeArrayAllocator()
    {
        for (int i = 0; i < MaxRank; i++)
        {
            data[i] = new EntMut[4];
            free[i] = new Stack<int>();
        }
    }

    public NodeArray Alloc(int rank)
    {
        if (!free[rank].TryPop(out var index))
        {
            index = counts[rank]++;

            int size = counts[rank] * BlockSize(rank);
            if (data[rank].Length < size)
                Array.Resize(ref data[rank], (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)(size + 1)));
        }

        return new NodeArray(rank, index, 0, this);
    }

    public void Free(NodeArray slot)
    {
        if (slot.Rank == 0)
            return;

        free[slot.Rank].Push(slot.Index);
    }

    public Span<EntMut> Span(NodeArray slot)
    {
        var size = BlockSize(slot.Rank);
        var start = slot.Index * size;
        return data[slot.Rank].AsSpan(start, size);
    }

    public int BlockSize(int rank) => 1 << (rank + 1);
}
