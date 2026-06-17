namespace AlvorKit.ECS;

/// <summary>Represents a mutable value handle to an entity generation.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntMut : IEntMut
{
    /// <summary>Converts a mutable handle to a read-only handle for the same entity generation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Ent(EntMut a) => new(a.Index, a.Generation);

    internal readonly int Index;
    internal readonly int Generation;

    /// <summary>Gets the stable handle for this entity generation.</summary>
    public EntHandle Handle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => new(Index, Generation);
    }

    /// <summary>Returns whether this handle still points at a live entity generation.</summary>
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

    /// <summary>Gets the component value for the requested value and marker types, or the default value when absent.</summary>
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

    /// <summary>Returns whether this entity currently has the requested component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has<T, N>()
    {
        if (!IsAlive)
            return false;

        if (!FetchPage<T, N>(out var page))
            return false;

        return page[SubIndex].Generation == Generation;
    }

    /// <summary>Sets the component value for the requested value and marker types.</summary>
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

    /// <summary>Removes the requested component and returns whether it was present.</summary>
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

    /// <summary>Formats this entity and selected components for diagnostics.</summary>
    public override string ToString() => Handle.ToString();
}
