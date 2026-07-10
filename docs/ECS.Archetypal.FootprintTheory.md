# Archetype Footprint Reduction: Theory and Cost Model

> Status: working theory for the
> [Archetype Footprint Reduction epic](ECS.Archetypal.FootprintReduction.md).
> The model defines the intended asymptotic behavior, practical byte targets,
> object-count targets, and hot-path constraints. Measurements taken during the
> epic may refine constants and thresholds without changing the sparse design.

The current dense implementation is recorded in the
[AFR-01 and AFR-02 baseline](ECS.Archetypal.FootprintBaseline.md).

## Objective

The archetypal implementation should allocate storage only for arch signatures,
transitions, alloc-local states, and component blocks that have actually been
observed.

The possible non-empty power set for `N` registered fields is:

\[
P = 2^N - 1
\]

Real workloads are expected to materialize a sparse subset of `P`. The
implementation must therefore avoid any representation whose retained memory
is proportional to all possible signatures or to the rectangular product of
materialized arches and registered fields.

The target is:

\[
O(M + S + E + R + Q + \text{payload})
\]

not:

\[
O(MN)
\]

The public archetypal API remains unchanged, and the number of fields in a group
is not bounded by a fixed-width mask.

## Terms

| Symbol | Meaning |
| --- | --- |
| `N` | Number of registered fields in the arch group |
| `P` | Number of possible non-empty signatures, `2^N - 1` |
| `M` | Number of materialized global arch signatures |
| `Kᵢ` | Number of fields in materialized arch `i` |
| `S` | Total materialized field memberships, `ΣKᵢ` |
| `E` | Number of cached directed structural transitions |
| `D` | Average cached directed transitions per arch, `E / M` |
| `R` | Number of active alloc-local arch states |
| `Q` | Number of active alloc-local storage-class handles |
| `C` | Row capacity of one active alloc-local arch state |
| `Sⱼ` | Stored size of component field `j` |
| `L` | Bytes of immutable layout metadata per field membership |
| `I` | Retained bytes in optional immutable wide-signature micro-indexes |

`M` can still grow very large because archetype variety is inherently
combinatorial. The design goal is not to conceal that cost. It is to ensure
that each materialized signature pays only for its actual fields and observed
relationships.

## Epic-Start Rectangular Costs

This section describes the representation measured by AFR-02 before runtime
changes began. AFR-10 has since replaced the eight-byte signature range with a
four-byte cumulative end; the dense graph and alloc-local costs remain.

### Group-Global Transition Matrix

At epic start, the graph stored one `EntArchTransition` containing two `int` values for
every `(arch-capacity slot, field-capacity slot)` pair. Its raw cell payload is:

\[
8C_aC_f
\]

where `C_a` and `C_f` are the geometrically grown arch and field capacities.

The jagged layout also retains approximately one managed array object per arch
capacity slot. On a 64-bit runtime, an array is approximately 24 bytes before
its aligned element payload.

Ignoring outer-array headers and spare capacity, the logical baseline cost for
one real arch with `K` fields is approximately:

\[
40 + 8N + 4K
\]

The terms are:

- Approximately 24 bytes for the per-arch transition-array object.
- `8N` bytes for the two-int transition cells.
- 8 bytes for the transition-array reference.
- 8 bytes for `EntArchSignatureRange`.
- `4K` bytes for packed field IDs.

Power-of-two capacity slack can make the retained cost substantially higher.

Transposing the matrix to `[fieldId][archId]` would remove per-arch array
objects, but it would retain the `O(MN)` cell cost. Transposition alone is not a
sufficient footprint design for sparse power-set exploration.

### Alloc-Local Directories and Buffers

The baseline alloc-local directories retain approximately this much metadata for
every arch-capacity slot in an alloc:

\[
16 + 8N
\]

- Approximately 16 bytes for `EntArchRowSet`.
- One 8-byte `T[]` reference in each registered field's arch directory.

When an arch becomes active, it additionally creates one `EntMut[]` and one
`T[]` for each of its `K` fields. Its approximate active cost is:

\[
16 + 8N + (24 + 8C) +
\sum_{j=1}^{K}\left(24 + \operatorname{align8}(CS_j)\right)
\]

The active arch creates `K + 1` managed array objects. Reducing initial capacity
reduces payload slack but does not reduce this object count.

For seven `int` fields:

| Initial capacity | Approximate alloc-local bytes | Array objects |
| ---: | ---: | ---: |
| 16 | 840 | 8 |
| 4 | 408 | 8 |

## Sparse Global Catalog

### Packed Exact Signatures

Materialized signatures remain sorted field-ID sequences stored in a shared
packed `int[]`. Their payload is:

\[
4S
\]

Because signatures are appended in arch-ID order, one cumulative end offset per
arch is sufficient:

\[
4M
\]

For arch `i`, the start is the previous arch's end and the count is the
difference between the current and previous ends.

AFR-10 replaced the baseline 8-byte `(Start, Count)` range with this 4-byte
value.

### Collision-Correct Signature Index

The signature index must always confirm a candidate by comparing the exact
packed field IDs. A hash narrows candidates but is not identity.

AFR-11 uses a shared open-address table containing only arch IDs. At a load
factor `α`, its payload is:

\[
\frac{4M}{\alpha}
\]

At exactly `α = 0.75`, this is approximately:

\[
5.33M
\]

The table has a minimum capacity of 16 and doubles before an insertion would
exceed 75% occupancy. Retained payload therefore varies from approximately
`5.33M` immediately before growth to `10.67M` immediately after growth. Cost
models and reports must use the actual table capacity rather than treating
5.33 bytes per arch as a fixed retained cost.

The requested signature supplies the probe hash. Each occupied candidate is
confirmed against its packed signature. Rehashing can recompute hashes from the
canonical stored signatures, so a hash does not have to be retained per arch.

No signature object or array is created for an arch. Zero marks an empty slot,
and append-only arch creation means no tombstone is required.

### Sparse Structural Edges

The transition cache should store only observed directed edges.

Each arch receives one 4-byte edge-head index:

\[
4M
\]

Each directed edge stores three `int` values:

- `fieldId`
- `dstArchId`
- `nextEdge`

Its cost is:

\[
12E
\]

An add/remove relationship is cached in both directions, so resolving one new
undirected relationship normally appends two directed entries.

Lookup is proportional to the observed degree `D`, not the registered field
count `N`. When edge density is low, this is substantially smaller than a dense
toggle table. If measurements later find large `D` values, a shared sparse hash
or an immutable per-arch edge index can be introduced while preserving `O(E)`
storage.

### Global Footprint Formula

Without field-layout metadata, the sparse global catalog is approximately:

\[
(13.33 \text{ to } 18.67)M + 4S + 12E
\]

The range of linear `M` terms consists of:

- 4 bytes for the cumulative signature end.
- Between approximately 5.33 and 10.67 retained bytes for the signature index
  under 75%-threshold doubling.
- 4 bytes for the sparse edge head.

The shared composite allocator requires an immutable layout entry for each
materialized field membership. If the common layout entry is 4 bytes, the
formula becomes:

\[
(13.33 \text{ to } 18.67)M + 8S + 12E + I
\]

If some layouts require a wider representation, use the measured average `L`:

\[
13.33M + (4 + L)S + 12E + I
\]

`I` is zero for arches using direct packed-signature search. When immutable
micro-indexes are built only for wide signatures, `I` remains proportional to
the memberships in those indexed signatures and therefore remains `O(S)`.

All values are logical payload estimates. Shared-array headers, geometric
growth slack, alignment, and allocator fragmentation remain additional terms.

### Sparse Example

Consider:

- `M = 100,000` materialized arches.
- `N = 1,000` registered fields.
- Average `K = 10`, so `S = 1,000,000`.
- Average `D = 2`, so `E = 200,000`.

The raw current two-int transition matrix costs:

\[
8MN = 800,000,000\text{ bytes}
\]

The sparse catalog without layout metadata or optional micro-indexes costs
approximately:

\[
13.33M + 4S + 12E = 7,733,000\text{ bytes}
\]

With a 4-byte layout entry per membership and no optional micro-index, it costs
approximately:

\[
11,733,000\text{ bytes}
\]

The sparse cost depends on materialized signatures and observed edges; it is
independent of the unexplored portion of the power set.

## Sparse Membership and Immutable Layout Lookup

The dense transition matrix currently doubles as the field-presence index.
Removing it requires a sparse membership strategy that remains fast enough for
`GetArchetypal`, `HasArchetypal`, and overwriting an existing field.

### Small Signatures

For small `K`, search the contiguous packed field span. A span search has:

- No auxiliary memory.
- No hashing.
- Good cache locality.
- Potential runtime vectorization.

The initial implementation should measure `ReadOnlySpan<int>.IndexOf` and a
sorted binary search. A threshold should be chosen from data rather than fixed
in the theory.

### Wide Signatures

For wider arches, build an immutable open-address micro-index when the arch is
created. The shared index slab stores `ordinal + 1`, not duplicate field IDs.

Lookup becomes:

1. Hash `fieldId` into the arch's table.
2. Read the candidate ordinal.
3. Confirm `packedFieldIds[start + ordinal] == fieldId`.
4. Probe until an exact match or an empty slot is found.

The ordinal directly addresses the parallel packed layout entry.

Normal signatures can use 16-bit ordinals. At a practical load factor, the
index is expected to add roughly 3–4 bytes per indexed membership. Very large
signatures can use a wider table.

The table is immutable after arch creation. Hot reads therefore require no
lock, dictionary, allocation, or mutable shared-state traversal.

An optional constant-size membership filter can reject many absent fields before
scanning. Any filter result remains advisory and must be confirmed against the
exact signature.

## Alloc-Local Sparse States

The alloc-local implementation should not allocate a directory entry for every
global arch.

Each alloc maintains:

- A sparse `archId -> stateId` index used on structural destinations.
- A dense array of active or retained alloc-local states.
- Packed handles for the storage classes owned by those states.

An alloc-local state contains the information required to address its blocks,
including:

- Global `ArchId`.
- Active row `Count`.
- Capacity or capacity order.
- Reference-free composite block handle.
- Range of reference-containing storage-class handles.

When a state becomes empty, it can remain in a bounded alloc-local cache or
release its blocks and recycle its `stateId`.

### Location Semantics

`EntArchLoc` can retain its current three-int footprint while storing:

- `AllocId`
- `StateId`
- `Row`

The dense state supplies the global `ArchId`. Normal value access therefore
does not consult the sparse `archId -> stateId` map. Structural movement uses
that map only to find or create the dst state.

This changes the meaning of one existing location field; it does not require a
specialized page or generation store for `EntArchLoc`.

## Composite Allocators

### Reference-Free Byte Allocator

Every closed `T` for which
`RuntimeHelpers.IsReferenceOrContainsReferences<T>()` is false can share one
alloc-local byte allocator.

The composite reference-free block for one active state contains column-major
storage for:

- `EntMut`
- Every reference-free field in the arch

The immutable layout entry for a reference-free field records the information
needed to calculate its column address. With packed unaligned columns, the
address can be expressed as:

\[
blockBase + C \times prefixBytes + Row \times sizeof(T)
\]

The closed generic access method knows `T`, so element size and typed reads or
writes can be specialized by the JIT. An aligned layout may be introduced for
types that benefit measurably; unaligned access avoids making alignment a
correctness requirement.

Reference-free blocks are intentionally left dirty when released. Every row is
overwritten before becoming visible through `Count`.

### Reference-Containing Typed Allocators

A `T` that is or contains references must remain in typed managed `T[]` storage
so the GC observes its correct descriptor.

Use one alloc-local typed allocator per distinct reference-containing `T`, not
one allocator per field name `N`. Differently named fields with the same value
type can share backing pages and free lists.

For one arch, all fields using the same reference-containing `T` can occupy
adjacent columns in one typed block. The layout entry records the type-local
column ordinal.

A released reference-containing block must be cleared before it is returned to
the allocator. This is different from the deliberate dirty-data policy for
reference-free blocks.

### Allocator Scope

Slabs and free lists are shared across arches within one alloc ownership
partition. They are not mutated across allocs.

This preserves the threading model:

- One owning thread mutates one alloc's blocks and free lists for the group.
- Different alloc owners use independent allocators concurrently.
- A higher-level page supplier may be shared only on cold page-growth paths;
  supplied pages then become alloc-local.

### Monolithic Slabs and Pages

The first implementation can use geometrically growing monolithic arrays. A
block handle is an offset, so array growth preserves logical addresses after
copying.

If whole-slab copying or LOH behavior becomes measurable, move to typed or byte
pages with alloc-local power-of-two free lists. Pages should be introduced from
measurement, not assumed necessary initially.

`ArrayPool<T>.Shared` is not a replacement for this design. It retains live
arrays per active arch, commonly over-rents, retains returned memory globally,
requires reference clearing, and introduces shared-pool synchronization.

## Active-State Lower Bound

For row capacity `C`, the unavoidable component payload is:

\[
C\left(8 + \sum_j S_j\right)
\]

The 8-byte term is `EntMut`. No allocator can reduce this payload without
compressing component values.

The remaining target overhead is:

- One dense row-state entry.
- Amortized sparse `archId -> stateId` index storage.
- One handle for the reference-free composite block.
- One handle per distinct reference-containing `T` used by the arch.
- Allocator fragmentation and shared page headers.

### Seven Reference-Free `int` Fields

At `C = 4`, the hard payload is:

\[
4(8 + 7 \times 4) = 144\text{ bytes}
\]

A practical sparse composite state adds approximately:

- 16 bytes for row state.
- 10.7 bytes amortized for a two-int sparse state-index slot at 75% occupancy.

The target is approximately 171 bytes plus slab fragmentation, compared with
approximately 408 bytes and eight arrays in the current capacity-four layout.

### Six `int` Fields and One Reference Field

At `C = 4`:

- Reference-free payload: `4 × (8 + 6 × 4) = 128` bytes.
- Reference payload: `4 × 8 = 32` bytes.
- State and sparse-index overhead: approximately 27 bytes.
- One typed block handle: approximately 4 bytes.

The target is approximately 191 bytes plus fragmentation, compared with
approximately 424 bytes currently.

## Hot-Path Model

Sparse catalog storage must not place a general-purpose dictionary lookup on
ordinary component access.

The intended point-access chain is:

```text
loc -> alloc-local state -> field ordinal -> layout -> block -> row
```

### Operation Costs

| Operation | Shared sparse work |
| --- | --- |
| `HasArchetypal` | Immutable signature or micro-index lookup |
| `GetArchetypal` | Same lookup, then one block address calculation and load |
| Existing-field `SetArchetypal` | Same lookup, then one block address calculation and store |
| Missing-field `SetArchetypal` | Sparse structural-edge lookup and row movement |
| `UnsetArchetypal` | Sparse structural-edge lookup and row movement |

The first three operations do not consult:

- The signature hash index.
- The sparse transition arena.
- The alloc's `archId -> stateId` map.
- A shared lock.
- An allocator free list.

Closed generic access should use the byte or typed slab directly. Virtual
column operations remain acceptable for heterogeneous structural copying,
growth, and clearing, but not for ordinary point reads or writes.

Bulk iteration should resolve field layout once before entering the row loop,
then traverse the contiguous column range directly.

## Object-Count Target

| Layer | Current scaling | Target scaling |
| --- | --- | --- |
| Transition arrays | One per arch-capacity slot | Shared edge-arena pages |
| Signature storage | Shared arrays | Shared arrays |
| Signature hash | Shared structures | Shared structures |
| Alloc row storage | One `EntMut[]` per active state | Shared alloc-local byte pages |
| Reference-free fields | One `T[]` per active membership | Shared byte pages |
| Reference-containing fields | One `T[]` per active membership | Typed pages per distinct `T` |

After shared capacity is available, materializing a global arch should create no
managed object. Activating an alloc-local state should create no managed object
when suitable blocks already exist in its alloc-local free lists.

Object creation should scale with shared array or slab-page growth, not with `M`,
`R`, or `S` directly.

## Threading Model

The sparse design preserves the existing contract:

- Signature interning, global arch creation, and shared edge insertion are
  serialized by the group catalog lock.
- Arch signatures, field layouts, and wide-signature micro-indexes are immutable
  after publication.
- Alloc-local states, byte slabs, typed slabs, and free lists are mutated only
  by the owning thread for that alloc and group.
- Different alloc owners may concurrently use the same global arch and group.

`GetArchetypal`, `HasArchetypal`, and overwriting an existing field remain free
of locks, managed allocations, and `Volatile` operations. Any publication
mechanism required for newly appended shared edges belongs only to structural
transition handling.

## Capacity and Fragmentation

The initial row capacity should be four:

`4 -> 8 -> 16 -> 32 -> ...`

Power-of-two row capacities make block size classes and reuse straightforward.
The physical footprint then contains two forms of slack:

- Unused rows within an active state's current capacity.
- Free or partially used space inside shared slabs or pages.

The implementation should report these separately. Hiding allocator
fragmentation inside a single retained-byte number makes regressions difficult
to diagnose.

A bounded empty-state cache should be expressed as a retained-byte budget rather
than only a count of empty arches, because capacity and component width can vary
substantially.

## Alternatives Not Selected Initially

### Fixed-Width Signature Masks

The number of registered fields is not bounded, so a fixed `uint` or `ulong`
signature is not a valid general representation.

### Dense Field-Major Transitions

Field-major transitions remove per-arch array objects but retain `O(MN)` cells.
They do not match sparse power-set exploration.

### One Dictionary Per Arch

Per-arch dictionaries multiply object headers, bucket arrays, entry arrays, and
poor cache locality. Shared packed or slab-backed indexes are required.

### Global Arch Eviction

Global definitions become comparatively compact in the sparse model. Reusing
global arch IDs would require invalidating signature indexes, edges, and
alloc-local references. Reclaim alloc-local empty blocks before considering
global definition eviction.

### Native Storage

Native byte arenas may eventually reduce managed pressure for reference-free
values. They do not reduce physical payload and add manual lifetime, alignment,
and unsafe-access complexity. Begin with managed byte slabs and measure first.

## Empirical Questions

The epic must measure rather than assume:

- The signature size where an immutable micro-index beats contiguous span
  search.
- Present-field versus absent-field lookup costs.
- The observed transition degree distribution `D`.
- Signature-index capacity and load across representative catalogs.
- Monolithic slab growth cost and LOH behavior.
- Slab fragmentation by component-size distribution.
- Aligned versus unaligned reference-free access for large structs and vectors.
- Point-access cost relative to the current jagged arrays.
- Cold signature creation before and after hash indexing.
- Reference-tail clearing and reference-block reuse costs.

The stress demo remains an integration check. Dedicated benchmarks must isolate
catalog creation, point access, structural movement, allocator growth, and
retained memory.

## Theoretical Acceptance Conditions

The final design satisfies the theory when:

- No retained structure is proportional to unexplored power-set signatures.
- Global metadata is `O(M + S + E)`.
- Alloc-local metadata is proportional to active or retained states and their
  actual storage classes, not `M × N`.
- Global arch materialization creates no object when shared arrays have spare
  capacity.
- Alloc-local activation creates no object when free blocks are available.
- Reference-free blocks remain intentionally dirty on reuse.
- Reference-containing blocks are cleared before reuse.
- Exact signatures resolve hash collisions.
- Ordinary point access remains allocation-free and lock-free.
- Different alloc owners continue to operate concurrently on the same group.
