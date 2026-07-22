namespace AlvorKit.ECS;

// Cold alloc and row-set catalog work shares the same closed generic state as the access and mutation paths.
internal static partial class EntArchRows<A>
{
    /// <summary>Adds retained alloc and row-set storage to one diagnostic snapshot.</summary>
    internal static void AccumulateMetrics(ref EntArchMetrics metrics)
    {
        metrics.AllocDirectoryCapacity = rowsByAlloc.Length;
        metrics.AddRowArray(rowsByAlloc);
        metrics.AddRowArray(rowSetPages);

        foreach (var page in rowSetPages)
        {
            if (page != null)
            {
                metrics.RowSetSlotCapacity += page.LongLength;
                metrics.AddRowArray(page);
            }
        }

        foreach (var allocRows in rowsByAlloc)
        {
            if (allocRows == null)
                continue;

            metrics.AllocDirectoryCount++;
            metrics.ArchDirectorySlotCapacity += allocRows.RowSetIdsByArch.LongLength;
            metrics.ActiveRowSetSlotCapacity += allocRows.ActiveRowSetIds.LongLength;
            metrics.AddRowArray(allocRows.RowSetIdsByArch);
            metrics.AddRowArray(allocRows.ActiveRowSetIds);
            metrics.AddStorageObjects(1);

            int rowSetId = allocRows.OwnedRowSetHead;
            while (rowSetId != NoRowSetId)
            {
                metrics.OwnedRowSetCount++;
                ref var rows = ref RowSetAt(rowSetId);
                if (rows.Ents != null)
                {
                    metrics.RetainedStateCount++;
                    metrics.RowCapacity += rows.Ents.LongLength;
                    metrics.RowSlack += rows.Ents.LongLength - rows.Count;
                    metrics.AddEntArray(rows.Ents, rows.Count);

                    if (rows.Count != 0)
                    {
                        metrics.ActiveStateCount++;
                        metrics.ActiveRowCount += rows.Count;
                    }
                }

                rowSetId = rows.NextOwnedRowSetId;
            }
        }
    }

    /// <summary>Returns one alloc's row and component buffers and recycles its row-set IDs.</summary>
    internal static void ClearAlloc(int allocId)
    {
        lock (EntArchGraph<A>.Sync)
        {
            if ((uint)allocId >= (uint)rowsByAlloc.Length)
                return;

            var allocRows = rowsByAlloc[allocId];
            if (allocRows == null)
                return;

            int rowSetId = allocRows.OwnedRowSetHead;
            while (rowSetId != NoRowSetId)
            {
                ref var rows = ref RowSetAt(rowSetId);
                int nextOwnedRowSetId = rows.NextOwnedRowSetId;

                if (rows.Ents != null)
                    EntArchArrayPool<EntMut>.Return(rows.Ents);
                foreach (int fieldId in EntArchGraph<A>.FieldIds(rows.ArchId))
                    EntArchGraph<A>.ColumnOps(fieldId).ClearRowSet(rowSetId);

                rows = default;
                rows.NextOwnedRowSetId = freeRowSetHead;
                freeRowSetHead = rowSetId;
                rowSetId = nextOwnedRowSetId;
            }

            if (allocRows.RowSetIdsByArch.Length != 0)
                EntArchArrayPool<int>.Return(allocRows.RowSetIdsByArch);
            if (allocRows.ActiveRowSetIds.Length != 0)
                EntArchArrayPool<int>.Return(allocRows.ActiveRowSetIds);
            rowsByAlloc[allocId] = null!;
        }
    }

    /// <summary>Gets or allocates the stable row-set ID for one observed alloc and arch pair.</summary>
    private static int GetOrCreateRowSet(int allocId, int archId)
    {
        int rowSetId = RowSetIdAt(allocId, archId);
        if (rowSetId != NoRowSetId)
            return rowSetId;

        bool registerGroup = false;
        lock (EntArchGraph<A>.Sync)
        {
            EnsureAllocCapacity(allocId);
            var allocRows = rowsByAlloc[allocId];
            if (allocRows == null)
            {
                allocRows = new();
                rowsByAlloc[allocId] = allocRows;
                registerGroup = true;
            }

            EnsureArchCapacity(allocRows, archId);
            rowSetId = allocRows.RowSetIdsByArch[archId];
            if (rowSetId == NoRowSetId)
            {
                rowSetId = AllocateRowSetId();
                ref var rows = ref RowSetAt(rowSetId);
                rows = default;
                rows.ArchId = archId;
                rows.ActiveIndex = NoActiveIndex;
                rows.NextOwnedRowSetId = allocRows.OwnedRowSetHead;
                allocRows.OwnedRowSetHead = rowSetId;
                allocRows.RowSetIdsByArch[archId] = rowSetId;
            }
        }

        if (registerGroup)
            EntReg.Allocators[allocId].RegisterArchGroup(EntArchGroupOps<A>.Instance);
        return rowSetId;
    }

    /// <summary>Grows the sparse alloc directory to contain an alloc ID.</summary>
    private static void EnsureAllocCapacity(int allocId)
    {
        if (rowsByAlloc.Length <= allocId)
            Array.Resize(ref rowsByAlloc, (int)BitOperations.RoundUpToPowerOf2((uint)(allocId + 1)));
    }

    /// <summary>Grows one alloc's direct arch-to-row-set directory.</summary>
    private static void EnsureArchCapacity(EntArchAllocRows allocRows, int archId)
    {
        if (allocRows.RowSetIdsByArch.Length > archId)
            return;

        int capacity = EntArchGraph<A>.ArchCapacity;
        int[] grown = EntArchArrayPool<int>.Rent(capacity);
        Array.Clear(grown);
        allocRows.RowSetIdsByArch.CopyTo(grown, 0);
        if (allocRows.RowSetIdsByArch.Length != 0)
            EntArchArrayPool<int>.Return(allocRows.RowSetIdsByArch);
        allocRows.RowSetIdsByArch = grown;
    }

    /// <summary>Reuses a released row-set ID or allocates a slot from a stable metadata page.</summary>
    private static int AllocateRowSetId()
    {
        int rowSetId;
        if (freeRowSetHead != NoRowSetId)
        {
            rowSetId = freeRowSetHead;
            freeRowSetHead = RowSetAt(rowSetId).NextOwnedRowSetId;
            return rowSetId;
        }

        rowSetId = nextRowSetId++;
        int pageIndex = rowSetId >> RowSetPageShift;
        if (rowSetPages.Length <= pageIndex)
            Array.Resize(ref rowSetPages, (int)BitOperations.RoundUpToPowerOf2((uint)(pageIndex + 1)));
        rowSetPages[pageIndex] ??= new EntArchRowSet[RowSetPageCapacity];
        return rowSetId;
    }

    /// <summary>Returns a stable reference to one paged row-set record.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref EntArchRowSet RowSetAt(int rowSetId) =>
        ref rowSetPages[rowSetId >> RowSetPageShift][rowSetId & RowSetPageMask];
}
