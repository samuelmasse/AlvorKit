namespace AlvorKit;

internal static class LifetimeComponentHasExisting
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Run(EntPtr ent, int operations)
    {
        int count = 0;

        for (int i = 0; i < operations; i++)
        {
            if (ent.HasLifetimeFirst)
                count++;
        }

        return count;
    }
}
