namespace AlvorKit;

internal static class TableInt32Helper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ulong Run(int[] keys, int mask, int passes)
    {
        ulong sum = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            for (var index = 0; index < keys.Length; index++)
                sum += (uint)TableHash.Index(keys[index], mask);
        }

        return sum;
    }
}
