# Archetype Footprint Reduction: Theory and Cost Model

> Status: AFR-24 is complete, AFR-25A's `SetArchetypal` structural slow-path
> split was measured and rejected, and AFR-25B's `ValuesAt` address-path and
> AFR-25C's unchecked terminal row access were measured and accepted.
> Specialized/direct `EntArchLoc` storage remains explicitly deferred and is
> not automatically the next candidate. AFR-26's decision gate
> follows the current AFR-25 work in the
> [Archetype Footprint Reduction epic](ECS.Archetypal.FootprintReduction.md).
> AFR-21 through AFR-24 have established the direct point-access, sparse
> global-catalog, and call-shape baselines described here. Later shared-storage
> work may refine constants, but it must preserve the measured hot-path
> contract.

The epic-start dense implementation is recorded in the
[AFR-01 and AFR-02 baseline](ECS.Archetypal.FootprintBaseline.md).

## Objective

The archetypal implementation should allocate storage only for registered
fields, materialized arch signatures, observed transitions, alloc-local states,
and component blocks that have actually been observed.

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
O(N + M + S + E + R + Q + \text{payload})
\]

not:

\[
O(MN)
\]

The public archetypal API remains unchanged, and the number of fields in a group
is not bounded by a fixed-width mask.

This memory target is subordinate to the execution target. Existing-component
`Get` and `Set` latency is optimized first; structural add/remove and movement
are second; cold catalog work is third. Footprint comparisons select only among
representations that equal or improve the accepted same-build point path.

Here, footprint means both total retained bytes and allocation topology. A
design is better when it retains fewer total bytes, creates fewer individual
managed objects, and performs fewer managed or native allocation events under
the same workload. Moving bytes outside the GC heap can improve GC behavior and
object count, but it does not make those bytes disappear from total footprint.

## Terms

| Symbol | Meaning |
| --- | --- |
| `N` | Number of registered fields in the arch group |
| `P` | Number of possible non-empty signatures, `2^N - 1` |
| `M` | Number of materialized global arch signatures |
| `K_i` | Number of fields in materialized arch `i` |
| `S` | Total materialized field memberships, `\sum K_i` |
| `E` | Number of stored directed structural edges, excluding implicit empty/singleton transitions |
| `D` | Average cached directed transitions per arch, `E / M` |
| `R` | Number of active alloc-local arch states |
| `Q` | Number of active alloc-local storage-class handles |
| `C` | Row capacity of one active alloc-local arch state |
| `S_j` | Stored size of component field `j` |
| `C_a` | Capacity of an arch-indexed catalog array |
| `C_f` | Capacity of a registered-field-indexed catalog array |
| `C_p` | Capacity of the packed field-ID array |
| `C_l` | Capacity of the parallel packed field-layout array |
| `C_i` | Capacity of the open-address signature index |
| `C_e` | Capacity of the sparse directed-edge arena |
| `C_s` | Capacity of the group-global structural signature scratch array |
| `B_ml` | Managed logical retained payload bytes, excluding object headers and alignment |
| `B_me` | Estimated managed retained bytes, including owned object headers and aligned payload |
| `B_n` | Native retained bytes requested from native allocation APIs |
| `B_t` | Total retained bytes, `B_me + B_n` |
| `O_m` | Number of individually owned managed objects |
| `A_m` | Cumulative managed allocation events in the measured workload |
| `A_n` | Cumulative native allocation events in the measured workload |
| `G` | Retained shared slab or page count |
| `W` | Retained row slack, free-page space, internal fragmentation, and alignment waste |

`M` can still grow very large because archetype variety is inherently
combinatorial. The design goal is not to conceal that cost. It is to ensure
that each materialized signature pays only for its actual fields and observed
relationships.

## Footprint and Allocation Accounting

The primary retained-byte equation is:

\[
B_t = B_{me} + B_n
\]

`B_ml` remains useful because it is deterministic from array capacities and
element widths. `B_me` adds estimated managed object and array headers plus
payload alignment. `B_n` is the sum of the requested capacities of every live
native allocation owned by the group and its alloc-local stores. The runtime's
allocator metadata and physical-page residency may add platform-specific cost,
so reports must identify `B_me` as an estimate rather than imply byte-perfect
process accounting.

Bytes alone do not describe GC and startup pressure. One 4 MiB page and 65,536
independent 64-byte arrays can retain comparable payload while having radically
different object traversal, allocation, and teardown behavior. The model
therefore treats these as independent outputs:

| Metric | Required meaning |
| --- | --- |
| Managed logical retained bytes | Capacity payload owned by managed arrays and value tables |
| Estimated managed retained bytes | Managed logical bytes plus estimated owned headers and alignment |
| Native retained bytes | Requested capacity of every currently live native allocation |
| Total retained bytes | Estimated managed retained bytes plus native retained bytes |
| Managed object count | Individually owned managed arrays and objects; runtime-shared empty arrays are excluded |
| Managed allocation event count | Cumulative owned managed allocations and replacement allocations during the scenario |
| Native allocation event count | Cumulative `NativeMemory.Alloc` calls that successfully create owned regions |
| Total allocation event count | Managed plus native allocation events |
| Page count | Retained pages split by reference-free native, reference-free managed, and typed managed kinds |
| Used payload bytes | Bytes occupied by live rows and required live metadata |
| Slack and fragmentation | Row slack, free retained block bytes, page-tail waste, size-class waste, and alignment padding, reported separately |

Allocation-event counters are workload counters, not snapshot properties. A
fresh-process benchmark can compare them directly; a long-running process must
report the interval or reset point. Slab replacement counts both the new
allocation and the retired allocation's transient peak, even if only one region
remains in the retained snapshot.

Native storage may lower `B_me` and `O_m`, but every live native byte remains in
`B_n` and therefore in `B_t`. Reports may separately claim lower GC-managed
footprint or fewer managed objects; they must not claim a total-byte reduction
unless `B_t` actually falls.

All speed evidence used to accept one footprint representation over another
must come from optimized Release builds. Point-access latency remains the first
gate; a smaller `B_t`, `O_m`, or allocation-event count does not justify a
slower happy path.

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

AFR-22 stores only observed directed edges. `edgeHeads` is one shared
`int[C_a]`; each slot is the head of that arch's linked edge chain:

\[
4C_a
\]

The shared `EntArchEdge[C_e]` arena stores three `int` values per directed
edge:

- `FieldId`
- `DstArchId`
- `NextEdgeIndex`

Its retained payload is:

\[
12C_e
\]

Index zero is the no-edge sentinel. Resolving one new non-empty relationship
appends two directed records, one for each direction. Empty/singleton
relationships are not stored in the arena. They use the direct
`singletonArchIds` field directory instead:

\[
4C_f
\]

Edge lookup is proportional to the observed degree `D`, not the registered
field count `N`. It is structural-only: ordinary `Get`, `Has`, and existing
`Set` do not traverse this chain. If measurements later find large structural
degrees, the edge representation can be specialized without changing point
access.

The catalog lock serializes edge creation and arena growth. A resolver writes
complete edge records before release-publishing their head indexes; concurrent
structural readers acquire-read those heads before traversing the immutable
records. The singleton directory uses the same acquire/release publication
discipline. These `Volatile` operations are deliberately absent from the point
path.

Building an uncached destination signature uses one group-global
`int[C_s]` scratch array:

\[
4C_s
\]

Scratch growth and use occur under the catalog lock. Its capacity follows the
widest structurally constructed signature, not `M * N`, and it is never
consulted by ordinary point access.

### Global Footprint Formula

The implemented AFR-21 through AFR-23 graph payload is best expressed in
actual retained capacities rather than an average bytes-per-arch estimate:

\[
B_{graph} =
4C_p + 4C_l + 4C_s + 4C_i + 8C_a + 4C_f + 12C_e
\]

The terms correspond directly to the shared arrays:

- `4C_p`: packed exact field IDs.
- `4C_l`: parallel four-byte field layouts.
- `4C_s`: structural destination-signature scratch.
- `4C_i`: open-address signature-index slots containing arch IDs.
- `4C_a`: cumulative signature ends.
- `4C_a`: sparse edge heads.
- `4C_f`: the direct field-to-singleton-arch directory.
- `12C_e`: three-int directed edge records.

This is a retained-capacity formula. It includes reserved sentinel slots and
geometric slack, which is why substituting `M`, `S`, or `E` directly can
understate a real snapshot. In particular, the signature index must use its
actual `C_i`, and the edge arena must use `C_e`, not `12E`.

The current catalog also retains the registered-field arrays. Each field slot
has an eight-byte `EntArchField` and one `EntArchColumnOps` reference. With
pointer size `p`, their additional payload is:

\[
(8 + p)C_f
\]

On a 64-bit runtime, the full logical catalog-array payload is therefore:

\[
B_{catalog} = B_{graph} + 16C_f
\]

The registered handler objects, shared-array headers, alignment, runtime type
data, and GC fragmentation are additional managed costs. They are reported
separately from logical payload. No per-arch transition array or per-arch
membership-index object remains.

### Sparse Example

Consider:

- `M = 100,000` materialized arches.
- `N = 1,000` registered fields.
- Average `K = 10`, so `S = 1,000,000`.
- Average `D = 2`, so `E = 200,000`.

Assume the corresponding geometric capacities are:

- `C_a = 131,072` and `C_f = 1,024`.
- `C_p = C_l = 1,048,576`.
- `C_i = C_e = 262,144`.
- `C_s = 16`, because the widest structurally constructed signature in this
  example fits that scratch capacity.

The former dense two-int transition cells alone would retain:

\[
8C_aC_f = 1,073,741,824\text{ bytes}
\]

The complete implemented sparse graph arrays, including exact signatures,
layouts, hash index, scratch, singleton directory, edge heads, and edge arena,
retain:

\[
B_{graph} = 13,635,648\text{ bytes}
\]

Adding the registered-field metadata and handler-reference arrays on a 64-bit
runtime gives:

\[
B_{catalog} = 13,652,032\text{ bytes}
\]

These are logical payload figures, not estimated managed bytes. The sparse cost
depends on registered fields, materialized signatures, the widest constructed
signature, and observed edges. It is independent of the unexplored portion of
the power set and contains no `C_a * C_f` term.

## Direct Point Membership and Exact Structural Signatures

AFR-21 does not search a packed signature to answer a point-access membership
question. The closed generic `EntArchColumn<T, N, A>` already identifies one
field and owns its alloc/arch column directory. Its `ValuesAt(allocId, archId)`
lookup returns the existing `T[]` column or `null`:

```text
closed generic field -> alloc slot -> arch slot -> T[] or null
```

That one result answers both questions needed by the public point operations:

- A non-null column means the field is present in the arch.
- `column[row]` is the component value.

`GetArchetypal` reuses the returned column for its load, `HasArchetypal` tests
it for null, and existing-field `SetArchetypal` writes through it and returns.
The lookup is `O(1)` with respect to signature width, registered field count,
and structural degree. It performs no hashing, packed scan, managed allocation,
lock acquisition, or `Volatile` operation.

AFR-25B simplified this lookup without changing its directory shape. `ValuesAt`
now snapshots its outer closed-generic directory once, uses unsigned bounds
checks for both nonnegative IDs, and relies on the permanent null arch-zero slot
instead of testing `archId == 0` separately. Exact and specialized callers lose
three compare/branch pairs. The generic-shared helper also performs fewer static
loads and branches. This is an address-sequence optimization only: it neither
changes the public API nor adds synchronization.

### Exact Signatures Remain Canonical

The direct column is a point-access presence and value slot; it is not arch
identity. Sorted packed field IDs remain the canonical signature, and the
global signature hash index always confirms a candidate against those exact
IDs. Parallel packed layouts remain the structural movement metadata.

No point operation searches for a field ordinal. The only field-membership
ordinal scan is an uncached structural removal, where the resolver finds the
removed field in the src signature before copying the retained IDs into the
locked group-global scratch array. Cached removal follows its sparse edge.
Structural addition also uses scratch to construct the dst signature, but the
point path never reads or writes scratch.

### Rejected Ordinal Hash

AFR-21 measured an immutable per-arch ordinal hash before accepting the direct
column path. Its isolated probe kernel was inexpensive, but its end-to-end
directory, hash, probe, confirmation, and layout traversal approximately
doubled point-access time for `K = 4` through `K = 32`: present `Get` was about
26.70–26.77 ns and `Has` about 24.93–25.09 ns. The ordinal hash was therefore
removed from the production point path.

The AFR-21 direct-column benchmark, whose worker is generic in `A`, was flat
across `K = 1, 4, 8, 16, 32`:

| Operation | Median range | Managed allocation |
| --- | ---: | ---: |
| Present `Get` | 8.716–8.826 ns | 0 B/op |
| Absent `Get` | 6.605–6.674 ns | 0 B/op |
| Present `Has` | 8.456–8.500 ns | 0 B/op |
| Absent `Has` | 6.370–6.467 ns | 0 B/op |
| Existing `Set` | 9.682–9.733 ns | 0 B/op |

These measurements make direct, width-independent lookup part of the design
contract rather than an optional optimization, but their absolute values are
specific to a generic-shared call shape.

### AFR-24 Call-Shape Baseline

AFR-24 replaced the preliminary single-row probe with paired seven-sample
steady-state sweeps on Release .NET 10.0.9. Each cell performed 5,000,000
operations after ten warmups and rotated through 1,024 Ents distributed across
four allocs and four Ent pages, 16 active arches, 64 alloc-local states, and 16
rows per state. Every measured point case allocated zero bytes per operation.

The formal matrix crossed concrete and generic-in-`A` callers with sealed-class
and readonly-struct group markers. Across the paired A/B sweep orders and the
measured value shapes:

- Generic-class rotating `Get` cost 2.86–3.13 times its matching
  concrete-class case.
- Generic-class rotating existing-field `Set` cost 2.21–3.28 times its
  matching concrete-class case.
- Generic-struct and concrete-struct cases stayed within approximately
  0.99–1.01 times one another.
- Concrete class and concrete struct markers were generally within about 4.1%
  of one another, with no universal winner.

The distinction is therefore generic reference-type canonical sharing versus
value-type specialization, not an inherent penalty for using a class as the
group marker. A concrete caller gives the JIT an exact reference-type marker
and can inline through its closed-generic static storage. A caller generic in a
reference-type `A` uses canonical shared code and retains generic-context and
static-base work. A value-type `A` receives a specialized instantiation, so its
generic caller can approach the concrete caller's generated code and latency.

The warmup count is part of the measurement contract. With only three warmups,
the generic-class helper exhibited two tiering modes near 6 ns/op and 13 ns/op.
Ten warmups stabilized the paired steady-state sweeps and prevented a tiering
transition from being mistaken for a call-shape or storage effect.

AFR-24 also measured stages inside the point path. These are absolute kernel
latencies, not additive components to sum into the end-to-end `Get` or `Set`
time:

| Steady stage | Concrete class A/B | Concrete struct A/B | Generic class A/B | Generic struct A/B |
| --- | ---: | ---: | ---: | ---: |
| Loc retrieval | 1.179 / 1.164 ns | 1.163 / 1.159 ns | 2.537 / 2.540 ns | 1.168 / 1.176 ns |
| `ValuesAt` directory resolution | 0.724 / 0.717 ns | 0.717 / 0.714 ns | 2.224 / 2.224 ns | 0.718 / 0.720 ns |

The final cached-row kernels measured 0.329 / 0.333 ns for a raw row load and
0.297 / 0.297 ns for a raw row store across the A/B sweeps. These raw-row
figures isolate the terminal array operation after column resolution; they do
not represent a public point-access call and must not be subtracted from or
added to the other stage measurements.

The absent cases above use a field whose outer column directory has never been
allocated in that worker, so they measure the best-case missing-column exit.
Before treating those numbers as the general absent-field cost, add a case in
which the same field is warm in another arch of the same alloc. This caveat does
not affect the present happy-path result or its width independence.

### AFR-25A Rejected Structural Slow-Path Split

AFR-25A tested whether moving the missing-field body of `SetArchetypal` into a
private `NoInlining` helper would make the existing-field caller faster. The
existing-column lookup and direct row store remained in the hot caller; only
singleton/add resolution, movement, and first-value initialization moved to the
cold helper.

The first candidate forwarded the value to that helper as `in T`. Even though
the helper was not called by the measured existing-field branch, the JIT
stack-homed the scalar loop value so that its address remained available to the
cold call. That spill made the exact/specialized scalar rotating cases
repeatably slower: concrete class regressed 11.02% and 10.94%, concrete struct
9.70% and 10.26%, and generic struct 7.91% and 7.45% across the two Release
sweeps.

Passing the cold-helper value by value removed that spill. It also reduced the
representative scalar rotating caller from 4,476–5,389 bytes to 709–1,114 bytes
under Tier1-OSR, and from 1,354–2,266 bytes to 650–1,015 bytes under FullOpts.
That code-size reduction did not produce a point-latency win. The aggregate
median candidate delta was -0.652% in one sweep and +0.067% in the reversed
sweep, while scalar concrete class, concrete struct, and generic struct still
regressed by 2.00%/3.30%, 4.49%/2.12%, and 1.90%/2.10%, respectively.

The latency gate therefore rejected both forms. Production
`SetArchetypal` behavior and its public API remain unchanged, and the temporary
candidate and benchmark scaffold were removed. This result is also a warning
against treating smaller generated code as a proxy for a faster point path:
cold-call ABI requirements can affect the branch that never takes the call.

### AFR-25B Accepted `ValuesAt` Address Simplification

AFR-25B tested the simplified outer-directory snapshot and unsigned bounds-check
sequence in the complete rotating `Get` and existing-field `Set` callers. The
full optimized Release sweep reported a -5.60% median candidate delta across
the matrix: -5.89% for `Get` and -4.28% for `Set`. No measured cell regressed,
and every case remained allocation-free with no garbage collections.

A shorter reverse-order confirmation retained 6.6% through 9.5% improvements
for the exact scalar sentinels. After the generic-class tiering result settled,
its scalar `Get` and `Set` improvements were 2.76% and 2.17%, respectively. An
A/A run of the unchanged path was neutral, supporting attribution to the
candidate rather than sweep order. Disassembly agreed with the latency result:
exact and specialized callers removed three compare/branch pairs, while the
generic-shared helper reduced static loads and branches.

The production change was therefore accepted. The temporary comparison harness
was removed, the public API stayed stable, and the point path remains
allocation-free, lock-free, and free of `Volatile`. The ownership model is also
unchanged: a single thread owns each alloc's group-local rows and columns, while
different alloc owners may concurrently use the same group and arch.

### AFR-25C Accepted Unchecked Terminal Row Access

AFR-25C replaces only the final `values[loc.Row]` load or existing-field store
after `ValuesAt` has returned a non-null column. At that point, `loc.Row` is a
subsystem-owned index: append and move operations return a valid row,
swap-back compaction repairs the moved Ent's loc immediately, and the
single-thread-per-alloc ownership rule prevents another thread from observing
or mutating an intermediate alloc-local row state. Column capacity tracks the
corresponding Ent rows and does not shrink while point access can address them.

Those invariants justify obtaining the typed array-data reference with
`MemoryMarshal.GetArrayDataReference(values)` and applying `Unsafe.Add` with
`loc.Row`. They do not authorize unchecked alloc/arch directory access or
unchecked structural writes: `ValuesAt` retains its directory bounds and null
checks, and movement, append, compaction, and first-value initialization retain
their structural contracts.

The resulting value is a managed typed byref, not a raw pointer. It remains
visible to the GC, so stores of classes and reference-containing structs retain
the required write barriers. Reference-containing columns must never replace
this operation with native pointers, byte stores, or another type-erased write
that hides references from the GC.

Complete optimized Release callers showed the strongest repeatable gains in
specialized scalar `Get`, at approximately 5% through 6%; specialized scalar
`Set` improved by approximately 2%. Generic-class results were modest to
neutral, with no repeatable broad regression. Generated code removed the final
array bounds check, every measured path remained at zero managed allocation,
and the candidate was accepted.

### Shared-Slab Cutover Gate

AFR-34 and the later shared-slab work are expected to remove the current
per-field jagged column directories. That footprint improvement cannot replace
`ValuesAt` with a signature scan, ordinal hash, dictionary, or another
higher-constant point lookup. The shared-storage design must first provide an
equally direct `O(1)` field-to-column slot or handle, and benchmarks must show
point access matching or improving on the direct-column path before cutover.

This constraint does not imply interlaced component rows. Current `T[]` columns
are contiguous, and future composite storage remains column-major. Iteration
resolves a column once, exposes it as a `Span<T>` or `ReadOnlySpan<T>`, and then
walks contiguous elements without repeating point lookup inside the row loop.

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

AFR-31 can compare retaining the current three-int meaning with a candidate
three-int shape storing:

- `AllocId`
- `StateId`
- `Row`

In that candidate, the dense state supplies the global `ArchId`. Normal value
access therefore does not consult the sparse `archId -> stateId` map.
Structural movement uses that map only to find or create the dst state.

`StateId` is not predetermined. The current `(AllocId, ArchId, Row)` shape or
another same-size direct locator remains preferable unless the complete
candidate point path wins the AFR-24 through AFR-26 gate. Specialized/direct
`EntArchLoc` storage is explicitly deferred; its independent result would not
select `StateId` or authorize a broader Ent lifecycle redesign.

## Composite Storage

### Reference-Free Byte Store

Every closed `T` for which
`RuntimeHelpers.IsReferenceOrContainsReferences<T>()` is false can share one
alloc-local byte store.

The store may be backed by large managed `byte[]` pages or by large
uninitialized native pages from `NativeMemory.Alloc`. Native backing is a
candidate for both reference-free payload and reference-free store metadata:
packed state descriptors, page descriptors, size-class heads, block handles,
and free ranges contain no GC references and do not require managed objects.
The managed alternative uses shared value arrays for the same metadata. AFR-32
must compare both; moving metadata native is not assumed to be faster.

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

One integer block handle identifies a page and offset, directly or through a
packed state table. Allocating a state carves a range from an existing page; it
does not allocate a managed block object, a native region, or a linked-list node
for that block. Free-list links and size-class membership live in page-local
headers or shared packed value tables. Object and allocation-event counts
therefore scale with page growth, not active state count.

AFR-20 encodes that layout in one four-byte value:

- `encoded >= 0` is the reference-free byte prefix.
- `encoded < 0` is `~typeColumn` for reference-containing storage.

The sign is therefore also the clearing classification. Prefixes start after
`EntMut`; reference-containing fields do not advance them. A type-column value
counts only earlier fields with the same exact `T`, so different `N` fields can
share one typed store without interlacing their columns with another type.
The initial cold compiler can obtain that count by scanning earlier fields;
only measurement of wide reference-heavy signatures should justify a reusable
per-storage-class count table.

The closed generic access method knows `T`, so element size and typed reads or
writes can be specialized by the JIT. An aligned layout may be introduced for
types that benefit measurably; unaligned access avoids making alignment a
correctness requirement.

Reference-free blocks are intentionally left dirty when released. Every row is
overwritten before becoming visible through `Count`.

### Type-Erasure Boundary for Reference-Free Values

The public API is closed generic because callers read and write a particular
`T`, and `T`, `N`, and `A` identify a field. That does not require closed
generic code throughout the storage implementation.

For a reference-free field, closed generic code is required only at two
boundaries:

1. Registration uses `T` to record the field ID, byte width, storage class, and
   any measured alignment class.
2. Public access uses the closed generic tuple to obtain its field ID. `Get`
   and `Set` additionally use `T` to perform a typed unaligned read or write.

After registration, block allocation, growth, movement, compaction, released
tail handling, and retained-byte metrics need only immutable layout metadata
and byte-block handles. `N` remains part of field identity but has no role in a
reference-free copy. The structural layer can therefore erase both `T` and `N`
for these operations. `A` continues to select the independent arch group and
its alloc-local storage; type erasure does not merge group ownership.

The correct storage test is:

```csharp
RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false
```

The C# `unmanaged` category is sufficient but is not the implementation
boundary. The public methods intentionally keep unconstrained `T`, and the
runtime test classifies the actual closed type. Interop blittability is also
not required: internal relocation may copy the managed representation of
reference-free booleans, characters, enums, padded structs, and explicit-layout
structs without interpreting those bytes.

Reference-containing values cannot cross this boundary. Storing their bytes in
a `byte[]` would hide references from the GC and bypass required write barriers.
They remain in typed GC-visible storage.

### Why Reinterpreting the Current Arrays Is Insufficient

The current graph stores one `EntArchColumnOps<T, N, A>` object and one static
`T[][][]` directory per registered field. The generic type is currently how a
heterogeneous `fieldId` finds its storage. Consequently, constructing a byte
span inside `EntArchColumnOps.Copy` would still pay for:

- The `fieldId -> EntArchColumnOps` lookup.
- A virtual call through the heterogeneous handler array.
- The alloc and arch levels of the jagged typed directory.
- One independently allocated component array per active membership.

It would also replace a JIT-known fixed-size assignment with a variable-size
copy. That is likely worse for 1-, 2-, 4-, and 8-byte values. The useful change
begins only after the shared byte slab and immutable block layouts exist; at
that point the physical storage is already bytes and no cast is required.

Today a move dispatches `Copy` once per retained field, optionally dispatches
another `Copy` for every src field during swap-back compaction, and dispatches
`Clear` for every src field. For an all-reference-free arch, every `Clear` call
returns without writing. A `K = 8` remove therefore performs 15 virtual calls
when no compaction is needed and 23 when swap-back is required.

After that cutover, a reference-free field also needs no
`EntArchColumnOps<T, N, A>` object. Its registry entry can be value metadata,
while its closed generic static retains only field identity and the public
typed access boundary. This can reduce handler objects and structural generic
code footprint. The much larger removal of per-membership arrays belongs to
the shared-slab tasks rather than to byte copying by itself; startup, JIT code,
and managed object counts should be reported separately.

### Structural Byte Movement

The planned column-major block layout keeps each component column contiguous.
Moving one row therefore still performs one copy for each retained
reference-free membership. A single whole-row copy is not generally available
without switching to row-major storage and giving up column iteration locality.

For one membership, the structural loop obtains:

- The src block offset and column prefix.
- The dst block offset and column prefix.
- The src and dst rows and capacities.
- The immutable element byte width.

It then copies exactly that value's managed bytes. The default complexity is
`O(K + B)`, where `K` is the retained reference-free field count and `B` is the
number of bytes copied. No virtual or generic dispatch is required.

`Span<byte>` is allocation-free and provides a clear, overlap-safe copy, but it
is not automatically the fastest per-value kernel. The implementation should
measure:

- Direct 1-, 2-, 4-, 8-, and 16-byte loads/stores.
- `Unsafe.CopyBlockUnaligned` for runtime-known widths.
- `Span<byte>.CopyTo` for wider values and block growth.
- Packed unaligned columns versus explicitly aligned wide columns.

Offsets rather than spans or pointers remain in persistent state. Every
operation reacquires a GC-tracked byref or span from the current slab, so a slab
resize cannot leave a stale interior pointer.

Correctness requires:

- Copying between layouts for the same field ID and exact byte width.
- Addressing `columnBase + row * width` with the correct state capacity.
- Completing every destination write before publishing its row through
  `Count`.
- Leaving reference-free tails dirty rather than adding a clearing pass.
- Using typed copy and clear operations for every reference-containing storage
  class.
- Preserving alloc ownership; mutable byte slabs and free lists remain local to
  one alloc owner.

Raw padding bytes may be relocated but must never become signature identity,
equality, hashing, or serialization input.

The current all-`int`, `K = 8` measurements show why this is worth isolating.
Cached add is 487.11 ns/move and cached remove is 537.89 ns/move. First and
middle compaction are 594.14 and 589.84 ns/move, while last-row removal is
440.23 ns/move. The roughly 150 ns difference includes the extra eight-field
compaction loop, or about 19 ns per field for its loop, lookup, virtual dispatch,
and four-byte assignment. This is evidence of removable overhead, not a
prediction of the byte path's final speedup.

The likely percentage win is largest for arches with many small reference-free
fields, where dispatch and address discovery dominate each assignment. Wide
values increasingly become memory-copy bound, and reference-heavy arches retain
typed work, so neither should be promised the same relative improvement.

### Reference-Containing Typed Stores

A `T` that is or contains references must remain in typed managed `T[]` storage
so the GC observes its correct descriptor.

Use one alloc-local typed store per distinct reference-containing `T`, not one
store per field name `N`. Differently named fields with the same value
type can share backing pages and free lists.

For one arch, all fields using the same reference-containing `T` can occupy
adjacent columns in one typed block. The layout entry records the type-local
column ordinal.

The typed store allocates large `T[]` pages and suballocates multiple arch-state
blocks from each page. One typed page is one managed array object and one
managed allocation event, not one object per field membership or per block.
Its free-list and block metadata should use packed integer/value tables rather
than managed node objects.

Native memory cannot contain `T` values that are or contain references. Native
metadata may describe typed blocks only when it contains no managed reference;
the actual `T[]` page roots and any directory holding those roots remain
GC-visible managed storage so pages cannot move out of reach of the collector.

A released reference-containing block must be cleared before it is returned to
the typed store. This is different from the deliberate dirty-data policy for
reference-free blocks.

### Store Scope

Slabs and free lists are shared across arches within one alloc ownership
partition. They are not mutated across allocs.

This preserves the threading model:

- One owning thread mutates one alloc's blocks and free lists for the group.
- Different alloc owners use independent stores concurrently.
- A higher-level page supplier may be shared only on cold page-growth paths;
  supplied pages then become alloc-local.

### Shared Slabs and Large Pages

Storage is acquired in large shared pages, not one allocation per arch state or
component block. Within one alloc and group:

- One reference-free byte store supplies blocks to every arch state.
- One typed managed store per distinct reference-containing `T` supplies typed
  blocks to every arch state using that `T`.
- Page directories, size-class heads, and block descriptors are packed shared
  tables. A free block is represented by integer offsets or indexes, never by a
  managed node object.

A monolithic slab is the one-page form of this design. Geometric replacement is
simple and can be a useful first measurement, but every growth event allocates
the larger region, temporarily retains old and new capacity, copies live data,
and then retires the old region. A multi-page store instead appends a page and
keeps existing page-relative handles stable. It trades one extra page selection
for lower growth-copy and transient-peak costs.

A stable block handle contains or resolves to `pageIndex`, `offset`, and the
size/capacity class. Mutable state stores handles, not interior managed byrefs.
For managed pages, each operation reacquires a GC-tracked byref from the current
array. For native pages, the owning page directory holds the base pointer and
requested byte count; transient addresses are derived from that owned base and
native-sized handle arithmetic. Point access uses already-resolved state and
layout metadata rather than a validation or lookup layer.

Native page ownership is deterministic:

- The alloc/group reference-free store owns every pointer returned by
  `NativeMemory.Alloc` and records its exact requested capacity.
- Store teardown calls `NativeMemory.Free` exactly once for every still-owned
  page. A finalizer is not the ownership mechanism.
- Individual block release returns a range to the owning page's free lists; it
  never frees an interior pointer.
- No state handle, span, byref, or page descriptor may outlive its owning store.
- Ownership transfer, if introduced, must update one authoritative page table;
  duplicated raw-pointer ownership is forbidden.

Reference-free native pages are allocated with `NativeMemory.Alloc`, not
`NativeMemory.AllocZeroed`. New and recycled bytes are dirty by contract and are
fully initialized before row publication. Typed managed pages retain normal GC
allocation and clearing rules because reference-containing values cannot be
stored natively.

`ArrayPool<T>.Shared` is not a replacement for this design. It retains live
arrays per active arch, commonly over-rents, retains returned memory globally,
requires reference clearing, and introduces shared-pool synchronization.

## Active-State Lower Bound

For row capacity `C`, the unavoidable component payload is:

\[
C\left(8 + \sum_j S_j\right)
\]

The 8-byte term is `EntMut`. No storage scheme can reduce this payload without
compressing component values.

This lower bound contributes to total retained bytes whether its page is a
managed array or a native region. Native placement can change GC scanning,
managed object count, and header/alignment overhead; it cannot subtract the
live component bytes from `B_t`.

The remaining target overhead is:

- One dense row-state entry.
- Amortized sparse `archId -> stateId` index storage.
- One handle for the reference-free composite block.
- One handle per distinct reference-containing `T` used by the arch.
- Slab fragmentation and shared page headers.

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

Sparse catalog storage must not place a general-purpose dictionary, signature
scan, or ordinal hash on ordinary component access.

The implemented AFR-21 point-access chain is:

```text
loc -> closed-generic column directory[alloc][arch] -> T[][row]
```

After shared slabs replace the current directory, the permitted shape is:

```text
loc -> closed-generic direct slot/handle -> column base -> row
```

The storage address may change; the direct, constant-time lookup contract may
not.

AFR-25A showed that shrinking `SetArchetypal` by isolating its structural body
does not by itself shorten this address chain. AFR-25B improved the second step
by simplifying `ValuesAt`'s directory access, and AFR-25C removed the final
array bounds check after a column has been resolved and its subsystem-owned row
has been established as valid. Specialized/direct `EntArchLoc` storage remains
an independent point-address candidate, but is explicitly deferred.

### Operation Costs

| Operation | Point or structural path |
| --- | --- |
| `HasArchetypal` | Direct closed-generic column lookup and null test |
| `GetArchetypal` | Same lookup, then one indexed row load |
| Existing-field `SetArchetypal` | Same lookup, then one indexed row store |
| Missing-field `SetArchetypal` | Direct miss, singleton/edge resolution, optional structural scratch, and row movement |
| `UnsetArchetypal` | Direct presence test, singleton/edge resolution, uncached signature scan/scratch, and row movement |

The first three operations do not consult:

- The signature hash index.
- The packed exact signature and field layouts.
- The sparse transition arena.
- The alloc's `archId -> stateId` map.
- A shared lock.
- `Volatile` publication.
- Structural signature scratch.
- A store free list.

Closed generic access should use the byte or typed slab directly. Reference-free
structural movement should use the type-erased byte path measured by AFR-36.
Typed dispatch remains acceptable where reference-containing storage requires
GC-visible copy and clear operations, but not for ordinary point reads or
writes.

Bulk iteration obtains the contiguous column once before entering the row loop,
then traverses a `Span<T>` or `ReadOnlySpan<T>` directly. The column-major
shared-slab plan preserves this shape; it does not interlace component values
by row.

## Object-Count Target

| Layer | Before its reduction | Implemented or target scaling |
| --- | --- | --- |
| Transition cache | One array per arch-capacity slot | One shared edge-head array and one shared edge arena; pages only if measurement justifies them |
| Signature storage | Shared arrays | Shared arrays |
| Signature hash | Shared structures | Shared structures |
| Alloc row storage | One `EntMut[]` per active state | Shared alloc-local reference-free pages |
| Reference-free fields | One `T[]` per active membership | Shared managed or native byte pages |
| Reference-containing fields | One `T[]` per active membership | Shared typed managed pages per distinct `T` |
| State and block metadata | Per-state arrays or objects | Packed shared managed value arrays or native tables |
| Free blocks | Potential node per free range | Integer offsets in page-local or shared packed free lists |
| Page directory | Not applicable to per-block arrays | One packed directory per store, grown geometrically |

After shared capacity is available, materializing a global arch should create no
managed object. Activating an alloc-local state should create no managed object
when suitable blocks already exist in its alloc-local free lists.

It should also perform no allocation event in that case. State activation
updates packed descriptors and free-list indexes only. Page growth is the
allocation boundary:

- A managed reference-free page creates one `byte[]` object and one managed
  allocation event.
- A native reference-free page creates no managed payload object and one native
  allocation event; page-directory growth is counted separately.
- A typed reference-containing page creates one `T[]` object and one managed
  allocation event.

No state block, typed block, free range, or size-class entry gets its own
managed wrapper or linked-list node. Object creation and allocation events
should scale with shared catalog-array and page growth, not directly with `M`,
`R`, `S`, or the number of free blocks.

## Threading Model

The sparse design preserves the existing contract:

- Signature interning, global arch creation, and shared edge insertion are
  serialized by the group catalog lock.
- Arch signatures and field layouts are immutable after publication.
- The group-global signature scratch array is grown and used only while holding
  the catalog lock.
- Structural edge records are initialized before release-publishing their head
  indexes. Structural readers acquire-read a head before following its immutable
  chain; singleton publication follows the same rule.
- Alloc-local states, byte slabs, typed slabs, and free lists are mutated only
  by the owning thread for that alloc and group.
- Different alloc owners may concurrently use the same global arch and group.

`GetArchetypal`, `HasArchetypal`, and overwriting an existing field use only the
owning alloc's direct column slots. They remain free of locks, managed
allocations, and `Volatile` operations. Acquire/release publication and shared
scratch belong only to structural transition handling.

## Capacity and Fragmentation

AFR-12 sets the initial row capacity to four:

`4 -> 8 -> 16 -> 32 -> ...`

Relative to the previous initial capacity of 16:

| Peak state occupancy | Previous capacity | AFR-12 capacity |
| ---: | ---: | ---: |
| 1–4 | 16 | 4 |
| 5–8 | 16 | 8 |
| 9 or more | Same power of two | Same power of two |

For one retained state, the logical payload change is:

\[
(C_{new} - C_{old})\left(8 + \sum_j sizeof(T_j)\right)
\]

The eight-byte term is the row's `EntMut`. Array headers cancel because the
number of retained arrays does not change. States that grow past four rows pay
additional transient replacement arrays even when their final capacity is the
same; retained capacity and growth allocation must therefore be reported
separately.

Power-of-two row capacities make block size classes and reuse straightforward.
The physical footprint then contains several distinct forms of slack:

- Unused rows within an active state's current capacity.
- Size-class rounding inside a live block.
- Alignment padding between columns or blocks.
- Free retained blocks available for reuse.
- Unusable page-tail space that cannot satisfy the next block request.

The implementation should report these separately. Hiding page fragmentation
inside a single retained-byte number makes regressions difficult to diagnose,
while excluding free or tail bytes would understate `B_t`.

Page size is a direct object/event-versus-slack tradeoff. Larger pages provide:

- Fewer managed objects for managed backing.
- Fewer managed or native allocation events.
- Fewer page-directory entries and less frequent directory growth.
- More room to reuse blocks across arches before another page is required.

They can also retain more unused tail capacity, increase transient bytes when a
monolithic slab grows, and make low-occupancy allocs disproportionately
expensive. Smaller pages reduce worst-case retained slack but increase object
count, allocation-event count, page-directory traffic, and page-selection/TLB
pressure. The implementation should benchmark a small set of power-of-two page
classes against observed block-size distributions rather than select one
unexplained constant for every store.

Each report must split page counts and retained capacities by reference-free
managed, reference-free native, and typed managed pages. For each kind, report
used payload, reusable free bytes, internal/size-class waste, alignment waste,
and page-tail waste. Allocation-event totals and transient peak growth bytes
remain separate from the retained snapshot.

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
poor cache locality. Global structural indexes must be shared and packed;
ordinary point access uses the direct closed-generic slot instead.

### Global Arch Eviction

Global definitions become comparatively compact in the sparse model. Reusing
global arch IDs would require invalidating signature indexes, edges, and
alloc-local references. Reclaim alloc-local empty blocks before considering
global definition eviction.

### Native Storage

Uninitialized `NativeMemory.Alloc` backing is a first-class candidate for both
reference-free payload pages and reference-free store metadata. Eligible data
includes composite byte columns, packed state/page descriptors, block-handle
tables, size-class heads, and free-range metadata. Native allocation occurs per
large page or geometrically grown shared table, never per state block.

Native backing can remove managed slab objects, GC scanning pressure, and
large-object-heap behavior. It does not reduce the unavoidable component
payload, and allocator placement does not hide memory from the cost model. An
`X`-byte managed page moved to an `X`-byte native page may reduce `B_me` and
`O_m`, but it still contributes `X` bytes to `B_n` and approximately the same
amount to `B_t`.

The native contract is:

- Allocate with `NativeMemory.Alloc`, never `NativeMemory.AllocZeroed`. New and
  recycled reference-free bytes are intentionally dirty.
- Initialize every value that becomes visible before publishing the dst row's
  `Count`.
- Record each page's owner, base pointer, and exact requested capacity in one
  authoritative store page table.
- Pair every successful page/table allocation with exactly one deterministic
  `NativeMemory.Free` during replacement or owning-store teardown.
- Release blocks into packed alloc-local free lists; never allocate or free a
  native region for an individual block and never free an interior pointer.
- Retain stable integer page/offset handles in state. Do not duplicate owning
  raw pointers in per-block managed wrapper objects.
- Use native-sized arithmetic when deriving addresses from capacity, offsets,
  and byte prefixes.
- Preserve the alloc/group ownership partition and single-owner mutation model.
- Report requested native capacity, native allocation events, native page
  count, used bytes, and every native slack category alongside managed metrics.

No value that is or contains references may enter native payload or be hidden
inside native metadata. Reference-containing component pages remain typed
managed `T[]` buffers, and their managed roots remain visible to the GC.

AFR-32 should compare managed and native page/table combinations under the same
workloads. The comparison must report `B_ml`, `B_me`, `B_n`, `B_t`, `O_m`,
allocation events, page counts, and fragmentation together, then apply the
Release point-speed gate. Native storage is selected for measured total-system
benefit, not because moving a byte outside the GC makes it disappear from the
report.

## Empirical Questions

AFR-21 resolved the point-membership question: the ordinal hash lost
end-to-end, and direct closed-generic column lookup is the benchmark reference.
AFR-24 resolved the initial call-shape and stage questions: generic
reference-type canonical sharing is the expensive case, value-type generic
instantiations specialize to approximately concrete-call cost, and the
loc/directory/raw-row baselines above isolate where future point-path changes
must improve generated code. AFR-25A then rejected the structural slow-path
split because its large code-size reduction did not improve existing-field
latency. AFR-25B accepted the simplified `ValuesAt` directory sequence after a
-5.60% full-sweep median result with no regressions. AFR-25C accepted typed
`Unsafe.Add` terminal row access: it removed the final bounds check, improved
specialized scalar `Get` most strongly, produced a smaller `Set` gain, and did
not produce a repeatable broad regression. The remaining epic must measure
rather than assume:

- When revisited, whether specialized/direct `EntArchLoc` storage reduces the
  complete point-address sequence; this investigation is currently deferred.
- Whether the AFR-34/shared-slab direct slot or handle matches or improves the
  current present, absent, value-shape, and signature-width point results.
- Column-resolution and contiguous `Span<T>` iteration cost after shared-slab
  cutover.
- The observed transition degree distribution `D`.
- Signature-index capacity and load across representative catalogs.
- Managed-array versus uninitialized `NativeMemory.Alloc` backing for both
  reference-free payload and packed reference-free metadata, including point
  cost, relocation, GC/LOH behavior, and deterministic teardown.
- Monolithic replacement versus appended large pages, including transient peak
  bytes, copy cost, page-selection cost, and stable-handle behavior.
- Page-size classes by component/block distribution, reporting managed and
  native allocation events, page counts, total retained bytes, reusable free
  bytes, size-class waste, alignment waste, and page-tail waste.
- Typed managed page sizes and reuse for reference-containing values, including
  clear cost and managed object/allocation-event count.
- Verification that state activation and block reuse create no per-block
  managed wrapper or free-list node.
- Aligned versus unaligned reference-free access for large structs and vectors.
- Point-access cost relative to the accepted direct-column benchmark.
- Default JIT behavior versus `AggressiveInlining`, `AggressiveOptimization`,
  and their combination on the final point and structural paths, including
  cold JIT cost, generated-code size, and observed inlining decisions.
- Cold signature creation before and after hash indexing.
- Reference-tail clearing and reference-block reuse costs.

The stress demo remains an integration check. Dedicated benchmarks must isolate
catalog creation, point access, structural movement, store growth, and
retained memory.

Every performance measurement and disassembly used for a decision must come
from an optimized Release build. Debug artifacts are not valid evidence.

Iteration uses staged evidence so every prototype does not pay the cost of the
full formal matrix. Start with quick scalar sentinels. Run longer generic-class
cases only when canonical-sharing behavior remains uncertain, and reserve the
5-million-operation, seven-sample full sweep for final confirmation of a
promising candidate. Reverse order or A/A checks remain available when a short
result may reflect tiering or process drift.

## Theoretical Acceptance Conditions

The final design satisfies the theory when:

- No retained structure is proportional to unexplored power-set signatures.
- Global metadata is `O(N + M + S + E)` with no `M × N` term.
- Alloc-local metadata is proportional to active or retained states and their
  actual storage classes, not `M × N`.
- Global arch materialization creates no object when shared arrays have spare
  capacity.
- Alloc-local activation creates no object or allocation event when free blocks
  are available.
- No state block, component block, free range, or size-class entry has its own
  managed wrapper or linked-list node.
- Reference-free blocks remain intentionally dirty on reuse.
- Reference-free native pages and tables use uninitialized allocation and every
  owned allocation is released deterministically exactly once.
- Reference-containing payload remains in typed managed buffers and is cleared
  before reuse.
- Reports separate managed logical bytes, estimated managed bytes, native
  retained bytes, total retained bytes, managed object count, managed/native
  allocation-event counts, page counts, and slack/fragmentation categories.
- Native retained bytes are included in total footprint; moving storage native
  is never reported as making its physical payload disappear.
- Exact signatures resolve hash collisions.
- Ordinary point access is faster than the accepted pre-change same-build
  reference, direct `O(1)`, allocation-free, lock-free, and free of `Volatile`,
  signature scans, and hashing.
- Every performance or disassembly claim used for acceptance comes from an
  optimized Release artifact.
- Different alloc owners continue to operate concurrently on the same group.
