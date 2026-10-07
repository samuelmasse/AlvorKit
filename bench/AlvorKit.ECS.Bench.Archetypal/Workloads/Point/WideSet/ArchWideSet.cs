namespace AlvorKit;

internal static class ArchWideSet
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntMut ent, int operations, EcsBenchWideValue value)
    {
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchWideValue, FWide, A>(value);
    }
}
