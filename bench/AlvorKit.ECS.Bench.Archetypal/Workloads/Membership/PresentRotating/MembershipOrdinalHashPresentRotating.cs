namespace AlvorKit;

internal static class MembershipOrdinalHashPresentRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int[] indexSlots, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        ReadOnlySpan<int> slots = indexSlots;
        int mask = signature.Length - 1;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            int fieldId = signature[i & mask];
            sum += ArchMembershipApproaches.FindOrdinalHash(signature, slots, fieldId);
        }

        return sum;
    }
}
