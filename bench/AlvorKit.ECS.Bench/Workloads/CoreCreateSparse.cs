namespace AlvorKit;

public static class CoreCreateSparse
{
    public static void Run(EntArena arena, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var ent = arena.Alloc();
            ent.SparseFirst = i;
            ent.SparseSecond = 2;
            ent.SparseThird = 3;
        }
    }
}
