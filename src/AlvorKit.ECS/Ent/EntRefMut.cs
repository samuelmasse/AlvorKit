namespace AlvorKit.ECS;

[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntRefMut : IEntMut
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Ent(EntRefMut a) => new(a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator EntMut(EntRefMut a) => new(a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRef(EntRefMut a) => new(a.obj, a.ent.Index, a.ent.Generation);

    private readonly EntObj? obj;
    private readonly EntMut ent;

    public EntHandle Handle => ent.Handle;

    public bool IsAlive => ent.IsAlive;
    internal int Index => ent.Index;
    internal int Generation => ent.Generation;
    internal int PageIndex => ent.PageIndex;
    internal int SubIndex => ent.SubIndex;
    internal EntAllocator? Allocator => ent.Allocator;
    internal EntRegView Registry => EntReg.View;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal EntRefMut(EntObj? obj, int index, int generation)
    {
        this.obj = obj;
        ent = new(index, generation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? Get<T, N>() => ent.Get<T, N>();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has<T, N>() => ent.Has<T, N>();

    /// <inheritdoc />
    public T? GetArchetypal<T, N, A>() => ent.GetArchetypal<T, N, A>();

    /// <inheritdoc />
    public bool HasArchetypal<T, N, A>() => ent.HasArchetypal<T, N, A>();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Unset<T, N>() => ent.Unset<T, N>();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);

    /// <inheritdoc />
    public void SetArchetypal<T, N, A>(in T value) => ent.SetArchetypal<T, N, A>(value);

    /// <inheritdoc />
    public bool UnsetArchetypal<T, N, A>() => ent.UnsetArchetypal<T, N, A>();

    public override string ToString() => ent.ToString();
}
