namespace AlvorKit;

internal static class HotConcreteStructReferenceSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut[] ents = state.Ents;
        EcsBenchReference value = state.ReferenceValue!;

        for (int i = 0; i < operations; i++)
            ents[i & 1023].SetArchetypal<EcsBenchReference, FReference, Afr24StructArch>(value);
    }
}
