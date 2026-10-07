namespace AlvorKit;

internal static class ArchReferenceSet
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntMut ent, int operations, EcsBenchReference value)
    {
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchReference, FReference, A>(value);
    }
}
