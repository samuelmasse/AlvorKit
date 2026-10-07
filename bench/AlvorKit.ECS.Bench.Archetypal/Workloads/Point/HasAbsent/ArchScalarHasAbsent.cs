namespace AlvorKit;

internal static class ArchScalarHasAbsent
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(EntMut ent, int operations)
    {
        long count = 0;

        for (int i = 0; i < operations; i++)
        {
            if (ent.HasArchetypal<int, F32, A>())
                count++;
        }

        return count;
    }
}
