namespace AlvorKit;

internal static class ArchScalarGet
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(EntMut ent, int operations)
    {
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<int, F00, A>();
        return sum;
    }
}
