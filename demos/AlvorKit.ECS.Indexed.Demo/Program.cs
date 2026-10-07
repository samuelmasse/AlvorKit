using var context = new EntIdxContext();
var projectiles = new EntIdxGatedBag<IndexedDemoComponents.IsProjectile, IndexedDemoComponents.IsReady>();
var scratched = new EntIdxBag<IndexedDemoComponents.IsScratched>();
var ids = new DemoIdIndex();

// Value changes can mark another component. Teardown never invokes this reaction.
context.OnChange<int, IndexedDemoComponents.Health>((ent, in change) =>
{
    Console.WriteLine($"health: {ent.Name} {change.Before} -> {change.After}, present={change.IsPresent}");
    ent.IsScratched = true;
});

// Index maintenance precedes reactions, even when registered later.
context.AddGatedBag(projectiles);
context.AddBag(scratched);
context.AddIndex(ids.Remove).OnChange<Guid, IndexedDemoComponents.Id>(ids.Update);
context.OnClearing(ent => Console.WriteLine($"clear: {ent.Name}, id-indexed={ids.Contains(ent.Id)}"));
context.OnDisposing(ent => Console.WriteLine($"dispose: {ent.Name}, id-indexed={ids.Contains(ent.Id)}"));

using var arena = new EntIdxArena(context);

Console.WriteLine("initialize data before publishing the ready gate");
var rocket = arena.Alloc().Mutate()
    .Name("rocket")
    .Id(Guid.Parse("11111111-1111-1111-1111-111111111111"))
    .Health(10)
    .IsProjectile(true)
    .Ent;
PrintBags(projectiles, scratched);
rocket.IsReady = true;
PrintBags(projectiles, scratched);

Console.WriteLine("normal writes and explicit Unset run reactions");
rocket.Health = 7;
rocket.UnsetHealth();
PrintBags(projectiles, scratched);

Console.WriteLine("Clear removes indexes without recreating scratched membership");
rocket.Clear();
Console.WriteLine($"allocation retained: {rocket.IsAlive}");
PrintBags(projectiles, scratched);

rocket.Mutate()
    .Name("reused rocket")
    .Id(Guid.Parse("22222222-2222-2222-2222-222222222222"))
    .Health(5)
    .IsProjectile(true)
    .IsReady(true);

Console.WriteLine("individual Dispose captures intact data, removes indexes, then releases the Ent");
rocket.Dispose();
Console.WriteLine($"arena allocated: {arena.Allocated}");
PrintBags(projectiles, scratched);

Console.WriteLine("arena Dispose bulk-invalidates Ents without per-Ent notifications");
var stale = arena.Alloc().Mutate()
    .Name("scope view")
    .IsProjectile(true)
    .IsReady(true)
    .Ent;
arena.Dispose();
Console.WriteLine($"arena alive: {arena.IsAlive}, former member alive: {stale.IsAlive}");
Console.WriteLine("bags and indexes are no longer valid views after bulk teardown");

// Captured bag spans must not be traversed while changing their own membership.
static void PrintBags(
    EntIdxGatedBag<IndexedDemoComponents.IsProjectile, IndexedDemoComponents.IsReady> projectiles,
    EntIdxBag<IndexedDemoComponents.IsScratched> scratched)
{
    Console.WriteLine($"ready projectiles: {projectiles.Count}, scratched: {scratched.Count}");
}

namespace AlvorKit
{
    /// <summary>Maintains a lookup after ID writes and removes entries while teardown can still read the ID.</summary>
    internal class DemoIdIndex
    {
        /// <summary>Maintained lookup owned by this demo scope.</summary>
        private readonly Dictionary<Guid, EntMutIdx> entsById = [];

        /// <summary>Moves the lookup from the previous ID to the committed ID before reactions run.</summary>
        internal void Update(EntMutIdx ent, in EntChange<Guid> change)
        {
            if (change.Before == change.After)
                return;

            if (change.Before != Guid.Empty)
                entsById.Remove(change.Before);

            if (change.After != Guid.Empty)
                entsById.Add(change.After, ent);
        }

        /// <summary>Detaches an entry while lifecycle teardown can still read its ID.</summary>
        internal void Remove(EntMutIdx ent)
        {
            if (ent.Id != Guid.Empty)
                entsById.Remove(ent.Id);
        }

        /// <summary>Inspects membership without changing the Ent or index.</summary>
        internal bool Contains(Guid id) => entsById.ContainsKey(id);
    }
}
