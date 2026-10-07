namespace AlvorKit;

internal static class HotGenericScalarSetOne
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(ArchHotFixture fixture, int operations)
    {
        EntMut ent = fixture.Ents[0];

        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<int, F00, A>(i);
    }
}
