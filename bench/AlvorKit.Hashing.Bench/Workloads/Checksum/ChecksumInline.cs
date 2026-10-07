namespace AlvorKit;

internal static class ChecksumInline
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ulong Run(ulong[] values, int passes)
    {
        ulong sum = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < values.Length; i++)
                sum += values[i];
        }

        return sum;
    }
}
