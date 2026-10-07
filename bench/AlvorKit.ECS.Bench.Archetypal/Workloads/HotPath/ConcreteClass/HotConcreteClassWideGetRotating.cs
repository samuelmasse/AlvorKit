namespace AlvorKit;

internal static class HotConcreteClassWideGetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(ArchHotFixture fixture, int operations)
    {
        EntMut[] ents = fixture.Ents;
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ents[i & 1023].GetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>().A;
        return sum;
    }
}
