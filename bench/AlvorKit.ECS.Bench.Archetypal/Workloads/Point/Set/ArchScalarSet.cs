namespace AlvorKit;

internal static class ArchScalarSet
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntMut ent, int operations)
    {
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<int, F00, A>(i);
    }
}
