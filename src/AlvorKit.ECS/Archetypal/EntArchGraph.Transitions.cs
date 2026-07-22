namespace AlvorKit.ECS;

// Transition lookup stays on the same closed generic type so the source split introduces no forwarding layer.
internal static partial class EntArchGraph<A>
{
    /// <summary>Finds an observed transition through its linear or indexed representation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindEdgeArchId(int archId, int fieldId)
    {
        int edgeIndex = Volatile.Read(ref edgeHeads[archId]);
        if (edgeIndex < NoEdgeIndex)
            return FindIndexedEdgeArchId(archId, fieldId);

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

    /// <summary>Publishes both directions of one observed add/remove transition.</summary>
    private static void CacheEdgePair(int srcArchId, int fieldId, int dstArchId)
    {
        EnsureEdgeCapacity(nextEdgeIndex + 2);
        int srcEdgeIndex = nextEdgeIndex++;
        int dstEdgeIndex = nextEdgeIndex++;

        CacheEdge(srcArchId, fieldId, dstArchId, srcEdgeIndex);
        CacheEdge(dstArchId, fieldId, srcArchId, dstEdgeIndex);
    }

    /// <summary>Publishes one directed transition and promotes its arch when its degree becomes high.</summary>
    private static void CacheEdge(int archId, int fieldId, int dstArchId, int edgeIndex)
    {
        int publishedHead = edgeHeads[archId];
        int previousHead = publishedHead < NoEdgeIndex ? -publishedHead : publishedHead;
        edges[edgeIndex] = new(fieldId, dstArchId, previousHead);

        bool indexed = publishedHead < NoEdgeIndex;
        if (!indexed && ReachesIndexThreshold(edgeIndex))
        {
            IndexEdges(archId, edgeIndex);
            highDegreeArchCount++;
            indexed = true;
        }
        else if (indexed)
        {
            EnsureTransitionIndexCapacity(transitionIndex!.Count + 1);
            InsertIndexedEdge(transitionIndex!, PackEdgeKey(archId, fieldId), dstArchId);
        }

        Volatile.Write(ref edgeHeads[archId], indexed ? -edgeIndex : edgeIndex);
    }

    /// <summary>Returns whether a transition chain is long enough to require indexed lookup.</summary>
    private static bool ReachesIndexThreshold(int edgeIndex)
    {
        for (int count = 0; count <= LinearEdgeThreshold; count++)
        {
            if (edgeIndex == NoEdgeIndex)
                return false;
            edgeIndex = edges[edgeIndex].NextEdgeIndex;
        }

        return true;
    }

    /// <summary>Adds an arch's existing transition chain to the shared sparse index.</summary>
    private static void IndexEdges(int archId, int edgeIndex)
    {
        int edgeCount = LinearEdgeThreshold + 1;
        EnsureTransitionIndexCapacity((transitionIndex?.Count ?? 0) + edgeCount);
        var index = transitionIndex!;
        for (int remaining = edgeCount; remaining != 0; remaining--)
        {
            ref readonly var edge = ref edges[edgeIndex];
            InsertIndexedEdge(index, PackEdgeKey(archId, edge.FieldId), edge.DstArchId);
            edgeIndex = edge.NextEdgeIndex;
        }
    }

    /// <summary>Finds one transition in the shared sparse index.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindIndexedEdgeArchId(int archId, int fieldId)
    {
        var index = Volatile.Read(ref transitionIndex)!;
        ulong key = PackEdgeKey(archId, fieldId);
        int slot = EdgeHash(key) & (index.Keys.Length - 1);
        while (true)
        {
            ulong storedKey = Volatile.Read(ref index.Keys[slot]);
            if (storedKey == key)
                return index.DstArchIds[slot];
            if (storedKey == 0)
                return UnresolvedTransitionArchId;
            slot = (slot + 1) & (index.Keys.Length - 1);
        }
    }

    /// <summary>Maintains a maximum 50% transition-index load and republishes grown storage.</summary>
    private static void EnsureTransitionIndexCapacity(int requiredCount)
    {
        var current = transitionIndex;
        int capacity = current?.Keys.Length ?? 0;
        if (capacity != 0 && requiredCount <= capacity / 2)
            return;

        int nextCapacity = capacity == 0 ? InitialTransitionIndexCapacity : capacity * 2;
        while (requiredCount > nextCapacity / 2)
            nextCapacity *= 2;

        var grown = new EntArchTransitionIndex(nextCapacity);
        if (current != null)
        {
            for (int slot = 0; slot < current.Keys.Length; slot++)
            {
                ulong key = current.Keys[slot];
                if (key != 0)
                    InsertIndexedEdge(grown, key, current.DstArchIds[slot]);
            }
        }

        Volatile.Write(ref transitionIndex, grown);
    }

    /// <summary>Inserts one transition into a table with sufficient capacity.</summary>
    private static void InsertIndexedEdge(EntArchTransitionIndex index, ulong key, int dstArchId)
    {
        int slot = EdgeHash(key) & (index.Keys.Length - 1);
        while (index.Keys[slot] != 0)
            slot = (slot + 1) & (index.Keys.Length - 1);

        index.DstArchIds[slot] = dstArchId;
        Volatile.Write(ref index.Keys[slot], key);
        index.Count++;
    }

    /// <summary>Packs an arch and field into the nonzero transition key.</summary>
    private static ulong PackEdgeKey(int archId, int fieldId) =>
        ((ulong)(uint)archId << 32) | (uint)fieldId;

    /// <summary>Hashes one packed transition key for a power-of-two table.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EdgeHash(ulong key)
    {
        uint folded = (uint)key ^ (uint)(key >> 32);
        return (int)(folded * 0x9E3779B1u);
    }

    /// <summary>Grows the append-only linear transition arena.</summary>
    private static void EnsureEdgeCapacity(int requiredLength)
    {
        if (edges.Length >= requiredLength)
            return;

        Array.Resize(ref edges, (int)BitOperations.RoundUpToPowerOf2((uint)requiredLength));
    }
}
