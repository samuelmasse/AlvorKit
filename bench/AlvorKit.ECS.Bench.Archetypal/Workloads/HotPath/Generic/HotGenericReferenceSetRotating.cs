namespace AlvorKit;

internal static class HotGenericReferenceSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(ArchHotFixture fixture, int operations)
    {
        var state = fixture;
        EntMut[] ents = state.Ents;
        EcsBenchReference value = state.ReferenceValue!;

        for (int i = 0; i < operations; i++)
            ents[i & 1023].SetArchetypal<EcsBenchReference, FReference, A>(value);
    }
}
