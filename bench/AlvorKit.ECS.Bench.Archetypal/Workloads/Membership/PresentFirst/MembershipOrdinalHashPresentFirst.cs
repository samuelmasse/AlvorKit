namespace AlvorKit;

internal static class MembershipOrdinalHashPresentFirst
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int[] indexSlots, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        ReadOnlySpan<int> slots = indexSlots;
        int fieldId = signature[0];
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ArchMembershipApproaches.FindOrdinalHash(signature, slots, fieldId);
        return sum;
    }
}
