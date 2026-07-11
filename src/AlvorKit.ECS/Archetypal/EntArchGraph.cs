namespace AlvorKit.ECS;

internal static class EntArchGraph<A>
{
    internal const int NoArchId = 0;
    internal const int UnresolvedTransitionArchId = NoArchId;
    internal const int NoFieldOrdinal = -1;

    private const int NoEdgeIndex = 0;
    private const int FirstArchId = 1;
    private const int FirstEdgeIndex = 1;
    private const int FirstFieldId = 1;
    private const int FirstStorageClassId = 1;
    private const int InitialFieldCapacity = 16;
    private const int InitialArchCapacity = 16;
    private const int InitialPackedFieldCapacity = 4096;
    private const int InitialSignatureIndexCapacity = 16;

    // Guards group-global graph/catalog mutations and shared-array growth.
    internal static readonly object Sync = new();

    private static int nextFieldId = FirstFieldId;
    private static int nextArchId = FirstArchId;
    private static int nextEdgeIndex = FirstEdgeIndex;
    private static int nextStorageClassId = FirstStorageClassId;
    private static int fieldCapacity;
    private static int archCapacity;
    private static int packedFieldCount;
    private static int singletonArchCount;
    private static int[] packedFieldIds = new int[InitialPackedFieldCapacity];
    private static EntArchFieldLayout[] packedFieldLayouts = [];
    // Structural resolution holds Sync, so one reusable buffer supports unbounded signature widths without stack growth.
    private static int[] signatureScratch = [];
    // Real arch IDs and their packed signatures are appended in the same order, so the previous end is the next start.
    private static int[] signatureEnds = [];
    private static int[] signatureArchIds = [];
    private static int[] edgeHeads = [];
    private static EntArchEdge[] edges = [];
    private static int[] singletonArchIds = [];
    private static EntArchColumnOps[] columnOps = [];
    private static EntArchField[] fields = [];

    static EntArchGraph()
    {
        EnsureCapacity(InitialFieldCapacity, InitialArchCapacity);
    }

    internal static int ArchCapacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => archCapacity;
    }

    internal static void AccumulateMetrics(ref EntArchMetrics metrics)
    {
        metrics.RegisteredFieldCount = nextFieldId - FirstFieldId;
        metrics.FieldCapacity = fieldCapacity;
        metrics.MaterializedArchCount = nextArchId - FirstArchId;
        metrics.ArchCapacity = archCapacity;
        metrics.SignatureMembershipCount = packedFieldCount;
        metrics.SignatureMembershipCapacity = packedFieldIds.Length;
        metrics.FieldLayoutCount = packedFieldCount;
        metrics.FieldLayoutCapacity = packedFieldLayouts.Length;
        metrics.SignatureIndexCount = metrics.MaterializedArchCount;
        metrics.SignatureIndexCapacity = signatureArchIds.Length;
        metrics.SignatureScratchCapacity = signatureScratch.Length;
        metrics.SingletonArchCount = singletonArchCount;
        metrics.SingletonDirectoryCapacity = singletonArchIds.Length;
        metrics.StoredTransitionEdgeCount = nextEdgeIndex - FirstEdgeIndex;
        metrics.TransitionEdgeCapacity = edges.Length;
        metrics.EdgeHeadCapacity = edgeHeads.Length;
        metrics.DirectedStructuralEdgeCount = metrics.StoredTransitionEdgeCount + 2L * singletonArchCount;

        metrics.AddCatalogArray(packedFieldIds, packedFieldCount);
        metrics.AddCatalogArray(packedFieldLayouts, packedFieldCount);
        metrics.AddCatalogArray(signatureScratch, 0);
        metrics.AddCatalogArray(signatureEnds, nextArchId);
        metrics.AddCatalogArray(signatureArchIds, metrics.SignatureIndexCount);
        metrics.AddCatalogArray(edgeHeads, nextArchId);
        metrics.AddCatalogArray(edges, edges.Length == 0 ? 0 : nextEdgeIndex);
        metrics.AddCatalogArray(singletonArchIds, nextFieldId);
        metrics.AddCatalogArray(columnOps, nextFieldId);
        metrics.AddCatalogArray(fields, nextFieldId);

        metrics.AddCatalogObjects(1L + metrics.RegisteredFieldCount);

        for (int fieldId = FirstFieldId; fieldId < nextFieldId; fieldId++)
            columnOps[fieldId].AccumulateMetrics(ref metrics);
    }

    internal static int RegisterStorageClass()
    {
        lock (Sync)
        {
            return nextStorageClassId++;
        }
    }

    internal static int RegisterField(EntArchColumnOps ops, int byteWidth, int storageClassId)
    {
        lock (Sync)
        {
            if (nextFieldId == fieldCapacity)
                EnsureCapacity(fieldCapacity * 2, archCapacity);

            int fieldId = nextFieldId++;
            columnOps[fieldId] = ops;
            fields[fieldId] = new(byteWidth, storageClassId);
            return fieldId;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetSingletonArchId(int fieldId) =>
        Volatile.Read(ref singletonArchIds[fieldId]);

    internal static int ResolveSingleton(int fieldId)
    {
        lock (Sync)
        {
            int archId = singletonArchIds[fieldId];
            if (archId != NoArchId)
                return archId;

            return CreateFromSignature([fieldId]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FindFieldOrdinal(int archId, int fieldId)
    {
        if (archId == NoArchId)
            return NoFieldOrdinal;

        return FieldIds(archId).IndexOf(fieldId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetTransitionArchId(int archId, int fieldId) =>
        FindEdgeArchId(archId, fieldId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<int> FieldIds(int archId)
    {
        int start = signatureEnds[archId - 1];
        int end = signatureEnds[archId];
        return new(packedFieldIds, start, end - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<EntArchFieldLayout> FieldLayouts(int archId)
    {
        int start = signatureEnds[archId - 1];
        int end = signatureEnds[archId];
        return new(packedFieldLayouts, start, end - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSingleton(int archId) =>
        signatureEnds[archId] - signatureEnds[archId - 1] == 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static EntArchColumnOps ColumnOps(int fieldId) => columnOps[fieldId];

    internal static int ResolveAdd(int srcArchId, int fieldId)
    {
        lock (Sync)
        {
            int dstArchId = FindEdgeArchId(srcArchId, fieldId);
            if (dstArchId != UnresolvedTransitionArchId)
                return dstArchId;

            var srcFieldIds = FieldIds(srcArchId);
            Span<int> dstFieldIds = SignatureScratch(srcFieldIds.Length + 1);
            InsertFieldId(srcFieldIds, fieldId, dstFieldIds);

            ulong signatureHash = EntArchSignatureIndex.Hash(dstFieldIds);
            dstArchId = FindBySignature(dstFieldIds, signatureHash);
            if (dstArchId == NoArchId)
                dstArchId = CreateFromSignature(dstFieldIds, signatureHash);

            CacheEdgePair(srcArchId, fieldId, dstArchId);
            return dstArchId;
        }
    }

    internal static int ResolveRemove(int srcArchId, int fieldId)
    {
        lock (Sync)
        {
            int dstArchId = FindEdgeArchId(srcArchId, fieldId);
            if (dstArchId != UnresolvedTransitionArchId)
                return dstArchId;

            var srcFieldIds = FieldIds(srcArchId);
            int fieldOrdinal = srcFieldIds.IndexOf(fieldId);
            Span<int> dstFieldIds = SignatureScratch(srcFieldIds.Length - 1);

            srcFieldIds[..fieldOrdinal].CopyTo(dstFieldIds);
            srcFieldIds[(fieldOrdinal + 1)..].CopyTo(dstFieldIds[fieldOrdinal..]);

            ulong signatureHash = EntArchSignatureIndex.Hash(dstFieldIds);
            dstArchId = FindBySignature(dstFieldIds, signatureHash);
            if (dstArchId == NoArchId)
                dstArchId = CreateFromSignature(dstFieldIds, signatureHash);

            CacheEdgePair(srcArchId, fieldId, dstArchId);
            return dstArchId;
        }
    }

    private static int FindBySignature(ReadOnlySpan<int> fieldIds, ulong signatureHash) =>
        EntArchSignatureIndex.Find(signatureArchIds, signatureHash, fieldIds, packedFieldIds, signatureEnds);

    private static int CreateFromSignature(ReadOnlySpan<int> fieldIds) =>
        CreateFromSignature(fieldIds, EntArchSignatureIndex.Hash(fieldIds));

    private static int CreateFromSignature(ReadOnlySpan<int> fieldIds, ulong signatureHash)
    {
        int archId = AllocateArchId();
        int start = packedFieldCount;
        int nextPackedFieldCount = start + fieldIds.Length;

        if (packedFieldIds.Length < nextPackedFieldCount)
            Array.Resize(ref packedFieldIds, (int)BitOperations.RoundUpToPowerOf2((uint)nextPackedFieldCount));
        if (packedFieldLayouts.Length < nextPackedFieldCount)
            Array.Resize(ref packedFieldLayouts, (int)BitOperations.RoundUpToPowerOf2((uint)nextPackedFieldCount));

        fieldIds.CopyTo(packedFieldIds.AsSpan(start, fieldIds.Length));
        CreateFieldLayouts(fieldIds, packedFieldLayouts.AsSpan(start, fieldIds.Length));
        signatureEnds[archId] = nextPackedFieldCount;
        packedFieldCount = nextPackedFieldCount;

        EnsureSignatureIndexCapacity(nextArchId - FirstArchId);
        EntArchSignatureIndex.Insert(signatureArchIds, signatureHash, archId);

        if (fieldIds.Length == 1)
        {
            int fieldId = fieldIds[0];
            Volatile.Write(ref singletonArchIds[fieldId], archId);
            singletonArchCount++;
        }

        return archId;
    }

    private static int AllocateArchId()
    {
        if (nextArchId == archCapacity)
            EnsureCapacity(fieldCapacity, archCapacity * 2);

        return nextArchId++;
    }

    private static void EnsureSignatureIndexCapacity(int requiredCount)
    {
        // Grow before the next insertion would exceed a 75% load.
        int maxCount = signatureArchIds.Length - (signatureArchIds.Length >> 2);
        if (requiredCount <= maxCount)
            return;

        var previousArchIds = signatureArchIds;
        int nextCapacity = previousArchIds.Length == 0
            ? InitialSignatureIndexCapacity
            : previousArchIds.Length * 2;
        signatureArchIds = new int[nextCapacity];

        foreach (int archId in previousArchIds)
        {
            if (archId != NoArchId)
                EntArchSignatureIndex.Insert(signatureArchIds, EntArchSignatureIndex.Hash(FieldIds(archId)), archId);
        }
    }

    private static Span<int> SignatureScratch(int requiredLength)
    {
        if (signatureScratch.Length < requiredLength)
            Array.Resize(ref signatureScratch, (int)BitOperations.RoundUpToPowerOf2((uint)requiredLength));

        return signatureScratch.AsSpan(0, requiredLength);
    }

    private static void EnsureCapacity(int requiredFieldCapacity, int requiredArchCapacity)
    {
        if (requiredFieldCapacity != fieldCapacity)
        {
            Array.Resize(ref columnOps, requiredFieldCapacity);
            Array.Resize(ref fields, requiredFieldCapacity);
            Array.Resize(ref singletonArchIds, requiredFieldCapacity);
            fieldCapacity = requiredFieldCapacity;
        }

        if (requiredArchCapacity != archCapacity)
        {
            Array.Resize(ref signatureEnds, requiredArchCapacity);
            Array.Resize(ref edgeHeads, requiredArchCapacity);
            archCapacity = requiredArchCapacity;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindEdgeArchId(int archId, int fieldId)
    {
        int edgeIndex = Volatile.Read(ref edgeHeads[archId]);
        var publishedEdges = edges;
        while (edgeIndex != NoEdgeIndex)
        {
            ref readonly var edge = ref publishedEdges[edgeIndex];
            if (edge.FieldId == fieldId)
                return edge.DstArchId;

            edgeIndex = edge.NextEdgeIndex;
        }

        return UnresolvedTransitionArchId;
    }

    private static void CacheEdgePair(int srcArchId, int fieldId, int dstArchId)
    {
        EnsureEdgeCapacity(nextEdgeIndex + 2);
        int srcEdgeIndex = nextEdgeIndex++;
        int dstEdgeIndex = nextEdgeIndex++;

        edges[srcEdgeIndex] = new(fieldId, dstArchId, edgeHeads[srcArchId]);
        edges[dstEdgeIndex] = new(fieldId, srcArchId, edgeHeads[dstArchId]);

        Volatile.Write(ref edgeHeads[srcArchId], srcEdgeIndex);
        Volatile.Write(ref edgeHeads[dstArchId], dstEdgeIndex);
    }

    private static void EnsureEdgeCapacity(int requiredLength)
    {
        if (edges.Length >= requiredLength)
            return;

        Array.Resize(ref edges, (int)BitOperations.RoundUpToPowerOf2((uint)requiredLength));
    }

    private static void InsertFieldId(ReadOnlySpan<int> srcFieldIds, int fieldId, Span<int> dstFieldIds)
    {
        int insertionIndex = 0;
        while (insertionIndex < srcFieldIds.Length && srcFieldIds[insertionIndex] < fieldId)
            insertionIndex++;

        srcFieldIds[..insertionIndex].CopyTo(dstFieldIds);
        dstFieldIds[insertionIndex] = fieldId;
        srcFieldIds[insertionIndex..].CopyTo(dstFieldIds[(insertionIndex + 1)..]);
    }

    private static void CreateFieldLayouts(ReadOnlySpan<int> fieldIds, Span<EntArchFieldLayout> layouts)
    {
        int bytePrefix = Unsafe.SizeOf<EntMut>();

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            ref readonly var field = ref fields[fieldIds[fieldIndex]];
            if (!field.ContainsReferences)
            {
                layouts[fieldIndex] = EntArchFieldLayout.ReferenceFree(bytePrefix);
                bytePrefix += field.ByteWidth;
                continue;
            }

            int typeColumn = 0;
            for (int previousIndex = 0; previousIndex < fieldIndex; previousIndex++)
            {
                if (fields[fieldIds[previousIndex]].StorageClassId == field.StorageClassId)
                    typeColumn++;
            }

            layouts[fieldIndex] = EntArchFieldLayout.ReferenceContaining(typeColumn);
        }
    }
}
