namespace AlvorKit;

internal static class HotConcreteClassWideSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut[] ents = state.Ents;
        EcsBenchWideValue value = state.WideValue;

        for (int i = 0; i < operations; i++)
            ents[i & 1023].SetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>(value);
    }
}
