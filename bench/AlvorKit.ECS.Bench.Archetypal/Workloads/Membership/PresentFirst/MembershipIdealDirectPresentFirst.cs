namespace AlvorKit;

internal static class MembershipIdealDirectPresentFirst
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int[] directOrdinals, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        ReadOnlySpan<int> ordinals = directOrdinals;
        int fieldId = signature[0];
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ArchMembershipApproaches.FindIdealDirect(ordinals, fieldId);
        return sum;
    }
}
