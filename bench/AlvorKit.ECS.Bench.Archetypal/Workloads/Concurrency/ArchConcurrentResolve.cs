namespace AlvorKit;

internal static class ArchConcurrentResolve
{
    internal static void Run(ArchConcurrentFixture fixture) => fixture.Run();
    internal static void Owner<A>(ArchConcurrentFixture fixture, int owner, int count)
    {
        using var arena = new EntArena();
        var ents = new EntMut[count];
        uint fields = (1u << 8) - 1;

        for (var i = 0; i < owner * count; i++)
            fields = ArchCombinations.Next(fields);

        for (var i = 0; i < count; i++)
        {
            ents[i] = arena.Alloc();
            ArchShapes.SetMask<A>(ents[i], fields);
            fields = ArchCombinations.Next(fields);
        }

        fixture.ReadyAndWait();

        for (var i = 0; i < count; i++)
            ents[i].SetArchetypal<int, FToggle, A>(i);
        var sum = count;
        fixture.Complete(sum);
    }
}
