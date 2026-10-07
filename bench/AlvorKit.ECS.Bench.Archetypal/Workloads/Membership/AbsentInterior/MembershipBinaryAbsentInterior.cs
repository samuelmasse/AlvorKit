namespace AlvorKit;

internal static class MembershipBinaryAbsentInterior
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(int[] fieldIds, int[] interiorFieldIds, int operations)
    {
        ReadOnlySpan<int> signature = fieldIds;
        int mask = signature.Length - 1;
        long sum = 0;

        for (int i = 0; i < operations; i++)
        {
            int fieldId = interiorFieldIds[i & mask];
            sum += ArchMembershipApproaches.FindBinary(signature, fieldId);
        }

        return sum;
    }
}
