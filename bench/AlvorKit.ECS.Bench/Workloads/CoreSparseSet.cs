namespace AlvorKit;

public static class CoreSparseSet
{
    public static void Run(EntPtr[] ents, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < ents.Length; i++)
            {
                var ent = ents[i];
                ent.SparseFirst = pass;
            }
        }
    }
}
