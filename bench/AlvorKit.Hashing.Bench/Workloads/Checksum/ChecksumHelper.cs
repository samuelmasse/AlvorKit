namespace AlvorKit;

internal static class ChecksumHelper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ulong Run(ulong[] values, int passes)
    {
        ulong sum = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            AdditiveChecksum64 checksum = default;

            for (var i = 0; i < values.Length; i++)
                checksum.Add(values[i]);
            sum += checksum.Value;
        }

        return sum;
    }
}
