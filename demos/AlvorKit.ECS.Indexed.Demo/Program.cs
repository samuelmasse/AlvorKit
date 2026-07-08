var context = new EntIdxContextBuilder();
var projectiles = new EntIdxGatedBagMut<IndexedDemoComponents.IsProjectile, IndexedDemoComponents.IsReady>();
var scratched = new EntIdxBagMut<IndexedDemoComponents.IsScratched>();
var ids = new DemoIdIndex();

// Register derived state before allocating entities. Indexed handles see only hooks carried by this context.
context.AddGatedBag(projectiles);
context.AddBag(scratched);
context.AddPre<Guid, IndexedDemoComponents.Id>(ids.Track);
context.AddPre<int, IndexedDemoComponents.Health>(
    (ent, in value) => Console.WriteLine($"pre health: {Label(ent)} {ent.Health} -> {value}"));
context.AddPost<int, IndexedDemoComponents.Health>(
    ent => Console.WriteLine($"post health: {Label(ent)} is now {ent.Health}"));
context.AddPreDispose(
    ent => Console.WriteLine($"pre dispose: {Label(ent)} id-indexed={ids.Contains(ent.Id)}"));

using var arena = new EntIdxArena(context.Ent);

Console.WriteLine("spawn a projectile, but keep its gate false until all data is ready");
var rocket = arena.Alloc().Mutate()
    .Name("rocket")
    .Id(Guid.Parse("11111111-1111-1111-1111-111111111111"))
    .Health(10)
    .IsProjectile(true)
    .Ent;

PrintBags(projectiles, scratched);

Console.WriteLine();
Console.WriteLine("flip the gate; the gated projectile bag updates immediately");
rocket.IsReady = true;
PrintBags(projectiles, scratched);

Console.WriteLine();
Console.WriteLine("ordinary writes still run hooks around the base ECS component set");
rocket.Health = 7;

Console.WriteLine();
Console.WriteLine("plain bags are marker-only and ignore the gate");
rocket.IsScratched = true;
PrintBags(projectiles, scratched);

Console.WriteLine();
Console.WriteLine("unsetting a marker removes the entity through the indexed unset pipeline");
rocket.UnsetIsProjectile();
PrintBags(projectiles, scratched);

Console.WriteLine();
Console.WriteLine("per-entity dispose runs pre-dispose hooks, clears components, and maintains indexes");
rocket.Dispose();
Console.WriteLine($"arena allocated: {arena.Allocated}");
PrintBags(projectiles, scratched);

Console.WriteLine();
Console.WriteLine("arena dispose is the fast bulk invalidation path; bags are stale by contract afterward");
var stale = arena.Alloc().Mutate()
    .Name("stale-view")
    .IsProjectile(true)
    .IsReady(true)
    .Ent;

PrintBags(projectiles, scratched);
arena.Dispose();
Console.WriteLine($"arena alive: {arena.IsAlive}");
Console.WriteLine($"gated projectile bag count after arena dispose: {projectiles.Count}");
Console.WriteLine($"first bag handle alive after arena dispose: {projectiles.Ents[0].IsAlive}");
Console.WriteLine($"stale handle alive: {stale.IsAlive}");

// Print the two maintained bags without allocating any per-entity snapshots.
static void PrintBags(
    EntIdxGatedBagMut<IndexedDemoComponents.IsProjectile, IndexedDemoComponents.IsReady> projectiles,
    EntIdxBagMut<IndexedDemoComponents.IsScratched> scratched)
{
    Console.WriteLine($"gated projectile bag ({projectiles.Count}):");
    foreach (var ent in projectiles.Ents)
        Console.WriteLine($"  {ent}");

    Console.WriteLine($"scratched bag ({scratched.Count}):");
    foreach (var ent in scratched.Ents)
        Console.WriteLine($"  {ent}");
}

// Use the component name while it is present; Clear() may unset it before other hooks run.
static string Label(EntMutIdx ent) => ent.HasName ? ent.Name : "<name cleared>";

/// <summary>Small demo hook consumer that maintains an id-to-entity index.</summary>
internal sealed class DemoIdIndex
{
    private readonly Dictionary<Guid, EntMutIdx> entsById = [];

    /// <summary>Updates the id index before the component value changes, while the old id is still readable.</summary>
    internal void Track(EntMutIdx ent, in Guid value)
    {
        if (ent.Id == value)
            return;

        if (ent.Id != default)
            entsById.Remove(ent.Id);

        if (value != default)
            entsById.Add(value, ent);
    }

    /// <summary>Returns whether the index currently contains the requested id.</summary>
    internal bool Contains(Guid id) => entsById.ContainsKey(id);
}
