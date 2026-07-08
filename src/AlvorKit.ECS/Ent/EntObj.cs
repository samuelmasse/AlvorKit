namespace AlvorKit.ECS;

[DebuggerTypeProxy(typeof(EntDebugView))]
public class EntObj : IEntMut
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Ent(EntObj a) => new(a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator EntMut(EntObj a) => new(a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRef(EntObj a) => new(a, a.ent.Index, a.ent.Generation);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator EntRefMut(EntObj a) => new(a, a.ent.Index, a.ent.Generation);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? Get<T, N>() => ent.Get<T, N>();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has<T, N>() => ent.Has<T, N>();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Unset<T, N>() => ent.Unset<T, N>();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);

    public override string ToString() => ent.ToString();
}
