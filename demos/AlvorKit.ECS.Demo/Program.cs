// Generated extension properties read and write components with ordinary property syntax.
var entity = new EntObj
{
    Name = "hero",
    Health = 90,
    Team = "blue"
};

// Mutate is useful when a caller wants fluent batch-style component updates.
entity.Mutate()
    .Health(100)
    .Team("vanguard");

Console.WriteLine(entity);
Console.WriteLine($"Health: {entity.Health}");
Console.WriteLine($"Has team: {entity.HasTeam}");

entity.UnsetTeam();
Console.WriteLine($"Has team after unset: {entity.HasTeam}");

using var arena = new EntArena();
var arenaEntity = arena.Alloc();

// Arena-backed entities expose the same generated component API while the arena owns storage.
arenaEntity.Name = "arena-spawn";
arenaEntity.Health = 25;

Console.WriteLine(arenaEntity);
Console.WriteLine($"Arena allocated: {arena.Allocated}");

arenaEntity.Dispose();
Console.WriteLine($"Arena allocated after dispose: {arena.Allocated}");
