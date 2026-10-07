namespace AlvorKit;

internal static class ManyArchDiscovery
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Run(EntArena arena, int passes)
    {
        var query = arena.QueryArchetypal<ManyArchFixture.ManyArch>()
            .With<int, ManyArchFixture.C0>()
            .With<int, ManyArchFixture.C1>()
            .With<int, ManyArchFixture.C2>()
            .With<int, ManyArchFixture.C3>();
        var count = 0;

        for (var pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
                count += chunk.Ents.Length;
        }

        return count;
    }
}
