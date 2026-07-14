Console.WriteLine("AlvorKit ECS generated archetypal components");

var arena = new EntArena();
EntPtr scoutPtr = arena.Alloc();
EntPtr guardPtr = arena.Alloc();
EntRefMut scout = scoutPtr;
EntRefMut guard = guardPtr;

// Name and Team are sparse, while Health, Position, and Velocity share the generated CombatComponents archetype group.
scout.Name = "Scout";
scout.Team = "Blue";
scout.Health = 80;
scout.Position = new(4, 7);
scout.Velocity = new(1, 0);

guard.Name = "Guard";
guard.Team = "Blue";
guard.Health = 120;
guard.Position = new(12, 7);

Console.WriteLine();
Console.WriteLine("Generated accessors:");
Console.WriteLine($"{scout.Name}: health={scout.Health}, position={scout.Position}, velocity={scout.Velocity}");
Console.WriteLine($"{guard.Name}: health={guard.Health}, position={guard.Position}, has velocity={guard.HasVelocity}");

scout.Mutate()
    .Health(75)
    .Position(new(5, 7));

Console.WriteLine();
Console.WriteLine("ComponentToString:");
Console.WriteLine(scout);
Console.WriteLine(guard);

Console.WriteLine();
Console.WriteLine("Debugger component view:");
PrintDebugComponents(scout);

guard.Clear();
Console.WriteLine();
Console.WriteLine($"Guard after Clear: alive={guard.IsAlive}, has name={guard.HasName}, has health={guard.HasHealth}");

scoutPtr.Dispose();
Console.WriteLine($"Scout after Dispose: alive={scoutPtr.IsAlive}");

Console.WriteLine($"Arena allocated before disposal: {arena.Allocated}");
arena.Dispose();
Console.WriteLine($"Arena alive after disposal: {arena.IsAlive}");

// Print the debugger wrappers without depending on debugger-only rendering.
static void PrintDebugComponents(IEnt ent)
{
    foreach (object component in new EntDebugView(ent).Components)
    {
        switch (component)
        {
            case EntDebugView.DebugViewComponentPrimitive primitive:
                Console.WriteLine($"  {primitive.Name} = {primitive.Value}");
                break;
            case EntDebugView.DebugViewComponent value:
                Console.WriteLine($"  {value.Name} = {value.Value}");
                break;
        }
    }
}
