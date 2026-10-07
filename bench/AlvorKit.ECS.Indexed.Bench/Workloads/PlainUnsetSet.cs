namespace AlvorKit;

public static class IndexedPlainUnsetSet
{
    public static void Run(ReadOnlySpan<EntPtrIdx> ents, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < ents.Length; i++)
            {
                var ent = ents[i];
                ent.Unset<int, BenchPlain>();
                ent.Set<int, BenchPlain>(pass);
            }
        }
    }
}
