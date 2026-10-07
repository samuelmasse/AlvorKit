namespace AlvorKit;

internal static class HotConcreteStructScalarSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(ArchHotFixture fixture, int operations)
    {
        EntMut[] ents = fixture.Ents;

        for (int i = 0; i < operations; i++)
            ents[i & 1023].SetArchetypal<int, F00, Afr24StructArch>(i);
    }
}
