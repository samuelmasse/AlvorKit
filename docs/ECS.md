# AlvorKit ECS

AlvorKit games use the AlvorKit Ent-component system for game Ents. An Ent is a
mutable simulated object whose identity and capabilities are
assembled from data: a player, enemy, projectile, item, world object, chunk, or
similar runtime object. Model those objects with generated ECS components,
Ent handles, and an arena instead of introducing a parallel Ent class
hierarchy, a bespoke component store, or another ECS.

Keep behavior in injected game systems and services. Components hold Ent
state; systems select Ents and apply behavior. Services, commands,
configuration, assets, protocol records, and ordinary value objects are not
game Ents and should remain normal C# types.

## Ent Terminology

Use `Ent` in every context and `Ents` for the plural. The long-form synonym is
banned. This applies to prose, code identifiers, type and member names,
parameters and locals, filenames, directories, labels, and compound names.
Use names such as `WorldEntLoader`, `WorldEnts`, and `IWorldEntComponents` even
where the longer word might otherwise read naturally.

This guide covers the stable game-facing packages:

- `AlvorKit.ECS` provides generated components, Ent handles, mutation, and
  arena ownership.
- `AlvorKit.ECS.Indexed` adds observed mutation, maintained bags, and hooks for
  game-owned indexes such as id, dirty, spatial, persistence, or replication
  indexes.

The runnable
[`AlvorKit.ECS.Indexed.Demo`](../demos/AlvorKit.ECS.Indexed.Demo/) shows the
same component, context, hook, bag, allocation, and disposal mechanics in one
small program. For the complete Indexed mutation contract, read
[`ECS.Indexed.md`](ECS.Indexed.md).

## Package Setup

A project that declares and uses ECS components references the base package and
the component generator:

```xml
<ItemGroup>
    <ProjectReference Include="$(AlvorKitRoot)src\AlvorKit.ECS\AlvorKit.ECS.csproj" />
    <ProjectReference Include="$(AlvorKitRoot)src\AlvorKit.ECS.Generator\AlvorKit.ECS.Generator.csproj"
        OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>

<ItemGroup>
    <Using Include="AlvorKit" />
</ItemGroup>
```

Add the Indexed package when a game Ent scope maintains bags, observes
component writes, or keeps derived indexes:

```xml
<ItemGroup>
    <ProjectReference Include="$(AlvorKitRoot)src\AlvorKit.ECS.Indexed\AlvorKit.ECS.Indexed.csproj" />
</ItemGroup>

<ItemGroup>
    <Using Include="AlvorKit" />
</ItemGroup>
```

Keep the references in the package that owns the Ent component declarations
and scope. Do not add ECS references to unrelated packages merely to pass an
Ent through them; preserve the dependency direction described in
[`ProjectSplitModel.md`](ProjectSplitModel.md).

## Declare Components

Declare related component properties in a `[Components]` interface. The source
generator creates component marker types, Ent accessors, presence checks,
unset methods, and fluent mutation methods:

```csharp
namespace MyGame;

[Components]
public interface IWorldComponents
{
    [ComponentToString] Guid Id { get; set; }
    [ComponentToString] string Name { get; set; }
    Position Position { get; set; }
    int Health { get; set; }
    bool IsLoaded { get; set; }
    bool IsProjectile { get; set; }
    bool IsScratched { get; set; }
}

public readonly record struct Position(float X, float Y);
```

For `Id`, the generated API includes:

- `WorldComponents.Id`, the `IComponent` marker used as a generic key;
- `ent.Id`, the typed getter and setter;
- `ent.HasId`, which tests component presence;
- `ent.UnsetId()`, which removes the component; and
- `.Mutate().Id(value)`, the fluent initialization method.

Presence is separate from value. Setting a component to its default value still
makes it present; call its generated `Unset...` method to remove it. Marker and
gate components used by Indexed bags must have the `bool` value type.

Keep components focused on state. Put simulation, loading, persistence,
rendering, networking, and presentation behavior in the services that consume
the components.

## Archetypal Queries And Rows

Mark components that should share dense structure-of-arrays storage with
`[Archetypal]`. Every marked property in one declaration belongs to that
declaration's generated archetype group:

```csharp
[Components]
public interface IMotionComponents
{
    string Name { get; set; }

    [Archetypal]
    Position Position { get; set; }

    [Archetypal]
    Velocity Velocity { get; set; }
}
```

The generator adds named query selectors for marked properties. Selection may
contain any number of components:

```csharp
var moving = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity();
```

Chunk iteration remains the bulk and explicit-SIMD interface:

```csharp
foreach (var chunk in moving)
{
    Span<Position> positions =
        chunk.Get<Position, MotionComponents.Position>();
    Span<Velocity> velocities =
        chunk.Get<Velocity, MotionComponents.Velocity>();

    for (int row = 0; row < positions.Length; row++)
        positions[row] += velocities[row];
}
```

Use generated rows when an algorithm reads more naturally one Ent at a time:

```csharp
foreach (var row in moving.Rows())
    row.Position += row.Velocity;
```

Each used `Rows()` query shape receives an exact generated row and enumerator in
the consuming compilation. Entering an arch binds the aligned Ent and selected
component columns once. The within-arch loop carries one row index, and each
named property uses cached-base-plus-index addressing. It performs no point loc,
graph, hash, virtual, or component-directory lookup per Ent and allocates no
iterator object. Unused component combinations generate no row types.

The project containing the `Rows()` call must reference
`AlvorKit.ECS.Generator` as an analyzer, even when the component declaration is
provided by another project. This lets the generator specialize the exact
closed query shape used by that consumer.

A query descriptor may be stored and enumerated repeatedly while its originating
arena is alive. It retains that arena's ID and generation; it does not extend
the arena's lifetime. Starting chunk enumeration or calling `Rows()` on a
default descriptor or after arena disposal throws `EntArenaDisposedException`,
even if another arena has reused the allocator ID. Validation runs once at
enumeration entry, with no per-row lifetime checks.

Structural changes are allowed after one enumeration has completely ended and
before another begins. Calling `Rows()` already creates an enumerator. While a
query enumerator, chunk, span, row value, or returned component ref is live, do
not add, remove, clear, compact, grow, or dispose rows in the same `(alloc, A)`.
Writes to existing component values are allowed.

One thread owns reads, writes, queries, and structural changes for a particular
`(alloc, A)`. Different threads may concurrently operate on the same `A` when
they own different allocs. Per-row access adds no locking, volatile
access, or ownership checks.

For the low-level storage and query contract, read
[`ECS.Archetypal.md`](ECS.Archetypal.md).

## Base Ent Ownership

An `EntArena` owns a set of allocated Ent slots. `EntPtr` owns one allocated
Ent and may dispose it individually:

```csharp
using var arena = new EntArena();

EntPtr rocket = arena.Alloc();
rocket.Name = "rocket";
rocket.Position = new(4, 7);
rocket.Health = 10;

if (rocket.HasHealth)
    rocket.Health--;

rocket.UnsetHealth();
rocket.Dispose();
```

Use the narrowest handle that expresses ownership:

| Handle | Meaning |
|---|---|
| `EntPtr` | Owning base handle; can mutate and individually dispose the Ent. |
| `EntMut` | Non-owning mutable base handle. |
| `Ent` | Non-owning read handle. |
| `EntPtrIdx` | Owning Indexed handle; mutations and disposal run the context hook pipeline. |
| `EntMutIdx` | Non-owning mutable Indexed handle stored in bags and passed to hooks. |

Handles are generational. After individual or arena disposal, old handles are
dead and cannot refer to a later Ent that reuses the same slot. Keep the
owning pointer wherever individual disposal belongs; pass non-owning handles to
systems and indexes that only observe or mutate the Ent.

`EntArena.Dispose()` is bulk scope teardown. It invalidates every allocation in
the arena. Use individual `EntPtr.Dispose()` when per-Ent ownership ends
before the arena does.

All Ent ownership is explicit. Dispose standalone owning pointers and arenas;
garbage collection does not release their component storage.

## When To Use Indexed ECS

Use base ECS when a local Ent collection needs only component storage and
direct handles. Use `AlvorKit.ECS.Indexed` when component changes must
immediately maintain any derived game state, including:

- dense active sets such as loaded players, projectiles, chunks, or dirty
  Ents;
- stable-id lookups;
- spatial membership;
- dirty-component tracking;
- persistence or replication state; or
- whole-Ent teardown callbacks.

Indexed component hooks and bag markers/gates require sparse components.
Registering an archetypal component throws `EntIdxRegistrationException` during
registration. Archetypal access deliberately remains unobserved for speed,
including through Indexed handles; sparse and archetypal components may coexist
on one Ent. Whole-Ent lifecycle callbacks can inspect both storage kinds.

Once a scope uses Indexed ECS, allocate its game Ents from its
`EntIdxArena` and mutate them through `EntPtrIdx` or `EntMutIdx`. Do not allocate
some Ents from a raw `EntArena` or convert the same scope to raw mutation to
bypass hooks. That leaves bags and indexes inconsistent.

## Scoped Indexed ECS

An Indexed game Ent scope owns one context, its arenas, and the bags and
indexes registered on that context. Give these types domain names and bind
them to the same game scope:

```csharp
[World]
public class WorldEntIdxContext : EntIdxContext;

[World]
public class WorldEntArena(WorldEntIdxContext context) : EntIdxArena(context)
{
    public override void Dispose()
    {
        base.Dispose();
        context.Dispose();
    }
}

[World]
public class WorldProjectileBag :
    EntIdxGatedBag<WorldComponents.IsProjectile, WorldComponents.IsLoaded>;

[World]
public class WorldScratchedBag : EntIdxBag<WorldComponents.IsScratched>;
```

`[World]` is the example game's scope attribute, not an ECS requirement.
Read [GameScopeOrganization.md](GameScopeOrganization.md) for composition.
Each bag is one publicly read-only object used by loaders and runtime systems.
Contexts may inherit to share loader contracts; each instance owns separate
registrations. Indexed handles remain unmanaged values carrying a borrowed
context identity.

## Register Before Allocating

Complete every loader's registrations before loading or spawning Ents:

```csharp
[WorldLoader]
public class WorldLoader(
    WorldEntIdxContext context,
    WorldProjectileBag projectiles,
    WorldScratchedBag scratched,
    WorldEntIndex ids,
    WorldDirtyTracker dirty,
    WorldEntDisposeTracker disposals)
{
    public void Run()
    {
        context.AddGatedBag(projectiles);
        context.AddBag(scratched);
        context.AddIndex(ids.Remove).OnChange<Guid, WorldComponents.Id>(ids.Update);
        context.OnChange<int, WorldComponents.Health>(dirty.Track);
        context.OnDisposing(disposals.Capture);
    }
}
```

Constructing an arena leaves registration open. Its first allocation closes
registration for the context permanently; late registration throws
`EntIdxRegistrationException`. Registration never scans existing Ents.

- `OnWrite<T, N>` observes every committed Set and present Unset after index
  maintenance. Its callback receives the Ent and requires no value snapshot.
  Use it to publish mutable arrays or buffers, including the same reference.
- `OnChange<T, N>` compares with `EqualityComparer<T>.Default` and observes
  changes in value or presence. Its `EntChange<T>` payload contains `Before`,
  `After`, `WasPresent`, and `IsPresent`. Use it for value-based persistence,
  replication, and derived state.
- `AddIndex(remove).OnChange<T, N>(update)` maintains an external index and
  removes membership during Clear and individual Dispose. Use its `OnWrite`
  registration when an index needs every publication. One index can watch
  several components and receives one removal callback.
- `OnClearing` and `OnDisposing` observe their respective lifetime operation
  while all components remain readable and the target cannot be mutated.
- `AddBag` maintains membership while one boolean marker is true.
- `AddGatedBag` requires both its boolean marker and gate to be true.

`Set` always commits and notifies write subscribers; equal values suppress only
change subscribers. Absent `Unset` is silent. A present default or null value
is distinct from absence. Change payloads are scoped views: one shallow copy
of the old value and a read-only reference to the committed value. Copy values
explicitly to retain them beyond delivery. Mutable array contents are not copied.
Equality implementations must be pure. Both callback kinds preserve registration
order within their phase; indexes always finish before reactions.

Index callbacks cannot mutate Indexed Ents. Reactions may synchronously write
other components or other Ents, but revisiting an active Ent/component pair
throws before the nested write. Clear and Dispose of an Ent with active write
delivery also throw. Every index update precedes the first ordinary reaction,
regardless of their relative registration order.

Callbacks must not throw: storage and earlier callbacks are not rolled back.
Indexed registration, reads, mutation, and teardown are single-threaded.

## Initialize Data Before Publishing Membership

Bag maintenance is immediate. Initialize all Ent data before setting the
marker and gate that publish it to systems:

```csharp
public class WorldProjectileSpawner(WorldEntArena arena)
{
    public EntPtrIdx Spawn(Guid id, Position position)
    {
        EntPtrIdx allocation = arena.Alloc().Mutate()
            .Id(id)
            .Name("rocket")
            .Position(position)
            .Health(10)
            .IsProjectile(true)
            .IsLoaded(true);

        return allocation;
    }
}
```

Here `IsLoaded` is the gate and is deliberately written last. As soon as both
`IsProjectile` and `IsLoaded` are true, the Ent appears in the projectile bag.
This keeps systems from observing a half-initialized Ent.

Keep the returned `EntPtrIdx` with the owner responsible for individual
disposal. Convert it to `EntMutIdx` when a non-owning mutable handle is needed.

## Iterate Bags Safely

Bag iteration is dense and allocation-free:

```csharp
public class WorldProjectileTick(WorldProjectileBag projectiles)
{
    public void Tick()
    {
        foreach (var ent in projectiles.Ents)
            ent.Health--;
    }
}
```

Mutating components that do not control the bag's membership is safe and is the
normal system shape. Do not change the bag's own marker or gate, or dispose an
Ent, while walking the captured span. Removal swap-fills the dense bag and
can skip or repeat work. Stage membership-changing work in a reusable buffer:

```csharp
private readonly List<EntMutIdx> scratch = [];

public void ClearScratched()
{
    foreach (var ent in scratched.Ents)
        scratch.Add(ent);

    foreach (var ent in scratch)
        ent.IsScratched = false;

    scratch.Clear();
}
```

Reuse the staging buffer. Do not allocate a new collection on every update or
tick.

## Maintain A Custom Key Index

Register an index's write inputs and its whole-Ent removal together:

```csharp
context.AddIndex(ids.Remove).OnChange<Guid, WorldComponents.Id>(ids.Update);

[World]
public class WorldEntIndex
{
    private readonly Dictionary<Guid, EntMutIdx> ents = [];

    public EntMutIdx this[Guid id] => ents[id];

    public void Update(EntMutIdx ent, in EntChange<Guid> write)
    {
        if (write.Before == write.After)
            return;

        if (write.Before != Guid.Empty)
            ents.Remove(write.Before);

        if (write.After != Guid.Empty)
            ents.Add(write.After, ent);
    }

    public void Remove(EntMutIdx ent)
    {
        if (ent.Id != Guid.Empty)
            ents.Remove(ent.Id);
    }
}
```

Ordinary `UnsetId()` supplies the old ID and an absent new value to `Update`.
Clear and individual Dispose call `Remove` while the ID remains readable.
They never call the ordinary write reactions.

## Clear, Disposal, And Scope Teardown

Individual Indexed lifetime operations:

1. Reject further Indexed mutation of the target, including through copied handles.
2. Run the corresponding Clear or Dispose notification with intact components.
3. Remove the Ent from every registered index and bag.
4. Clear storage or release the allocation directly, without write reactions.

`Clear()` preserves the allocation. It invokes `OnClearing` on every explicit
call for a live Ent, including an empty one. `Dispose()` invokes only
`OnDisposing`; repeated disposal is inert. Generic `IEntMut` Clear calls obey
the same Indexed lifetime contract. Archetypal and sparse components are both
readable during lifecycle notifications and both removed afterward.

Use lifecycle callbacks for external effects such as erasing a persisted
record or capturing a network deletion. Do not reset components on the target
inside those callbacks. Register both notifications when Clear needs the same
external cleanup. Ordinary dirty reactions cannot recreate components during
teardown, and component visitation order no longer controls index correctness.

`EntIdxArena.Dispose()` bulk-invalidates its allocations without per-Ent
callbacks. Bags and game-owned indexes may still contain dead handles and are
invalid views afterward. Use individual Dispose when external cleanup or
continued use of maintained indexes is required.

Keep the context, arenas, bags, and indexes in one scope lifetime. Dispose
every arena before its context; the context rejects disposal while an arena
is alive. Sharing a context across arenas is supported, but bulk teardown of
one arena invalidates any shared views containing its Ents. Arena and context
disposal during their active callbacks are rejected.

Archetypal access, raw storage mutation, and writes through retained references
remain outside sparse observation and its guards. Follow their explicit
ownership and lifetime contracts.

## Required Game Ent Rules

For game Ents:

1. Use generated `[Components]` declarations and AlvorKit ECS handles and
   arenas.
2. Keep behavior in injected services and systems; keep Ent state in
   components.
3. Use Indexed ECS whenever component writes maintain bags, indexes, dirty
   state, persistence, replication, or teardown behavior.
4. Register Indexed bags and hooks before allocating Ents.
5. Mutate an Indexed scope only through `EntPtrIdx` and `EntMutIdx`.
6. Initialize data before setting bag markers and gates; publish the gate last.
7. Do not mutate a bag's membership while iterating its span; stage the work.
8. Keep owning pointers until individual disposal is complete.
9. Keep the Indexed context, arena, bags, and indexes in the same scope
   lifetime; explicitly dispose arenas before their context.
10. Treat arena disposal as bulk invalidation, not per-Ent cleanup.

For detailed hook ordering, reentrancy, bag storage, registration validation,
and mutation semantics, continue with [`ECS.Indexed.md`](ECS.Indexed.md).
