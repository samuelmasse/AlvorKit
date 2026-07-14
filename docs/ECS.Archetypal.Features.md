# Archetypal Component Feature Completion

## Scope

This plan completes archetypal components as an ordinary AlvorKit ECS feature.
It starts from the direct typed-column implementation and does not resume shared
slabs, native component storage, specialized loc storage, disassembly work, or
`MethodImplOptions` tuning.

The first implementation batch stopped before queries and is complete. The
next batch added the basic alloc-scoped span representation, explicit-SIMD
benchmarks, and direct final-shape allocation. Per-Ent iteration and generated
query accessors remain separate prototypes because they have different
code-generation tradeoffs.

## Required Public Shape

Component interfaces may mix sparse and archetypal properties:

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

All archetypal properties declared by one component interface use its generated
component group as `A`. In this example, `Position` and `Velocity` use
`MotionComponents` as their archetype group. Unmarked properties retain the
existing sparse representation and generated API.

Generated entity access remains property-shaped. `Has`, get, set, unset, lazy
initialization, builder methods, access modifiers, nullability, and
`ComponentToString` must route to the selected storage representation without
requiring handwritten generic calls.

## Phase 0: Direct-Column Package Cleanup

Remove implementation machinery that exists only for the abandoned shared-slab
design. `EntArchFieldLayout`, `EntArchField`, and `EntArchStorageClass` are not
used to address the current direct columns and should not remain production
dependencies. Simplify field registration and the corresponding diagnostics and
tests while retaining semantic graph, transition, row, column, compaction, and
concurrency coverage.

The supported implementation remains:

- one graph per archetype group;
- direct typed columns per exact `(T, N, A)`;
- dense rows partitioned by alloc;
- swap-back compaction;
- exact power-of-two pooled row and component arrays;
- one owner thread per `(alloc, A)` and concurrent ownership only across
  different allocs.

The footprint documents become historical design records rather than the active
implementation backlog. The main archetypal documentation must describe the
supported package on its own.

## Phase 1: Type-Erased Integration

Add one type-erased group operation object per closed `A`. An alloc records only
the archetype groups it has used. Registration occurs when the alloc first
creates a row for that group.

Group operations must support:

- removing one Ent from the group;
- bulk-releasing every row and column owned by an alloc;
- exposing registered component operations to debugger and string views;
- processing finalizer-deferred cleanup without moving rows on the finalizer
  thread.

`EntArchColumnOps<T, N, A>` already provides one object per registered field. It
should also provide type-erased component metadata, presence testing, and boxed
value access. No additional object is needed per Ent, arch, or column buffer.

Sparse field lifecycle and archetypal group lifecycle remain separate even when
they share a small read-only component-view abstraction.

## Phase 2: Clear and Disposal

`Clear` removes archetypal groups before it enumerates ordinary page fields.
Removing an Ent from a group compacts its row, repairs the moved Ent's loc, and
clears the removed Ent's `EntArchLoc<A>`. The Ent remains alive and may receive
new sparse or archetypal components afterward.

`EntPtr.Dispose` claims the generation, removes the old generation from each
group used by its alloc, clears sparse reference fields, completes the generation
transition, and returns the index. Cleanup reads the stored loc for the original
generation directly because public `Get` correctly stops working after the
disposal claim.

`EntArena.Dispose` bulk-releases each used group's alloc-local rows and component
columns before sparse pages and the allocator ID are recycled. Bulk teardown
does not compact individual rows because every Ent in the alloc is dying.

`EntObj` finalization must not mutate allocator-zero archetype rows on the GC
finalizer thread. It queues the old Ent for alloc-owner cleanup. Structural
archetypal operations, `Clear`, and future queries drain that queue before using
the alloc's group rows. Existing-component point access remains unchanged and
does not inspect the queue.

Lifecycle acceptance cases include mixed storage, multiple groups, compaction,
reference-containing values, Ent index reuse, allocator ID reuse, dead handles,
and deferred `EntObj` cleanup.

## Phase 3: Generated Archetypal Declaration

Add `[Archetypal]` as a property-only attribute. The generator model records the
storage choice for every property and selects explicit archetypal templates for
`Has`, get, set, unset, lazy initialization, and builder methods. Unmarked
property output remains unchanged.

Archetypal reads are part of `IEnt`, and archetypal mutations are part of
`IEntMut`. Default interface implementations preserve existing custom handle
implementers, existing Ent wrappers forward those members to their internal
`EntMut`, and handwritten `EntMut` calls retain the direct public methods.

Generator verification covers sparse-only, archetypal-only, and mixed groups;
value and reference types; nullable values; lazy initialization;
`ComponentToString`; access modifiers; and `SkipBuilder`. A compilation fixture
must compile emitted source against the runtime package in addition to checking
source fragments.

## Phase 4: Debugger and String Integration

`EntDebugView` discovers both sparse fields and registered archetypal columns
through a common read-only component view. It filters by presence on the selected
Ent, sorts deterministically, and retains the existing primitive and object
debugger wrappers. Internal `EntArchLoc<A>` storage is never displayed as a user
component.

`EntHandle.ToStringCycleDetection` uses the same combined component discovery so
`[ComponentToString]` has identical behavior for sparse and archetypal
components, including nested Ent cycle detection.

Tests cover mixed storage, multiple groups, primitive and reference values,
unset, clear, deterministic disposal, arena disposal, and duplicate simple
component names from different groups.

## Phase 5: Primary Demo

Replace the manual `C00` through `C06` archetype stress program with a concise
generated-component walkthrough. It declares mixed sparse and archetypal
properties, creates several shapes in an `EntArena`, uses generated entity
accessors, shows debugger/string-visible values, clears one Ent, disposes one
Ent, and disposes the arena.

Exhaustive shape stress remains in tests or the benchmark demo rather than the
primary teaching demo.

## Pre-Query Stop Point (Completed)

Phases 0 through 5 passed focused Release verification before query work began.
That batch added no active-arch indexing, query public types, query selection
nodes, chunk wrappers, row cursors, or generated query accessors.

## Basic Span Query Batch

Queries are alloc-scoped because alloc ownership is the threading boundary. A
query must select any number of required archetypal components and yield only
active arches containing every selection. Fixed one-, two-, or three-component
overload families and generated component power sets are not acceptable.

The implemented underlying shape is a recursively typed selection chain:

```csharp
var query = arena
    .QueryArchetypal<MotionComponents>()
    .With<Position, MotionComponents.Position>()
    .With<Velocity, MotionComponents.Velocity>();
```

Repeated `With<T, N>()` adds selection types without runtime descriptor arrays,
`params`, boxing, or setup allocation. Generated `WithPosition()` names remain
future generator work.

Two iteration models must be prototyped over the same multi-component selection:

1. Chunk iteration now exposes aligned Ent and mutable component spans.
2. Per-Ent iteration remains a future prototype with Ent and ref-returning
   generated component accessors.

The per-Ent prototype is considered direct only if it binds each selected column
once when entering an arch and addresses a row from cached managed base refs. It
must not repeat `EntArchLoc`, jagged directory, graph, hash, or virtual lookups
for every component on every Ent.

## Query Experiment Matrix

All query experiments run in Release with short iterations. The initial dense
read sentinel now compares ordered and deterministically shuffled sparse point
Get, ordered and shuffled archetypal point Get, and the basic span query. The
remaining matrix compares:

- chunk spans with an indexed loop;
- a generic per-Ent row accessor;
- generated per-Ent ref accessors;
- one, two, four, and eight selected components;
- first-selected and last-selected access through a typed selection chain;
- read accumulation and component writes;
- small and large row counts;
- value and reference-containing components;
- allocations after setup.

The experiments must determine whether the JIT collapses typed binding-chain
selection to cached base-ref plus row addressing. If it does not, options include
keeping the row iterator as an explicit convenience API or generating declared,
fixed query projections for direct per-Ent access. Chunk spans remain available
regardless of the row-iterator result.

Structural add/remove in the same `(alloc, A)` is forbidden while query chunks,
rows, or spans are active. Existing values may be modified through returned
spans or refs. Different allocs may be queried on different threads.

## Explicit SIMD Query Result (Completed)

The basic query API needs no SIMD-specific storage mode. Each selected component
is already a separate contiguous span, so an unmanaged component span can be
reinterpreted as `Vector256<T>` values and followed by a scalar tail. No columns
are interlaced and scalar iteration continues to use the same representation.

The short Release comparison over 100,000 Ents produced these zero-allocation
means on the local Ryzen 9 9950X:

| Query body | AlvorKit scalar | AlvorKit explicit SIMD |
| --- | ---: | ---: |
| One component | 19.31 us | 3.37 us |
| Two components | 19.33 us | 5.45 us |
| Three components | 26.00 us | 8.99 us |
| Two components across multiple matching arches | 19.26 us | 4.46 us |

The vectorized rows are in the leading comparison tier: the corresponding best
measured competitor results were 3.69 us, 6.58 us, 8.50 us, and 5.51 us. These
numbers establish that chunk discovery and typed span binding do not erase the
benefit of explicit vectorization. They do not imply that arbitrary component
types or system bodies will vectorize automatically.

## Final-Shape Construction (Completed)

`EntArena.AllocArchetypal<A>()` now builds a compile-time typed chain of field
values and creates the Ent directly in its complete arch. The first use of a
closed chain resolves its sorted signature; subsequent uses read a cached arch
ID. Creation performs one row append and writes the values directly to their
typed columns, avoiding all intermediate arches and transition moves produced
by sequential component setters.

The external creation comparison now contains three explicit AlvorKit rows:

- sparse components, showing the hybrid ECS's low-cost creation option;
- archetypal sequential setters, representing genuine incremental shape
  changes;
- archetypal final-shape construction, representing known-at-creation shapes.

In the short Release run over 100,000 Ents, sparse creation took approximately
0.7-1.3 ms for one through three components. Final-shape archetypal creation took
approximately 1.2-2.4 ms. For two and three fields it was roughly 15-40 times
faster than sequential archetypal setters because it did not repeatedly occupy
and empty intermediate row sets. The benchmark's stateful one-iteration
creation method has visible run-to-run noise, so these ranges establish the
cost tier and the structural improvement rather than a precise ordering between
field counts.

## Remaining Query Work

The next query decision remains the one-Ent-at-a-time API. It should be
prototyped against the existing chunk spans and retained only if generated or
generic accessors can bind each column once per arch and reduce each row access
to cached-base addressing. Generated query names and broader component-count,
reference-component, and small-row experiments also remain open.

## Pre-Query Completion Gate (Satisfied)

Before query work begins, the package must have:

- no production dependency on abandoned shared-storage layout metadata;
- generated `[Archetypal]` entity access;
- correct `Clear`, `EntPtr`, `EntObj`, and `EntArena` lifecycle behavior;
- debugger and `ComponentToString` integration;
- a generated-component demo;
- focused runtime and generator tests passing in Release;
- documentation matching the implemented ownership and lifecycle contracts.
