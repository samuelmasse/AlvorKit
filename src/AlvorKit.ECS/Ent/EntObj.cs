namespace AlvorKit.ECS;

/// <summary>Represents a reference-type entity whose slot is reclaimed by finalization.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public class EntObj : IEntMut
{
    /// <summary>Converts this object to a read-only value handle for the same entity generation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Ent(EntObj a) => new(a.ent.Index, a.ent.Generation);

    /// <summary>Converts this object to a mutable value handle for the same entity generation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator EntMut(EntObj a) => new(a.ent.Index, a.ent.Generation);

    /// <summary>Converts this object to a read-only reference handle that keeps the object alive.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRef(EntObj a) => new(a, a.ent.Index, a.ent.Generation);

    /// <summary>Converts this object to a mutable reference handle that keeps the object alive.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRefMut(EntObj a) => new(a, a.ent.Index, a.ent.Generation);

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

    /// <summary>Allocates an object-backed entity from the shared object allocator.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntObj()
    {
        int index = EntReg.Allocators[0].Next();
        int pageIndex = index >> EntReg.PageBits;
        int subIndex = index & EntReg.PageMask;
        int generation = ++EntReg.PageGenerations[pageIndex][subIndex];
        ent = new(index, generation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    ~EntObj()
    {
        int pageIndex = ent.Index >> EntReg.PageBits;
        int subIndex = ent.Index & EntReg.PageMask;
        ref int generation = ref EntReg.PageGenerations[pageIndex][subIndex];

        generation++;
        ent.Reset();
        generation++;
        EntReg.Allocators[EntReg.PageAllocators[pageIndex]].Add(ent.Index);
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

    /// <summary>Formats this entity and selected components for diagnostics.</summary>
    public override string ToString() => ent.ToString();
}
