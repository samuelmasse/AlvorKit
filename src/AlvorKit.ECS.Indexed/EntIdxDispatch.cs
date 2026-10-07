namespace AlvorKit;

/// <summary>Tracks synchronous Indexed calls across contexts without allocating a managed call stack.</summary>
internal static unsafe class EntIdxDispatch
{
    /// <summary>Thread-local pointer to the innermost live stack frame.</summary>
    [ThreadStatic]
    private static EntIdxOperation* current;

    /// <summary>Borrowed reference used to push and restore stack-resident operations.</summary>
    internal static ref EntIdxOperation* Current => ref current;

    /// <summary>Validates a lifetime operation against active callbacks before publishing its frame.</summary>
    internal static void Enter(EntIdxOperation* operation)
    {
        ValidateActiveWrite(current, operation->Ent, operation->Component);
        Push(operation);
    }

    /// <summary>Links a stack frame whose lifetime is bounded by its caller.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Push(EntIdxOperation* operation)
    {
        operation->Previous = current;
        current = operation;
    }

    /// <summary>Unobserved writes need validation, but cannot reenter and need no active stack frame.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ValidateWrite(EntIdxOperation* active, Ent ent, nint component)
    {
        if (active != null)
            ValidateActiveWrite(active, ent, component);
    }

    /// <summary>Rejects index-phase mutation, same-component recursion, and lifetime conflicts.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ValidateActiveWrite(EntIdxOperation* current, Ent ent, nint component)
    {
        if (current != null && current->Indexing)
            throw new InvalidOperationException("Index maintenance cannot mutate or allocate Indexed Ents.");

        for (var active = current; active != null; active = active->Previous)
        {
            if (active->Ent != ent)
                continue;

            if (active->Component == 0)
                throw new InvalidOperationException("An Ent cannot be mutated during its Clear or Dispose notifications.");

            if (component == 0)
                throw new InvalidOperationException("An Ent cannot be cleared or disposed during its active write delivery.");

            if (active->Component == component)
                throw new InvalidOperationException("An Indexed reaction cannot write an already-active Ent/component pair.");
        }
    }

    /// <summary>Restores the previous frame in the enclosing finally block.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Exit(EntIdxOperation* previous) => current = previous;

    /// <summary>Rejects allocation and ownership changes while maintaining indexes.</summary>
    internal static void EnsureNotIndexing()
    {
        if (current != null && current->Indexing)
            throw new InvalidOperationException("Index maintenance cannot mutate or allocate Indexed Ents.");
    }

    /// <summary>Rejects context or arena teardown while that context is delivering callbacks.</summary>
    internal static void EnsureIdle(EntIdxContextKey context)
    {
        EnsureNotIndexing();

        for (var active = current; active != null; active = active->Previous)
        {
            if (active->Context == context)
                throw new InvalidOperationException("An Indexed context or arena cannot be disposed during its active callbacks.");
        }
    }
}
