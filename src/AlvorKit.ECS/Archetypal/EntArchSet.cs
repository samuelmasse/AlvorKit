namespace AlvorKit.ECS;

internal static class EntArchSet<A>
{
    internal static readonly object Lock;
    internal static int NextFieldId;
    internal static int NextArchId;
    internal static int FieldCapacity;
    internal static int ArchCapacity;

    // packed field indice list for each arch [a1f1, a1f2, a1f3, a2f1, a2f2, ...]
    internal static int[] ArchFields;
    internal static int ArchFieldsIndex;
    // [archId] -> storage info for archetype to index into ArchFields and know the count
    internal static EntArchStorageInfo[] ArchStorageInfo;

    // [archId][fieldId] -> directional graph for field adds and removes
    internal static EntGraphEdge[][] Graph;

    // [allocatorId][archId][row] -> ents for a given archetype in an allocator scope
    internal static EntArchRow[][] Ents;

    internal static EntArchFieldHandler[] Handlers;

    internal struct EntArchRow
    {
        internal EntMut[] Data;
        internal int Count;
    }

    static EntArchSet()
    {
        Lock = new();

        NextFieldId = 1;
        NextArchId = 2; // 0 - null arch, 1 - bootstrap arch

        Graph = [[]];
        Ents = [];
        ArchStorageInfo = [];
        Handlers = [];

        ResizeGraph(0x10, 0x10);
        ArchFields = new int[0x1000];
    }

    internal static ref EntGraphEdge GraphAt(int archId, int fieldId) => ref Graph[archId][fieldId];

    internal static Span<int> ArchFieldIds(int archId)
    {
        var info = ArchStorageInfo[archId];
        return new(ArchFields, info.StartIndex, info.FieldCount);
    }

    internal static int NewField()
    {
        lock (Lock)
        {
            if (NextFieldId == FieldCapacity)
                ResizeGraph(FieldCapacity * 2, ArchCapacity);

            return NextFieldId++;
        }
    }

    internal static int NewArch(int fieldId)
    {
        lock (Lock)
        {
            if (GraphAt(1, fieldId).Add != 0)
                return GraphAt(1, fieldId).Add;

            return CreateArch([fieldId]);
        }
    }

    internal static int AddEnt(int allocatorId, int archId, EntMut ent)
    {
        if (Ents.Length <= allocatorId)
        {
            lock (Lock)
            {
                if (Ents.Length <= allocatorId)
                    Array.Resize(ref Ents, (int)BitOperations.RoundUpToPowerOf2((uint)(allocatorId + 1)));
            }
        }

        if (Ents[allocatorId] == null || Ents[allocatorId].Length <= archId)
        {
            lock (Lock)
            {
                Array.Resize(ref Ents[allocatorId], ArchCapacity);
            }
        }

        ref var ents = ref Ents[allocatorId][archId];
        PrepareAdd(allocatorId, archId, ref ents);
        ents.Data[ents.Count] = ent;
        return ents.Count++;
    }

    internal static void RemoveEnt(int allocatorId, int archId, int row)
    {
        ref var ents = ref Ents[allocatorId][archId];
        int lastRow = ents.Count - 1;

        if (row != lastRow)
        {
            foreach (var fieldId in ArchFieldIds(archId))
                Handlers[fieldId].Move(allocatorId, archId, lastRow, archId, row);

            var lastEnt = ents.Data[lastRow];
            ents.Data[row] = lastEnt;
            lastEnt.Set<EntArchLoc, A>(new(allocatorId, archId, row));
        }

        foreach (var fieldId in ArchFieldIds(archId))
            Handlers[fieldId].Clear(allocatorId, archId, lastRow);

        ents.Count--;
    }

    internal static int MoveEnt(int allocatorId, int srcArchId, int srcRow, int dstArchId, int fieldsArchId)
    {
        if (Ents[allocatorId] == null || Ents[allocatorId].Length <= dstArchId)
        {
            lock (Lock)
            {
                Array.Resize(ref Ents[allocatorId], ArchCapacity);
            }
        }

        ref var srcEnts = ref Ents[allocatorId][srcArchId];
        ref var dstEnts = ref Ents[allocatorId][dstArchId];

        int dstRow = dstEnts.Count;
        PrepareAdd(allocatorId, dstArchId, ref dstEnts);

        var ent = srcEnts.Data[srcRow];
        dstEnts.Data[dstRow] = ent;

        foreach (var fieldId in ArchFieldIds(fieldsArchId))
            Handlers[fieldId].Move(allocatorId, srcArchId, srcRow, dstArchId, dstRow);

        int lastRow = srcEnts.Count - 1;

        if (srcRow != lastRow)
        {
            foreach (var fieldId in ArchFieldIds(srcArchId))
                Handlers[fieldId].Move(allocatorId, srcArchId, lastRow, srcArchId, srcRow);

            var lastEnt = srcEnts.Data[lastRow];
            srcEnts.Data[srcRow] = lastEnt;
            lastEnt.Set<EntArchLoc, A>(new(allocatorId, srcArchId, srcRow));
        }

        foreach (var fieldId in ArchFieldIds(srcArchId))
            Handlers[fieldId].Clear(allocatorId, srcArchId, lastRow);

        srcEnts.Count--;
        return dstEnts.Count++;
    }

    internal static void PrepareAdd(int allocatorId, int archId, ref EntArchRow row)
    {
        if (row.Data == null || row.Count == row.Data.Length)
        {
            if (row.Data == null)
                row.Data = new EntMut[0x10];
            else Array.Resize(ref row.Data, row.Data.Length * 2);

            var fieldIds = ArchFieldIds(archId);
            foreach (var fieldId in fieldIds)
                Handlers[fieldId].Resize(allocatorId, archId, row.Data.Length);
        }
    }

    internal static int ExtendArch(int srcArch, int fieldId)
    {
        lock (Lock)
        {
            if (GraphAt(srcArch, fieldId).Add != 0)
                return GraphAt(srcArch, fieldId).Add;

            var srcFields = ArchFieldIds(srcArch);
            Span<int> fieldIds = stackalloc int[srcFields.Length + 1];

            srcFields.CopyTo(fieldIds);
            fieldIds[^1] = fieldId;
            fieldIds.Sort();

            int archId = FindArch(fieldIds);
            if (archId == 0)
                archId = CreateArch(fieldIds);

            GraphAt(srcArch, fieldId).Add = archId;
            GraphAt(archId, fieldId).Remove = srcArch;

            return archId;
        }
    }

    internal static int ReduceArch(int srcArch, int fieldId)
    {
        lock (Lock)
        {
            if (GraphAt(srcArch, fieldId).Remove != 0)
                return GraphAt(srcArch, fieldId).Remove;

            var srcFields = ArchFieldIds(srcArch);
            int fieldIndex = srcFields.IndexOf(fieldId);

            Span<int> fieldIds = stackalloc int[srcFields.Length - 1];
            srcFields[..fieldIndex].CopyTo(fieldIds);
            srcFields[(fieldIndex + 1)..].CopyTo(fieldIds[fieldIndex..]);

            int archId = FindArch(fieldIds);
            if (archId == 0)
                archId = CreateArch(fieldIds);

            GraphAt(srcArch, fieldId).Remove = archId;
            GraphAt(archId, fieldId).Add = srcArch;

            return archId;
        }
    }

    private static int FindArch(ReadOnlySpan<int> fieldIds)
    {
        for (int archId = 2; archId < NextArchId; archId++)
        {
            if (ArchFieldIds(archId).SequenceEqual(fieldIds))
                return archId;
        }

        return 0;
    }

    private static int CreateArch(ReadOnlySpan<int> fieldIds)
    {
        int archId = NewArch();
        int startIndex = ArchFieldsIndex;
        int requiredLength = startIndex + fieldIds.Length;

        if (ArchFields.Length < requiredLength)
            Array.Resize(ref ArchFields, (int)BitOperations.RoundUpToPowerOf2((uint)requiredLength));

        fieldIds.CopyTo(ArchFields.AsSpan(startIndex, fieldIds.Length));
        ArchStorageInfo[archId] = new(startIndex, fieldIds.Length);
        ArchFieldsIndex = requiredLength;

        foreach (var fieldId in fieldIds)
            GraphAt(archId, fieldId).Add = archId;

        if (fieldIds.Length == 1)
        {
            int fieldId = fieldIds[0];
            GraphAt(1, fieldId).Add = archId;
            GraphAt(archId, fieldId).Remove = -1;
        }

        return archId;
    }

    internal static int NewArch()
    {
        if (NextArchId == ArchCapacity)
            ResizeGraph(FieldCapacity, ArchCapacity * 2);

        return NextArchId++;
    }

    internal static void ResizeGraph(int fieldCapacity, int archCapacity)
    {
        if (fieldCapacity != FieldCapacity)
        {
            for (int i = 0; i < Graph.Length; i++)
                Array.Resize(ref Graph[i], fieldCapacity);

            Graph[0].AsSpan().Fill(new(-1, 0));
            Array.Resize(ref Handlers, fieldCapacity);
            FieldCapacity = fieldCapacity;
        }

        if (archCapacity != ArchCapacity)
        {
            Array.Resize(ref Graph, archCapacity);
            Array.Resize(ref ArchStorageInfo, archCapacity);
            ArchCapacity = archCapacity;

            for (int i = 0; i < Graph.Length; i++)
            {
                if (Graph[i] == null)
                    Graph[i] = new EntGraphEdge[fieldCapacity];
            }
        }
    }
}
