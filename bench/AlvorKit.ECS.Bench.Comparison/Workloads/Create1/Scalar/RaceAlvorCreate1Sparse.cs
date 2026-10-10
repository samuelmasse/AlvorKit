namespace AlvorKit;

internal static class RaceAlvorCreate1Sparse
{
    internal static EntPtr Run(RaceAlvorKitBaseContext fixture, int entCount, int passes)
    {
        EntPtr last = default;

        for (var pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < entCount; ++i)
            {
                EntPtr ent = fixture.Arena.Alloc();
                last = ent;
                ent.SparseComponent1 = 0;
            }
        }
        return last;
    }
}
