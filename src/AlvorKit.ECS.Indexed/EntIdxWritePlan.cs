namespace AlvorKit;

/// <summary>Registration builds ordered callback chains once; ordinary write notifications require no snapshot.</summary>
internal unsafe class EntIdxWritePlan<T, N>(EntIdxPlan? next) : EntIdxPlan(next)
{
    /// <summary>Runtime type identity of the component write, used by reentrancy validation.</summary>
    private readonly nint component = typeof(EntIdxWrite<T, N>).TypeHandle.Value;
    /// <summary>Ordered every-write reactions, including equal writes.</summary>
    private EntWriteHandler? writes;
    /// <summary>Ordered every-write index maintenance callbacks.</summary>
    private EntWriteHandler? indexWrites;
    /// <summary>Ordered reactions for changed writes, including adapted write observers.</summary>
    private EntChangeHandler<T>? changes;
    /// <summary>Ordered index callbacks for changed writes.</summary>
    private EntChangeHandler<T>? indexChanges;
    /// <summary>Whether this plan needs equality comparison and a borrowed change view.</summary>
    private bool observesChanges;

    /// <summary>Selects comparison and snapshot work only when change observers exist.</summary>
    internal bool ObservesChanges => observesChanges;

    /// <summary>Removes this plan from the typed context registry.</summary>
    internal override void Release(int contextIndex) => EntIdxPlans<T, N>.Set(contextIndex, null);

    /// <summary>Appends an every-write observer to its phase and any existing changed-write chain.</summary>
    internal void AddWrite(EntWriteHandler handler, bool index)
    {
        if (index)
        {
            indexWrites += handler;

            if (observesChanges)
                indexChanges += OnChangedWrite(handler);
        }
        else
        {
            writes += handler;

            if (observesChanges)
                changes += OnChangedWrite(handler);
        }
    }

    /// <summary>Enables changed-write delivery while preserving earlier observer order.</summary>
    internal void AddChange(EntChangeHandler<T> handler, bool index)
    {
        if (!observesChanges)
        {
            observesChanges = true;

            if (writes != null)
                changes = OnChangedWrite(writes);

            if (indexWrites != null)
                indexChanges = OnChangedWrite(indexWrites);
        }

        if (index)
            indexChanges += handler;
        else changes += handler;
    }

    /// <summary>Adapts an Ent-only observer once at registration, avoiding per-write allocations.</summary>
    private static EntChangeHandler<T> OnChangedWrite(EntWriteHandler handler) => (ent, in change) => handler(ent);

    /// <summary>Compares once, commits even equal replacements, and synchronously delivers the appropriate chain.</summary>
    internal void Set(EntPtrIdx ent, ref (int Generation, T? Value) slot, in T value, ref EntIdxOperation* active)
    {
        bool present = slot.Generation == ent.Generation;

        if (present && EqualityComparer<T>.Default.Equals(slot.Value, value))
        {
            slot.Value = value;

            if (writes != null || indexWrites != null)
                NotifyWrite(ent, ref active);

            return;
        }

        T? before = present ? slot.Value : default;
        slot.Generation = ent.Generation;
        slot.Value = value;
        var change = new EntChange<T>(in before, in slot.Value, present, true);
        NotifyChange(ent, in change, ref active);
    }

    /// <summary>Captures the present value, clears its slot, and publishes a removal view.</summary>
    internal void Unset(EntPtrIdx ent, ref (int Generation, T? Value) slot, ref EntIdxOperation* active)
    {
        T? before = slot.Value;
        slot = default;
        var change = new EntChange<T>(in before, in slot.Value, true, false);
        NotifyChange(ent, in change, ref active);
    }

    /// <summary>Delivers index callbacks before reactions using one stack-resident guard frame.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void NotifyWrite(in EntPtrIdx ent, ref EntIdxOperation* active)
    {
        if (writes == null && indexWrites == null)
            return;

        EntIdxOperation operation;
        operation.Previous = active;
        operation.Context = ent.Context;
        operation.Ent = ent;
        operation.Component = component;
        operation.Indexing = true;
        active = &operation;

        try
        {
            indexWrites?.Invoke(ent);
            operation.Indexing = false;
            writes?.Invoke(ent);
        }
        finally
        {
            active = operation.Previous;
        }
    }

    /// <summary>Borrows before and after values through ordered index and reaction callbacks.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void NotifyChange(in EntPtrIdx ent, in EntChange<T> change, ref EntIdxOperation* active)
    {
        EntIdxOperation operation;
        operation.Previous = active;
        operation.Context = ent.Context;
        operation.Ent = ent;
        operation.Component = component;
        operation.Indexing = true;
        active = &operation;

        try
        {
            indexChanges?.Invoke(ent, in change);
            operation.Indexing = false;
            changes?.Invoke(ent, in change);
        }
        finally
        {
            active = operation.Previous;
        }
    }
}
