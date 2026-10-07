namespace AlvorKit;

public static class IndexedKeyMove
{
    public static void Run(ReadOnlySpan<EntPtrIdx> ents, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < ents.Length; i++)
            {
                var ent = ents[i];
                ent.Set<int, BenchKey>(i + 1 + (pass % 2) * ents.Length);
            }
        }
    }
}
