namespace AlvorKit;

public static class IndexedArrayPublish
{
    public static void Run(ReadOnlySpan<EntPtrIdx> ents, int passes, int[] array)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < ents.Length; i++)
            {
                var ent = ents[i];
                ent.Set<int[], BenchArray>(array);
            }
        }
    }
}
