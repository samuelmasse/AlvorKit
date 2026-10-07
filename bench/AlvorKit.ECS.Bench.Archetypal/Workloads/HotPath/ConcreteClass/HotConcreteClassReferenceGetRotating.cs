namespace AlvorKit;

internal static class HotConcreteClassReferenceGetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(ArchHotFixture fixture, int operations)
    {
        EntMut[] ents = fixture.Ents;
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ents[i & 1023].GetArchetypal<EcsBenchReference, FReference, Afr24ClassArch>()!.Value;
        return sum;
    }
}
