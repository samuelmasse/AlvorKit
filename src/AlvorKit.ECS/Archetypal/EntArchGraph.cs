namespace AlvorKit.ECS;

/// <summary>Interns immutable arch signatures and their observed structural transitions for one group.</summary>
internal static partial class EntArchGraph<A>
{
    internal const int NoArchId = 0;
    internal const int UnresolvedTransitionArchId = NoArchId;
    internal const int NoFieldOrdinal = -1;

    private const int NoEdgeIndex = 0;
    internal const int FirstArchId = 1;
    private const int FirstEdgeIndex = 1;
    private const int FirstFieldId = 1;
    private const int InitialFieldCapacity = 16;
    private const int InitialArchCapacity = 16;
    private const int InitialPackedFieldCapacity = 4096;
    private const int InitialSignatureIndexCapacity = 16;
    private const int LinearEdgeThreshold = 8;
    private const int InitialTransitionIndexCapacity = 32;

    // Guards group-global graph/catalog mutations and shared-array growth.
    internal static readonly object Sync = new();

    private static int nextFieldId = FirstFieldId;
    private static int nextArchId = FirstArchId;
    private static int publishedArchEnd = FirstArchId;
    private static int nextEdgeIndex = FirstEdgeIndex;
    private static int fieldCapacity;
    private static int archCapacity;
    private static int packedFieldCount;
    private static int singletonArchCount;
    /// <summary>Counts archs whose observed transitions crossed the indexed-lookup threshold.</summary>
    private static int highDegreeArchCount;
    private static int[] packedFieldIds = new int[InitialPackedFieldCapacity];
    // Structural resolution holds Sync, so one reusable buffer supports unbounded signature widths without stack growth.
    private static int[] signatureScratch = [];
    // Real arch IDs and their packed signatures are appended in the same order, so the previous end is the next start.
    private static int[] signatureEnds = [];
    private static int[] signatureArchIds = [];
    private static int[] edgeHeads = [];
    private static EntArchEdge[] edges = [];
    /// <summary>Stores transitions for every high-degree arch in one sparse group-wide table.</summary>
    private static EntArchTransitionIndex? transitionIndex;
    private static int[] singletonArchIds = [];
    private static EntArchColumnOps[] columnOps = [];

    static EntArchGraph()
    {
        EnsureCapacity(InitialFieldCapacity, InitialArchCapacity);
    }

    /// <summary>Gets the current arch-directory capacity.</summary>
    internal static int ArchCapacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => archCapacity;
    }

    /// <summary>Gets the exclusive end of the fully initialized arch ID range.</summary>
    internal static int PublishedArchEnd
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Volatile.Read(ref publishedArchEnd);
    }

    /// <summary>Adds graph, signature, transition, and column storage to one diagnostic snapshot.</summary>
    internal static void AccumulateMetrics(ref EntArchMetrics metrics)
    {
        metrics.RegisteredFieldCount = nextFieldId - FirstFieldId;
        metrics.FieldCapacity = fieldCapacity;
        metrics.MaterializedArchCount = nextArchId - FirstArchId;
        metrics.ArchCapacity = archCapacity;
        metrics.SignatureMembershipCount = packedFieldCount;
        metrics.SignatureMembershipCapacity = packedFieldIds.Length;
        metrics.SignatureIndexCount = metrics.MaterializedArchCount;
        metrics.SignatureIndexCapacity = signatureArchIds.Length;
        metrics.SignatureScratchCapacity = signatureScratch.Length;
        metrics.SingletonArchCount = singletonArchCount;
        metrics.SingletonDirectoryCapacity = singletonArchIds.Length;
        metrics.StoredTransitionEdgeCount = nextEdgeIndex - FirstEdgeIndex;
        metrics.TransitionEdgeCapacity = edges.Length;
        metrics.EdgeHeadCapacity = edgeHeads.Length;
        metrics.HighDegreeArchCount = highDegreeArchCount;
        metrics.TransitionIndexCount = transitionIndex?.Count ?? 0;
        metrics.TransitionIndexCapacity = transitionIndex?.Keys.Length ?? 0;
        metrics.DirectedStructuralEdgeCount = metrics.StoredTransitionEdgeCount + 2L * singletonArchCount;

        metrics.AddCatalogArray(packedFieldIds, packedFieldCount);
        metrics.AddCatalogArray(signatureScratch, 0);
        metrics.AddCatalogArray(signatureEnds, nextArchId);
        metrics.AddCatalogArray(signatureArchIds, metrics.SignatureIndexCount);
        metrics.AddCatalogArray(edgeHeads, nextArchId);
        metrics.AddCatalogArray(edges, edges.Length == 0 ? 0 : nextEdgeIndex);
        metrics.AddCatalogArray(singletonArchIds, nextFieldId);
        metrics.AddCatalogArray(columnOps, nextFieldId);

        if (transitionIndex != null)
        {
            metrics.AddCatalogArray(transitionIndex.Keys, transitionIndex.Count);
            metrics.AddCatalogArray(transitionIndex.DstArchIds, transitionIndex.Count);
            metrics.AddCatalogObjects(1);
        }

        metrics.AddCatalogObjects(1L + metrics.RegisteredFieldCount);

        for (int fieldId = FirstFieldId; fieldId < nextFieldId; fieldId++)
            columnOps[fieldId].AccumulateMetrics(ref metrics);
    }

    /// <summary>Registers one exact closed-generic component field and returns its stable field ID.</summary>
    internal static int RegisterField(EntArchColumnOps ops)
    {
        lock (Sync)
        {
            if (nextFieldId == fieldCapacity)
                EnsureCapacity(fieldCapacity * 2, archCapacity);

            int fieldId = nextFieldId++;
            columnOps[fieldId] = ops;
            lock (EntReg.Lock)
                EntReg.ComponentViews.Add(ops);
            return fieldId;
        }
    }

    /// <summary>Returns the published singleton arch for a field, or zero until it is materialized.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetSingletonArchId(int fieldId) =>
        Volatile.Read(ref singletonArchIds[fieldId]);

    /// <summary>Returns or materializes the singleton arch for one field.</summary>
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

    /// <summary>Returns or materializes the canonical arch described by a typed creation initializer.</summary>
    internal static int ResolveSignature<TInit>()
        where TInit : struct, IEntArchInit<A>
    {
        lock (Sync)
        {
            Span<int> fieldIds = SignatureScratch(TInit.FieldCount);
            TInit.WriteFieldIds(fieldIds);
            fieldIds.Sort();

            ulong signatureHash = EntArchSignatureIndex.Hash(fieldIds);
            EnsureSignatureIndexCapacity(1);
            int archId = FindBySignature(fieldIds, signatureHash);
            return archId == NoArchId
                ? CreateFromSignature(fieldIds, signatureHash)
                : archId;
        }
    }

    /// <summary>Returns a field's ordinal in an arch signature, or <see cref="NoFieldOrdinal"/> when absent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FindFieldOrdinal(int archId, int fieldId)
    {
        if (archId == NoArchId)
            return NoFieldOrdinal;

        return FieldIds(archId).IndexOf(fieldId);
    }

    /// <summary>Returns an observed structural transition destination, or zero when unresolved.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetTransitionArchId(int archId, int fieldId) =>
        FindEdgeArchId(archId, fieldId);

    /// <summary>Gets the immutable sorted field IDs belonging to an arch.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<int> FieldIds(int archId)
    {
        int start = signatureEnds[archId - 1];
        int end = signatureEnds[archId];
        return new(packedFieldIds, start, end - start);
    }

    /// <summary>Returns whether an arch contains exactly one field.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSingleton(int archId) =>
        signatureEnds[archId] - signatureEnds[archId - 1] == 1;

    /// <summary>Gets the cold structural operations registered for a field.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static EntArchColumnOps ColumnOps(int fieldId) => columnOps[fieldId];

    /// <summary>Returns or materializes the arch reached by adding one field.</summary>
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

    /// <summary>Returns or materializes the arch reached by removing one field.</summary>
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
}
