namespace AlvorKit;

internal static class ArchHighOccupancy
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntArena alloc, EntMut[] ents)
    {
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = alloc.Alloc();
            int width = (i & 3) switch
            {
                0 => 1,
                1 => 4,
                2 => 8,
                _ => 16,
            };
            ArchShapes.SetWidth<A>(ents[i], width);
        }
    }
}
