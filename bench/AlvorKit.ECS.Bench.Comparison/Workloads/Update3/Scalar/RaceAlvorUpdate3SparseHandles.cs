namespace AlvorKit;

internal static class RaceAlvorUpdate3SparseHandles
{
    internal static void Run(RaceAlvorSparseContext fixture, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            EntPtr[] ents = fixture.Ents;

            for (var i = 0; i < ents.Length; i++)
            {
                EntPtr ent = ents[i];
                ent.SparseComponent1 = ent.SparseComponent1 + ent.SparseComponent2 + ent.SparseComponent3;
            }
        }
    }
}
