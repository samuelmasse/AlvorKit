namespace AlvorKit;

internal static class MembershipIdealDirectPresentRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int[] directOrdinals, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        ReadOnlySpan<int> ordinals = directOrdinals;
        int mask = signature.Length - 1;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            int fieldId = signature[i & mask];
            sum += ArchMembershipApproaches.FindIdealDirect(ordinals, fieldId);
        }

        return sum;
    }
}
