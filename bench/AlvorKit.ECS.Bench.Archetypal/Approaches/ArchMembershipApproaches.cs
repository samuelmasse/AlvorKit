namespace AlvorKit;

internal static class ArchMembershipApproaches
{
    internal static int FindIndexOf(ReadOnlySpan<int> fieldIds, int fieldId) => fieldIds.IndexOf(fieldId);
    internal static int FindIdealDirect(ReadOnlySpan<int> ordinals, int fieldId) => ordinals[fieldId] - 1;
    internal static int OrdinalHashCapacity(int fieldCount) => (int)BitOperations.RoundUpToPowerOf2((uint)(fieldCount * 2));
    internal static void BuildOrdinalHash(ReadOnlySpan<int> fieldIds, Span<int> slots)
    {
        int mask = slots.Length - 1;

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
        {
            int slot = TableHash.Index(fieldIds[ordinal], mask);

            while (slots[slot] != 0)
                slot = (slot + 1) & mask;
            slots[slot] = ordinal + 1;
        }
    }

    internal static int FindOrdinalHash(ReadOnlySpan<int> fieldIds, ReadOnlySpan<int> slots, int fieldId)
    {
        int mask = slots.Length - 1;
        int slot = TableHash.Index(fieldId, mask);

        while (true)
        {
            int encodedOrdinal = slots[slot];

            if (encodedOrdinal == 0)
                return -1;
            int ordinal = encodedOrdinal - 1;

            if (fieldIds[ordinal] == fieldId)
                return ordinal;
            slot = (slot + 1) & mask;
        }
    }

    internal static int FindBinary(ReadOnlySpan<int> fieldIds, int fieldId)
    {
        int low = 0;
        int high = fieldIds.Length - 1;

        while (low <= high)
        {
            int middle = (low + high) >> 1;
            int candidate = fieldIds[middle];

            if (candidate < fieldId)
                low = middle + 1;
            else if (candidate > fieldId)
                high = middle - 1;
            else
                return middle;
        }

        return -1;
    }
}
