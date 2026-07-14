namespace AlvorKit.ECS;

[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntPtr : IDisposable, IEntMut
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Ent(EntPtr a) => new(a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntMut(EntPtr a) => new(a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRef(EntPtr a) => new(null, a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRefMut(EntPtr a) => new(null, a.ent.Index, a.ent.Generation);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Dispose()
    {
        int pageIndex = ent.Index >> EntReg.PageBits;
        int subIndex = ent.Index & EntReg.PageMask;

        if (Interlocked.CompareExchange(ref EntReg.PageGenerations[pageIndex][subIndex], Generation + 1, Generation) == Generation)
        {
            ent.ResetArchetypal();
            ent.Reset();
            EntReg.PageGenerations[pageIndex][subIndex]++;
            EntReg.Allocators[EntReg.PageAllocators[pageIndex]].Add(ent.Index);
        }
    }

    public override string ToString() => ent.ToString();
}
