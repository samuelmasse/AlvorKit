namespace AlvorKit;

internal static class HotGenericReferenceSetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut ent = state.Ents[0];
        EcsBenchReference value = state.ReferenceValue!;

        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchReference, FReference, A>(value);
    }
}
