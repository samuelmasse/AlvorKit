namespace AlvorKit;

internal static class LifetimeComponentUnsetSet
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Run(EntPtr ent, int operations)
    {
        for (int i = 0; i < operations; i++)
        {
            ent.UnsetLifetimeFirst();
            ent.LifetimeFirst = i;
        }

        return ent.LifetimeFirst;
    }
}
