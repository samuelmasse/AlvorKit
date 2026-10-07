namespace AlvorKit;

internal static class MembershipIndexOfPresentRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        int mask = signature.Length - 1;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            int fieldId = signature[i & mask];
            sum += ArchMembershipApproaches.FindIndexOf(signature, fieldId);
        }

        return sum;
    }
}
