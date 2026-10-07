namespace AlvorKit;

internal static class ArchConcurrentSet
{
    internal static void Run(ArchConcurrentFixture fixture) => fixture.Run();
    internal static void Owner<A>(ArchConcurrentFixture fixture, int owner, int count)
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        ArchShapes.SetWidth<A>(ent, 8);
        fixture.ReadyAndWait();

        for (var i = 0; i < count; i++)
            ent.SetArchetypal<int, F00, A>(i);
        var sum = ent.GetArchetypal<int, F00, A>();
        fixture.Complete(sum);
    }
}
