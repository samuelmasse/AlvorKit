namespace AlvorKit.ECS;

/// <summary>Represents a disposable mutable entity allocated from the default allocator or an arena.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntPtr : IDisposable, IEntMut
{
    /// <summary>Converts this pointer to a read-only handle for the same entity generation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Ent(EntPtr a) => new(a.ent.Index, a.ent.Generation);

    /// <summary>Converts this pointer to a mutable value handle for the same entity generation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntMut(EntPtr a) => new(a.ent.Index, a.ent.Generation);

    /// <summary>Converts this pointer to a read-only reference handle.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRef(EntPtr a) => new(null, a.ent.Index, a.ent.Generation);

    /// <summary>Converts this pointer to a mutable reference handle.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRefMut(EntPtr a) => new(null, a.ent.Index, a.ent.Generation);

    private readonly EntMut ent;

    /// <summary>Gets the stable handle for this entity generation.</summary>
    public EntHandle Handle => ent.Handle;

    /// <summary>Returns whether this handle still points at a live entity generation.</summary>
    public bool IsAlive => ent.IsAlive;
    internal int Index => ent.Index;
    internal int Generation => ent.Generation;
    internal int PageIndex => ent.PageIndex;
    internal int SubIndex => ent.SubIndex;
    internal EntAllocator? Allocator => ent.Allocator;
    internal EntRegView Registry => EntReg.View;

    /// <summary>Allocates a disposable entity from the shared pointer allocator.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntPtr() : this(1) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal EntPtr(int allocatorIndex)
    {
        int index = EntReg.Allocators[allocatorIndex].Next();
        int pageIndex = index >> EntReg.PageBits;
        int subIndex = index & EntReg.PageMask;
        int generation = ++EntReg.PageGenerations[pageIndex][subIndex];
        ent = new(index, generation);
    }

    /// <summary>Gets the component value for the requested value and marker types, or the default value when absent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? Get<T, N>() => ent.Get<T, N>();

    /// <summary>Returns whether this entity currently has the requested component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has<T, N>() => ent.Has<T, N>();

    /// <summary>Removes the requested component and returns whether it was present.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Unset<T, N>() => ent.Unset<T, N>();

    /// <summary>Sets the component value for the requested value and marker types.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);

    /// <summary>Disposes this entity, clears reference components, and returns its slot to the allocator.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Dispose()
    {
        int pageIndex = ent.Index >> EntReg.PageBits;
        int subIndex = ent.Index & EntReg.PageMask;

        if (Interlocked.CompareExchange(ref EntReg.PageGenerations[pageIndex][subIndex], Generation + 1, Generation) == Generation)
        {
            ent.Reset();
            EntReg.PageGenerations[pageIndex][subIndex]++;
            EntReg.Allocators[EntReg.PageAllocators[pageIndex]].Add(ent.Index);
        }
    }

    /// <summary>Formats this entity and selected components for diagnostics.</summary>
    public override string ToString() => ent.ToString();
}
