namespace AlvorKit;

internal static class MembershipIndexOfPresentFirst
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        int fieldId = signature[0];
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ArchMembershipApproaches.FindIndexOf(signature, fieldId);
        return sum;
    }
}
