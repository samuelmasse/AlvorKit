namespace AlvorKit;

internal static class LifetimeComponentGetExisting
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Run(EntPtr ent, int operations)
    {
        int sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ent.LifetimeFirst;
        return sum;
    }
}
