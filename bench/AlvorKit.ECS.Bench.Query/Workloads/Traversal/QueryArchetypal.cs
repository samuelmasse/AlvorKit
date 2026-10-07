namespace AlvorKit;

internal static class QueryArchetypal
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(EntMut[] ents, int passes)
    {
        long sum = 0;

        for (int pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < ents.Length; i++)
                sum += ents[i].GetArchetypal<int, ArchValue, QueryArch>();
        }

        return sum;
    }
}
