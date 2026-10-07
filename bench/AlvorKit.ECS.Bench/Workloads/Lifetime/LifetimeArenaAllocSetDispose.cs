namespace AlvorKit;

internal static class LifetimeArenaAllocSetDispose
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static EntHandle Run(int operations)
    {
        EntHandle observed = default;
        using var arena = new EntArena();

        for (int i = 0; i < operations; i++)
        {
            var ent = arena.Alloc();
            ent.LifetimeFirst = i;
            observed = ent.Handle;
            ent.Dispose();
        }

        return observed;
    }
}
