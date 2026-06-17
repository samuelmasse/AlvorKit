namespace AlvorKit.ECS;

/// <summary>Represents a read-only reference handle that can keep an object-backed entity alive.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntRef : IEntRead
{
    /// <summary>Converts this reference to a read-only value handle for the same entity generation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Ent(EntRef a) => new(a.ent.Index, a.ent.Generation);

    private readonly EntObj? obj;
    private readonly EntMut ent;

    /// <summary>Gets the stable handle for this entity generation.</summary>
    public EntHandle Handle => ent.Handle;
    internal bool IsAlive => ent.IsAlive;
    internal int Index => ent.Index;
    internal int Generation => ent.Generation;
    internal int PageIndex => ent.PageIndex;
    internal int SubIndex => ent.SubIndex;
    internal EntAllocator? Allocator => ent.Allocator;
    internal EntRegView Registry => EntReg.View;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal EntRef(EntObj? obj, int index, int generation)
    {
        this.obj = obj;
        ent = new(index, generation);
    }

    /// <summary>Gets the component value for the requested value and marker types, or the default value when absent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? Get<T, N>() => ent.Get<T, N>();

    /// <summary>Returns whether this entity currently has the requested component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has<T, N>() => ent.Has<T, N>();

    /// <summary>Formats this entity and selected components for diagnostics.</summary>
    public override string ToString() => ent.ToString();
}
