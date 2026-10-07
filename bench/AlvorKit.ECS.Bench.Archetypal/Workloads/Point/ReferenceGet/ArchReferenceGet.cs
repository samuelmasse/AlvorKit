namespace AlvorKit;

internal static class ArchReferenceGet
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(EntMut ent, int operations)
    {
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchReference, FReference, A>()!.Value;
        return sum;
    }
}
