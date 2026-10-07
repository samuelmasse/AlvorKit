namespace AlvorKit;

/// <summary>One stack-resident synchronous operation; its pointer never survives the enclosing try/finally.</summary>
internal unsafe struct EntIdxOperation
{
    /// <summary>Previous stack frame restored after synchronous delivery.</summary>
    internal EntIdxOperation* Previous;
    /// <summary>Context whose lifetime must remain intact during delivery.</summary>
    internal EntIdxContextKey Context;
    /// <summary>Ent protected against conflicting reentrant mutation.</summary>
    internal Ent Ent;
    /// <summary>Typed write identity; zero identifies a lifetime operation.</summary>
    internal nint Component;
    /// <summary>Whether user callbacks are currently maintaining indexes.</summary>
    internal bool Indexing;
}
