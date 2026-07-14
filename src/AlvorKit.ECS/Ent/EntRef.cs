namespace AlvorKit.ECS;

[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntRef : IEntRead
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Ent(EntRef a) => new(a.ent.Index, a.ent.Generation);

    private readonly EntObj? obj;
    private readonly EntMut ent;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? Get<T, N>() => ent.Get<T, N>();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has<T, N>() => ent.Has<T, N>();

    /// <inheritdoc />
    public T? GetArchetypal<T, N, A>() => ent.GetArchetypal<T, N, A>();

    /// <inheritdoc />
    public bool HasArchetypal<T, N, A>() => ent.HasArchetypal<T, N, A>();

    public override string ToString() => ent.ToString();
}
