namespace AlvorKit;

internal static class HotConcreteStructRefStructSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut[] ents = state.Ents;
        EcsBenchRefStruct value = state.RefStructValue;

        for (int i = 0; i < operations; i++)
            ents[i & 1023].SetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24StructArch>(value);
    }
}
