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
    <Using Include="AlvorKit.ECS" />
    <Using Include="AlvorKit.ECS.Generator" />
</ItemGroup>
```

Add the Indexed package when a game Ent scope maintains bags, observes
component writes, or keeps derived indexes:

```xml
<ItemGroup>
    <ProjectReference Include="$(AlvorKitRoot)src\AlvorKit.ECS.Indexed\AlvorKit.ECS.Indexed.csproj" />
</ItemGroup>

<ItemGroup>
    <Using Include="AlvorKit.ECS.Indexed" />
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
namespace MyGame.World;

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

A query descriptor may be stored and enumerated repeatedly. Structural changes
are allowed after one enumeration has completely ended and before another
begins. While a chunk, span, row enumerator, row value, or returned component
ref is live, do not add, remove, clear, compact, grow, or dispose rows in the
same `(alloc, A)`. Writes to existing component values are allowed.

One thread owns reads, writes, queries, and structural changes for a particular
`(alloc, A)`. Different threads may concurrently operate on the same `A` when
they own different allocs. Query and row hot paths add no locking, volatile
access, or ownership checks.

For the representation, Release measurements, generated code shape, rejected
alternatives, and remaining runtime-mode checks, read
[`ECS.Archetypal.PerEntRowIteration.md`](ECS.Archetypal.PerEntRowIteration.md).

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
| `EntObj` | Garbage-collected owner, mainly for object-lifetime state such as an Indexed context. |
| `EntPtrIdx` | Owning Indexed handle; mutations and disposal run the context hook pipeline. |
| `EntMutIdx` | Non-owning mutable Indexed handle stored in bags and passed to hooks. |

Handles are generational. After individual or arena disposal, old handles are
dead and cannot refer to a later Ent that reuses the same slot. Keep the
owning pointer wherever individual disposal belongs; pass non-owning handles to
systems and indexes that only observe or mutate the Ent.

`EntArena.Dispose()` is bulk scope teardown. It invalidates every allocation in
the arena. Use individual `EntPtr.Dispose()` when per-Ent ownership ends
before the arena does.

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

Once a scope uses Indexed ECS, allocate its game Ents from its
`EntIdxArena` and mutate them through `EntPtrIdx` or `EntMutIdx`. Do not allocate
some Ents from a raw `EntArena` or convert the same scope to raw mutation to
bypass hooks. That leaves bags and indexes inconsistent.

## Scoped Indexed ECS

An Indexed game Ent scope owns one context builder, one Indexed arena, and
the bags and indexes registered on that context. Give these types domain names
and bind them to the same game scope:

```csharp
[World]
public sealed class WorldEntIdxContextBuilder : EntIdxContextBuilder;

[World]
public sealed class WorldEntArena(WorldEntIdxContextBuilder context) :
    EntIdxArena(context.Ent);

[World]
public sealed class WorldProjectileBagMut :
    EntIdxGatedBagMut<WorldComponents.IsProjectile, WorldComponents.IsLoaded>;

[World]
public sealed class WorldProjectileBag(WorldProjectileBagMut bag) :
    EntIdxGatedBag<WorldComponents.IsProjectile, WorldComponents.IsLoaded>(bag);

[World]
public sealed class WorldScratchedBagMut :
    EntIdxBagMut<WorldComponents.IsScratched>;

[World]
public sealed class WorldScratchedBag(WorldScratchedBagMut bag) :
    EntIdxBag<WorldComponents.IsScratched>(bag);
```

`[World]` is the example game's scope attribute, not an ECS requirement. Use the
scope name that owns the Ents. Read
[`GameScopeOrganization.md`](GameScopeOrganization.md) before creating or
reorganizing game scopes and loader scopes.

The mutable bag type is registration state owned by loaders. Runtime systems
should depend on the read wrapper unless they specifically participate in
registration.

Builders may inherit when a child Ent scope intentionally carries the
parent scope's registrations. Each builder instance still owns a separate
context Ent, so hooks do not leak between scope instances.

## Register Before Allocating

Register every bag and hook in loader code before any system allocates or
mutates Ents for the scope:

```csharp
[WorldLoader]
public sealed class WorldLoader(
    WorldEntIdxContextBuilder context,
    WorldProjectileBagMut projectiles,
    WorldScratchedBagMut scratched,
    WorldEntIndex ids,
    WorldSpatialIndex spatial,
    WorldEntDisposeTracker disposals)
{
    public void Run()
    {
        context.AddGatedBag(projectiles);
        context.AddBag(scratched);
        context.AddPre<Guid, WorldComponents.Id>(ids.Intercept);
        context.AddPost<Position, WorldComponents.Position>(spatial.Intercept);
        context.AddPreDispose(disposals.Intercept);
    }
}
```

Registration is not retroactive. A bag or hook added after Ents have
already changed does not scan or reconstruct earlier state.

Use the registrations according to their observable timing:

- `AddPre<T, N>` runs before the write. The old component value is still
  readable, and the hook receives the requested new value. Use it for key
  indexes and dirty comparisons.
- `AddPost<T, N>` runs after the write. Current component state is final. Use it
  for spatial or other derived membership.
- `AddPreDispose` runs before any component is cleared. Use it when cleanup
  needs the whole intact Ent.
- `AddBag` maintains membership when one boolean marker is true.
- `AddGatedBag` maintains membership only while both its boolean marker and
  boolean gate are true.

Hooks run in registration order and may trigger writes to other components.
They must not throw: the pipeline has no rollback, so an exception can leave
component storage and derived state inconsistent. Indexed registration and
mutation for one context are single-threaded.

## Initialize Data Before Publishing Membership

Bag maintenance is immediate. Initialize all Ent data before setting the
marker and gate that publish it to systems:

```csharp
public sealed class WorldProjectileSpawner(WorldEntArena arena)
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
public sealed class WorldProjectileTick(WorldProjectileBag projectiles)
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

A pre-set hook can remove the old key while it is still readable and add the
new key:

```csharp
[World]
public sealed class WorldEntIndex
{
    private readonly Dictionary<Guid, EntMutIdx> ents = [];

    public EntMutIdx this[Guid id] => ents[id];

    public void Intercept(EntMutIdx ent, in Guid value)
    {
        if (ent.Id == value)
            return;

        if (ent.Id != default)
            ents.Remove(ent.Id);

        if (value != default)
            ents.Add(value, ent);
    }
}
```

An ordinary `UnsetId()` runs the same pre-hook with the default `Guid`, so the
old key is removed. Individual `EntPtrIdx.Dispose()` clears components through
their Indexed unset pipelines. Use a pre-dispose hook as well when cleanup
depends on multiple components or must happen before component clearing begins.

## Disposal And Scope Teardown

Individual Indexed disposal maintains derived state:

1. Pre-dispose hooks run while the Ent is intact.
2. Present components are cleared through their Indexed unset pipelines.
3. Bags and component-local indexes observe those unsets.
4. The underlying allocation is released.

`EntIdxArena.Dispose()` is intentionally different. It bulk-invalidates the
arena without running per-Ent hooks. Bags and game-owned indexes may still
contain dead handles and must be treated as invalid after arena disposal. Keep
the context, arena, bags, and indexes in one scope lifetime so they all become
unreachable together.

If teardown needs persistence erasure, network messages, or maintained indexes,
dispose the relevant `EntPtrIdx` allocations individually before disposing the
arena. Use arena disposal for final bulk scope teardown.

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
   lifetime.
10. Treat arena disposal as bulk invalidation, not per-Ent cleanup.

For detailed hook ordering, reentrancy, bag storage, registration validation,
and mutation semantics, continue with [`ECS.Indexed.md`](ECS.Indexed.md).
