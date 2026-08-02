# ECS Archetypal Components

This is the detailed storage and query contract for archetypal components.
For the game-facing declaration, allocation, query, row, ownership, and
lifetime rules, start with [`ECS.md`](ECS.md).

The implemented surface includes direct typed columns, sparse canonical
signatures and transitions, pooled alloc-local rows, lifecycle integration,
final-shape allocation, span queries, and exact generated per-Ent row
enumerators.

## Scope

The direct public archetypal API is:

- `GetArchetypal<T, N, A>()`
- `HasArchetypal<T, N, A>()`
- `SetArchetypal<T, N, A>(in T value)`
- `UnsetArchetypal<T, N, A>()`
- `EntArena.AllocArchetypal<A>()`
- `EntArena.QueryArchetypal<A>().With<T, N>()`

The implementation supports an unbounded number of registered fields in a
group. An arch ID is not a fixed-width component mask. Generated groups also
provide named query selectors and exact `Rows()` enumerators for query shapes
used by consuming compilations.

Generated component interfaces may mark individual properties with
`[Archetypal]`. Marked properties use the generated component-group type as `A`;
unmarked properties retain ordinary sparse storage. `Clear`, `EntPtr.Dispose`,
`EntObj` finalization, and `EntArena.Dispose` all participate in archetypal
lifecycle.

## Generated Declaration

`[Archetypal]` applies to one property in a `[Components]` interface. The source
generator emits the same property, `Has`, `Unset`, lazy-initialization, and
builder surfaces as an ordinary component, but routes their storage operations
through the generated component-group type:

```csharp
[Components]
public interface IMotionComponents
{
    string Name { get; set; }

    [Archetypal]
    Position Position { get; set; }
}
```

`Name` remains sparse. `Position` uses `MotionComponents` as `A`. Archetypal
reads are part of `IEnt`; archetypal mutation is part of `IEntMut`, so every Ent
wrapper supports the generated accessors consistently.

## Lifecycle Integration

Each alloc records the archetype groups it has used. `Clear` asks those groups
to remove the Ent before clearing ordinary sparse fields. Removal compacts the
dense row, repairs the moved Ent's loc, and clears the removed loc.

`EntPtr.Dispose` removes the claimed generation before returning its index.
`EntArena.Dispose` bulk-releases all alloc-local row and component arrays before
recycling pages and the allocator ID. `EntObj` finalization queues archetypal
cleanup because the finalizer thread is not the alloc owner; the owner drains
that cleanup before later structural operations.

The debugger and `EntHandle.ToString` enumerate a shared component-view registry
containing ordinary fields and registered archetypal column operations. Internal
`EntArchLoc<A>` fields are lifecycle metadata and are never user-visible.

## Threading Contract

The existing ownership model remains unchanged:

- The signature catalog, field registration, signature hash index, and sparse
  edge arena are shared by one arch group `A`.
- Creation of an arch and mutation of shared catalog structures are serialized
  by the group lock.
- Rows and component columns are partitioned by `allocId`.
- One owning thread reads and writes the archetypal data for a given alloc and
  group.
- Different owning threads may concurrently use the same group and arch when
  they operate through different allocs.

The signature hash index is consulted only while resolving an unknown
structural transition under the existing graph lock. `Volatile` reads and
writes are confined to structural publication through the singleton directory
and sparse edge heads. They make a completely initialized singleton or edge
chain visible after its catalog data has been written.

`GetArchetypal`, `HasArchetypal`, and overwriting an existing field go directly
through alloc-local closed-generic column storage. They contain no graph lock,
signature or edge search, managed allocation, or `Volatile` operation.

## Final-Shape Allocation

Sequential setters are still the correct API when an existing Ent changes
shape. They are unnecessarily expensive when constructing a new Ent whose full
shape is already known: each setter otherwise enters an intermediate arch,
moves the preceding values, and empties the preceding row set.

`AllocArchetypal<A>()` collects the intended fields and values in a typed value
chain, resolves the complete canonical signature, and appends the Ent directly
to that final arch:

```csharp
EntPtr ent = arena
    .AllocArchetypal<MotionComponents>()
    .With<Position, MotionComponents.Position>(position)
    .With<Velocity, MotionComponents.Velocity>(velocity)
    .Create();
```

The chain is composed only of nested value types. It creates no descriptor
array, boxes no value, and does not materialize an intermediate Ent or arch.
Each closed builder shape caches its resolved arch ID in static generic state.
Its first use registers the selected columns, sorts their field IDs into the
canonical signature, and resolves that signature under the graph lock. Later
`Create()` calls load the cached arch ID, allocate the Ent, append one row, set
one loc, and write each supplied value directly to its typed column.

Different `With` orders produce different closed builder types but resolve to
the same canonical arch. A field may appear only once in a builder chain; as
with the rest of the low-level archetypal API, satisfying that internal
contract is the caller's responsibility.

## Span Queries

Span queries are rooted in one `EntArena`, which is the alloc ownership
boundary. Repeated `With<T, N>()` calls build an unbounded compile-time
selection chain without descriptor arrays, `params`, boxing, or setup
allocation:

```csharp
var query = arena
    .QueryArchetypal<MotionComponents>()
    .With<Position, MotionComponents.Position>()
    .With<Velocity, MotionComponents.Velocity>();

foreach (var chunk in query)
{
    ReadOnlySpan<EntMut> ents = chunk.Ents;
    Span<Position> positions = chunk.Get<Position, MotionComponents.Position>();
    Span<Velocity> velocities = chunk.Get<Velocity, MotionComponents.Velocity>();

    for (int i = 0; i < ents.Length; i++)
        positions[i] += velocities[i];
}
```

Enumeration scans the selected alloc's arch directory. It rejects empty states
before testing the component selection and yields only nonempty arches that
contain every required field. Each chunk holds the already-resolved Ent array,
alloc ID, arch ID, and row count. `Get<T, N>()` resolves a typed column once for
the chunk and returns only its active rows. Asking for an optional component
that is not present in that arch returns an empty span.

The inner indexed loop is direct span indexing. It performs no Ent loc lookup,
graph lookup, hash lookup, virtual dispatch, or managed allocation per row.
Returned component spans are mutable and aligned with `Ents` by row.

Columns remain separate and contiguous; they are not interlaced. Callers may
therefore cast the active portion of an unmanaged component span to hardware
vectors and process the scalar tail. For example, an `int` column can use
`MemoryMarshal.Cast<int, Vector256<int>>(values)` without any ECS-specific SIMD
API or storage copy. The ECS does not automatically vectorize query bodies: the
ordinary indexed loop remains the scalar option, while explicit SIMD is an
opt-in implementation of the system operating on the same chunk spans.

Structural add or remove in the same `(alloc, A)` is forbidden while a query
enumerator, chunk, or returned span is active. Existing component values may be
modified through the spans. Different alloc owners may query the same `A` and
arch concurrently.
