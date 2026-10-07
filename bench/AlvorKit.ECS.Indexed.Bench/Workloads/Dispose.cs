namespace AlvorKit;

public static class IndexedDispose
{
    public static void Run(ReadOnlySpan<EntPtrIdx> ents)
    {
        foreach (var ent in ents)
            ent.Dispose();
    }
}
