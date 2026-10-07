namespace AlvorKit;

internal static class HotGenericReferenceGetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(ArchHotFixture fixture, int operations)
    {
        EntMut ent = fixture.Ents[0];
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchReference, FReference, A>()!.Value;
        return sum;
    }
}
