namespace AlvorKit;

internal static class HotConcreteStructScalarGetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(ArchHotFixture fixture, int operations)
    {
        EntMut ent = fixture.Ents[0];
        long sum = 0;

        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<int, F00, Afr24StructArch>();
        return sum;
    }
}
