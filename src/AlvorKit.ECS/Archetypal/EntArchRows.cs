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

        lock (EntArchGraph<A>.Sync)
        {
            Array.Resize(ref rowsByAlloc[allocId], EntArchGraph<A>.ArchCapacity);
        }
    }

    private static void EnsureAppendCapacity(int allocId, int archId, ref EntArchRowSet rows)
    {
        if (rows.Ents != null && rows.Count != rows.Ents.Length)
            return;

        if (rows.Ents == null)
            rows.Ents = new EntMut[InitialRowCapacity];
        else Array.Resize(ref rows.Ents, rows.Ents.Length * 2);

        foreach (int fieldId in EntArchGraph<A>.FieldIds(archId))
            EntArchGraph<A>.ColumnOps(fieldId).Resize(allocId, archId, rows.Ents.Length);
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
