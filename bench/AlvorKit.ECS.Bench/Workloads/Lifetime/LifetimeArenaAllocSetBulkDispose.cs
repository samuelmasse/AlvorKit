namespace AlvorKit;

internal static class LifetimeArenaAllocSetBulkDispose
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
            ent.LifetimeSecond = i + 1;
            observed = ent.Handle;
        }

        return observed;
    }
}
