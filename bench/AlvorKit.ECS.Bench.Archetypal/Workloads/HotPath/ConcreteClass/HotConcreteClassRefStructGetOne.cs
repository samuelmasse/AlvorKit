namespace AlvorKit;

internal static class HotConcreteClassRefStructGetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(ArchHotFixture fixture, int operations)
    {
        EntMut ent = fixture.Ents[0];
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>().Text.Length;
        return sum;
    }
}
