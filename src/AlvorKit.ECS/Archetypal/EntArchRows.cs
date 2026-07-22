namespace AlvorKit.ECS;

/// <summary>Stores and mutates alloc-owned dense row sets through recyclable group-global IDs.</summary>
internal static partial class EntArchRows<A>
{
    internal const int NoRowSetId = 0;
    internal const int FirstRowSetId = 1;

    private const int InitialRowCapacity = 4;
    private const int InitialActiveCapacity = 4;
    // Fixed pages keep refs into existing row-set metadata valid while another alloc owner grows the catalog.
    private const int RowSetPageShift = 6;
    private const int RowSetPageCapacity = 1 << RowSetPageShift;
    private const int RowSetPageMask = RowSetPageCapacity - 1;
    private const int NoActiveIndex = -1;

    private static EntArchAllocRows[] rowsByAlloc = [];
    private static EntArchRowSet[][] rowSetPages = [];
    private static int nextRowSetId = FirstRowSetId;
    private static int freeRowSetHead = NoRowSetId;

    /// <summary>Gets the live row count for one row set.</summary>
    internal static int CountAt(int rowSetId) => RowSetAt(rowSetId).Count;

    /// <summary>Gets the rented Ent capacity for one row set.</summary>
    internal static int CapacityAt(int rowSetId)
    {
        var ents = RowSetAt(rowSetId).Ents;
        return ents?.Length ?? 0;
    }

    /// <summary>Gets an alloc's row-set ID for an arch, or zero when that pair was never observed.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int RowSetIdAt(int allocId, int archId)
    {
        var rowsByAllocSnapshot = rowsByAlloc;
        if ((uint)allocId >= (uint)rowsByAllocSnapshot.Length)
            return NoRowSetId;

        var allocRows = rowsByAllocSnapshot[allocId];
        if (allocRows == null || (uint)archId >= (uint)allocRows.RowSetIdsByArch.Length)
            return NoRowSetId;

        return allocRows.RowSetIdsByArch[archId];
    }

    /// <summary>Gets the immutable arch ID assigned to one row set.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ArchIdAt(int rowSetId) => RowSetAt(rowSetId).ArchId;

    /// <summary>Gets the number of active row sets owned by an alloc.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ActiveCountAt(int allocId)
    {
        var rowsByAllocSnapshot = rowsByAlloc;
        return (uint)allocId < (uint)rowsByAllocSnapshot.Length && rowsByAllocSnapshot[allocId] != null
            ? rowsByAllocSnapshot[allocId].ActiveCount
            : 0;
    }

    /// <summary>Gets one row-set ID from an alloc's dense active list.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ActiveRowSetIdAt(int allocId, int index) =>
        rowsByAlloc[allocId].ActiveRowSetIds[index];

    /// <summary>Gets a row set's Ent storage when it contains live rows.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryGetActive(int rowSetId, out EntMut[] ents, out int count)
    {
        if (rowSetId != NoRowSetId)
        {
            ref var rows = ref RowSetAt(rowSetId);
            if (rows.Count != 0)
            {
                ents = rows.Ents;
                count = rows.Count;
                return true;
            }
        }

        ents = null!;
        count = 0;
        return false;
    }

    /// <summary>Gets the active Ent storage and count for a known nonempty row set.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GetActive(int rowSetId, out EntMut[] ents, out int count)
    {
        ref var rows = ref RowSetAt(rowSetId);
        ents = rows.Ents;
        count = rows.Count;
    }

    /// <summary>Appends an Ent to an alloc-local arch and returns its new location.</summary>
    internal static EntArchLoc Append(int allocId, int archId, EntMut ent)
    {
        int rowSetId = GetOrCreateRowSet(allocId, archId);
        ref var rows = ref RowSetAt(rowSetId);
        EnsureAppendCapacity(rowSetId, ref rows);

        int row = rows.Count;
        rows.Ents[row] = ent;
        rows.Count = row + 1;
        if (row == 0)
            Activate(allocId, rowSetId, ref rows);
        return new(rowSetId, archId, row);
    }

    /// <summary>Removes one row with swap-back compaction and bounded capacity reduction.</summary>
    internal static void Remove(int allocId, int rowSetId, int row)
    {
        ref var rows = ref RowSetAt(rowSetId);
        int lastRow = rows.Count - 1;

        if (row != lastRow)
            Compact(rowSetId, row, lastRow, ref rows);

        ClearTailFields(rowSetId, lastRow, rows.ArchId);
        rows.Count = lastRow;
        ReduceCapacity(rowSetId, ref rows);
        if (lastRow == 0)
            Deactivate(allocId, ref rows);
    }

    /// <summary>Moves one Ent between alloc-local archs and copies the requested common fields.</summary>
    internal static EntArchLoc Move(
        int allocId,
        int srcRowSetId,
        int srcRow,
        int dstArchId,
        int commonFieldsArchId)
    {
        int dstRowSetId = GetOrCreateRowSet(allocId, dstArchId);
        ref var srcRows = ref RowSetAt(srcRowSetId);
        ref var dstRows = ref RowSetAt(dstRowSetId);
        int dstRow = dstRows.Count;

        EnsureAppendCapacity(dstRowSetId, ref dstRows);
        dstRows.Ents[dstRow] = srcRows.Ents[srcRow];
        CopyFields(srcRowSetId, srcRow, dstRowSetId, dstRow, commonFieldsArchId);

        dstRows.Count = dstRow + 1;
        if (dstRow == 0)
            Activate(allocId, dstRowSetId, ref dstRows);

        int srcLastRow = srcRows.Count - 1;
        if (srcRow != srcLastRow)
            Compact(srcRowSetId, srcRow, srcLastRow, ref srcRows);

        ClearTailFields(srcRowSetId, srcLastRow, srcRows.ArchId);
        srcRows.Count = srcLastRow;
        ReduceCapacity(srcRowSetId, ref srcRows);
        if (srcLastRow == 0)
            Deactivate(allocId, ref srcRows);
        return new(dstRowSetId, dstArchId, dstRow);
    }

    /// <summary>Copies every field belonging to the common signature between two rows.</summary>
    private static void CopyFields(
        int srcRowSetId,
        int srcRow,
        int dstRowSetId,
        int dstRow,
        int commonFieldsArchId)
    {
        foreach (int fieldId in EntArchGraph<A>.FieldIds(commonFieldsArchId))
            EntArchGraph<A>.ColumnOps(fieldId).Copy(srcRowSetId, srcRow, dstRowSetId, dstRow);
    }

    /// <summary>Fills a removed row from the tail and repairs the moved Ent's location.</summary>
    private static void Compact(int rowSetId, int row, int lastRow, ref EntArchRowSet rows)
    {
        foreach (int fieldId in EntArchGraph<A>.FieldIds(rows.ArchId))
            EntArchGraph<A>.ColumnOps(fieldId).Copy(rowSetId, lastRow, rowSetId, row);

        var movedEnt = rows.Ents[lastRow];
        rows.Ents[row] = movedEnt;
        movedEnt.Set<EntArchLoc, A>(new(rowSetId, rows.ArchId, row));
    }

    /// <summary>Clears reference-containing component values at an inactive tail row.</summary>
    private static void ClearTailFields(int rowSetId, int lastRow, int archId)
    {
        foreach (int fieldId in EntArchGraph<A>.FieldIds(archId))
            EntArchGraph<A>.ColumnOps(fieldId).Clear(rowSetId, lastRow);
    }
}
