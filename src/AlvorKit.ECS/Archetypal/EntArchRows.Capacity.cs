namespace AlvorKit.ECS;

// Payload and active-list capacity changes stay outside point access and query enumeration.
internal static partial class EntArchRows<A>
{
    /// <summary>Grows aligned Ent and component buffers before an append.</summary>
    private static void EnsureAppendCapacity(int rowSetId, ref EntArchRowSet rows)
    {
        int capacity = rows.Ents?.Length ?? 0;
        if (rows.Count != capacity)
            return;

        capacity = capacity == 0
            ? InitialRowCapacity
            : capacity * 2;
        rows.Ents = rows.Ents == null
            ? EntArchArrayPool<EntMut>.Rent(capacity)
            : EntArchArrayPool<EntMut>.Grow(rows.Ents, capacity, rows.Count);

        foreach (int fieldId in EntArchGraph<A>.FieldIds(rows.ArchId))
            EntArchGraph<A>.ColumnOps(fieldId).EnsureCapacity(rowSetId, capacity, rows.Count);
    }

    /// <summary>Returns empty buffers or halves them below 25% occupancy.</summary>
    private static void ReduceCapacity(int rowSetId, ref EntArchRowSet rows)
    {
        int currentCapacity = rows.Ents.Length;
        int capacity;
        if (rows.Count == 0)
            capacity = 0;
        else if (currentCapacity > InitialRowCapacity && rows.Count < currentCapacity / 4)
            capacity = currentCapacity / 2;
        else return;

        if (capacity == 0)
        {
            EntArchArrayPool<EntMut>.Return(rows.Ents);
            rows.Ents = null!;
        }
        else rows.Ents = EntArchArrayPool<EntMut>.Reduce(rows.Ents, capacity, rows.Count);

        foreach (int fieldId in EntArchGraph<A>.FieldIds(rows.ArchId))
            EntArchGraph<A>.ColumnOps(fieldId).ReduceCapacity(rowSetId, capacity, rows.Count);
    }

    /// <summary>Adds a newly nonempty row set to its alloc's dense active list.</summary>
    private static void Activate(int allocId, int rowSetId, ref EntArchRowSet rows)
    {
        var allocRows = rowsByAlloc[allocId];
        if (allocRows.ActiveCount == allocRows.ActiveRowSetIds.Length)
        {
            int capacity = allocRows.ActiveCount == 0
                ? InitialActiveCapacity
                : allocRows.ActiveCount * 2;
            allocRows.ActiveRowSetIds = allocRows.ActiveRowSetIds.Length == 0
                ? EntArchArrayPool<int>.Rent(capacity)
                : EntArchArrayPool<int>.Grow(allocRows.ActiveRowSetIds, capacity, allocRows.ActiveCount);
        }

        rows.ActiveIndex = allocRows.ActiveCount;
        allocRows.ActiveRowSetIds[allocRows.ActiveCount++] = rowSetId;
    }

    /// <summary>Removes an empty row set from its alloc's dense active list.</summary>
    private static void Deactivate(int allocId, ref EntArchRowSet rows)
    {
        var allocRows = rowsByAlloc[allocId];
        int lastIndex = --allocRows.ActiveCount;
        if (rows.ActiveIndex != lastIndex)
        {
            int movedRowSetId = allocRows.ActiveRowSetIds[lastIndex];
            allocRows.ActiveRowSetIds[rows.ActiveIndex] = movedRowSetId;
            RowSetAt(movedRowSetId).ActiveIndex = rows.ActiveIndex;
        }

        rows.ActiveIndex = NoActiveIndex;
        ReduceActiveCapacity(allocRows);
    }

    /// <summary>Returns an empty active list or halves it below 25% occupancy.</summary>
    private static void ReduceActiveCapacity(EntArchAllocRows allocRows)
    {
        int capacity = allocRows.ActiveRowSetIds.Length;
        if (allocRows.ActiveCount == 0)
        {
            EntArchArrayPool<int>.Return(allocRows.ActiveRowSetIds);
            allocRows.ActiveRowSetIds = [];
        }
        else if (capacity > InitialActiveCapacity && allocRows.ActiveCount < capacity / 4)
        {
            allocRows.ActiveRowSetIds = EntArchArrayPool<int>.Reduce(
                allocRows.ActiveRowSetIds,
                capacity / 2,
                allocRows.ActiveCount);
        }
    }
}
