namespace AlvorKit.ECS;

/// <summary>Caches group-global matching arch IDs for one exact compile-time query selection.</summary>
internal static class EntArchQueryCache<A, TSelect>
    where TSelect : struct, IEntArchSelect<A>
{
    private const int InitialArchIdCapacity = 4;
    /// <summary>Marks the low packed scan word after the membership array is completely initialized.</summary>
    private const uint MatchBitsReadyFlag = 1U << 31;

    private static int[] matchingArchIds = [];
    /// <summary>Stores lazily published constant-time membership for active-side scans.</summary>
    private static ulong[] matchingArchBits = [];
    // Publish the scanned arch end and matching prefix length as one atomic pair.
    private static long publishedScan = Pack(EntArchGraph<A>.FirstArchId, 0, false);

    /// <summary>Captures the immutable prefix length available to one query enumeration.</summary>
    internal static int CaptureCount(int allocId, out bool matchBitsReady)
    {
        int graphArchEnd = EntArchGraph<A>.PublishedArchEnd;
        long snapshot = Volatile.Read(ref publishedScan);
        if (DecodeScannedArchEnd(snapshot) < graphArchEnd)
            snapshot = Refresh(allocId);

        matchBitsReady = DecodeMatchBitsReady(snapshot);
        return DecodeMatchingArchCount(snapshot);
    }

    /// <summary>Gets one ID from the immutable matching prefix.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ArchIdAt(int index) => matchingArchIds[index];

    /// <summary>Ensures active-row-set enumeration can test one arch ID in constant time.</summary>
    internal static void EnsureMatchBits()
    {
        lock (EntArchGraph<A>.Sync)
        {
            long snapshot = Volatile.Read(ref publishedScan);
            if (DecodeMatchBitsReady(snapshot))
                return;

            int wordCount = WordCount(DecodeScannedArchEnd(snapshot));
            var initializedBits = new ulong[(int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(1, wordCount))];
            int matchingArchCount = DecodeMatchingArchCount(snapshot);
            for (int index = 0; index < matchingArchCount; index++)
                SetMatchBit(initializedBits, matchingArchIds[index]);

            matchingArchBits = initializedBits;
            Volatile.Write(
                ref publishedScan,
                Pack(DecodeScannedArchEnd(snapshot), matchingArchCount, true));
        }
    }

    /// <summary>Returns whether the lazily built membership bits contain an arch ID.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool MatchesArch(int archId) =>
        (matchingArchBits[archId >> 6] & (1UL << (archId & 63))) != 0;

    /// <summary>Gets the number of matching IDs in the currently published cache prefix.</summary>
    internal static int MatchingArchCount =>
        DecodeMatchingArchCount(Volatile.Read(ref publishedScan));

    /// <summary>Gets the exclusive arch ID through which the cache has inspected signatures.</summary>
    internal static int ScannedArchEnd =>
        DecodeScannedArchEnd(Volatile.Read(ref publishedScan));

    /// <summary>Extends the cached immutable prefix through every currently published arch.</summary>
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
            bool matchBitsReady = DecodeMatchBitsReady(snapshot);
            if (matchBitsReady)
                EnsureBitCapacity(graphArchEnd);
            for (int archId = scannedArchEnd; archId < graphArchEnd; archId++)
            {
                if (TSelect.Matches(allocId, archId))
                {
                    Append(archId, ref matchingArchCount);
                    if (matchBitsReady)
                        SetMatchBit(matchingArchBits, archId);
                }
            }

            snapshot = Pack(graphArchEnd, matchingArchCount, matchBitsReady);
            Volatile.Write(ref publishedScan, snapshot);
            return snapshot;
        }
    }

    /// <summary>Appends one matching arch ID to the cache's immutable published prefix.</summary>
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

    /// <summary>Grows membership storage to contain every arch below an exclusive end.</summary>
    private static void EnsureBitCapacity(int archEnd)
    {
        int wordCount = WordCount(archEnd);
        if (matchingArchBits.Length < wordCount)
            Array.Resize(ref matchingArchBits, (int)BitOperations.RoundUpToPowerOf2((uint)wordCount));
    }

    /// <summary>Marks one arch as matching in initialized or already published membership storage.</summary>
    private static void SetMatchBit(ulong[] bits, int archId) =>
        bits[archId >> 6] |= 1UL << (archId & 63);

    /// <summary>Returns the number of 64-bit words covering an exclusive arch end.</summary>
    private static int WordCount(int archEnd) => (archEnd + 63) >> 6;

    /// <summary>Packs the scanned prefix, matching count, and membership publication state into one atomic snapshot.</summary>
    private static long Pack(int scannedArchEnd, int matchingArchCount, bool matchBitsReady) =>
        ((long)scannedArchEnd << 32) |
        (matchBitsReady ? MatchBitsReadyFlag : 0) |
        (uint)matchingArchCount;

    /// <summary>Decodes the exclusive scanned arch end from a published snapshot.</summary>
    private static int DecodeScannedArchEnd(long value) => (int)(value >> 32);

    /// <summary>Decodes the matching prefix length from a published snapshot.</summary>
    private static int DecodeMatchingArchCount(long value) =>
        (int)((uint)value & ~MatchBitsReadyFlag);

    /// <summary>Returns whether a published snapshot makes the membership array safe for lock-free reads.</summary>
    private static bool DecodeMatchBitsReady(long value) =>
        ((uint)value & MatchBitsReadyFlag) != 0;
}
