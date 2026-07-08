namespace AlvorKit.ECS;

[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntMut : IEntMut
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Ent(EntMut a) => new(a.Index, a.Generation);

    internal readonly int Index;
    internal readonly int Generation;

    public EntHandle Handle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => new(Index, Generation);
    }

    public bool IsAlive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => EntReg.PageGenerations[PageIndex][SubIndex] == Generation;
    }

    internal int PageIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => Index >> EntReg.PageBits;
    }

    internal int SubIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => Index & EntReg.PageMask;
    }

    internal EntAllocator? Allocator => IsAlive ? EntReg.Allocators[EntReg.PageAllocators[PageIndex]] : null;
    internal EntRegView Registry => EntReg.View;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal EntMut(int index, int generation)
    {
        Index = index;
        Generation = generation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? Get<T, N>()
    {
        if (!IsAlive)
            return default;

        if (!FetchPage<T, N>(out var page))
            return default;

        ref var val = ref page[SubIndex];
        return val.Generation == Generation ? val.Value : default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has<T, N>()
    {
        if (!IsAlive)
            return false;

        if (!FetchPage<T, N>(out var page))
            return false;

        return page[SubIndex].Generation == Generation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Set<T, N>(in T value)
    {
        if (!IsAlive)
            return;

        if (!FetchPage<T, N>(out var page))
        {
            lock (EntStorage<T, N>.Lock)
            {
                if (!FetchPage<T, N>(out page))
                    page = CreatePage<T, N>();
            }
        }

        ref var val = ref page[SubIndex];

        if (val.Generation != Generation)
        {
            val.Generation = Generation;
            val.Value = default;
        }

        val.Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Unset<T, N>()
    {
        if (!IsAlive)
            return false;

        if (!FetchPage<T, N>(out var page))
            return false;

        ref var val = ref page[SubIndex];
        if (val.Generation == Generation)
        {
            val = default;
            return true;
        }
        else return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal void Reset()
    {
        foreach (var field in EntReg.PageRefFields.Fields(PageIndex))
            field.Reset(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal void Reset<T, N>()
    {
        if (!FetchPage<T, N>(out var page))
            return;

        page[SubIndex] = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private bool FetchPage<T, N>([NotNullWhen(true)] out (int Generation, T? Value)[]? page)
    {
        page = EntStorage<T, N>.Sparse[PageIndex];
        return page != null;
    }

    private (int Generation, T? Value)[] CreatePage<T, N>()
    {
        var page = new (int, T?)[EntReg.PageSize];
        EntStorage<T, N>.Sparse[PageIndex] = page;

        EntReg.PageFields.Add(PageIndex, EntStorage<T, N>.Field);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            EntReg.PageRefFields.Add(PageIndex, EntStorage<T, N>.Field);

        return page;
    }

    public override string ToString() => Handle.ToString();
}
