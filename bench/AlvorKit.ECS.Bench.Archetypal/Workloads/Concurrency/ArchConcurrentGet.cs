namespace AlvorKit;

internal static class ArchConcurrentGet
{
    internal static void Run(ArchConcurrentFixture fixture) => fixture.Run();
    internal static void Owner<A>(ArchConcurrentFixture fixture, int owner, int count)
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        ArchShapes.SetWidth<A>(ent, 8);
        fixture.ReadyAndWait();
        long sum = 0;

        for (var i = 0; i < count; i++)
            sum += ent.GetArchetypal<int, F00, A>();
        fixture.Complete(sum);
    }
}
