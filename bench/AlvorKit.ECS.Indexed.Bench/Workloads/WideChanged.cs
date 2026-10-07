namespace AlvorKit;

public static class IndexedWideChanged
{
    public static void Run(ReadOnlySpan<EntPtrIdx> ents, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            BenchWideValue value = new(1, 2, 3, 4, 5, 6, 7, pass);

            for (var i = 0; i < ents.Length; i++)
            {
                var ent = ents[i];
                ent.Set<BenchWideValue, BenchWide>(value);
            }
        }
    }
}
