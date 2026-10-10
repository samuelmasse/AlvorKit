namespace AlvorKit;

internal static class RaceAlvorCreate1ArchetypalMutator
{
    internal static EntPtr Run(RaceAlvorKitBaseContext fixture, int entCount, int passes)
    {
        EntPtr last = default;

        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < entCount; i++)
            {
                last = fixture.Arena.Alloc().Mutate()
                    .Component1(0);
            }
        }

        return last;
    }
}
