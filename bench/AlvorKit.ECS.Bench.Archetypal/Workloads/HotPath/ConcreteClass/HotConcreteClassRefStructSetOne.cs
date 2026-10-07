namespace AlvorKit;

internal static class HotConcreteClassRefStructSetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut ent = state.Ents[0];
        EcsBenchRefStruct value = state.RefStructValue;

        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>(value);
    }
}
