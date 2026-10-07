namespace AlvorKit;

internal static class TableUInt64Inline
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ulong Run(ulong[] keys, int mask, int passes)
    {
        ulong sum = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            for (var index = 0; index < keys.Length; index++)
                sum += (uint)InlineTableHash.Index(keys[index], mask);
        }

        return sum;
    }
}
