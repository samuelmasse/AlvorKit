namespace AlvorKit.ECS.Indexed;

/// <summary>Represents a disposable indexed entity handle that runs hook pipelines on mutation.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntPtrIdx : IEntMut, IDisposable
{
    private readonly EntPtr ent;
    private readonly Ent context;

    /// <summary>Converts this pointer to a mutable indexed value handle.</summary>
    public static implicit operator EntMutIdx(EntPtrIdx a) => new(a);

    /// <summary>Converts this pointer to a read-only base ECS handle.</summary>
    public static implicit operator Ent(EntPtrIdx a) => a.ent;

    internal EntPtrIdx(EntPtr ent, Ent context)
    {
        this.ent = ent;
        this.context = context;
    }

    /// <summary>Gets the stable handle for this entity generation.</summary>
    public EntHandle Handle => ent.Handle;

    /// <summary>Returns whether this handle still points at a live entity generation.</summary>
    public bool IsAlive => ent.IsAlive;

    /// <summary>Gets the component value for the requested value and marker types, or the default value when absent.</summary>
    public T? Get<T, N>() => ent.Get<T, N>();

    /// <summary>Returns whether this entity currently has the requested component.</summary>
    public bool Has<T, N>() => ent.Has<T, N>();

    /// <summary>Sets a component after pre hooks run and before post hooks run.</summary>
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

    /// <summary>Unsets a present component through the pre and post hook pipeline.</summary>
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

    /// <summary>Runs pre-dispose hooks, clears components through indexed unsets, and returns the slot.</summary>
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

    /// <summary>Formats this entity and selected components for diagnostics.</summary>
    public override string ToString() => ent.ToString();
}

