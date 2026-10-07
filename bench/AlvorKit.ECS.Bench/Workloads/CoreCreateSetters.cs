namespace AlvorKit;

public static class CoreCreateSetters
{
    public static void Run(EntArena arena, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var ent = arena.Alloc();
            ent.First = i;
            ent.Second = 2;
            ent.Third = 3;
        }
    }
}
