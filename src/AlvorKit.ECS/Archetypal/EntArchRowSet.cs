namespace AlvorKit.ECS;

internal struct EntArchRowSet
{
    /// <summary>Stores the dense Ent rows aligned with every component column.</summary>
    internal EntMut[] Ents;
    /// <summary>Counts live rows in <see cref="Ents"/>.</summary>
    internal int Count;
    /// <summary>Identifies the immutable component signature stored by this row set.</summary>
    internal int ArchId;
    /// <summary>Indexes this row set in its alloc's active list, or -1 while empty.</summary>
    internal int ActiveIndex;
    /// <summary>Links row sets owned by the same alloc or IDs in the global free list.</summary>
    internal int NextOwnedRowSetId;
}
