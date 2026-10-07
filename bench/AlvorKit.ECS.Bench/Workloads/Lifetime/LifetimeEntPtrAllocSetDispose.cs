namespace AlvorKit;

internal static class LifetimeEntPtrAllocSetDispose
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static EntHandle Run(int operations)
    {
        EntHandle observed = default;

        for (int i = 0; i < operations; i++)
        {
            var ent = new EntPtr
            {
                LifetimeFirst = i
            };
            observed = ent.Handle;
            ent.Dispose();
        }

        return observed;
    }
}
