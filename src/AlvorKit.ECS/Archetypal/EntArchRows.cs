namespace AlvorKit.ECS;

internal static class EntArchRows<A>
{
    private const int InitialRowCapacity = 4;

    private static EntArchRowSet[][] rowsByAlloc = [];

    internal static void AccumulateMetrics(ref EntArchMetrics metrics)
    {
        metrics.AllocDirectoryCapacity = rowsByAlloc.Length;
        metrics.AddRowArray(rowsByAlloc);

        foreach (var rowsByArch in rowsByAlloc)
        {
            if (rowsByArch == null)
                continue;

            metrics.AllocDirectoryCount++;
            metrics.ArchDirectorySlotCapacity += rowsByArch.LongLength;
            metrics.AddRowArray(rowsByArch);

            foreach (ref var rows in rowsByArch.AsSpan())
            {
                if (rows.Ents == null)
                    continue;

                metrics.RetainedStateCount++;
                metrics.RowCapacity += rows.Ents.LongLength;
                metrics.RowSlack += rows.Ents.LongLength - rows.Count;
                metrics.AddEntArray(rows.Ents, rows.Count);

                if (rows.Count == 0)
                    continue;

                metrics.ActiveStateCount++;
                metrics.ActiveRowCount += rows.Count;
            }
        }
    }

    internal static int CountAt(int allocId, int archId) => rowsByAlloc[allocId][archId].Count;

    internal static int CapacityAt(int allocId, int archId) => rowsByAlloc[allocId][archId].Capacity;

    internal static int ArchCapacityAt(int allocId)
    {
        var rowsByAllocSnapshot = rowsByAlloc;
        if ((uint)allocId >= (uint)rowsByAllocSnapshot.Length)
            return 0;

        return rowsByAllocSnapshot[allocId]?.Length ?? 0;
    }

    internal static bool TryGetActive(int allocId, int archId, out EntMut[] ents, out int count)
    {
        var rowsByAllocSnapshot = rowsByAlloc;
        if ((uint)allocId < (uint)rowsByAllocSnapshot.Length)
        {
            var rowsByArch = rowsByAllocSnapshot[allocId];
            if (rowsByArch != null && (uint)archId < (uint)rowsByArch.Length)
            {
                ref var rows = ref rowsByArch[archId];
                if (rows.Count != 0)
                {
                    ents = rows.Ents;
                    count = rows.Count;
                    return true;
                }
            }
        }

        ents = null!;
        count = 0;
        return false;
    }

    internal static void ClearAlloc(int allocId)
    {
        if ((uint)allocId >= (uint)rowsByAlloc.Length)
            return;

        var rowsByArch = rowsByAlloc[allocId];
        if (rowsByArch == null)
            return;

        foreach (ref var rows in rowsByArch.AsSpan())
        {
            if (rows.Ents != null)
                EntArchArrayPool<EntMut>.Return(rows.Ents);
        }

        rowsByAlloc[allocId] = null!;
    }

    internal static int Append(int allocId, int archId, EntMut ent)
    {
        EnsureAllocCapacity(allocId);
        EnsureArchCapacity(allocId, archId);

        ref var rows = ref rowsByAlloc[allocId][archId];
        EnsureAppendCapacity(allocId, archId, ref rows);

        int row = rows.Count;
        rows.Ents[row] = ent;
        rows.Count = row + 1;
        return row;
    }

    internal static void Remove(int allocId, int archId, int row)
    {
        ref var rows = ref rowsByAlloc[allocId][archId];
        int lastRow = rows.Count - 1;

        if (row != lastRow)
            Compact(allocId, archId, row, lastRow, ref rows);

        ClearTailFields(allocId, archId, lastRow);
        rows.Count = lastRow;
        ReduceCapacity(allocId, archId, ref rows);
    }

    internal static int Move(int allocId, int srcArchId, int srcRow, int dstArchId, int commonFieldsArchId)
    {
        EnsureArchCapacity(allocId, dstArchId);

        ref var srcRows = ref rowsByAlloc[allocId][srcArchId];
        ref var dstRows = ref rowsByAlloc[allocId][dstArchId];
        int dstRow = dstRows.Count;

        EnsureAppendCapacity(allocId, dstArchId, ref dstRows);
        dstRows.Ents[dstRow] = srcRows.Ents[srcRow];
        CopyFields(allocId, srcArchId, srcRow, dstArchId, dstRow, commonFieldsArchId);

        int srcLastRow = srcRows.Count - 1;
        if (srcRow != srcLastRow)
            Compact(allocId, srcArchId, srcRow, srcLastRow, ref srcRows);

        ClearTailFields(allocId, srcArchId, srcLastRow);
        srcRows.Count = srcLastRow;
        ReduceCapacity(allocId, srcArchId, ref srcRows);
        dstRows.Count = dstRow + 1;
        return dstRow;
    }

    private static void EnsureAllocCapacity(int allocId)
    {
        if (rowsByAlloc.Length > allocId)
            return;

        lock (EntArchGraph<A>.Sync)
        {
            if (rowsByAlloc.Length <= allocId)
                Array.Resize(ref rowsByAlloc, (int)BitOperations.RoundUpToPowerOf2((uint)(allocId + 1)));
        }
    }

    private static void EnsureArchCapacity(int allocId, int archId)
    {
        if (rowsByAlloc[allocId] != null && rowsByAlloc[allocId].Length > archId)
            return;

        bool registerGroup = false;
        lock (EntArchGraph<A>.Sync)
        {
            registerGroup = rowsByAlloc[allocId] == null;
            Array.Resize(ref rowsByAlloc[allocId], EntArchGraph<A>.ArchCapacity);
        }

        if (registerGroup)
            EntReg.Allocators[allocId].RegisterArchGroup(EntArchGroupOps<A>.Instance);
    }

    private static void EnsureAppendCapacity(int allocId, int archId, ref EntArchRowSet rows)
    {
        if (rows.Count != rows.Capacity)
            return;

        int capacity = rows.Capacity == 0
            ? InitialRowCapacity
            : rows.Capacity * 2;
        rows.Ents = rows.Ents == null
            ? EntArchArrayPool<EntMut>.Rent(capacity)
            : EntArchArrayPool<EntMut>.Grow(rows.Ents, capacity, rows.Count);
        rows.Capacity = capacity;

        foreach (int fieldId in EntArchGraph<A>.FieldIds(archId))
            EntArchGraph<A>.ColumnOps(fieldId).EnsureCapacity(allocId, archId, capacity, rows.Count);
    }

    private static void ReduceCapacity(int allocId, int archId, ref EntArchRowSet rows)
    {
        int capacity;
        if (rows.Count == 0)
            capacity = 0;
        else if (rows.Capacity > InitialRowCapacity && rows.Count < rows.Capacity / 4)
            capacity = rows.Capacity / 2;
        else return;

        if (capacity == 0)
        {
            EntArchArrayPool<EntMut>.Return(rows.Ents);
            rows.Ents = null!;
        }
        else rows.Ents = EntArchArrayPool<EntMut>.Reduce(rows.Ents, capacity, rows.Count);

        rows.Capacity = capacity;
        foreach (int fieldId in EntArchGraph<A>.FieldIds(archId))
            EntArchGraph<A>.ColumnOps(fieldId).ReduceCapacity(allocId, archId, capacity, rows.Count);
    }

    private static void CopyFields(
        int allocId,
        int srcArchId,
        int srcRow,
        int dstArchId,
        int dstRow,
        int commonFieldsArchId)
    {
        foreach (int fieldId in EntArchGraph<A>.FieldIds(commonFieldsArchId))
            EntArchGraph<A>.ColumnOps(fieldId).Copy(allocId, srcArchId, srcRow, dstArchId, dstRow);
    }

    private static void Compact(int allocId, int archId, int row, int lastRow, ref EntArchRowSet rows)
    {
        foreach (int fieldId in EntArchGraph<A>.FieldIds(archId))
            EntArchGraph<A>.ColumnOps(fieldId).Copy(allocId, archId, lastRow, archId, row);

        var movedEnt = rows.Ents[lastRow];
        rows.Ents[row] = movedEnt;
        movedEnt.Set<EntArchLoc, A>(new(allocId, archId, row));
    }

    private static void ClearTailFields(int allocId, int archId, int lastRow)
    {
        foreach (int fieldId in EntArchGraph<A>.FieldIds(archId))
            EntArchGraph<A>.ColumnOps(fieldId).Clear(allocId, archId, lastRow);
    }
}
