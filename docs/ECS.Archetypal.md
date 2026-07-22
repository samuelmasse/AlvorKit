# ECS Archetypal Components

> Status: direct typed columns, sparse canonical signatures, sparse transition
> edges, exact pooled capacity, lifecycle integration, generated declaration,
> debugger discovery, `ComponentToString` integration, final-shape allocation,
> and alloc-scoped span queries with explicit-SIMD support are implemented.
> Per-Ent query accessors and further query
> alternatives remain in the Release experiment matrix in the
> [feature-completion plan](ECS.Archetypal.Features.md).

The earlier [footprint reduction epic](ECS.Archetypal.FootprintReduction.md),
[theory](ECS.Archetypal.FootprintTheory.md), and
[baseline](ECS.Archetypal.FootprintBaseline.md) are historical design records.
They are not the active package backlog.

The current Release comparison is consolidated in the standalone
[archetypal benchmark report](ECS.Archetypal.BenchmarkReport.html).

## Scope

The direct public archetypal API is:

- `GetArchetypal<T, N, A>()`
- `HasArchetypal<T, N, A>()`
- `SetArchetypal<T, N, A>(in T value)`
- `UnsetArchetypal<T, N, A>()`
- `EntArena.AllocArchetypal<A>()`
- `EntArena.QueryArchetypal<A>().With<T, N>()`

The implementation supports an unbounded number of registered fields in a
group. An arch ID is not a fixed-width component mask.

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

## Pooled Row Capacity

[`EntArchRows<A>`](../src/AlvorKit.ECS/Archetypal/EntArchRows.cs) gives a newly
occupied `(allocId, archId)` row set an initial logical capacity of four. That
capacity applies to its `EntMut[]` and every component column in the arch.

The normal doubling sequence is:

`4 -> 8 -> 16 -> 32 -> ...`

The direct row and component arrays use `EntArchArrayPool<T>`, one exact typed
pool shared by every archetypal field name, group, arch, and alloc using that
same `T`. Physical array length equals logical capacity; different `T` values do
not share arrays.

After removal, capacity changes use hysteresis:

- Exactly 25% occupancy retains the current logical capacity.
- Occupancy below 25% halves logical capacity once for that removal.
- Zero rows returns the `EntMut[]` and every component array, clears the state
  slot, and resets logical capacity to zero.
- Reactivation rents a new capacity-four buffer set and begins again at row zero.

Each `(T, capacity)` bucket is a dense stack that starts empty and grows with
observed return demand. It does not reserve slots from the machine's processor
count. Its short lock protects only structural rent, return, growth, and trim
operations; push and pop are O(1), growth is amortized O(1), and existing
component reads and writes never enter it. Cached reference-containing arrays
are cleared before publication; reference-free arrays remain dirty. On a Gen2
change, the next pool operation scans the fixed bucket set. A bucket survives
that first observation; a later Gen2 observation drops its cached buffers and
stack metadata if no intervening rent or return used it. This avoids discarding
a warm capacity set during a structural burst while still releasing inactive
payload.

Four remains the compromise between sparse-arch memory and growth cost. A
one-row arch has exactly three spare slots per direct array. Row indexing and
steady-state component access remain unchanged. Pooled growth and shrink copy
only active rows and run on structural paths.

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

The packed signature remains the structural authority. Direct typed columns
remain the storage representation.

### Rejected `SetArchetypal` Structural Split

AFR-25A extracted the missing-column structural body into a private
`NoInlining` helper while retaining the existing-column lookup and store in the
hot caller. Forwarding `in T` made the JIT stack-home the scalar loop value for
the possible cold call. Passing the helper value by value removed that spill
and substantially reduced generated code, but the two reversed Release sweeps
were latency-neutral overall: their aggregate median deltas were -0.652% and
+0.067%.

The scalar exact/specialized rotating cases were repeatably slower even after
the spill was removed. Concrete class regressed 2.00%/3.30%, concrete struct
4.49%/2.12%, and generic struct 1.90%/2.10%. Because existing-field latency is
the acceptance gate, smaller code did not justify the change. Production API
and behavior stayed unchanged, and the temporary candidate and benchmark
scaffold were removed.

### Accepted `ValuesAt` Address Simplification

AFR-25B kept the existing closed-generic jagged directory while simplifying its
hot lookup. `ValuesAt` now snapshots the outer directory once, performs unsigned
bounds checks for `allocId` and `archId`, and relies on arch slot zero remaining
permanently null instead of branching explicitly on `archId == 0`.

This sequence removes three compare/branch pairs from exact and specialized
callers. In the generic-shared helper it also reduces static directory loads and
branches. It does not change storage ownership, publication, or the public API.
No `Volatile` operation was added: one thread still owns each alloc's
group-local columns, and different alloc owners may concurrently use the same
group and arch.

The complete optimized Release sweep measured a -5.60% median candidate delta:
-5.89% for `Get` and -4.28% for existing-field `Set`. There were no cell
regressions, managed allocations, or garbage collections. A shorter
reverse-order confirmation retained 6.6% through 9.5% improvements for the
exact scalar sentinels; after tiering settled, generic-class scalar `Get` and
`Set` improved by 2.76% and 2.17%. An unchanged-path A/A check was neutral.

The optimization is now production code, while its temporary comparison harness
has been removed. AFR-25C subsequently optimized the final row access without
changing the directory representation.

### Accepted `Unsafe.Add` Row Access

AFR-25C replaces only the final `values[loc.Row]` load or existing-field store
with `Unsafe.Add` over
`MemoryMarshal.GetArrayDataReference(values)`. It does so only after `ValuesAt`
has returned a non-null column. Structural creation and movement continue to use
ordinary indexed access; the public API and storage representation are
unchanged.

The unchecked access relies on an existing controlled invariant rather than
adding validation to the hot path. Append and move establish a capacity-backed
dst row, retained fields are copied, and the checked first write initializes a
new field before point access resumes. Row and column capacities grow together,
and swap-back compaction repairs the moved Ent's `loc` before subsequent access.
One thread owns a given alloc's group-local rows and columns, so another thread
cannot observe a transient row update in that alloc. Different alloc owners may
still use the same group and arch concurrently.

Consequently, AFR-25C adds no row bounds check, impossible-state branch, lock,
`Volatile` operation, or managed allocation. The address remains a managed
`ref T`; assignments through it preserve the GC write barrier for reference
types and structs that contain references. It is not converted to a native
pointer or untyped byte address.

The unchanged-path A/A control remained within ±0.91%. In the quick scalar
exact/specialized comparison, the candidate won all six forward cases by 1.83%
through 6.43%. The reversed comparison also won every case except one noisy
baseline outlier; stable wins were approximately 1.88% through 4.63% or more.
Generic-class `Get` improved by about 1.00% to 1.21%, while generic-class `Set`
was neutral at +0.31% and -0.16%. Broad value and reference shapes showed no
repeatable regression after candidate-first retests. Every measured case
remained at 0 B/op.

Release code generation confirms that the final terminal array bounds check is
gone. Representative generated-code sizes changed as follows:

| Caller | FullOpts baseline -> final public | Paired Tier1 baseline -> candidate |
| --- | ---: | ---: |
| Concrete `Get` | 343 -> 337 | 364 -> 358 |
| Concrete `Set` | 1,539 -> 1,525 | 6,318 -> 6,306 |
| Generic-class `Get` | 542 -> 533 | 617 -> 608 |
| Generic-class `Set` | 2,266 -> 2,242 | 5,834 -> 5,819 |

The Set Tier1 size is sensitive to its synthesized PGO inline profile, so the
Tier1 column uses the same-build paired capture rather than comparing two
different post-promotion profiles.

The production point path now combines AFR-25B's simplified directory lookup
with AFR-25C's unchecked final row access. Specialized/direct `EntArchLoc`
storage remains explicitly deferred; a later task must deliberately resume it.

### Cold Field Registration and the Selected Point Representation

AFR-25D separates the hot column directory from precise field registration.
`EntArchColumn<T, N, A>` owns only `Values`, `ValuesAt`, and a forwarding
`FieldId` property. The existing `EntArchColumnOps<T, N, A>` owns the precise
static constructor that registers the field.

This preserves `beforefieldinit` on the hot values holder. An absent `Get`,
`Has`, or `Unset`, and a `Set` on a dead Ent, do not register an otherwise
unused field. The first live structural `Set` requests `FieldId` and performs
the registration under the graph lock. The split adds no new production
generic type and no per-field object.

The gain is concentrated in shared generic callers using a class group marker.
The final scalar measurements improved generic-class `Get` by roughly 30% and
existing-field `Set` by roughly 17% to 20%, while exact and generic-struct paths
remained near their previous 1.7 to 1.8 ns range. All measured point cases
remained allocation-free.

AFR-25E tested whether wrapping `A` in an internal value-type key would force
exact generic specialization. It did not: class-group callers remained
canonical shared code and struct-group callers were already specialized. The
candidate was removed.

The row-set cutover now uses this production address sequence:

```text
EntArchLoc = (RowSetId, ArchId, Row)
closed generic field -> row-set slot -> typed T[] -> row
```

Existing-component access does not consult signatures, transition edges, state
maps, field ordinals, allocators, or free lists. `AllocId` is derived from the
Ent page only on structural paths. `ArchId` remains in the 12-byte loc so
add/remove does not require a row-set metadata lookup. The flat field directory
removes the previous dependent alloc-to-arch lookup and preserves contiguous
typed columns for span iteration.

## Row-Set Storage and Adaptive Discovery

One row-set ID is assigned for each observed `(alloc, arch)` pair. Each alloc
keeps one direct `archId -> rowSetId` array for structural destinations and one
compact list of active row-set IDs. Row-set metadata is stored in fixed pages
of 64 records, so another alloc owner can grow the group catalog without
invalidating refs into existing records. Empty row sets return all Ent and
component payload arrays but retain their ID for fast reactivation; alloc
cleanup recycles the IDs.

A closed query shape caches matching global arch IDs. Enumeration compares that
match count with the alloc's active-row-set count and scans the smaller side.
When the active side wins, a lazily created query bitset performs constant-time
arch membership tests. New arch publication extends the matching list and any
existing bits. Structural changes in the queried alloc/group remain forbidden
while an enumerator, chunk, row, or span is active.

## Sparse Transition Edge Arena

AFR-22 and AFR-23 removed the dense `arch capacity × field capacity`
transition matrix. Registering more fields now grows field metadata and the
singleton directory; it does not allocate transition cells for every arch.

The implemented structural cache uses:

- `edgeHeads`, with one four-byte head slot per arch-capacity slot. Zero means
  no cached transition, a positive value selects the compact list, and a
  negative value selects the high-degree shared index.
- One append-only `EntArchEdge[]`. Index zero is reserved, and every real edge
  is a 12-byte `(FieldId, DstArchId, NextEdgeIndex)` value.
- Two directed edge entries for each resolved add/remove relationship.
- No retained entry for an unobserved `(archId, fieldId)` relationship.

`GetTransitionArchId` follows the compact list through degree eight. Publishing
a ninth observed edge also migrates that arch into one group-shared
open-address index, making higher-degree lookup expected constant time without
allocating one table per arch. An unknown relationship is resolved under the
catalog lock by constructing the exact canonical dst signature, interning or
finding that arch, and appending the inverse pair.

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

## Reference-Tail Clearing

[`EntArchRows<A>.ClearTailFields`](../src/AlvorKit.ECS/Archetypal/EntArchRows.cs)
currently visits every field in the src arch. Each visit dispatches to the
field's `EntArchColumnOps.Clear` implementation, where
`RuntimeHelpers.IsReferenceOrContainsReferences<T>()` decides whether the tail
value must be cleared.

The runtime helper is a generic intrinsic for each closed column type. Fields
without references intentionally remain dirty beyond `Count`; fields containing
references clear the old tail so removed values do not remain GC roots.

## Historical Note: Direct Location Storage

[`EntArchLoc`](../src/AlvorKit.ECS/Archetypal/EntArchLoc.cs) is currently stored
through the ordinary sparse component path with `Get<EntArchLoc, A>()`,
`Set<EntArchLoc, A>()`, and `Unset<EntArchLoc, A>()`.

A specialized/direct `EntArchLocStorage<A>` could potentially:

- Return a location by reference for structural updates.
- Avoid repeated generic sparse lookups when a move reads and then writes the
  location.
- Derive `allocId` from the Ent's page rather than storing it in every location.
- Use a location-specific page layout.

This prototype is not part of the active feature plan. The current sparse loc is
integrated with `Clear`, disposal, and deferred finalizer cleanup.

Production integration would require direct involvement in page allocation,
generation validation, sparse reset behavior, and Ent lifecycle. Those wider
changes remain outside this focused archetypal package work.

The implemented location already uses `RowSetId` as its direct column handle
while retaining `ArchId` and `Row`. Specialized location page storage remains
outside the archetypal package scope.

## Verification

### Focused Behavior Tests

Focused archetypal coverage includes:

- Set-driven movement from a non-tail row repairing the compacted Ent's loc.
- Different field-add orders resolving to the same arch ID.
- Removing and re-adding a field reusing inverse transitions.
- A forced signature-hash collision resolving by exact signature comparison.
- Catalog growth preserving all existing hash-chain entries.
- Sparse edge-arena growth preserving every cached relationship.
- High-degree transition migration preserving every cached inverse relationship.
- No edge storage being created for unobserved `(arch, field)` pairs.
- A newly occupied row set starting at capacity four and growing correctly.
- Exact 25% occupancy retaining capacity, lower occupancy halving it, and an
  empty alloc/arch state returning all direct payload buffers.
- Adaptive query discovery scanning the smaller of matching archs and active row sets.
- Pooled reference-containing arrays being cleared before reuse.
- Swap-back compaction preserving values and repairing the moved Ent's `loc`.
- Reference tails being cleared while value-only tails remain intentionally
  dirty.
- Different alloc owners concurrently resolving and using the same group.

The collision test should exercise the index with two distinct signatures and
the same supplied hash rather than depending on an accidental collision from
the production hash function.

### Historical Measurements

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

Benchmark iteration is deliberately staged. Begin with quick scalar sentinels,
then run longer generic-class cases only when canonical-sharing behavior needs
to settle. Reserve the 5-million-operation, seven-sample full matrix for final
confirmation of a promising candidate. Use reverse-order or A/A checks when a
short result may instead reflect tiering or process drift.

## Feature Completion

Implemented package integration includes:

1. Exact signatures, hash indexing, sparse transition edges, and the high-degree transition index.
2. Flat row-set-indexed typed columns, paged row metadata, compaction, pooling, shrink, and empty-state release.
3. Type-erased alloc-local group lifecycle.
4. `Clear`, `EntPtr`, `EntObj`, and `EntArena` cleanup.
5. Generated property-level `[Archetypal]` access.
6. Debugger and `ComponentToString` component discovery.
7. Alloc-scoped arbitrary multi-component span queries with adaptive cached discovery.
8. Typed final-shape allocation without intermediate structural transitions.

The basic span representation and explicit-SIMD validation are now implemented.
Generated query names, per-Ent accessors, active-arch indexing, and the remaining
alternatives in [ECS.Archetypal.Features.md](ECS.Archetypal.Features.md) still
require focused Release comparison.
