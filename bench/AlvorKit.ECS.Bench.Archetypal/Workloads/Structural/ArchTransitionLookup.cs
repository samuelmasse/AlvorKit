namespace AlvorKit;

internal static class ArchTransitionLookup
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(int[] fieldIds, int centerArchId, int width, int operations)
    {
        long sum = 0;

        for (int operation = 0; operation < operations; operation++)
        {
            int fieldId = fieldIds[operation & (width - 1)];
            sum += EntArchGraph<A>.GetTransitionArchId(centerArchId, fieldId);
        }

        return sum;
    }
}
