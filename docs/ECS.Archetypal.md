# ECS Archetypal Implementation Direction

> Status: working implementation direction. This document records the next
> internal improvements under consideration; it is not a finalized design.

The broader footprint work is tracked by the
[Archetype Footprint Reduction epic](ECS.Archetypal.FootprintReduction.md) and
its [theory and cost model](ECS.Archetypal.FootprintTheory.md). This document
retains the detailed catalog decisions that feed the epic. Where an earlier
dense-layout suggestion conflicts with the epic, the sparse epic design wins.
The pre-change behavior and measurement surface are recorded in the
[AFR-01 and AFR-02 baseline](ECS.Archetypal.FootprintBaseline.md).

## Scope

The public archetypal API remains stable:

- `GetArchetypal<T, N, A>()`
- `HasArchetypal<T, N, A>()`
- `SetArchetypal<T, N, A>(in T value)`
- `UnsetArchetypal<T, N, A>()`

The implementation must continue to support an unbounded number of registered
fields in an arch group. Seven fields is the current demo size, not a system
limit. Consequently, an arch ID cannot be a fixed-width component bit mask.

This work is focused on the archetypal implementation itself. Ent lifecycle,
arena disposal, sparse-component integration, and other interaction with the
rest of the Ent system are intentionally out of scope.

Defining ownership and deterministic release for any archetypal native buffer
is in scope; redesigning general Ent lifecycle is not. No native candidate is
accepted without a concrete owner and matching `NativeMemory.Free` path.

## Optimization Priority

The optimization order is explicit:

1. Existing-component `GetArchetypal` and `SetArchetypal` latency is the
   primary objective.
2. Structural add/remove, movement, compaction, retained bytes, managed object
   count, and managed allocation events are the second objective.
3. Cold catalog construction and diagnostics are the third objective.

Constant-time complexity alone is not sufficient for the first objective. The
complete generated address sequence, call shape, loads, branches, bounds
checks, and inlining determine whether a design is acceptable. A footprint or
structural improvement cannot justify slowing existing-field reads or writes.

Progress through the agreed improvements:

1. Complete: store cumulative signature ends and preserve sorted signatures by
   insertion.
2. Complete: add collision-checked hash indexing for arch signatures.
3. Complete: reduce the initial row capacity from 16 to 4.
4. Complete: add four-byte packed immutable field layouts.
5. Complete: use each closed-generic column directly for ordinary membership
   and point access.
6. Complete: replace dense transition rows and their root/self-loops with a
   singleton directory and shared sparse edge arena.
7. Complete: establish AFR-24's formal Release point-path attribution and
   generated-code baseline.
8. Next: complete AFR-25's direct-address prototypes and AFR-26's decision
   gate.
9. Build AFR-30's sparse alloc-local state index only after the point-addressing
   representation passes that gate.

Specialized storage for `EntArchLoc` was considered but is deferred because it
would couple this work to the sparse page and generation systems.

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

## Previous Signature Lookup Cost

Before AFR-11,
[`EntArchGraph<A>.FindBySignature`](../src/AlvorKit.ECS/Archetypal/EntArchGraph.cs)
scanned every existing arch and compared its packed signature with the
requested signature.

If `M` unique arches are created, the scan considers this many prior arch
candidates before accounting for the field comparisons themselves:

\[
\frac{M(M - 1)}{2}
\]

Examples:

| Unique arches | Prior-signature candidates |
| ---: | ---: |
| 127 | 8,001 |
| 1,023 | 522,753 |
| 32,767 | 536,821,761 |
| 117,010 | 6,845,611,545 |

Cached transitions avoided this scan after an edge was resolved, but first
materialization became increasingly expensive as the historical arch count
grew. AFR-11 replaces that catalog scan with expected constant probing.

## Signature Hash Index

### Representation

The existing packed, sorted field IDs remain the canonical signatures. AFR-10
now stores one cumulative packed-field end per arch; the previous end is the
next signature's start. This metadata change does not alter signature identity.
The hash index narrows the set of signatures that must be compared; it does not
replace exact comparison.

The implementation uses:

- One power-of-two `int[]` containing only arch IDs.
- Linear probing with a maximum load of 75%.
- Zero as the empty-slot marker. Real arch IDs begin above zero.
- The existing packed signatures for exact collision resolution.

The catalog is append-only, so the table needs neither deletion markers nor
tombstones. Hashes are not retained in the table. Every occupied candidate in a
probe sequence is checked against the exact packed field IDs, including when
different signatures have the same 64-bit hash.

No array, list, or signature object is allocated per lookup. The table allocates
only when it is first created or doubled during cold catalog growth.

### Hash Requirements

The signature hash must:

- Include the signature length and every sorted field ID.
- Produce the same value for the same signature.
- Require no managed allocation.
- Be suitable for incremental catalog growth under the graph lock.
- Always be followed by exact signature comparison before an arch is reused.

It does not need to be cryptographic. Exact comparison is what preserves
correctness in the event of a hash collision.

### Lookup

Finding an arch becomes:

1. Hash the proposed sorted signature.
2. Fold the high and low hash halves and mask into the power-of-two table.
3. Return `NoArchId` when the slot is empty.
4. Compare an occupied candidate's packed signature with the requested field
   IDs.
5. Return the candidate on an exact match; otherwise advance to the next slot.

In the expected case, only one packed signature is compared. Creating `M`
unique arches changes from a quadratic catalog scan toward linear catalog work,
with signature hashing and copying remaining proportional to the number of
fields in the new arch.

### Insertion

`CreateFromSignature` adds each real arch to the hash index after its packed
signature and boundary metadata have been stored. Growth scans the previous
table and recomputes hashes from those immutable packed signatures, so no hash
field is retained per arch.

The outside-group state is not a real signature and is not inserted into the
index.

All lookup and insertion occur while holding `EntArchGraph<A>.Sync`, preserving
the current graph mutation model.

## Preserve Sorted Signatures by Insertion

`ResolveAdd` now uses the fact that the src signature is sorted and that the
new field is absent:

1. Find the new field ID's insertion position.
2. Copy the src prefix before that position.
3. Write the new field ID.
4. Copy the remaining src suffix.

This is linear in the signature length, avoids a general-purpose sort, and
makes the canonical-signature invariant visible in the code.

The insertion position currently uses a linear search. This can change later if
measured signature sizes justify a different strategy.

`ResolveRemove` already preserves ordering by copying the ranges on either side
of the removed field.

## Initial Row Capacity Four

[`EntArchRows<A>`](../src/AlvorKit.ECS/Archetypal/EntArchRows.cs) gives a newly
occupied `(allocId, archId)` row set an initial capacity of four.

That capacity is applied to:

- The row set's `EntMut[]`.
- Every component column in the arch.

The normal doubling sequence is:

`4 -> 8 -> 16 -> 32 -> ...`

Four is the measured compromise between sparse-arch memory and growth cost:

- A one-row arch wastes at most three initial slots per array rather than
  fifteen.
- A commonly populated arch does not pay the repeated growth sequence starting
  at one.
- Row indexing and steady-state component access remain unchanged.

AFR-12 measured this independently from hash indexing. Across the 47-case quick
profile, retained row/component capacity fell in every case without changing
catalog data, used payload, or managed object counts. Point access remained
unchanged. States that grow beyond four rows pay the additional `4 -> 8` and
`8 -> 16` intermediate resize allocations.

## Packed Immutable Field Layouts

AFR-20 adds `packedFieldLayouts` beside the exact packed field IDs. Both arrays
use the same cumulative signature range, so a local field ordinal addresses the
field ID and its storage layout without an object or dense `(arch, field)` cell.

Each `EntArchFieldLayout` is one four-byte encoded integer:

- A nonnegative value is a reference-free byte-column prefix beginning after
  `EntMut`.
- A negative value is the bitwise complement of a reference-containing
  type-local column.
- The sign also says whether released storage requires reference clearing.

Reference-containing fields with the same exact `T` share a storage class even
when `N` differs. A different reference-containing `T` starts its own typed
column sequence. The graph records byte width and storage class once per
registered field, not once per materialized membership.

The layout array starts empty and grows independently from the older 4,096-slot
field-ID array. Layouts and field IDs are written before the signature index,
singleton directory, or sparse edge arena makes a new arch reachable. Ordinary
point access does not consume this metadata. Structural removal scans the exact
packed src signature for a local ordinal only when its sparse edge is unknown;
the shared stores will consume the corresponding layout in AFR-32 through
AFR-35.

The ops directory remains a separate dense reference array. Current structural
copy, clear, and resize therefore preserve their prior lookup stride until the
shared-block cutover removes reference-free handler dispatch.

## Direct Closed-Generic Column Membership

[`EntArchColumn<T, N, A>.ValuesAt`](../src/AlvorKit.ECS/Archetypal/EntArchColumn.cs)
uses the closed generic field identity to inspect the existing
`allocId -> archId -> T[]` directory. A non-null column means that the arch has
the field; a missing column means that it does not. The operation does not
search the packed signature or consult group-global transition metadata.

The public membership paths use that result directly:

- `GetArchetypal` returns the row value or the default value when no column
  exists.
- `HasArchetypal` tests whether the column exists.
- `SetArchetypal` writes the existing row and returns immediately when the
  column exists. Only a missing column enters structural resolution.
- `UnsetArchetypal` also rejects a missing column directly. A non-singleton
  removal consults the sparse edge and scans the packed signature for the
  removed field's ordinal only when that edge has not been resolved before.

An immutable ordinal hash was prototyped because its isolated lookup kernel was
promising. The end-to-end point benchmarks rejected it: routing the public path
through global arch metadata cost more than it saved and added retained
membership-index storage. Direct closed-generic lookup was both smaller and
faster, so no ordinal hash is retained.

The AFR-21 isolated suite is generic in `A`. Across `K = 1, 4, 8, 16, 32`, its
median point costs were effectively width-independent:

| Operation | Median range | Allocation |
| --- | ---: | ---: |
| Present `Get` | 8.716–8.826 ns | 0 B/op |
| Present `Has` | 8.461–8.500 ns | 0 B/op |
| Existing-field `Set` | 9.682–9.729 ns | 0 B/op |

Those values characterize a generic-shared call shape; they are not a
universal concrete-call baseline. AFR-24 formalized the distinction on .NET
10.0.9 in Release: each case ran 5 million timed operations after 10 warmups,
with seven samples and two complete sweeps whose case order was reversed. The
rotating fixture used 1,024 Ents across four allocs/pages, 16 active arches,
64 alloc/arch states, and 16 rows per state. Every case allocated 0 B/op.

Steady rotating medians across the four value shapes and both sweeps were:

| Call shape for `A` | Present `Get` | Existing `Set` |
| --- | ---: | ---: |
| Concrete sealed class | 1.934–2.156 ns | 1.913–3.664 ns |
| Concrete readonly struct | 1.940–2.142 ns | 1.921–3.691 ns |
| Method generic in `A`, sealed class | 6.026–6.212 ns | 6.275–8.122 ns |
| Method generic in `A`, readonly struct | 1.936–2.142 ns | 1.917–3.725 ns |

The cost belongs to call-site generic sharing for reference-type `A`, not to
class markers in general. Concrete class calls and specialized generic-struct
calls essentially coincide, so AFR-24 does not recommend changing the group
marker API to structs. An earlier three-warmup run showed generic-class `Get`
startup modes near 6 ns and 13 ns; 10 warmups removed that tiering bimodality,
so the earlier startup result is not part of the baseline.

The complete methodology and generated-code interpretation are recorded in
[AFR-24's footprint-reduction section](ECS.Archetypal.FootprintReduction.md#afr-24--hot-path-attribution-and-codegen-baseline).
Raw reports are in
`out/ecs-archetypal/afr24-hot-path-steady-a.json` and
`out/ecs-archetypal/afr24-hot-path-steady-b.json`; representative Release
disassembly is under `out/ecs-archetypal/afr24-codegen/`.

The packed signature remains the structural authority and the parallel layouts
remain available for the shared-block cutover. They are no longer on the
ordinary point path.

## Sparse Transition Edge Arena

AFR-22 and AFR-23 removed the dense `arch capacity × field capacity`
transition matrix. Registering more fields now grows field metadata and the
singleton directory; it does not allocate transition cells for every arch.

The implemented structural cache uses:

- `edgeHeads`, with one four-byte head slot per arch-capacity slot. Zero means
  that the arch has no cached transition.
- One append-only `EntArchEdge[]`. Index zero is reserved, and every real edge
  is a 12-byte `(FieldId, DstArchId, NextEdgeIndex)` value.
- Two directed edge entries for each resolved add/remove relationship.
- No retained entry for an unobserved `(archId, fieldId)` relationship.

`GetTransitionArchId` starts at the src arch's head and follows its compact
linked list, so cached lookup depends on observed degree `D`, not registered
field count `N`. An unknown relationship is resolved under the catalog lock by
constructing the exact canonical dst signature, interning or finding that arch,
and appending the inverse pair.

Both edge records are fully initialized before `Volatile.Write` publishes their
new heads. A structural reader uses `Volatile.Read` on the head before walking
the immutable published records. Arena growth remains cold and serialized by
the graph lock. These publication operations are not used by point access.

Singleton entry uses a separate `singletonArchIds` directory indexed by field
ID. It is populated lazily when that singleton signature is first materialized.
The directory's `Volatile` read/write pair publishes the completed singleton
without creating an edge from the outside-group state. Singleton exit is
recognized from signature length.

There is no transition-only root arch and no membership self-loop. Real arch
IDs begin at `FirstArchId = 1`; `NoArchId = 0` continues to mean that the Ent is
outside the group. The removed dense matrix reports zero transition-cell
capacity, while retained transition storage now scales with arch capacity and
observed edges.

### Reusable Structural Signature Scratch

Unknown add and remove resolution must construct a canonical dst signature.
That width is not bounded, so the implementation does not use a `stackalloc`
whose size grows with the field count. Instead, the graph owns one reusable
`signatureScratch` array:

- Structural resolution already holds the group lock, so one shared buffer is
  sufficient.
- The array grows geometrically to the next power of two only when a wider
  signature is first encountered.
- Subsequent resolutions reuse its leading span without managed allocation.
- `SignatureScratchCapacity` exposes the retained capacity to footprint
  diagnostics.

## Managed Object and Allocation Reduction

Footprint means more than payload bytes. The storage work also aims to replace
many independently allocated arrays and handler objects with a small number of
large shared buffers or pages:

- Reference-free rows, fields, state metadata, indexes, handles, and free-list
  links may use uninitialized `NativeMemory.Alloc` backing when it passes the
  point-path gate and has deterministic ownership.
- Values that are or contain references remain in typed managed arrays, but
  those arrays are shared pages per alloc/group/storage class rather than one
  array per active arch-field membership.
- Free lists and block metadata use indexes or offsets inside shared buffers;
  they do not allocate a managed node object per block.
- Page size balances object count, growth copies, working set, and internal
  fragmentation. One huge buffer is not automatically better than a small
  measured number of large pages.

Reports keep managed retained bytes, native retained bytes, their sum, managed
object count, managed and native allocation-call counts, page count, slack, and
fragmentation separate. Moving bytes outside the GC is not reported as
eliminating memory.

The detailed cost model and remaining tasks are defined in the
[footprint theory](ECS.Archetypal.FootprintTheory.md) and
[epic](ECS.Archetypal.FootprintReduction.md).

## Optional Reference-Field Precomputation

### Current Behavior

[`EntArchRows<A>.ClearTailFields`](../src/AlvorKit.ECS/Archetypal/EntArchRows.cs)
currently visits every field in the src arch. Each visit dispatches to the
field's `EntArchColumnOps.Clear` implementation, where
`RuntimeHelpers.IsReferenceOrContainsReferences<T>()` decides whether the tail
value must be cleared.

The runtime helper is a generic intrinsic and is expected to be constant-folded
for each closed column type. Precomputation is therefore not intended to remove
an expensive runtime type inspection. Its purpose is to avoid iteration and
virtual dispatch for value-only columns that intentionally do no clearing.

### Proposed Metadata

At field registration, column operations should expose whether their value type
is or contains references.

AFR-20 now records reference classification in every packed layout. Tail
clearing can scan those entries without recomputing the classification. A
second packed sequence containing only reference-containing fields remains an
optional optimization if that scan is still measurable.

Tail clearing would then iterate the reference-only signature:

- Copying rows still iterates all common fields.
- Tail clearing visits only fields that require clearing for GC correctness.
- Fields without references remain intentionally dirty beyond `Count`.

For an arch with six `int` fields and one `string` field, tail clearing would
perform one column dispatch instead of seven.

### Tradeoff

The optimization adds:

- One packed `int[]` for reference field IDs.
- One cumulative reference-field end per arch, unless immutable layout metadata
  makes the second packed sequence unnecessary.
- Extra cold work when an arch is created.

It provides little benefit when most fields contain references. It should
therefore follow hash indexing, sorted insertion, the sparse transition cutover,
and the row-capacity change. The composite storage's immutable field layouts
may provide the same classification without a second packed signature. Retain
separate reference-only metadata only if structural benchmarks show a useful
improvement.

## Deferred: Specialized Location Storage

[`EntArchLoc`](../src/AlvorKit.ECS/Archetypal/EntArchLoc.cs) is currently stored
through the ordinary sparse component path with `Get<EntArchLoc, A>()`,
`Set<EntArchLoc, A>()`, and `Unset<EntArchLoc, A>()`.

A specialized `EntArchLocStorage<A>` could potentially:

- Return a location by reference for structural updates.
- Avoid repeated generic sparse lookups when a move reads and then writes the
  location.
- Derive `allocId` from the Ent's page rather than storing it in every location.
- Use a location-specific page layout.

Those changes require direct involvement in page allocation, generation
validation, sparse reset behavior, and Ent lifecycle. They also make the
archetypal package less independent from the rest of the Ent implementation.

Location specialization is not needed for signature hashing, sorted insertion,
row-capacity reduction, sparse transitions, or shared block allocation. It is
therefore deferred from this focused archetypal work.

The epic may change the meaning of the existing middle `EntArchLoc` integer from
`ArchId` to alloc-local `StateId`. That preserves the current sparse storage and
three-int footprint; it is not the specialized page-storage design deferred
here.

## Verification

### Focused Behavior Tests

Focused archetypal coverage includes:

- Different field-add orders resolving to the same arch ID.
- Removing and re-adding a field reusing inverse transitions.
- A forced signature-hash collision resolving by exact signature comparison.
- Catalog growth preserving all existing hash-chain entries.
- Sparse edge-arena growth preserving every cached relationship.
- No edge storage being created for unobserved `(arch, field)` pairs.
- A newly occupied row set starting at capacity four and growing correctly.
- Swap-back compaction preserving values and repairing the moved Ent's `loc`.
- Reference tails being cleared while value-only tails remain intentionally
  dirty.
- Different alloc owners concurrently resolving and using the same group.

The collision test should exercise the index with two distinct signatures and
the same supplied hash rather than depending on an accidental collision from
the production hash function.

### Measurements

Measure changes separately for:

- Existing-component `Get` and `Set` as the primary full-caller cases, with
  concrete and generic call sites, class and struct group markers, and rotating
  Ents/pages/arches/allocs.
- Cold creation of many unique signatures.
- Warm structural transitions whose edges are already cached.
- Moving the first, middle, and last row of an arch.
- Many low-occupancy arches versus a few high-occupancy arches.
- Groups dominated by value-only fields versus reference-containing fields.
- Sparse edge degree and lookup cost.
- Alloc-local state-index load and block-slab fragmentation.

Track elapsed time, allocated bytes, managed allocation events, retained
managed objects, managed and native retained bytes, page count, slack, and
fragmentation. The existing stress demo is a useful integration check, but its
boxed expected values, strings, and delegate dispatch make it unsuitable as
the only performance measurement.
All accepted benchmarks and disassembly must use an optimized Release build.
Debug output is never accepted or cited as performance evidence.

## Implementation Order

1. Complete: add direct archetypal tests and baseline measurements.
2. Complete: replace signature ranges with cumulative ends and replace
   add-then-sort with sorted insertion.
3. Complete: add the collision-correct signature hash index.
4. Complete: change the initial row capacity from 16 to 4.
5. Complete: add immutable four-byte packed field layouts.
6. Complete: use direct closed-generic columns for ordinary membership and
   reject the measured ordinal-hash alternative.
7. Complete: replace the dense transition matrix and transition-only root with
   the singleton directory and shared sparse edge arena.
8. Complete: establish AFR-24's formal concrete and generic point-path baseline
   and generated-code attribution.
9. Next: prototype faster direct address paths in AFR-25 and select one only
   through AFR-26's same-build performance gate.
10. Build AFR-30's sparse alloc-local state model off-path after the locator
    decision; do not assume that `StateId` belongs in `loc` before it wins.
11. Continue through shared block stores using
   the dependency order in the
   [Archetype Footprint Reduction epic](ECS.Archetypal.FootprintReduction.md).

Each step preserves the public API and can be reviewed and measured separately.
