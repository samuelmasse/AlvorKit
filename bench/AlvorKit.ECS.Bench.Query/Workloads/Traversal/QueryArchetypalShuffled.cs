namespace AlvorKit;

internal static class QueryArchetypalShuffled
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EntMut[] shuffledEnts, int passes)
    {
        long sum = 0;

        for (int pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < shuffledEnts.Length; i++)
                sum += shuffledEnts[i].GetArchetypal<int, ArchValue, QueryArch>();
        }

        return sum;
    }
}
