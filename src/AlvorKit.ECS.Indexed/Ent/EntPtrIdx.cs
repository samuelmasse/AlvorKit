namespace AlvorKit.ECS.Indexed;

[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntPtrIdx : IEntMut, IDisposable
{
    private readonly EntPtr ent;
    private readonly Ent context;

    public static implicit operator EntMutIdx(EntPtrIdx a) => new(a);

    public static implicit operator Ent(EntPtrIdx a) => a.ent;

    internal EntPtrIdx(EntPtr ent, Ent context)
    {
        this.ent = ent;
        this.context = context;
    }

    public EntHandle Handle => ent.Handle;

    public bool IsAlive => ent.IsAlive;

    public T? Get<T, N>() => ent.Get<T, N>();

    public bool Has<T, N>() => ent.Has<T, N>();

    public void Set<T, N>(in T value)
    {
        if (!IsAlive)
            return;

        EntMutIdx mut = this;
        foreach (var hook in context.Get<ReadOnlyMemory<EntIdxPreHook<T>>, EntIdxPre<T, N>>().Span)
            hook(mut, value);

        ent.Set<T, N>(value);

        foreach (var hook in context.Get<ReadOnlyMemory<EntIdxPostHook>, EntIdxPost<T, N>>().Span)
            hook(mut);
    }

    public bool Unset<T, N>()
    {
        if (!IsAlive || !ent.Has<T, N>())
            return false;

        EntMutIdx mut = this;
        T value = default!;
        foreach (var hook in context.Get<ReadOnlyMemory<EntIdxPreHook<T>>, EntIdxPre<T, N>>().Span)
            hook(mut, value);

        bool unset = ent.Unset<T, N>();

        foreach (var hook in context.Get<ReadOnlyMemory<EntIdxPostHook>, EntIdxPost<T, N>>().Span)
            hook(mut);

        return unset;
    }

    public void Dispose()
    {
        if (!IsAlive)
            return;

        EntMutIdx mut = this;
        foreach (var hook in context.Get<ReadOnlyMemory<EntIdxPreDisposeHook>, EntIdxPreDispose>().Span)
            hook(mut);

        this.Clear();
        ent.Dispose();
    }

    public override string ToString() => ent.ToString();
}
