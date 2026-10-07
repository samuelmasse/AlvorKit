namespace AlvorKit;

internal static class Epoch32EpochIndex
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EpochIndex32 index, int[] keys, int passes)
    {
        long sum = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            index.Begin();

            for (var i = 0; i < keys.Length; i++)
                index.GetOrAdd(keys[i], i, out _);

            for (var i = 0; i < keys.Length; i++)
            {
                index.TryGet(keys[i], out var slot);
                sum += slot;
            }
        }

        return sum;
    }
}
