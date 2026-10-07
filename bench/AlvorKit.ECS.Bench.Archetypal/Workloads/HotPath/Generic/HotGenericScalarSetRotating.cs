namespace AlvorKit;

internal static class HotGenericScalarSetRotating
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(ArchHotFixture fixture, int operations)
    {
        EntMut[] ents = fixture.Ents;

        for (int i = 0; i < operations; i++)
            ents[i & 1023].SetArchetypal<int, F00, A>(i);
    }
}
