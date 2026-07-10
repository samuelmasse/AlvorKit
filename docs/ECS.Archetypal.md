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

Progress through the agreed improvements:

1. Complete: store cumulative signature ends and preserve sorted signatures by
   insertion.
2. Complete: add collision-checked hash indexing for arch signatures.
3. Complete: reduce the initial row capacity from 16 to 4.
4. Complete: add four-byte packed immutable field layouts.
5. Next: move membership lookup to the packed signature.
6. Replace dense transition rows with a shared sparse edge arena.
7. Consider precomputed reference-field lists for tail clearing.

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

The proposed hash index is consulted only while resolving an unknown structural
transition under the existing graph lock. It adds no synchronization to
`GetArchetypal`, `HasArchetypal`, overwriting an existing field, or alloc-local
row operations. No `Volatile` access is added to those paths.

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

The transition-only root and the outside-group state are not real signatures
and should not be inserted into the index.

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
field-ID array. Layouts and field IDs are written before the signature index or
transitions publish a new arch. Ordinary point access does not consume this
metadata yet, so `Get`, `Has`, and existing-field `Set` remain unchanged and
allocation-free. AFR-21 will use the local ordinal returned by sparse
membership lookup; the shared stores will consume the layout in AFR-32 through
AFR-35.

The ops directory remains a separate dense reference array. Current structural
copy, clear, and resize therefore preserve their prior lookup stride until the
shared-block cutover removes reference-free handler dispatch.

## Replace Dense Transitions With a Sparse Edge Arena

[`EntArchGraph<A>.EnsureCapacity`](../src/AlvorKit.ECS/Archetypal/EntArchGraph.cs)
currently allocates transition storage for the rectangular product of arch and
field capacity. Allocating only assigned transition rows would remove spare-row
objects, but every materialized arch would still pay for every registered field.

The power set is expected to be sparsely explored. Transition storage should
therefore depend on observed relationships rather than `M × N`.

The target representation is:

- One edge-head index per materialized arch.
- Shared append-only arrays for `fieldId`, `dstArchId`, and `nextEdge`.
- Two directed entries for each resolved add/remove relationship.
- No edge entry for an unobserved `(arch, field)` pair.

Cached transition lookup depends on the observed degree of the src arch. An
unknown edge resolves through the exact signature hash index under the existing
catalog lock, then appends both directions to the shared arena.

Field presence moves to the exact packed signature and its optional immutable
micro-index. Once that cutover is complete, transition self-loops and the dense
transition-only root encoding can be removed.

The detailed cost model and implementation tasks are defined in the
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

Add direct archetypal tests for:

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

- Cold creation of many unique signatures.
- Warm structural transitions whose edges are already cached.
- Overwriting an existing archetypal field.
- Moving the first, middle, and last row of an arch.
- Many low-occupancy arches versus a few high-occupancy arches.
- Groups dominated by value-only fields versus reference-containing fields.
- Sparse edge degree and lookup cost.
- Alloc-local state-index load and block-slab fragmentation.

Track both elapsed time and retained/allocated bytes. The existing stress demo
is a useful integration check, but its boxed expected values, strings, and
delegate dispatch make it unsuitable as the only performance measurement.

## Implementation Order

1. Complete: add direct archetypal tests and baseline measurements.
2. Complete: replace signature ranges with cumulative ends and replace
   add-then-sort with sorted insertion.
3. Complete: add the collision-correct signature hash index.
4. Complete: change the initial row capacity from 16 to 4.
5. Complete: add immutable four-byte packed field layouts.
6. Next: move field membership to the packed signature.
7. Replace the dense transition matrix with the shared sparse edge arena.
8. Continue through alloc-local sparse states and shared block stores using
   the dependency order in the
   [Archetype Footprint Reduction epic](ECS.Archetypal.FootprintReduction.md).

Each step preserves the public API and can be reviewed and measured separately.
