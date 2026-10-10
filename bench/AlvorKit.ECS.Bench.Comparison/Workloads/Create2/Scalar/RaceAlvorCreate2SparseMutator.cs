namespace AlvorKit;

internal static class RaceAlvorCreate2SparseMutator
{
    internal static EntPtr Run(RaceAlvorKitBaseContext fixture, int entCount, int passes)
    {
        EntPtr last = default;
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < entCount; i++)
            {
                last = fixture.Arena.Alloc().Mutate()
                    .SparseComponent1(0)
                    .SparseComponent2(0);
            }
        }

        return last;
    }
}
