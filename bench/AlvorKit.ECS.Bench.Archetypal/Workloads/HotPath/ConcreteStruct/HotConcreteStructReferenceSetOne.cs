namespace AlvorKit;

internal static class HotConcreteStructReferenceSetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut ent = state.Ents[0];
        EcsBenchReference value = state.ReferenceValue!;

        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchReference, FReference, Afr24StructArch>(value);
    }
}
