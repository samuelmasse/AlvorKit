namespace AlvorKit;

public static class IndexedClear
{
    public static void Run(ReadOnlySpan<EntPtrIdx> ents)
    {
        foreach (var ent in ents)
            ent.Clear();
    }
}
