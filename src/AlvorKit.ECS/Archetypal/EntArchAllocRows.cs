namespace AlvorKit.ECS;

/// <summary>Stores the sparse row-set index and active row sets owned by one alloc in one arch group.</summary>
internal sealed class EntArchAllocRows
{
    /// <summary>Maps observed arch IDs directly to row-set IDs for this alloc.</summary>
    internal int[] RowSetIdsByArch = [];
    /// <summary>Stores row-set IDs whose row count is nonzero.</summary>
    internal int[] ActiveRowSetIds = [];
    /// <summary>Counts the used prefix of <see cref="ActiveRowSetIds"/>.</summary>
    internal int ActiveCount;
    /// <summary>Starts the linked list of every row set owned by this alloc.</summary>
    internal int OwnedRowSetHead;
}
