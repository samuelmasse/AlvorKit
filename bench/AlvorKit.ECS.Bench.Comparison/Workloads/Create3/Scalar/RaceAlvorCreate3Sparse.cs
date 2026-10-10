namespace AlvorKit;

internal static class RaceAlvorCreate3Sparse
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
                ent.SparseComponent2 = 0;
                ent.SparseComponent3 = 0;
            }
        }
        return last;
    }
}
