namespace AlvorKit;

internal static class TableUInt64Int32Inline
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ulong Run(ulong[] keys, int[] second, int mask, int passes)
    {
        ulong sum = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            for (var index = 0; index < keys.Length; index++)
                sum += (uint)InlineTableHash.Index(keys[index], second[index], mask);
        }

        return sum;
    }
}
