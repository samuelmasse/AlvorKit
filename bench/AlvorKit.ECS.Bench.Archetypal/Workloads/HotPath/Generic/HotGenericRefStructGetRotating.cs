namespace AlvorKit;

internal static class HotGenericRefStructGetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(ArchHotFixture fixture, int operations)
    {
        EntMut[] ents = fixture.Ents;
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ents[i & 1023].GetArchetypal<EcsBenchRefStruct, FRefStruct, A>().Text.Length;
        return sum;
    }
}
