namespace AlvorKit;

internal static class LifetimeComponentSetExisting
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Run(EntPtr ent, int operations)
    {
        for (int i = 0; i < operations; i++)
            ent.LifetimeFirst = i;
        return ent.LifetimeFirst;
    }
}
