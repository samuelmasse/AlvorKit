namespace AlvorKit;

internal static class HotGenericRefStructSetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut ent = state.Ents[0];
        EcsBenchRefStruct value = state.RefStructValue;

        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, A>(value);
    }
}
