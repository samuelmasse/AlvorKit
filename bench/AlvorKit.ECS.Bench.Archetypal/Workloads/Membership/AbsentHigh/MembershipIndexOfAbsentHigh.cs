namespace AlvorKit;

internal static class MembershipIndexOfAbsentHigh
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        int fieldId = signature[^1] + 1;
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ArchMembershipApproaches.FindIndexOf(signature, fieldId);
        return sum;
    }
}
