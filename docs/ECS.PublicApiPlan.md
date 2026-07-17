# ECS Public API Completion Plan

## Status

This is the active public-API roadmap for the AlvorKit ECS after the first
archetypal implementation. Storage layout, arch graph changes, pooling,
location storage, active-arch indexing, JIT tuning, and other implementation
plumbing are deliberately deferred until the caller-facing contracts below are
approved.

The current package already provides the essential storage behavior:

- generated sparse and archetypal component properties;
- generated point `Has`, get, set, and unset operations;
- arena and Ent lifecycle integration;
- debugger and `ComponentToString` discovery;
- typed final-shape archetypal allocation;
- alloc-scoped, arbitrary multi-component span queries; and
- compact aligned rows with allocation-free enumeration after setup.

The remaining work is primarily API productization: make the fast path easy to
express, make representation boundaries honest, define lifetime and mutation
rules, and keep implementation types out of normal application code.

Related references:

- [`ECS.md`](ECS.md) is the current game-facing guide.
- [`ECS.Archetypal.md`](ECS.Archetypal.md) documents the implemented archetypal
  storage and performance model.
- [`ECS.Archetypal.Features.md`](ECS.Archetypal.Features.md) records the completed
  feature batches and earlier query experiments.
- [`ECS.Indexed.md`](ECS.Indexed.md) defines the observed sparse mutation and bag
  contracts that must not be implied by raw column writes.

## Goals

The completed public API should:

1. Make generated names the normal way to create, access, and query components.
2. Preserve the generic `T`, `N`, and `A` operations as an advanced low-level
   surface and compatibility layer.
3. Keep direct span iteration as the maximum-throughput query primitive.
4. Support any number of selected components without generated component
   combinations or fixed-arity overload families.
5. Expose read-only and mutable access deliberately.
6. State exactly when queries, chunks, spans, refs, and handles remain valid.
7. Match the actual threading model without locks, snapshots, defensive copies,
   or publication work in point and iteration hot paths.
8. Keep the hybrid storage model useful: sparse components serve point access
   and observed/indexed state, while archetypal components serve aligned bulk
   iteration.
9. Add no managed allocation to generated query construction, enumeration, or
   component access after storage setup.
10. Leave room for later row iteration and joins without weakening the simple
    same-group span contract.

Performance priority remains:

1. existing-component point read and write;
2. direct span access and the inner indexed loop;
3. query discovery, creation, and structural add/remove; and
4. management-path footprint and convenience.

No generated facade, metadata addition, lifetime rule, or convenience iterator
may add work to existing point `Get`/`Set` or direct span indexing. New API
layers must forward to exact closed generic operations so callers that do not
use a new feature pay nothing for it.

## Non-Goals

This plan does not design or implement:

- a scheduler or system base class;
- automatic parallel query execution;
- a public arch graph, arch ID, row ID, or storage allocator API;
- active-arch indexes, query-plan caches, pool changes, or location shortcuts;
- change tracking over raw mutable spans;
- arbitrary callback predicates, expression trees, or LINQ integration;
- sorting, relations, events, replication, or persistence;
- automatic SIMD generation;
- a general command buffer before real systems demonstrate the need; or
- every possible cross-group and sparse/archetypal join.

The ECS should remain storage and iteration infrastructure. Application systems
remain ordinary injected instance classes rather than implementing an ECS-owned
system interface.

## Existing Public Baseline

### Declaration and generated point access

One component interface may contain sparse and archetypal properties:

```csharp
[Components]
public interface IMotionComponents
{
    string Name { get; set; }
    bool IsActive { get; set; }

    [Archetypal]
    Position Position { get; set; }

    [Archetypal]
    Velocity Velocity { get; set; }

    [Archetypal]
    Acceleration Acceleration { get; set; }

    [Archetypal]
    bool IsSleeping { get; set; }
}
```

The generated property API already hides the raw component value type, marker
type, and arch group:

```csharp
if (ent.HasPosition)
    ent.Position = nextPosition;

ent.UnsetPosition();
```

That property-shaped surface remains the primary point-access API.

### Final-shape creation

The current low-level API creates a new Ent directly in one final shape:

```csharp
EntPtr ent = arena
    .AllocArchetypal<MotionComponents>()
    .With<Position, MotionComponents.Position>(position)
    .With<Velocity, MotionComponents.Velocity>(velocity)
    .Create();
```

`EntArchCreate` encodes the shape in a nested generic struct chain and the
values in the corresponding nested struct values. The first use of one exact
chain resolves its sorted signature and caches the arch ID; each `Create()`
then appends one row and writes its columns directly.

This is an efficient low-level mechanism, but callers should not need to
understand `EntArchInit`, `IEntArchInit`, or the repeated `T`, `N`, and `A`
arguments for generated components.

### Span queries

The current query API supports an unbounded required selection:

```csharp
var query = arena.QueryArchetypal<MotionComponents>()
    .With<Position, MotionComponents.Position>()
    .With<Velocity, MotionComponents.Velocity>();

foreach (var chunk in query)
{
    ReadOnlySpan<EntMut> ents = chunk.Ents;
    Span<Position> positions =
        chunk.Get<Position, MotionComponents.Position>();
    Span<Velocity> velocities =
        chunk.Get<Velocity, MotionComponents.Velocity>();
}
```

The spans are aligned because every row in one chunk belongs to the same alloc,
arch group, and arch. `Get<T, N>()` may also retrieve an unselected component;
it returns an empty span when the current arch does not contain that column.

The low-level mechanism is sufficient. The missing work is names, filters,
access intent, lifetime documentation, and the decision about row-oriented
iteration.

## Vocabulary and Representation Contract

The public documentation should use these meanings consistently:

- **Component**: one typed value keyed by its generated marker.
- **Sparse component**: point-oriented storage associated directly with an Ent
  slot.
- **Archetypal component**: a component stored in an aligned dense column.
- **Alloc**: one internal storage-ownership partition. Each live `EntArena`
  owns a distinct alloc. Standalone `EntPtr` values created with `new EntPtr()`
  share the standalone-pointer alloc, and `EntObj` values share the object alloc.
- **Arch group**: the independent archetypal cohort identified by `A`. All
  `[Archetypal]` properties in one generated component interface share its
  generated component group as `A`.
- **Arch**: one exact component-membership shape inside an arch group.
- **Chunk**: the nonempty rows for one `(alloc, arch group, arch)`, exposed as
  aligned Ent and component spans.
- **Point access**: accessing one component through an Ent handle.
- **Existing-value write**: changing a component already present without
  changing shape.
- **Structural mutation**: adding or removing an archetypal component, clearing
  or disposing an Ent, or otherwise moving/removing a dense row.

An Ent may participate in multiple arch groups independently. Each group has
its own shape and row order for that Ent.

The standalone `EntPtr` constructor and `EntObj` remain supported for point
access and their existing ownership/lifetime models. They do not provide arena
query or final-shape entry points. Because instances of each standalone owner
kind share an alloc, separate standalone instances are not separate threading
partitions: one thread owns archetypal work for their shared `(alloc, A)`.

## Public Decisions to Freeze Before Plumbing

### Decision 1: one arch group is the span-query boundary

A span query targets exactly one `A`. Within that group, any number of selected
columns can be aligned. Two groups cannot promise aligned spans because their
shapes, row orders, and membership may differ.

The initial contract is therefore:

- components meant to be processed together as spans belong to the same
  generated component group;
- a span query never performs a hidden cross-group or sparse point join;
- sparse components remain point-access data or participate in existing Indexed
  bags;
- a future cross-group operation, if justified, is explicitly row-oriented and
  identifies its driving group; and
- a component interface is an iteration-cohort design decision, not merely a
  source-file organization device.

If a game needs separate cohorts, it declares separate `[Components]`
interfaces. It does not need a second grouping attribute in the first public
version.

### Decision 2: generated names are the normal surface

The raw APIs remain available:

```csharp
query.With<Position, MotionComponents.Position>();
chunk.Get<Position, MotionComponents.Position>();
```

Generated callers should instead write:

```csharp
query.WithPosition();
chunk.ReadPosition();
chunk.WritePosition();
```

Generated extension methods preserve type-state chaining and exact group
typing. They must not allocate descriptor objects, arrays, delegates, closures,
or boxed marker values.

Keep the arena entry points explicit in the first version:

```csharp
arena.AllocArchetypal<MotionComponents>();
arena.QueryArchetypal<MotionComponents>();
```

Do not initially generate `AllocMotionComponents()` and
`QueryMotionComponents()`. The group argument is meaningful public information,
the existing names disclose the storage/iteration model, and generated arena
extension names can collide when component groups with the same simple name are
imported from different namespaces. Reconsider shorter entry names only after
real game call sites show that the remaining group argument is a burden.

The raw generic identity `(T, N, A)` remains a trusted advanced contract. The
first pass does not add runtime metadata validation or new generic constraints
to every hot method. Generated names prevent ordinary callers from mismatching
the value type, component marker, and group. Handwritten raw callers own the
correct pairing.

### Decision 3: membership and access intent stay separate

`WithPosition()` means that matching archs must contain `Position`. It does
not declare a scheduler access mode.

Read/write intent appears when acquiring a column:

```csharp
ReadOnlySpan<Velocity> velocities = chunk.ReadVelocity();
Span<Position> positions = chunk.WritePosition();
```

This matches the current owner-thread model, lets one query descriptor serve
read-only and mutating systems, and avoids introducing scheduler semantics that
the ECS does not have.

The existing mutable `Get<T, N>()` remains as a compatibility and low-level
escape hatch. New examples should prefer generated `Read...` and `Write...`
methods.

### Decision 4: optionality is per chunk

All rows of an arch have the same component membership. An unselected component
is therefore either present for the entire chunk or absent for the entire
chunk.

The generated optional surface should be explicit:

```csharp
if (chunk.TryReadAcceleration(out ReadOnlySpan<Acceleration> acceleration))
{
    // Acceleration exists for every Ent in this chunk.
}
```

Mutable optional access follows the same pattern:

```csharp
if (chunk.TryWriteAcceleration(out Span<Acceleration> acceleration))
{
    // The returned span is aligned with chunk.Ents.
}
```

Do not add `OptionalAcceleration()` to the query initially. It would not change
matching or binding for the current chunk model. Reconsider typed optional
selection only if a future row projection needs optional fields encoded in its
type.

`EntArchChunk<A>` does not encode its query selection in its type. Therefore
`ReadProperty()` and `WriteProperty()` retain the current `Get<T, N>()`
semantics: they return an empty span when the current arch lacks the column.
A matching `WithProperty()` guarantees that the column is present for every
chunk returned by that query, but the compiler does not enforce that pairing.

Failed `TryReadProperty` and `TryWriteProperty` calls return `false` and assign a
default/empty span to the out parameter. The first public version does not add a
selection-typed chunk or projection solely to make required presence static.

### Decision 5: structural mutation is forbidden during an active view

While an enumerator, chunk, span, row cursor, or row ref is active for one
`(alloc, A)`:

- existing component values may be changed through returned mutable spans or
  refs;
- adding or removing a component in that alloc/group is forbidden;
- clearing or disposing any Ent whose operation removes or moves a row in that
  alloc/group is forbidden, even when its arch does not match the current query;
  and
- a structural mutation may move rows, replace pooled arrays, and invalidate
  every outstanding view.

Sparse mutation and mutation of another arch group on the same owner thread do
not invalidate this group's column arrays and are allowed, provided the call
does not clear/dispose the Ent or indirectly mutate the queried group. This does
not add a concurrent-access promise for different groups in one alloc.

This is a caller contract. The runtime does not add mutation counters, locks,
snapshots, copies, or redundant guards to enforce it.

Structural work affecting the queried group, plus clear and disposal, is
applied after enumeration. The first public version uses a caller-owned reusable
staging buffer rather than a general command API.

### Decision 6: threading follows alloc/group ownership

The public threading contract is:

- one thread exclusively owns reads, writes, queries, and row changes for one
  `(alloc, A)` at a time;
- read-only access does not imply multiple-reader safety;
- different threads may operate on the same arch group through different
  allocs;
- a readonly query descriptor does not make concurrent enumeration of one
  alloc/group safe; and
- no query-level lock, snapshot, defensive copy, or volatile publication is
  promised on point, chunk, or row access.

Do not promise simultaneous mutation of different groups in one alloc until
shared Ent lifecycle and allocator metadata paths have been audited for that
specific contract.

### Decision 7: Indexed observation and raw columns are separate models

Indexed hooks and maintained bags observe writes made through their handle
pipeline. Direct mutable spans and refs write columns without one callback per
Ent and therefore cannot transparently preserve per-Ent hook semantics.

The first public contract should be explicit:

- archetypal query writes are raw existing-value writes;
- raw span/ref writes do not fire Indexed pre/post hooks;
- components whose every write must maintain a hook, bag, dirty index, or other
  derived state remain sparse/Indexed;
- `EntPtrIdx` and `EntMutIdx` archetypal point get/has/set/unset use reachable
  base semantics and do not fire component pre/post hooks;
- Indexed registration rejects archetypal markers in `AddPre`, `AddPost`,
  `AddBag`, and either side of `AddGatedBag`;
- Indexed `Clear` and per-Ent disposal do not fire component-local archetypal
  unset hooks;
- the whole-Ent pre-dispose hook still runs before clear and may inspect the
  intact Ent;
- `EntIdxArena` exposes no archetypal final-shape or query entry point in the
  first version; and
- no hidden callback loop is added to `Write...()` accessors.

This makes the hybrid ECS a deliberate advantage instead of presenting two
mutation paths with silently different guarantees. A later bulk-observation
API would be a separate feature with explicit range, ordering, and dirty-state
semantics.

The first compatibility matrix is:

| Arena/storage | Point access | Bulk iteration | Observed mutation |
| --- | --- | --- | --- |
| Base + sparse | Generated get/set/has/unset | No general sparse query | None |
| Base + archetypal | Generated point access | Same-group chunks and spans | None |
| Indexed + sparse | Indexed handle pipeline | Maintained bags | Pre/post hooks and bag maintenance |
| Indexed + archetypal | Reachable base point semantics | Not exposed by `EntIdxArena` | Component hook/bag registration is rejected; whole-Ent pre-dispose still runs |

Full observed Indexed/archetypal composition requires a separate design for
allocation, query handles, chunk Ent handles, bulk writes, clear, and disposal.
Until then, rejection is deterministic rather than advisory.

## Target Everyday API

### Generated final-shape creation

The expected creation syntax is:

```csharp
EntPtr ent = arena
    .AllocArchetypal<MotionComponents>()
    .WithPosition(position)
    .WithVelocity(velocity)
    .Create();
```

Requirements:

- generate `WithProperty(in value)` only for accessible archetypal setters;
- support both the first component and every later chain position;
- retain arbitrary component count;
- preserve order-independent final arch resolution;
- allow repeated use of the same closed chain without setup allocation;
- honor nullable and reference-containing component types;
- do not generate every final-shape combination; and
- keep `Create()` unavailable until at least one component has been supplied.

The caller contract is that one final-shape builder supplies each component at
most once. The implementation does not need duplicate-field validation.

Sparse properties continue to use the existing point or fluent mutation API
after the Ent is created:

```csharp
EntPtr ent = arena
    .AllocArchetypal<MotionComponents>()
    .WithPosition(position)
    .WithVelocity(velocity)
    .Create();

ent.Mutate().Name("player");
```

### Generated span query

The expected primary query syntax is:

```csharp
var moving = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity()
    .WithoutIsSleeping();

foreach (var chunk in moving)
{
    Span<Position> positions = chunk.WritePosition();
    ReadOnlySpan<Velocity> velocities = chunk.ReadVelocity();

    for (int i = 0; i < positions.Length; i++)
        positions[i] += velocities[i];
}
```

Generated members for an archetypal property should include:

- `WithProperty()` on the root query and an existing filter chain;
- `ReadProperty()` when the getter is accessible;
- `WriteProperty()` when the setter is accessible;
- `TryReadProperty(out ReadOnlySpan<T>)` when the getter is accessible; and
- `TryWriteProperty(out Span<T>)` when the setter is accessible.

`WithoutProperty()` is added with the exclusion algebra in ECS-API-05 rather
than in the first generated-access batch.

Only `[Archetypal]` properties generate these members. Sparse properties do not
appear as span-query columns.

### Query every active arch in a group

The root query should itself be enumerable:

```csharp
foreach (var chunk in arena.QueryArchetypal<MotionComponents>())
{
    // Every nonempty arch currently used by this alloc and group.
}
```

This defines a useful base for an exclusion-only query:

```csharp
var awake = arena.QueryArchetypal<MotionComponents>()
    .WithoutIsSleeping();
```

Root enumeration means every Ent participating in the group. There is no
stored empty arch, so an Ent with no component in the group does not appear.

This is not arena-wide enumeration. The first milestone does not add a general
`EntArena.Ents` scan or a sparse query language. A system that needs a stable
iterable set should use an archetypal cohort or an Indexed bag. Revisit an
arena-wide view only for a concrete lifecycle/tooling use case.

### Essential filters and terminals

`With` requires a column. `Without` rejects an arch containing a column. Both
are same-group, per-arch membership tests.

The first terminal operations should be:

```csharp
bool anyMoving = moving.Any();
int movingCount = moving.Count();
```

`Any()` stops after the first matching nonempty arch. `Count()` sums Ent rows,
not chunks.

Do not initially add `First`, `Single`, callback-based `ForEach`, sorting,
general predicates, OR expressions, or change filters. They either add little
over `foreach` or require a materially different execution contract.

## Complete Planned API Examples

The examples in this section cover every work item below. **Target** examples
show the recommended stable API. **Experimental** examples show syntax that must
pass its decision gate before becoming public. **Conceptual generated output**
shows the contract the generator/runtime must provide; application code should
normally use the generated facade instead.

Target snippets are specifications for the next implementation batches. They
are not claims that every named member already compiles on the current branch.

| Work item | Example coverage |
| --- | --- |
| ECS-API-01 | Component cohorts, multiple groups, alloc ownership, threading, and Indexed boundaries |
| ECS-API-02 | Generated facade, raw generic equivalent, and hidden type-state infrastructure |
| ECS-API-03 | Named final-shape creation, arbitrary order, reuse, and sparse follow-up |
| ECS-API-04 | Named selection, read/write columns, and optional columns |
| ECS-API-05 | Root queries, `With`, `Without`, `Any`, and `Count` |
| ECS-API-06 | Query reuse, legal between-loop mutation, disposal, and staging |
| ECS-API-07 | Flattened Ent iteration and bound row cursors |
| ECS-API-08 | Common liveness, `TryGet`, nullability, and archetypal refs |
| ECS-API-09 | Reusable structural-mutation staging and a deferred-command candidate |
| ECS-API-10 | Additional-group final-shape attachment and an explicit row join candidate |
| ECS-API-11 | Generated representation markers and Indexed registration rejection |
| ECS-API-12 | End-to-end system and verification examples |

### Shared declarations used by the examples

**Target:** frequently co-iterated columns share one component interface. A
separate interface defines an independent cohort.

```csharp
[Components]
public interface IMotionComponents
{
    string Name { get; set; }
    bool IsActive { get; set; }

    [Archetypal]
    Position Position { get; set; }

    [Archetypal]
    Velocity Velocity { get; set; }

    [Archetypal]
    Acceleration Acceleration { get; set; }

    [Archetypal]
    bool IsSleeping { get; set; }

    [Archetypal]
    RenderState RenderState { get; set; }
}

[Components]
public interface ICombatComponents
{
    [Archetypal]
    Health Health { get; set; }

    [Archetypal]
    Armor Armor { get; set; }
}

public readonly record struct Position(float X, float Y);
public readonly record struct Velocity(float X, float Y);
public readonly record struct Acceleration(float X, float Y);
public readonly record struct Health(int Value);
public readonly record struct Armor(int Value);
public sealed class RenderState
{
    public int MutableValue { get; set; }
}
```

`Name` and `IsActive` remain sparse. `Position`, `Velocity`, `Acceleration`,
`IsSleeping`, and `RenderState` share `MotionComponents`. `Health` and `Armor`
share the independent `CombatComponents` group.

### Generated point API and raw equivalent

**Target application code:**

```csharp
if (ent.HasPosition)
{
    Position position = ent.Position;
    ent.Position = position with { X = position.X + 1 };
}

ent.UnsetPosition();
```

The generated members forward to the trusted low-level identity:

```csharp
bool has = ent.HasArchetypal<
    Position,
    MotionComponents.Position,
    MotionComponents>();

Position? position = ent.GetArchetypal<
    Position,
    MotionComponents.Position,
    MotionComponents>();

ent.SetArchetypal<
    Position,
    MotionComponents.Position,
    MotionComponents>(nextPosition);

ent.UnsetArchetypal<
    Position,
    MotionComponents.Position,
    MotionComponents>();
```

The first form belongs in normal systems and documentation. The second remains
available for handwritten markers, other generators, tests, and advanced code.

### Generated accessibility, nullability, and `SkipBuilder`

**Target declaration:**

```csharp
[Components(SkipBuilder = true)]
public interface IVisibilityComponents
{
    [Archetypal]
    string? Label { get; internal set; }

    [Archetypal]
    Position Position { get; set; }
}
```

The generated surface follows these rules:

```csharp
var query = arena.QueryArchetypal<VisibilityComponents>()
    .WithLabel()       // Public: membership is visible to getter or setter.
    .WithoutPosition();

foreach (var chunk in query)
{
    ReadOnlySpan<string?> labels = chunk.ReadLabel(); // Public getter access.

    // Internal to the declaring assembly because Label's setter is internal.
    Span<string?> writableLabels = chunk.WriteLabel();
}
```

Final-shape `WithLabel(value)` follows setter accessibility. Query
`WithLabel()`/`WithoutLabel()` follow the wider getter/setter accessibility.
`ReadLabel()`/`TryReadLabel()` follow the getter, while
`WriteLabel()`/`TryWriteLabel()` follow the setter.

`SkipBuilder` suppresses the existing `EntMutator` fluent block:

```csharp
// Not generated for IVisibilityComponents:
// ent.Mutate().Position(position);
```

It does not suppress final-shape creation or query methods:

```csharp
EntPtr ent = arena
    .AllocArchetypal<VisibilityComponents>()
    .WithPosition(position)
    .Create();
```

Public/internal accessor combinations are supported. A protected,
private-protected, or otherwise illegal top-level extension-member mapping
produces a generator diagnostic until an explicit normalization rule is
designed.

### Final-shape creation

**Target:** create directly in the known final motion shape.

```csharp
EntPtr first = arena
    .AllocArchetypal<MotionComponents>()
    .WithPosition(new(10, 20))
    .WithVelocity(new(1, 0))
    .Create();
```

Generated names are order-independent:

```csharp
EntPtr second = arena
    .AllocArchetypal<MotionComponents>()
    .WithVelocity(new(0, 1))
    .WithPosition(new(30, 40))
    .Create();
```

Both chains resolve the same canonical arch even though their closed builder
types and one-time caches differ.

The builder is a reusable value snapshot while its arena remains alive:

```csharp
var stationaryAtOrigin = arena
    .AllocArchetypal<MotionComponents>()
    .WithPosition(new(0, 0))
    .WithVelocity(new(0, 0));

EntPtr a = stationaryAtOrigin.Create();
EntPtr b = stationaryAtOrigin.Create();
```

Sparse initialization follows ordinary point creation:

```csharp
EntPtr player = arena
    .AllocArchetypal<MotionComponents>()
    .WithPosition(new(10, 20))
    .WithVelocity(new(1, 0))
    .Create();

player.Name = "player";
```

The equivalent advanced chain remains:

```csharp
EntPtr raw = arena
    .AllocArchetypal<MotionComponents>()
    .With<Position, MotionComponents.Position>(new(10, 20))
    .With<Velocity, MotionComponents.Velocity>(new(1, 0))
    .Create();
```

Calling the same `WithProperty` twice is outside the builder contract:

```csharp
// Invalid caller use. No duplicate-field guard is added to the hot mechanism.
var invalid = arena
    .AllocArchetypal<MotionComponents>()
    .WithPosition(new(0, 0))
    .WithPosition(new(1, 1));
```

### Root, inclusion, and exclusion queries

**Target root query:** enumerate every nonempty arch in one alloc/group.

```csharp
foreach (var chunk in arena.QueryArchetypal<MotionComponents>())
{
    ReadOnlySpan<EntMut> ents = chunk.Ents;

    // A root chunk may or may not have Position.
    ReadOnlySpan<Position> positions = chunk.ReadPosition();
    if (positions.IsEmpty)
        continue;
}
```

**Target required selection:**

```csharp
var moving = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity();
```

**Target exclusion:**

```csharp
var awakeMoving = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity()
    .WithoutIsSleeping();
```

**Target exclusion-only query:**

```csharp
var everyAwakeMotionEnt = arena
    .QueryArchetypal<MotionComponents>()
    .WithoutIsSleeping();
```

The raw equivalents remain available:

```csharp
var rawQuery = arena.QueryArchetypal<MotionComponents>()
    .With<Position, MotionComponents.Position>()
    .With<Velocity, MotionComponents.Velocity>()
    .Without<bool, MotionComponents.IsSleeping>();
```

### Required read/write span access

**Target:** selection establishes presence for every yielded chunk; access intent
is chosen when the column is acquired.

```csharp
foreach (var chunk in awakeMoving)
{
    ReadOnlySpan<Velocity> velocities = chunk.ReadVelocity();
    Span<Position> positions = chunk.WritePosition();

    for (int i = 0; i < positions.Length; i++)
    {
        Position position = positions[i];
        Velocity velocity = velocities[i];
        positions[i] = new(
            position.X + velocity.X,
            position.Y + velocity.Y);
    }
}
```

The chunk type itself is not selection-typed. Direct access on a chunk that
lacks the column returns an empty span:

```csharp
foreach (var chunk in arena.QueryArchetypal<MotionComponents>())
{
    Span<Position> positions = chunk.WritePosition();
    if (positions.IsEmpty)
        continue;

    // Position exists for every row of this chunk.
}
```

`ReadOnlySpan<T>` prevents replacing column elements through that view. It does
not freeze an object referenced by a reference-containing component:

```csharp
ReadOnlySpan<RenderState> states = chunk.ReadRenderState();

// states[0] cannot be replaced through this span.
if (!states.IsEmpty)
    states[0].MutableValue++; // The referenced object remains mutable.
```

### Optional column access

**Target:** an optional component is present or absent for the entire chunk.

```csharp
foreach (var chunk in moving)
{
    if (chunk.TryReadAcceleration(
        out ReadOnlySpan<Acceleration> accelerations))
    {
        // accelerations.Length == chunk.Ents.Length
    }

    if (chunk.TryWriteIsSleeping(out Span<bool> sleeping))
    {
        // sleeping is mutable and aligned with chunk.Ents.
    }
}
```

Failure assigns an empty span:

```csharp
bool present = chunk.TryReadAcceleration(out var accelerations);

if (!present)
    Debug.Assert(accelerations.IsEmpty);
```

No `.OptionalAcceleration()` query selector is generated in the first version.

### Query terminals

**Target:**

```csharp
bool hasAwakeMoving = awakeMoving.Any();
int awakeMovingCount = awakeMoving.Count();
```

`Any()` stops at the first matching nonempty arch. `Count()` sums matching Ent
rows rather than returning the number of chunks.

An empty group behaves normally:

```csharp
var empty = arena.QueryArchetypal<MotionComponents>()
    .WithAcceleration();

Debug.Assert(!empty.Any());
Debug.Assert(empty.Count() == 0);
```

### Query reuse and legal mutation boundaries

**Target:** a descriptor can be reused around completed enumerations.

```csharp
var query = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity();

foreach (var chunk in query)
    Integrate(chunk);

// Legal after the first enumeration has ended.
player.IsSleeping = true;

foreach (var chunk in query)
    Integrate(chunk);
```

The second enumeration observes current membership and live component values.
It is not reading a snapshot captured by the first enumeration.

Shape mutation inside the active view is invalid even when the mutated Ent is
not in the current chunk:

```csharp
foreach (var chunk in query)
{
    // Invalid: adding IsSleeping can move a MotionComponents row.
    chunk.Ents[0].IsSleeping = true;

    // Also invalid if anotherMotionEnt participates in this alloc/group.
    anotherMotionEnt.Clear();
}
```

Starting a new enumeration or terminal after arena disposal throws:

```csharp
var stale = arena.QueryArchetypal<MotionComponents>()
    .WithPosition();

arena.Dispose();

Assert.ThrowsExactly<EntArenaDisposedException>(() => stale.Any());
Assert.ThrowsExactly<EntArenaDisposedException>(() => stale.Count());
Assert.ThrowsExactly<EntArenaDisposedException>(() =>
{
    stale.GetEnumerator();
});
```

**Conceptual exception wording:**

```csharp
public class EntArenaDisposedException() :
    ObjectDisposedException(
        "Attempted to perform an operation on a disposed EntArena.",
        innerException: null);
```

The stale descriptor never attaches to a later arena that reuses the internal
alloc ID.

### Structural mutation staging

**Target:** collect shape changes and apply them after enumeration.

```csharp
private readonly List<EntMut> pendingSleep = [];

public void SleepStoppedEnts(EntArena arena)
{
    pendingSleep.Clear();

    var query = arena.QueryArchetypal<MotionComponents>()
        .WithVelocity()
        .WithoutIsSleeping();

    foreach (var chunk in query)
    {
        ReadOnlySpan<EntMut> ents = chunk.Ents;
        ReadOnlySpan<Velocity> velocities = chunk.ReadVelocity();

        for (int i = 0; i < ents.Length; i++)
        {
            if (velocities[i] == default)
                pendingSleep.Add(ents[i]);
        }
    }

    foreach (EntMut ent in pendingSleep)
        ent.IsSleeping = true;
}
```

The owning system reuses the list. It may reserve capacity during loading so
the update path does not grow it.

Sparse or other-group work that cannot affect the queried group is allowed on
the owner thread:

```csharp
foreach (var chunk in query)
{
    foreach (EntMut ent in chunk.Ents)
    {
        ent.Name = "moving"; // Sparse write.
        ent.Health = new(100); // Another group on the same owner thread.
    }
}
```

`Clear`, disposal, or any operation that can move/remove a row in the queried
`(alloc, A)` is staged until the view ends.

### Alloc ownership and threading

**Target:** different arenas own different allocs and can run the same group on
different threads.

```csharp
using var firstArena = new EntArena();
using var secondArena = new EntArena();

Parallel.Invoke(
    () => Integrate(firstArena),
    () => Integrate(secondArena));
```

Each `Integrate` call exclusively owns its arena's `MotionComponents` data for
the duration of the call.

The following is outside the contract because both calls use the same
`(alloc, MotionComponents)`:

```csharp
// Invalid concurrent ownership.
Parallel.Invoke(
    () => Integrate(firstArena),
    () => Integrate(firstArena));
```

Standalone owners share allocs by owner kind:

```csharp
var firstStandalone = new EntPtr();
var secondStandalone = new EntPtr();
var firstObject = new EntObj();
var secondObject = new EntObj();
```

`firstStandalone` and `secondStandalone` are not separate archetypal threading
partitions. Likewise, separate `EntObj` instances are not separate allocs.

### Indexed and archetypal compatibility

**Target:** Indexed sparse components retain observed semantics. Archetypal
point members on Indexed handles use reachable base semantics and are not
observed by component hooks.

```csharp
var context = new EntIdxContextBuilder();
var active = new EntIdxBagMut<MotionComponents.IsActive>();
context.AddBag(active);

using var arena = new EntIdxArena(context.Ent);

EntPtrIdx ent = arena.Alloc();

// Sparse Indexed mutation is observed and maintains the bag.
ent.IsActive = true;
Debug.Assert(active.Contains(ent));

// Reachable base archetypal point semantics. No component pre/post hook runs.
ent.Position = new(10, 20);
ent.Position = new(11, 20);
ent.UnsetPosition();

// No final-shape/query entry point exists on EntIdxArena in this version:
// arena.AllocArchetypal<MotionComponents>();
// arena.QueryArchetypal<MotionComponents>();
```

Registration rejects an archetypal marker:

```csharp
static void OnPositionChanged(EntMutIdx ent)
{
}

var context = new EntIdxContextBuilder();

Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
    context.AddPost<Position, MotionComponents.Position>(
        OnPositionChanged));
```

Archetypal markers are also rejected as a bag marker or gate:

```csharp
var sleeping = new EntIdxBagMut<MotionComponents.IsSleeping>();

Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
    context.AddBag(sleeping));
```

Whole-Ent pre-dispose remains valid:

```csharp
context.AddPreDispose(ent =>
{
    if (ent.HasPosition)
        SavePosition(ent.Position);
});
```

The pre-dispose callback runs before clear. Component-local archetypal unset
hooks do not run during clear/disposal because such registration is rejected.

### Common liveness and `TryGet`

**Target application code:**

```csharp
IEnt ent = GetTarget();

if (!ent.IsAlive)
    return;

if (ent.TryGetPosition(out Position position))
    Use(position);
```

`TryGet` distinguishes absence from a present default value:

```csharp
target.Position = default;

Debug.Assert(target.TryGetPosition(out Position position));
Debug.Assert(position == default);

target.UnsetPosition();

Debug.Assert(!target.TryGetPosition(out position));
```

**Conceptual runtime signatures:**

```csharp
public interface IEnt
{
    EntHandle Handle { get; }

    T? Get<T, N>();
    bool Has<T, N>();
    T? GetArchetypal<T, N, A>();
    bool HasArchetypal<T, N, A>();

    bool IsAlive => Handle.IsAlive;

    bool TryGet<T, N>(
        [MaybeNullWhen(false)] out T value)
    {
        if (!Has<T, N>())
        {
            value = default!;
            return false;
        }

        value = Get<T, N>()!;
        return true;
    }

    bool TryGetArchetypal<T, N, A>(
        [MaybeNullWhen(false)] out T value)
    {
        if (!HasArchetypal<T, N, A>())
        {
            value = default!;
            return false;
        }

        value = GetArchetypal<T, N, A>()!;
        return true;
    }
}
```

**Conceptual generated forwarding:**

```csharp
public bool TryGetPosition(
    [MaybeNullWhen(false)] out Position value) =>
    ent.TryGetArchetypal<
        Position,
        MotionComponents.Position,
        MotionComponents>(out value);

public bool TryGetName(
    [MaybeNullWhen(false)] out string value) =>
    ent.TryGet<string, MotionComponents.Name>(out value);
```

Built-in handles use one presence/value lookup. The default interface path may
compose existing operations for compatibility with custom `IEnt`
implementations.

### Representation marker metadata

**Conceptual generated output:** sparse markers continue implementing only
`IComponent`; archetypal markers identify their group through the additive
marker interface.

```csharp
public interface IArchetypalComponent<A> : IComponent;

public abstract class MotionComponents : IComponentGroup
{
    public abstract class Name : IComponent
    {
        public static EntComponent Component =>
            new(typeof(string), typeof(Name));
    }

    public abstract class Position :
        IArchetypalComponent<MotionComponents>
    {
        public static EntComponent Component =>
            new(typeof(global::MyGame.Position), typeof(Position));
    }
}
```

The descriptor remains unchanged:

```csharp
EntComponent component = MotionComponents.Position.Component;
var (valueType, markerType) = component;
```

Indexed registration can reject the marker contract during load-time
validation without changing `EntComponent` size, equality, or hashing.

```csharp
Assert.IsTrue(
    typeof(IArchetypalComponent<MotionComponents>)
        .IsAssignableFrom(typeof(MotionComponents.Position)));

Assert.IsFalse(
    typeof(IArchetypalComponent<MotionComponents>)
        .IsAssignableFrom(typeof(MotionComponents.Name)));
```

### Hidden but supported type-state infrastructure

**Conceptual runtime declaration:**

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IEntArchSelect<A>
{
    static abstract bool Matches(int allocId, int archId);
}

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct EntArchSelect<T, N, A> :
    IEntArchSelect<A>
{
    public static bool Matches(int allocId, int archId) =>
        EntArchColumn<T, N, A>.ValuesAt(allocId, archId) is not null;
}
```

The same discoverability rule applies to raw concrete point redeclarations:

```csharp
public readonly record struct EntPtr
{
    private readonly EntMut ent;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public T? GetArchetypal<T, N, A>() =>
        ent.GetArchetypal<T, N, A>();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetArchetypal<T, N, A>(in T value) =>
        ent.SetArchetypal<T, N, A>(value);
}
```

Application code still sees ordinary inferred values:

```csharp
var query = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity();
```

Quick Info or diagnostics may show the nested type-state names. Callers never
need to write them on the generated path.

### Experimental flattened Ent iteration

**Experimental candidate:**

```csharp
foreach (EntMut ent in moving.Ents())
{
    Position position = ent.Position;
    Velocity velocity = ent.Velocity;
    ent.Position = new(
        position.X + velocity.X,
        position.Y + velocity.Y);
}
```

This must be allocation-free if retained, but it repeats generated point
lookups. It is a convenience path, not a replacement for direct spans.

### Generated bound row cursor

**Implemented shape:**

```csharp
foreach (var row in moving.Rows())
    row.Position = new(
        row.Position.X + row.Velocity.X,
        row.Position.Y + row.Velocity.Y);
```

The row cursor binds columns once per arch and each access reduces to
cached-base-plus-row addressing. Exact shapes are generated only for closed
queries actually used with `Rows()`; no projection power set is emitted.

Optional row refs require explicit `Try` access:

```csharp
foreach (var row in moving.Rows())
{
    if (row.HasAcceleration)
        row.Acceleration = default;
}
```

The exact optional ref representation is part of the row prototype; it is not
frozen by the span API.

### Experimental archetypal point ref

**Experimental candidate:**

```csharp
if (ent.HasPosition)
{
    ref Position position = ref ent.GetPositionRef();
    position = position with { X = position.X + 1 };
}
```

The ref requires the archetypal component to be present and cannot survive a
structural mutation of its `(alloc, MotionComponents)`. Sparse refs require a
separate design and are not implied by this example.

### Experimental additional-group final-shape attachment

**Experimental candidate:** initialize another group without occupying its
intermediate archs.

```csharp
ent.AttachArchetypal<CombatComponents>()
    .WithHealth(new(100))
    .WithArmor(new(25))
    .Apply();
```

The API is not accepted until `Apply()` semantics for existing group membership,
ownership, invalidation, and replacement are settled.

### Experimental explicit cross-group join

**Experimental candidate:** a row join names its driving group and does not
claim aligned cross-group spans.

```csharp
var combatants = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .Join<CombatComponents>()
    .WithHealth();

foreach (var row in combatants.Rows())
    Update(ref row.Position, in row.Health);
```

This implies per-Ent lookup into `CombatComponents`. It remains deferred until
real systems justify the join cost and public shape.

The current explicit alternative is a same-group driving query plus point
access:

```csharp
foreach (var chunk in moving)
{
    ReadOnlySpan<EntMut> ents = chunk.Ents;

    for (int i = 0; i < ents.Length; i++)
    {
        if (ents[i].TryGetHealth(out Health health))
            UseHealth(ents[i], health);
    }
}
```

### Deferred command-buffer candidate

Caller-owned staging remains the target first version. If real systems require
a general command API, a possible shape is:

```csharp
commands.SetIsSleeping(ent, true);
commands.UnsetVelocity(ent);
commands.Clear(otherEnt);
commands.Dispose(ownedEnt);

commands.Apply();
```

This syntax is illustrative only. Before acceptance, the command contract must
cover operation ordering, ownership, sparse/archetypal storage, Indexed hooks,
reference-containing values, repeated operations on one Ent, and reusable
allocation-free buffers after setup.

### End-to-end generated system

**Target:** an ordinary injected instance system owns its reusable staging
buffer and uses generated query names.

```csharp
public sealed class MotionSystem
{
    private readonly List<EntMut> stopped = [];

    public void Update(EntArena arena)
    {
        stopped.Clear();

        var moving = arena.QueryArchetypal<MotionComponents>()
            .WithPosition()
            .WithVelocity()
            .WithoutIsSleeping();

        foreach (var chunk in moving)
        {
            ReadOnlySpan<EntMut> ents = chunk.Ents;
            Span<Position> positions = chunk.WritePosition();
            ReadOnlySpan<Velocity> velocities = chunk.ReadVelocity();

            for (int i = 0; i < positions.Length; i++)
            {
                Position position = positions[i];
                Velocity velocity = velocities[i];

                positions[i] = new(
                    position.X + velocity.X,
                    position.Y + velocity.Y);

                if (velocity == default)
                    stopped.Add(ents[i]);
            }
        }

        foreach (EntMut ent in stopped)
            ent.IsSleeping = true;
    }
}
```

No ECS-owned system interface or scheduler is introduced.

### Runtime behavior verification examples

**Root, inclusion, exclusion, and terminals:**

```csharp
[TestMethod]
public void QueryFiltersAndTerminals_SelectExpectedEnts()
{
    using var arena = new EntArena();

    arena.AllocArchetypal<MotionComponents>()
        .WithPosition(new(1, 2))
        .WithVelocity(new(3, 4))
        .Create();

    arena.AllocArchetypal<MotionComponents>()
        .WithPosition(new(5, 6))
        .WithVelocity(new(0, 0))
        .WithIsSleeping(true)
        .Create();

    var awake = arena.QueryArchetypal<MotionComponents>()
        .WithPosition()
        .WithVelocity()
        .WithoutIsSleeping();

    Assert.IsTrue(awake.Any());
    Assert.AreEqual(1, awake.Count());
}
```

**Optional access:**

```csharp
[TestMethod]
public void OptionalColumn_IsResolvedOncePerChunk()
{
    using var arena = new EntArena();

    arena.AllocArchetypal<MotionComponents>()
        .WithPosition(new(1, 2))
        .Create();

    foreach (var chunk in arena.QueryArchetypal<MotionComponents>())
    {
        bool found = chunk.TryReadAcceleration(out var values);
        Assert.AreEqual(found, !values.IsEmpty);
    }
}
```

**Stale descriptor:**

```csharp
[TestMethod]
public void QueryAfterArenaDispose_Throws()
{
    var arena = new EntArena();
    var query = arena.QueryArchetypal<MotionComponents>()
        .WithPosition();

    arena.Dispose();

    Assert.ThrowsExactly<EntArenaDisposedException>(() => query.Any());
    Assert.ThrowsExactly<EntArenaDisposedException>(() => query.Count());
}
```

**Independent alloc owners:**

```csharp
[TestMethod]
public void DifferentArenas_CanUseSameGroupConcurrently()
{
    using var first = CreateMotionArena();
    using var second = CreateMotionArena();

    Parallel.Invoke(
        () => Integrate(first),
        () => Integrate(second));
}
```

**Zero allocation after warmup:**

```csharp
var query = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity();

Consume(query); // JIT and warmup.

long before = GC.GetAllocatedBytesForCurrentThread();
Consume(query);
long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

Assert.AreEqual(0L, allocated);
```

All accepted performance and disassembly examples run in an optimized Release
build. Short runs are used while choosing the API; long confirmation is reserved
for the accepted candidate.

## Public API Work Items

### ECS-API-01: approve the arch-group and hybrid-storage contract

Write the normative public wording for:

- one component interface as one archetypal iteration cohort;
- one group per span query;
- Ent participation in multiple independent groups;
- arena alloc ownership versus the shared standalone `EntPtr` and `EntObj`
  allocs;
- sparse components as point/Indexed data;
- raw archetypal writes bypassing Indexed hooks; and
- the absence of hidden joins.

Acceptance criteria:

- the same wording appears in the game-facing ECS guide and detailed
  archetypal guide;
- examples show how to split components into cohorts intentionally;
- no example implies that separate groups can return aligned spans; and
- Indexed registration behavior for `[Archetypal]` markers is explicit rather
  than accidental.

This item blocks generated query design because the generator must know which
properties are legal on each receiver.

### ECS-API-02: define the stable low-level versus generated layers

Classify the existing public types into two layers.

Normal application surface:

- `EntArena`, Ent handles, and generated component members;
- `EntArchCreate<A>` and `EntArchQuery<A>` entry points;
- inferred query/create values; and
- `EntArchChunk<A>` plus generated column methods.

Advanced low-level surface:

- `With<T, N>()`;
- `Get<T, N>()` on chunks;
- raw `GetArchetypal`, `HasArchetypal`, `SetArchetypal`, and
  `UnsetArchetypal`; and
- inferred type-state nodes.

Implementation-shaped public types such as `EntArchInit`, `IEntArchInit`,
`EntArchSelect`, and `IEntArchSelect` may need public accessibility because they
appear in inferred signatures. After the generated facade exists, hide them
from ordinary IntelliSense with `EditorBrowsable(Never)` instead of adding
runtime wrappers.

They are runtime type-state infrastructure used by generated code and are not
recommended application extension points. However, if they remain public at API freeze, they are advanced
supported contracts and changes to their signatures carry compatibility cost.
Their methods expose alloc IDs, arch IDs, and rows, so external implementations
also take responsibility for the trusted low-level ownership contract.

`EditorBrowsable` only removes normal completion choices. Inferred types, Quick
Info, diagnostics, and generated documentation may still display these names.
If that remaining exposure is unacceptable, the types must be redesigned before
freeze; hiding attributes cannot solve it.

Acceptance criteria:

- generated examples never spell `T`, `N`, or `A` beyond choosing the group;
- low-level examples remain available in the detailed reference;
- no compatibility member is removed in this phase; and
- hiding machinery changes discovery only, not runtime behavior;
- hiding raw point members is applied consistently to interface and concrete
  handle redeclarations; and
- inferred machinery may remain visible in type information, but callers never
  need to spell it on the generated path.

### ECS-API-03: generate named final-shape creation methods

Add the `WithProperty(value)` surface described above.

Generator rules:

- setter accessibility controls creation-method accessibility;
- `[Components(SkipBuilder = true)]` retains its existing meaning: it suppresses
  the generated `EntMutator` fluent block, not final-shape creation or query
  access. Add a separate option only if a real collision or code-size use case
  needs to suppress the new facade;
- lazy initialization has no special creation semantics;
- duplicate names in different groups remain isolated by the exact receiver
  group;
- generic component value types, nullable values, delegates, value-only values,
  and reference-containing component types compile; and
- the generated chain uses the existing type-state builder rather than runtime
  descriptors.

Acceptance criteria:

- one, two, four, and more named calls compile in arbitrary order;
- different call orders resolve to the same final arch;
- creation writes the supplied values exactly once;
- no intermediate arch is occupied; and
- repeated creation through one chain allocates no builder objects.

### ECS-API-04: generate named query selectors and chunk accessors

Add `WithProperty`, `ReadProperty`, `WriteProperty`, `TryReadProperty`, and
`TryWriteProperty` for archetypal properties.

Generator access rules:

- the wider getter/setter accessibility controls `With`, because membership is
  needed by both readers and writers;
- getter accessibility controls `Read` and `TryRead`;
- setter accessibility controls `Write` and `TryWrite`;
- sparse properties produce none of these arch-column members;
- query access never triggers `[ComponentLazyInitialize]`; and
- returned nullable spans preserve the declared component nullability.

Acceptance criteria:

- arbitrary selection length compiles without generated combinations;
- a query using `WithProperty()` returns only chunks whose corresponding direct
  access span is nonempty and aligned with `chunk.Ents`;
- direct access on a root or differently filtered chunk returns an empty span
  when the column is absent;
- optional access succeeds or fails once per chunk, never once per row;
- failed optional access assigns an empty span;
- read access cannot replace column elements through its returned type; for
  reference-containing components, this does not make the referenced objects
  deeply immutable;
- write access returns the direct mutable column span; and
- no generated call allocates after setup.

### ECS-API-05: add root enumeration, exclusion, and terminals

Complete the minimal query algebra:

- root query means every active arch in the selected alloc/group;
- `With` requires membership;
- `Without` excludes membership;
- `Any()` tests for at least one matching Ent; and
- `Count()` counts matching Ent rows.

Contradictory or repeated filters do not need defensive validation. They may
naturally match nothing or repeat a membership test.

Generate `WithoutProperty()` on the root query and existing filter chains in
this same batch. It uses the wider getter/setter accessibility because both
readers and writers may need to exclude a component by membership.

Acceptance criteria:

- root, inclusion-only, exclusion-only, and mixed queries behave as defined;
- empty groups return no chunks, `Any() == false`, and `Count() == 0`;
- terminals allocate nothing;
- all filters stay inside one group; and
- query setup still supports an unbounded number of filters.

### ECS-API-06: freeze query reuse, lifetime, and invalidation

The current query descriptor is a small readonly value and appears naturally
reusable. Its stable public contract should be:

- a descriptor may be stored and enumerated repeatedly while its arena remains
  alive;
- each new enumeration applies pending owner cleanup before it begins and then
  observes the current arch membership;
- structural changes are allowed between completed enumerations;
- a descriptor must never begin observing a later arena that reused the same
  alloc ID; and
- an enumerator, chunk, span, cursor, or ref is invalid after a structural
  mutation affecting its alloc/group or after arena disposal.

Whether the descriptor captures arena generation and exactly when pending
cleanup drains are plumbing questions. The public lifetime above is the target.

This is not a snapshot contract. Component values remain live and mutable.
Membership remains stable only because mutation of the queried alloc/group is
forbidden until that enumeration ends. A later enumeration sees legal
structural changes made after the previous one completed.

Starting `GetEnumerator()`, `Any()`, or `Count()` after the originating arena is
disposed throws `EntArenaDisposedException`. Repeating enumeration after
disposal has the same result. No stale descriptor may attach to a recycled
alloc.

The normative safe-mutation workflow is also part of this milestone: collect
structural work into a caller-owned reusable buffer, finish the enumeration,
then apply it. ECS-API-09 validates and documents that workflow in real systems;
it does not postpone the rule.

Acceptance criteria:

- a stored query works before and after legal between-loop structural changes;
- `GetEnumerator()`, `Any()`, and `Count()` on a query from a disposed arena
  throw `EntArenaDisposedException` and cannot observe a reused alloc;
- docs place invalidation rules beside the first query example;
- `EntArenaDisposedException` describes an operation on a disposed arena rather
  than describing every failure as a write; and
- no runtime synchronization is introduced to make misuse recoverable.

### ECS-API-07: implement one-Ent-at-a-time iteration (completed)

This API must be measured before it becomes stable.

Candidate A is a simple flattened Ent view:

```csharp
foreach (EntMut ent in moving.Ents())
{
    ent.Position += ent.Velocity;
}
```

It is easy to use but repeats point access and does not reuse the already-bound
chunk columns. If retained, it is a convenience path rather than the preferred
hot path.

Candidate B is a bound row cursor:

```csharp
foreach (var row in moving.Rows())
    row.Position += row.Velocity;
```

The bound cursor is acceptable only if it:

- binds selected columns once when entering each arch;
- caches Ent and component base refs;
- advances only a row index inside the arch;
- performs no per-component Ent location, graph, hash, virtual, or directory
  lookup in the inner loop; and
- allocates nothing.

Short Release comparisons must cover direct chunk spans, flattened Ent access,
generic row binding, and generated row properties with one, two, four, and eight
components; reads and writes; small and large chunks; first and last selected
fields; and value-only and reference-containing types.

Decision gate:

- retain the bound row cursor if its generated access is direct cached-base
  addressing with an acceptable constant cost;
- optionally retain flattened Ent iteration as clearly labelled convenience;
- otherwise keep spans as the only primary iteration API; and
- never generate every query projection combination.

Implemented outcome:

- generated `WithProperty()` selectors build the existing unbounded typed
  selection chain;
- a demand-driven generator emits one exact row/enumerator for each used closed
  `Rows()` receiver type;
- every selected column is bound once per matching arch;
- every selected component property returns a writable direct base-plus-row ref;
- one-, two-, and eight-field Release measurements are within 4% of spans or
  faster, with 0 B loop allocation; and
- the two-field Tier-1 inner loop contains one row/count branch and two indexed
  loads with no accessor or column lookup call.

### ECS-API-08: complete point-read semantics and evaluate refs separately

The point API currently uses a default value for both an absent component and a
present component whose value is default. It also exposes `IsAlive` on mutable
handles but not through the common read contract.

Additive public candidates are:

```csharp
if (ent.TryGetPosition(out Position position))
{
    // The component is present, even when position is its default value.
}

if (!ent.IsAlive)
{
    // The handle no longer identifies a live Ent.
}
```

Recommended contract:

- expose `IsAlive` on `IEnt` with a compatibility default implementation based
  on `Handle`, so read and mutable handles answer the same lifetime question;
- expose direct public `IsAlive` properties on the built-in read handles rather
  than leaving their current implementations internal;
- add raw sparse and archetypal `TryGet` operations and generated
  `TryGetProperty(out value)`;
- built-in handles use one presence/value lookup rather than composing `Has`
  and `Get`;
- custom `IEnt` implementations may receive a compatibility default
  implementation; and
- `TryGet` returns false for a dead handle or an absent component, while
  `IsAlive` distinguishes those cases when needed.

Raw and generated `TryGet` methods use the equivalent of
`[MaybeNullWhen(false)] out T value`. This preserves useful nullable analysis
for non-nullable reference components while allowing the false path to assign
default.

The raw `EntMutator<T>.Set<CT, N>` method remains sparse. Generated mutator
methods already route each property to its declared storage. Document the raw
method as representation-specific rather than implying that it selects storage
from component metadata.

Generated point properties currently return values and use the ordinary setter
for writes. A direct existing-component ref could improve branch-heavy point
mutation:

```csharp
ref Position position = ref ent.GetPositionRef();
```

This experiment applies only to archetypal components. Sparse refs would have a
different page/generation lifetime and require a separate contract. The
archetypal ref surface is not required for query completion: the component must
already be present and the ref cannot survive any structural mutation of its
alloc/group.

Acceptance gate:

- expose it only for mutable handles;
- do not structurally add a missing component;
- do not add a wrapper allocation or lookup object;
- document the exact ref lifetime;
- compare it in Release with generated property get/set and span/row access;
  and
- add it only if a real point-heavy system benefits enough to justify the
  lifetime burden.

### ECS-API-09: validate caller-owned staging in real systems

The first query API should show a reusable caller-owned buffer:

```csharp
pendingSleep.Clear();

foreach (var chunk in moving)
{
    ReadOnlySpan<EntMut> ents = chunk.Ents;
    ReadOnlySpan<Velocity> velocities = chunk.ReadVelocity();

    for (int i = 0; i < ents.Length; i++)
    {
        if (velocities[i].LengthSquared == 0)
            pendingSleep.Add(ents[i]);
    }
}

foreach (EntMut ent in pendingSleep)
    ent.IsSleeping = true;
```

The owning system reuses the buffer and controls when shape changes apply.

A general deferred-mutation API is a later feature. It must not begin until its
public contract covers sparse and archetypal writes, unset, clear, disposal,
operation ordering, Indexed hooks, reference-containing values, ownership, and
buffer reuse without boxing.

### ECS-API-10: evaluate additional-group final-shape attachment

`AllocArchetypal<A>()` efficiently initializes one group while allocating a new
Ent. An existing Ent can join another group through sequential generated
setters, but that may occupy intermediate archs.

After real multi-group creation patterns exist, evaluate an explicit operation
such as:

```csharp
ent.AttachArchetypal<RenderComponents>()
    .WithMesh(mesh)
    .WithMaterial(material)
    .Apply();
```

Do not add this merely for symmetry. Before approval, define:

- whether the Ent must currently be outside the group;
- whether `Apply()` adds, replaces, or rejects an existing shape;
- whether the receiver must be mutable or owning;
- interaction with active query views; and
- whether existing sequential setters are already sufficient.

This is later than generated creation and query work.

### ECS-API-11: expose representation metadata without changing descriptor layout

`EntComponent` currently exposes value and marker types. Internal debug plumbing
knows whether a component is sparse or archetypal and which group owns it, but
public tooling and Indexed registration cannot inspect that contract directly.

Do not add stored fields to the public `EntComponent` record struct in an
additive release. That would change its size/layout and record equality/hash
semantics even though its two-argument construction still compiled.

Instead, generate an additive marker contract:

```csharp
public interface IArchetypalComponent<A> : IComponent;
```

Generated sparse markers continue implementing `IComponent`. Generated
archetypal markers implement `IArchetypalComponent<TheirGroup>`. The generic
interface identifies both representation and group without exposing storage
addresses or changing `EntComponent` layout. A public inspection helper may be
added over this marker if a tooling consumer needs a non-reflection shape.

Do not expose arch IDs, row IDs, column arrays, or allocator data as metadata.

Acceptance criteria:

- existing `EntComponent` construction, deconstruction, size expectations,
  equality, and hashing remain unchanged;
- sparse markers do not implement the archetypal marker contract;
- archetypal markers implement it with the correct generated group type;
- Indexed registration can reject unsupported archetypal hook/bag contracts;
- debugger and `ComponentToString` output remain unchanged; and
- the metadata does not become a hot runtime component-enumeration API.

### ECS-API-12: documentation, demo, and API freeze

Once the surface decisions are implemented, update the primary ECS guide and
archetypal reference with one consistent generated example covering:

- mixed sparse/archetypal declaration;
- generated point access;
- named final-shape creation;
- multiple required query components;
- exclusion;
- read-only and mutable spans;
- optional chunk access;
- `Any()` and `Count()`;
- caller-owned structural staging;
- query invalidation; and
- alloc/group thread ownership.

The demo should use generated names. The detailed reference may additionally
show the raw generic surface.

The public API is ready to freeze when a consumer can use the generated path
without naming `EntArchInit`, `EntArchSelect`, field marker generic arguments,
arch IDs, rows, or storage arrays.

## Compatibility and Migration

The first completion pass should be additive:

- retain `AllocArchetypal<A>()`, `QueryArchetypal<A>()`, raw `With<T, N>()`, and
  chunk `Get<T, N>()`;
- add generated names over the existing mechanisms;
- add `Without`, root enumeration, `Any`, and `Count` without changing existing
  inclusion semantics;
- hide implementation types from IntelliSense only after generated names cover
  normal use; and
- do not obsolete mutable `Get<T, N>()` until generated read/write names have
  shipped and application code has been tried against them.

Experimental APIs remain explicitly unfrozen:

- flattened `Ents()` enumeration;
- direct point refs;
- cross-group joins;
- deferred mutation; and
- additional-group final-shape attachment.

These should not constrain plumbing until their public shape passes its own
decision gate.

## Generator Verification Matrix

Generated API tests should compile emitted source against the runtime package,
not only compare text fragments.

Required declaration cases:

- sparse-only, archetypal-only, and mixed interfaces;
- one and many archetypal properties;
- value, nullable value, reference, nullable reference, and delegate values;
- public and internal component groups;
- asymmetric public/internal getter and setter accessibility;
- `[ComponentLazyInitialize]`;
- `[ComponentToString]`;
- `[Components(SkipBuilder = true)]`; and
- duplicate property names in different groups.

The current declaration model requires every component property to have both a
getter and setter. Getter-only and setter-only declarations are not added by
this plan. Public and internal accessibility can be mirrored by generated
top-level extension members. Protected, private-protected, or other accessor
forms that cannot be copied legally onto those members require a generator
diagnostic or an explicitly documented normalization rule; the archetypal
facade must not silently promise unsupported modifiers.

Required generated-chain cases:

- first and subsequent `WithProperty` calls;
- first and subsequent `WithoutProperty` calls;
- mixed inclusion/exclusion order;
- one, two, four, and more selected components;
- required `ReadProperty` and `WriteProperty` access;
- optional `TryReadProperty` and `TryWriteProperty` access; and
- generated point `TryGetProperty`; and
- no query/create members for sparse properties.

## Runtime Acceptance Matrix

Behavior tests should cover:

- root query enumeration;
- required and excluded arch membership;
- optional present and absent columns;
- multiple matching archs and one matching arch;
- empty group and empty alloc;
- repeated enumeration around legal structural changes;
- arena disposal and alloc ID reuse;
- `GetEnumerator()`, `Any()`, and `Count()` throwing
  `EntArenaDisposedException` after originating arena disposal;
- read-only versus mutable access types;
- alignment of Ents and every returned column;
- final-shape creation in different call orders;
- multiple allocs using the same group;
- different alloc owners operating concurrently; and
- deterministic Indexed rejection for archetypal hook/bag registration;
- reachable Indexed archetypal point get/set/has/unset using unobserved base
  semantics; and
- Indexed clear/dispose behavior, including whole-Ent pre-dispose but no
  component-local archetypal unset hooks.

Point behavior tests should additionally cover live-present-default,
live-absent, dead-handle, sparse, archetypal, and custom `IEnt` adapter cases for
`IsAlive` and `TryGet`.

Performance checks for accepted hot-path APIs use optimized Release builds.
Quick iteration should use short runs; long confirmation is reserved for an API
candidate that has already passed behavior and allocation checks.

After setup, verify zero managed allocations for:

- query construction;
- generated `With` and `Without` chaining;
- `foreach` enumeration;
- `Any()` and `Count()`;
- generated required and optional column access; and
- any retained row cursor; and
- any retained flattened `Ents()` enumerable and enumerator.

## Recommended Order

| Order | Work item | Result |
| ---: | --- | --- |
| 1 | ECS-API-01 | Approve group, hybrid-storage, Indexed, and threading boundaries. |
| 2 | ECS-API-11 | Add representation metadata and make Indexed rejection explicit. |
| 3 | ECS-API-02 | Freeze the generated versus low-level surface. |
| 4 | ECS-API-06 | Freeze query reuse and invalidation before exposing more views. |
| 5 | ECS-API-08 | Add common liveness and point `TryGet`; keep refs experimental. |
| 6 | ECS-API-03 | Add named final-shape creation. |
| 7 | ECS-API-04 | Add named query and chunk access. |
| 8 | ECS-API-05 | Add root enumeration, exclusion, `Any`, and `Count`. |
| 9 | ECS-API-07 | Implement exact demand-generated one-Ent-at-a-time rows. |
| 10 | ECS-API-09 | Document caller-owned staging with real systems. |
| 11 | ECS-API-10 | Evaluate final-shape attachment for additional groups if needed. |
| 12 | ECS-API-12 | Complete docs/demo and freeze the accepted API. |

The work through ECS-API-05 forms the public point, span-query, and creation
milestone. Row iteration is the next decision, not a prerequisite for shipping
the span API.

## Plumbing Deferred Until the Surface Is Approved

The approved public surface may later be backed by:

- different selection/filter type-state nodes;
- alloc-generation capture;
- different pending-cleanup drain placement;
- active-arch indexing;
- cached column bindings;
- query-specific JIT specialization;
- direct point refs;
- a different internal representation of final-shape initialization; or
- other storage and footprint work.

None of those choices should leak into generated method names or application
code. The public contract is expressed in groups, component names, queries,
chunks, spans, Ent handles, and lifetimes; not graph nodes, field IDs, arch IDs,
rows, alloc IDs, pools, or column directories.

## Definition of Public API Completion

The ECS public API is complete for the current feature set when:

1. A generated component declaration is sufficient to obtain point access,
   final-shape creation, same-group query filters, and named column access.
2. A query selects any number of components and can exclude components without
   runtime descriptor allocation.
3. Callers can explicitly choose read-only or mutable column access.
4. Optional columns have an unambiguous generated API.
5. Root enumeration, `Any()`, and `Count()` cover the basic query operations.
6. Same-group alignment, structural invalidation, query reuse, and threading
   are documented as stable contracts.
7. The relationship between archetypal raw writes and Indexed observation is
   explicit.
8. Generic implementation types are not normal completion choices, never need
   to be spelled on the generated path, and are absent from ordinary examples.
9. The generated API is verified across access, nullability, mixed storage, and
   builder options.
10. Read and mutable handles share a public liveness question, and `TryGet`
    distinguishes component absence from a present default value.
11. Generated marker metadata reports sparse versus archetypal representation
    and the group type without changing `EntComponent` layout or exposing
    storage addressing.
12. Accepted hot paths allocate nothing after setup and are checked in Release.

Cross-group joins, direct point refs, automatic deferred mutation, and a bound
row cursor are valuable possible extensions. They are not required to declare
the same-group span API complete and should be added only after their separate
contracts and performance are demonstrated.
