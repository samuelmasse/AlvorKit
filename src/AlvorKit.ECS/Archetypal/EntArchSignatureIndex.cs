namespace AlvorKit.ECS;

internal static class EntArchSignatureIndex
{
    // Real arch IDs start above zero, so zero identifies a slot that has never been occupied.
    internal const int EmptySlot = 0;

    // 64-bit FNV-1a over the signature length and each field ID as a little-endian uint.
    private const ulong HashOffset = 14695981039346656037UL;
    private const ulong HashPrime = 1099511628211UL;

    internal static ulong Hash(ReadOnlySpan<int> fieldIds)
    {
        ulong hash = Mix(HashOffset, (uint)fieldIds.Length);

        foreach (int fieldId in fieldIds)
            hash = Mix(hash, (uint)fieldId);

        return hash;
    }

    internal static int Find(
        int[] archIds,
        ulong hash,
        ReadOnlySpan<int> fieldIds,
        int[] packedFieldIds,
        int[] signatureEnds)
    {
        int mask = archIds.Length - 1;
        int slot = InitialSlot(hash, mask);

        while (true)
        {
            int archId = archIds[slot];
            if (archId == EmptySlot)
                return EmptySlot;

            int start = signatureEnds[archId - 1];
            int end = signatureEnds[archId];
            if (fieldIds.SequenceEqual(packedFieldIds.AsSpan(start, end - start)))
                return archId;

            slot = (slot + 1) & mask;
        }
    }

    internal static void Insert(int[] archIds, ulong hash, int archId)
    {
        int mask = archIds.Length - 1;
        int slot = InitialSlot(hash, mask);

        while (archIds[slot] != EmptySlot)
            slot = (slot + 1) & mask;

        archIds[slot] = archId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int InitialSlot(ulong hash, int mask) =>
        unchecked((int)(hash ^ (hash >> 32))) & mask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix(ulong hash, uint value)
    {
        unchecked
        {
            hash = (hash ^ (byte)value) * HashPrime;
            hash = (hash ^ (byte)(value >> 8)) * HashPrime;
            hash = (hash ^ (byte)(value >> 16)) * HashPrime;
            return (hash ^ (byte)(value >> 24)) * HashPrime;
        }
    }
}
