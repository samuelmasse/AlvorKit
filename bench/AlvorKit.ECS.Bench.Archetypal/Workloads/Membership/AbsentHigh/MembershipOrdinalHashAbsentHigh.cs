namespace AlvorKit;

internal static class MembershipOrdinalHashAbsentHigh
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int[] indexSlots, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        ReadOnlySpan<int> slots = indexSlots;
        int fieldId = signature[^1] + 1;
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ArchMembershipApproaches.FindOrdinalHash(signature, slots, fieldId);
        return sum;
    }
}
