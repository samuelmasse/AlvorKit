namespace AlvorKit;

internal static class HotConcreteStructWideSetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut ent = state.Ents[0];
        EcsBenchWideValue value = state.WideValue;

        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchWideValue, FWide, Afr24StructArch>(value);
    }
}
