namespace AlvorKit;

internal static class ArchLowOccupancy
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntArena alloc, EntMut[] ents)
    {
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = alloc.Alloc();
            uint signature = (uint)(i + 1) ^ ((uint)(i + 1) >> 1);
            ArchShapes.SetMask<A>(ents[i], signature);
        }
    }
}
