namespace AlvorKit;

internal static class ArchRefStructSet
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntMut ent, int operations, EcsBenchRefStruct value)
    {
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, A>(value);
    }
}
