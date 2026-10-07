namespace AlvorKit;

/// <summary>Unmanaged owning Ent handle with a borrowed Indexed context identity.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntPtrIdx : IEntMut, IDisposable
{
    private readonly EntPtr ent;
    /// <summary>Borrowed context slot whose lifetime is protected by the owning arena.</summary>
    private readonly EntIdxContextKey context;

    /// <summary>Context identity used by synchronous dispatch guards.</summary>
    internal EntIdxContextKey Context => context;
    /// <summary>Allocation generation used to distinguish present sparse values from stale slots.</summary>
    internal int Generation => ent.Generation;

    public EntHandle Handle => ent.Handle;
    public bool IsAlive => ent.IsAlive;

    /// <summary>Combines a live arena allocation with its borrowed registration identity.</summary>
    internal EntPtrIdx(EntPtr ent, EntIdxContextKey context)
    {
        this.ent = ent;
        this.context = context;
    }

    public static implicit operator EntMutIdx(EntPtrIdx a) => new(a);

    public static implicit operator Ent(EntPtrIdx a) => a.ent;

    public T? Get<T, N>() => ent.Get<T, N>();

    public bool Has<T, N>() => ent.Has<T, N>();

    /// <summary>Commits a sparse write, maintains indexes, then synchronously delivers reactions.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Set<T, N>(in T value)
    {
        if (!IsAlive)
            return;

        ref var active = ref EntIdxDispatch.Current;

        if (active != null)
            EntIdxDispatch.ValidateWrite(active, ent, typeof(EntIdxWrite<T, N>).TypeHandle.Value);

        var plan = EntIdxPlans<T, N>.Get(context.Index);
        ref var slot = ref ((EntMut)ent).RequireSparseSlot<T, N>();

        if (plan != null && plan.ObservesChanges)
            plan.Set(this, ref slot, in value, ref active);
        else
        {
            slot.Generation = ent.Generation;
            slot.Value = value;
            plan?.NotifyWrite(this, ref active);
        }
    }

    /// <summary>Publishes removal of a present sparse component; absent removal is silent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Unset<T, N>()
    {
        if (ent.Index == 0)
            return false;

        ref var slot = ref ((EntMut)ent).FindSparseSlot<T, N>();

        if (Unsafe.IsNullRef(ref slot) || slot.Generation != ent.Generation)
        {
            var active = EntIdxDispatch.Current;

            if (active != null && IsAlive)
                EntIdxDispatch.ValidateWrite(active, ent, typeof(EntIdxWrite<T, N>).TypeHandle.Value);

            return false;
        }

        if (!IsAlive)
            return false;

        UnsetPresent<T, N>(ref slot);
        return true;
    }

    /// <summary>Commits a known-present removal and dispatches its observers without another storage lookup.</summary>
    private unsafe void UnsetPresent<T, N>(ref (int Generation, T? Value) slot)
    {
        ref var active = ref EntIdxDispatch.Current;

        if (active != null)
            EntIdxDispatch.ValidateWrite(active, ent, typeof(EntIdxWrite<T, N>).TypeHandle.Value);

        var plan = EntIdxPlans<T, N>.Get(context.Index);

        if (plan != null && plan.ObservesChanges)
            plan.Unset(this, ref slot, ref active);
        else
        {
            slot = default;
            plan?.NotifyWrite(this, ref active);
        }
    }

    /// <summary>Notifies Clear, detaches indexes, and removes all storage without releasing this allocation.</summary>
    public void Clear() => Teardown(false);

    /// <summary>Notifies Dispose and detaches indexes before release. Repeated disposal is inert.</summary>
    public void Dispose() => Teardown(true);

    /// <summary>Index maintenance has already established a live Ent; resolve and commit its private slot once.</summary>
    internal void SetBagIndex<TIndex>(int index)
    {
        ref var slot = ref ((EntMut)ent).RequireSparseSlot<int, TIndex>();
        slot.Generation = ent.Generation;
        slot.Value = index;
    }

    /// <summary>Removal has already validated occupied membership and therefore the private slot's existence.</summary>
    internal void UnsetBagIndex<TIndex>() => ((EntMut)ent).FindSparseSlot<int, TIndex>() = default;

    /// <summary>Delivers intact-Ent notifications, detaches indexes, then clears storage or releases ownership.</summary>
    private unsafe void Teardown(bool dispose)
    {
        if (!IsAlive)
            return;

        EntIdxOperation operation = new() { Context = context, Ent = ent };
        EntIdxDispatch.Enter(&operation);

        try
        {
            var callbacks = EntIdxContexts.Get(context).Lifecycle;
            var notifications = dispose ? callbacks.Disposing.Span : callbacks.Clearing.Span;

            foreach (var notification in notifications)
                notification(this);

            operation.Indexing = true;

            foreach (var remove in callbacks.Removing.Span)
                remove(this);

            if (dispose)
                ent.Dispose();
            else ent.Clear();
        }
        finally
        {
            EntIdxDispatch.Exit(operation.Previous);
        }
    }

    public override string ToString() => ent.ToString();
}
