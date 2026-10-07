namespace AlvorKit;

internal static class Epoch32Dictionary
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(Dictionary<int, int> index, int[] keys, int passes)
    {
        long sum = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            index.Clear();

            for (var i = 0; i < keys.Length; i++)
                index.Add(keys[i], i);

            for (var i = 0; i < keys.Length; i++)
                sum += index[keys[i]];
        }

        return sum;
    }
}
