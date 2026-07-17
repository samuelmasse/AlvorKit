namespace AlvorKit.ECS;

/// <summary>Caches group-global matching arch IDs for one exact compile-time query selection.</summary>
internal static class EntArchQueryCache<A, TSelect>
    where TSelect : struct, IEntArchSelect<A>
{
    private const int InitialArchIdCapacity = 4;

    private static int[] matchingArchIds = [];
    // Publish the scanned arch end and matching prefix length as one atomic pair.
    private static long publishedScan = Pack(EntArchGraph<A>.FirstArchId, 0);

    /// <summary>Captures the immutable prefix length available to one query enumeration.</summary>
    internal static int CaptureCount(int allocId)
    {
        int graphArchEnd = EntArchGraph<A>.PublishedArchEnd;
        long snapshot = Volatile.Read(ref publishedScan);
        if (DecodeScannedArchEnd(snapshot) < graphArchEnd)
            snapshot = Refresh(allocId);

        return DecodeMatchingArchCount(snapshot);
    }

    /// <summary>Gets one ID from the immutable matching prefix.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ArchIdAt(int index) => matchingArchIds[index];

    /// <summary>Gets the number of matching IDs in the currently published cache prefix.</summary>
    internal static int MatchingArchCount =>
        DecodeMatchingArchCount(Volatile.Read(ref publishedScan));

    /// <summary>Gets the exclusive arch ID through which the cache has inspected signatures.</summary>
    internal static int ScannedArchEnd =>
        DecodeScannedArchEnd(Volatile.Read(ref publishedScan));

    private static long Refresh(int allocId)
    {
        lock (EntArchGraph<A>.Sync)
        {
            long snapshot = Volatile.Read(ref publishedScan);
            int scannedArchEnd = DecodeScannedArchEnd(snapshot);
            int graphArchEnd = EntArchGraph<A>.PublishedArchEnd;
            if (scannedArchEnd >= graphArchEnd)
                return snapshot;

            int matchingArchCount = DecodeMatchingArchCount(snapshot);
            for (int archId = scannedArchEnd; archId < graphArchEnd; archId++)
            {
                if (TSelect.Matches(allocId, archId))
                    Append(archId, ref matchingArchCount);
            }

            snapshot = Pack(graphArchEnd, matchingArchCount);
            Volatile.Write(ref publishedScan, snapshot);
            return snapshot;
        }
    }

    private static void Append(int archId, ref int count)
    {
        if (count == matchingArchIds.Length)
        {
            // Avoid separate one- and two-entry arrays while keeping an unused cache allocation-free.
            int capacity = count == 0
                ? InitialArchIdCapacity
                : count * 2;
            Array.Resize(ref matchingArchIds, capacity);
        }

        matchingArchIds[count++] = archId;
    }

    private static long Pack(int scannedArchEnd, int matchingArchCount) =>
        ((long)scannedArchEnd << 32) | (uint)matchingArchCount;

    private static int DecodeScannedArchEnd(long value) => (int)(value >> 32);

    private static int DecodeMatchingArchCount(long value) => (int)value;
}
