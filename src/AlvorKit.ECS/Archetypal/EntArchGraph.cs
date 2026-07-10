namespace AlvorKit.ECS;

internal static class EntArchGraph<A>
{
    internal const int NoArchId = 0;
    internal const int UnresolvedTransitionArchId = NoArchId;
    internal const int OutsideGroupArchId = -1;

    private const int TransitionRootArchId = 1;
    private const int FirstArchId = 2;
    private const int FirstFieldId = 1;
    private const int InitialFieldCapacity = 16;
    private const int InitialArchCapacity = 16;
    private const int InitialPackedFieldCapacity = 4096;

    // Guards group-global graph/catalog mutations and jagged-array outer growth.
    internal static readonly object Sync = new();

    private static int nextFieldId = FirstFieldId;
    private static int nextArchId = FirstArchId;
    private static int fieldCapacity;
    private static int archCapacity;
    private static int packedFieldCount;
    private static int[] packedFieldIds = new int[InitialPackedFieldCapacity];
    private static EntArchSignatureRange[] signatureRanges = [];
    private static EntArchTransition[][] transitions = [[]];
    private static EntArchColumnOps[] columnOps = [];

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

        metrics.AddCatalogArray(packedFieldIds, packedFieldCount);
        metrics.AddCatalogArray(signatureRanges, nextArchId);
        metrics.AddCatalogArray(transitions, nextArchId);
        metrics.AddCatalogArray(columnOps, nextFieldId);

        for (int archId = 0; archId < transitions.Length; archId++)
        {
            var transitionsByField = transitions[archId];
            metrics.TransitionCellCapacity += transitionsByField.LongLength;
            metrics.AddCatalogArray(transitionsByField, archId < nextArchId ? nextFieldId : 0);
        }

        // Arch 0 contains outside-group sentinels. Add self-loops encode field presence, not structural edges.
        for (int archId = TransitionRootArchId; archId < nextArchId; archId++)
        {
            var transitionsByField = transitions[archId];

            for (int fieldId = FirstFieldId; fieldId < nextFieldId; fieldId++)
            {
                ref var transition = ref transitionsByField[fieldId];

                if (transition.AddArchId != UnresolvedTransitionArchId && transition.AddArchId != archId)
                    metrics.DirectedStructuralEdgeCount++;

                if (transition.RemoveArchId != UnresolvedTransitionArchId)
                    metrics.DirectedStructuralEdgeCount++;
            }
        }

        metrics.AddCatalogObjects(1L + metrics.RegisteredFieldCount);

        for (int fieldId = FirstFieldId; fieldId < nextFieldId; fieldId++)
            columnOps[fieldId].AccumulateMetrics(ref metrics);
    }

    internal static int RegisterField(EntArchColumnOps ops)
    {
        lock (Sync)
        {
            if (nextFieldId == fieldCapacity)
                EnsureCapacity(fieldCapacity * 2, archCapacity);

            int fieldId = nextFieldId++;
            columnOps[fieldId] = ops;
            return fieldId;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetSingletonArchId(int fieldId) =>
        Transition(TransitionRootArchId, fieldId).AddArchId;

    internal static int ResolveSingleton(int fieldId)
    {
        lock (Sync)
        {
            ref var transition = ref Transition(TransitionRootArchId, fieldId);
            if (transition.AddArchId != UnresolvedTransitionArchId)
                return transition.AddArchId;

            return CreateFromSignature([fieldId]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ContainsField(int archId, int fieldId) =>
        Transition(archId, fieldId).AddArchId == archId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetAddArchId(int archId, int fieldId) =>
        Transition(archId, fieldId).AddArchId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetRemoveArchId(int archId, int fieldId) =>
        Transition(archId, fieldId).RemoveArchId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<int> FieldIds(int archId)
    {
        var range = signatureRanges[archId];
        return new(packedFieldIds, range.Start, range.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static EntArchColumnOps ColumnOps(int fieldId) => columnOps[fieldId];

    internal static int ResolveAdd(int srcArchId, int fieldId)
    {
        lock (Sync)
        {
            ref var transition = ref Transition(srcArchId, fieldId);
            if (transition.AddArchId != UnresolvedTransitionArchId)
                return transition.AddArchId;

            var srcFieldIds = FieldIds(srcArchId);
            Span<int> dstFieldIds = stackalloc int[srcFieldIds.Length + 1];

            srcFieldIds.CopyTo(dstFieldIds);
            dstFieldIds[^1] = fieldId;
            dstFieldIds.Sort();

            int dstArchId = FindBySignature(dstFieldIds);
            if (dstArchId == NoArchId)
                dstArchId = CreateFromSignature(dstFieldIds);

            transition.AddArchId = dstArchId;
            Transition(dstArchId, fieldId).RemoveArchId = srcArchId;
            return dstArchId;
        }
    }

    internal static int ResolveRemove(int srcArchId, int fieldId)
    {
        lock (Sync)
        {
            ref var transition = ref Transition(srcArchId, fieldId);
            if (transition.RemoveArchId != UnresolvedTransitionArchId)
                return transition.RemoveArchId;

            var srcFieldIds = FieldIds(srcArchId);
            int removedFieldIndex = srcFieldIds.IndexOf(fieldId);
            Span<int> dstFieldIds = stackalloc int[srcFieldIds.Length - 1];

            srcFieldIds[..removedFieldIndex].CopyTo(dstFieldIds);
            srcFieldIds[(removedFieldIndex + 1)..].CopyTo(dstFieldIds[removedFieldIndex..]);

            int dstArchId = FindBySignature(dstFieldIds);
            if (dstArchId == NoArchId)
                dstArchId = CreateFromSignature(dstFieldIds);

            transition.RemoveArchId = dstArchId;
            Transition(dstArchId, fieldId).AddArchId = srcArchId;
            return dstArchId;
        }
    }

    private static int FindBySignature(ReadOnlySpan<int> fieldIds)
    {
        for (int archId = FirstArchId; archId < nextArchId; archId++)
        {
            if (FieldIds(archId).SequenceEqual(fieldIds))
                return archId;
        }

        return NoArchId;
    }

    private static int CreateFromSignature(ReadOnlySpan<int> fieldIds)
    {
        int archId = AllocateArchId();
        int start = packedFieldCount;
        int nextPackedFieldCount = start + fieldIds.Length;

        if (packedFieldIds.Length < nextPackedFieldCount)
            Array.Resize(ref packedFieldIds, (int)BitOperations.RoundUpToPowerOf2((uint)nextPackedFieldCount));

        fieldIds.CopyTo(packedFieldIds.AsSpan(start, fieldIds.Length));
        signatureRanges[archId] = new(start, fieldIds.Length);
        packedFieldCount = nextPackedFieldCount;

        foreach (int fieldId in fieldIds)
            Transition(archId, fieldId).AddArchId = archId;

        if (fieldIds.Length == 1)
        {
            int fieldId = fieldIds[0];
            Transition(TransitionRootArchId, fieldId).AddArchId = archId;
            Transition(archId, fieldId).RemoveArchId = OutsideGroupArchId;
        }

        return archId;
    }

    private static int AllocateArchId()
    {
        if (nextArchId == archCapacity)
            EnsureCapacity(fieldCapacity, archCapacity * 2);

        return nextArchId++;
    }

    private static void EnsureCapacity(int requiredFieldCapacity, int requiredArchCapacity)
    {
        if (requiredFieldCapacity != fieldCapacity)
        {
            for (int archId = 0; archId < transitions.Length; archId++)
                Array.Resize(ref transitions[archId], requiredFieldCapacity);

            // A default loc is outside the group, so arch 0 must never look like a field-presence self-loop.
            transitions[NoArchId].AsSpan().Fill(new(OutsideGroupArchId, UnresolvedTransitionArchId));
            Array.Resize(ref columnOps, requiredFieldCapacity);
            fieldCapacity = requiredFieldCapacity;
        }

        if (requiredArchCapacity != archCapacity)
        {
            Array.Resize(ref transitions, requiredArchCapacity);
            Array.Resize(ref signatureRanges, requiredArchCapacity);
            archCapacity = requiredArchCapacity;

            for (int archId = 0; archId < transitions.Length; archId++)
            {
                if (transitions[archId] == null)
                    transitions[archId] = new EntArchTransition[fieldCapacity];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref EntArchTransition Transition(int archId, int fieldId) =>
        ref transitions[archId][fieldId];
}
