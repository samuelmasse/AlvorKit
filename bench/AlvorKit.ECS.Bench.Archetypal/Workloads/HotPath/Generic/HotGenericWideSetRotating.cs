namespace AlvorKit;

internal static class HotGenericWideSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut[] ents = state.Ents;
        EcsBenchWideValue value = state.WideValue;

        for (int i = 0; i < operations; i++)
            ents[i & 1023].SetArchetypal<EcsBenchWideValue, FWide, A>(value);
    }
}
