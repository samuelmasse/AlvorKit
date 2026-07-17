# Archetypal Per-Ent Row Iteration

## Status and decision

This document records the design and Release-mode research for allocation-free,
one-Ent-at-a-time iteration over an archetypal query.

Implementation status as of July 16, 2026: the accepted exact generated design
is implemented. Generated component groups expose named `WithProperty()` query
selectors. A demand-driven generator recognizes closed queries used with
`Rows()` and emits only those exact row/enumerator shapes in the consuming
compilation. The generated enumerator uses the existing span-chunk query for
cold arch discovery, binds selected managed column bases once per arch, and
uses writable base-plus-row properties in the inner loop.

Focused runtime and generator tests cover one, two, and eight selected fields,
empty and repeated enumeration, multiple matching archs, compaction between
enumerations, multiple allocs, writable properties, reference-containing fields, raw
generic query chains, and generated named query chains. The permanent short
Release benchmark now compares spans and generated rows at one, two, and eight
fields. Remaining ReadyToRun, NativeAOT, and final `MethodImplOptions` work is
runtime-mode tuning rather than missing row behavior.

The leading public shape is:

```csharp
foreach (var row in moving.Rows())
{
    EntMut ent = row.Ent;
    row.Position.Value += row.Velocity.Value;
}
```

The leading implementation is an exact generated row value containing:

- one managed base ref for the Ent column;
- one managed base ref for every selected component column; and
- one shared row index.

The query row enumerator binds those bases once when it enters an arch. Each row
property then reduces to `Unsafe.Add(ref base, row)`. The row does not contain
refs to the current elements and does not contain one `Span<T>` per component.

In the current two-component Release prototype, this design matched or slightly
beat the direct span loop after Tier-1 dynamic PGO. With eight selected fields,
it materially beat the direct span loop because it used one row-count check
instead of retaining a bounds check for every independent span.

This is a research decision, not yet a production implementation. Remaining
work is concentrated in:

- generated API and type emission;
- cold arch discovery and column binding;
- unprofiled, ReadyToRun, and NativeAOT behavior;
- reference-containing and varied-size components;
- fragmented queries with many small matching archs; and
- a permanent short Release benchmark and disassembly gate.

This report is the detailed evidence for the `ECS-API-07` row-iteration item in
[ECS.PublicApiPlan.md](ECS.PublicApiPlan.md). The broader query contract remains
described there. Existing archetypal behavior is described in
[ECS.Archetypal.Features.md](ECS.Archetypal.Features.md).

## Scope

This design covers a flattened per-Ent view of one archetypal query over one
archetype group `A`.

It covers:

- required multi-component selection within one `A`;
- aligned Ent and component access;
- writable component refs;
- allocation-free enumeration;
- generated exact properties;
- the within-arch hot loop;
- the transition between matching archs; and
- the JIT behavior required to make the abstraction disappear.

It does not cover:

- lifecycle interaction outside the archetypal package;
- cross-group joins;
- sparse/archetypal joins;
- structural mutation while a query view is live;
- debug-view integration;
- optional fields in the first implementation; or
- general MethodImpl tuning, which remains an end-of-work tuning task.

The span query remains the primary bulk and SIMD-oriented interface. The row
iterator is a second view over the same aligned columns for algorithms that are
more naturally expressed one Ent at a time.

## Required contracts

### Thread ownership

The row iterator follows the archetypal threading model:

- one thread exclusively owns reads, writes, queries, and structural changes
  for one `(alloc, A)` at a time;
- two threads may concurrently operate on the same `A` when they use different
  allocs; and
- the row hot path performs no locking, volatile access, or ownership checks.

### Structural stability

While a query, chunk, row enumerator, row value, component ref, or component
span is live for `(alloc, A)`, code must not perform a structural operation that
can move or remove rows in that same `(alloc, A)`.

Forbidden operations include:

- adding or removing an archetypal component in `A`;
- clearing an Ent in a way that removes its row from `A`;
- disposing an Ent whose row belongs to `A`; and
- any operation that can grow, replace, compact, rent, or return the relevant
  row or component arrays.

Writing existing component values through returned refs is allowed. The rule is
about row and storage stability, not component immutability.

The runtime does not need a lock or counter in the row hot path to enforce this
contract. It is an API usage rule, consistent with the rest of the low-level ECS.

### Alignment invariant

For one active `(alloc, A, arch)`:

- the Ent array and all component arrays have the same active row count;
- row `i` in every selected column belongs to the same Ent;
- a required selected column exists for every active row; and
- the arrays remain stable for the lifetime of the view.

The generated row implementation intentionally trusts this invariant. It does
not repeat a bounds or presence check in every accessor.

## Public API target

### Direct component access

```csharp
var moving = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity();

foreach (var row in moving.Rows())
{
    EntMut ent = row.Ent;
    row.Position.Value += row.Velocity.Value;
}
```

Every selected component is exposed as a writable ref property:

- `Position` returns `ref Position`;
- `Velocity` returns `ref Velocity`; and
- `Ent` returns the aligned `EntMut` handle by value.

Reading a property does not copy unless the caller asks for a value. Assigning
or mutating through it updates the column slot directly. Local ref aliases are
optional and should be reserved for repeated access or ref forwarding. The
exact property names are generated from the component declaration names and
avoid a generic `Get<T,N>()` lookup inside the row loop.

### Writable property and direct-use verification

The generated property getter retains `AggressiveInlining`. A focused .NET 10
Release comparison found that a writable ref property and the former ref-return
methods produced byte-for-byte identical machine code: 37 bytes for the read
loop and 32 bytes for the write loop, with no allocation.

Direct property use was then compared with introducing local ref aliases for a
two-component update:

```csharp
// Local aliases.
ref Position position = ref row.Position;
ref readonly Velocity velocity = ref row.Velocity;
position.X += velocity.X;
position.Y += velocity.Y;

// Direct properties.
row.Position.X += row.Velocity.X;
row.Position.Y += row.Velocity.Y;
```

The direct form emitted a 62-byte loop instead of 64 bytes. It retained scaled
indexed addressing while the local-alias form materialized current addresses
with an extra shift and add. An alternating same-process measurement over
16,384 rows reported 0.2994 ns/row direct and 0.3644 ns/row with local aliases.
The exact difference remains runtime- and body-dependent, but the result makes
direct property use the preferred normal form. Local refs remain available when
an algorithm genuinely benefits from retaining or forwarding an alias.

### Repeated enumeration

The query value is reusable when the arena and alloc remain valid:

```csharp
var moving = arena.QueryArchetypal<MotionComponents>()
    .WithPosition()
    .WithVelocity();

foreach (var row in moving.Rows())
    Update(row);

foreach (var row in moving.Rows())
    Render(row);
```

Each `Rows()` call creates a new stack-only enumerator value. It does not allocate
a managed iterator object.

### Ent access only when needed

The Ent base is part of the row shape so retrieving `row.Ent` is direct and
aligned. The JIT may remove the Ent base from the active inner-loop state when a
particular loop never accesses `row.Ent`.

```csharp
foreach (var row in moving.Rows())
    row.Position.Value += row.Velocity.Value;
```

### No tuple-ref form

This syntax can exist only with copied component values:

```csharp
foreach (var (ent, position, velocity) in moving.Rows())
{
}
```

C# deconstruction uses value outputs. It cannot bind the deconstructed
`position` variable as `ref Position` or `velocity` as `ref readonly Velocity`.
The desired writable form is not expressible as:

```csharp
// Not valid C#.
foreach (var (ent, ref position, ref readonly velocity) in moving.Rows())
{
}
```

Ref-wrapper values could make a tuple-like form compile, but callers would need
`position.Value`, assignment could replace the wrapper instead of the component,
and the representation would preserve much of the row-proxy complexity. It is
not the leading API.

## How C# lowers the loop

A pattern-based `foreach` does not require `IEnumerable<T>`, interface dispatch,
boxing, or an allocated iterator. Conceptually, C# lowers:

```csharp
foreach (var row in query.Rows())
{
    Use(row);
}
```

to:

```csharp
var rows = query.Rows().GetEnumerator();

while (rows.MoveNext())
{
    var row = rows.Current;
    Use(row);
}
```

The exact language rules are documented in the
[C# `foreach` specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/statements#1395-the-foreach-statement).

Therefore `foreach` is not inherently slower than a handwritten `while` loop.
Performance depends on whether the JIT can:

- inline `GetEnumerator`, `MoveNext`, `Current`, and the row property getters;
- promote the row and enumerator structs into independent values;
- keep hot state in registers;
- isolate the rare arch-transition path; and
- fold `base + row * sizeof(T)` into normal indexed memory operands.

## Addressing mathematics

For a contiguous column of `T`, the address of row `r` is:

```text
address(T, r) = base(T) + r × sizeof(T)
```

For two columns:

```text
position(r) = positionBase + r × sizeof(Position)
velocity(r) = velocityBase + r × sizeof(Velocity)
```

The archetypal alignment invariant means the same `r` selects the matching Ent,
position, velocity, and every other selected component.

The key design question is when these addresses are formed.

### Rejected timing: form every current address in `Current`

```csharp
public PairRow Current => new(
    ref Unsafe.Add(ref positionBase, row),
    ref Unsafe.Add(ref velocityBase, row));
```

Here `Current` turns each base into a current-element ref before the loop body
uses the row.

### Accepted timing: carry bases and form an address at the access

```csharp
public PairRow Current => new(
    ref positionBase,
    ref velocityBase,
    row);
```

```csharp
public ref Position Position
{
    get => ref Unsafe.Add(ref positionBase, row);
}
```

After inlining and struct promotion, the accepted form gives the JIT the base,
index, and element type at the final load or store. The x64 address mode can then
encode the scale directly for common element sizes.

For non-power-of-two component sizes, both span and row paths may require
additional address arithmetic. The row design still avoids per-access lookup and
per-component length checks; varied component sizes remain part of the permanent
benchmark matrix.

## Why the first natural row prototype was slower

### Original row representation

The first natural `foreach` prototype returned a row containing refs to the
current component elements:

```csharp
internal readonly ref struct PairRow
{
    private readonly ref int position;
    private readonly ref int velocity;

    internal PairRow(ref int position, ref int velocity)
    {
        this.position = ref position;
        this.velocity = ref velocity;
    }

    internal ref int Position => ref position;
    internal ref int Velocity => ref velocity;
}
```

```csharp
public PairRow Current => new(
    ref Unsafe.Add(ref positionBase, row),
    ref Unsafe.Add(ref velocityBase, row));
```

The row did not allocate, and Tier-1 eliminated the explicit row struct. It was
still slower because it retained address-materialization work.

The relevant old hot-loop shape was approximately:

```asm
movsxd  rax, row
shl     rax, 2
lea     rcx, [positionBase + rax]
add     rax, velocityBase
mov     ecx, [rcx]
add     ecx, [rax]
```

The JIT first created the two current-element addresses, then loaded through
them. The explicit shift, address formation, and dependency chain remained even
though the row value itself disappeared.

### Revised row representation

The revised row stores bases and one index:

```csharp
internal readonly ref struct PairRow
{
    private readonly ref int positionBase;
    private readonly ref int velocityBase;
    private readonly int row;

    internal PairRow(ref int positionBase, ref int velocityBase, int row)
    {
        this.positionBase = ref positionBase;
        this.velocityBase = ref velocityBase;
        this.row = row;
    }

    internal ref int Position => ref Unsafe.Add(ref positionBase, row);

    internal ref int Velocity => ref Unsafe.Add(ref velocityBase, row);
}
```

The relevant revised hot loop was:

```asm
movsxd  rax, row
mov     ecx, [positionBase + 4*rax]
add     ecx, [velocityBase + 4*rax]
```

There was no row construction, accessor call, generic lookup, hash lookup,
bounds check, or intermediate current-element ref in the final inner loop.

## Proposed generated implementation

The following code is illustrative. Names and visibility may change during the
generator implementation, but the state and hot-loop shape are intentional.

### Exact row value

```csharp
public readonly ref struct MotionRow
{
    private readonly ref EntMut ents;
    private readonly ref Position positions;
    private readonly ref Velocity velocities;
    private readonly int row;

    internal MotionRow(
        ref EntMut ents,
        ref Position positions,
        ref Velocity velocities,
        int row)
    {
        this.ents = ref ents;
        this.positions = ref positions;
        this.velocities = ref velocities;
        this.row = row;
    }

    public EntMut Ent => Unsafe.Add(ref ents, row);

    public ref Position Position => ref Unsafe.Add(ref positions, row);

    public ref Velocity Velocity => ref Unsafe.Add(ref velocities, row);
}
```

The prototype compiled safe base-ref accessors without using
`UnscopedRefAttribute`. The stored refs point directly to the existing arrays;
the row does not return a ref to its own stack storage.

### Exact flattened enumerator

```csharp
public ref struct MotionRows<S>
    where S : struct, IEntArchSelect<MotionComponents>
{
    private EntArchQuery<MotionComponents, S>.Enumerator archs;
    private ref EntMut ents;
    private ref Position positions;
    private ref Velocity velocities;
    private int row;
    private int count;

    internal MotionRows(EntArchQuery<MotionComponents, S> query)
    {
        archs = query.GetEnumerator();
        ents = ref Unsafe.NullRef<EntMut>();
        positions = ref Unsafe.NullRef<Position>();
        velocities = ref Unsafe.NullRef<Velocity>();
        row = -1;
        count = 0;
    }

    public MotionRows<S> GetEnumerator() => this;

    public readonly MotionRow Current
        => new(ref ents, ref positions, ref velocities, row);

    public bool MoveNext()
    {
        int next = row + 1;
        if ((uint)next < (uint)count)
        {
            row = next;
            return true;
        }

        return MoveNextArch();
    }

    private bool MoveNextArch()
    {
        while (archs.MoveNext())
        {
            var chunk = archs.Current;
            ReadOnlySpan<EntMut> nextEnts = chunk.Ents;
            if (nextEnts.IsEmpty)
                continue;

            ents = ref MemoryMarshal.GetReference(nextEnts);
            positions = ref MemoryMarshal.GetReference(
                chunk.Get<Position, PositionName>());
            velocities = ref MemoryMarshal.GetReference(
                chunk.Get<Velocity, VelocityName>());
            count = nextEnts.Length;
            row = 0;
            return true;
        }

        return false;
    }
}
```

The production query enumerator walks a static generic cache of matching arch
IDs and performs only the alloc-local active-row check at each candidate. Cache
refresh scans newly materialized signatures outside the row loop. Some
illustrative checks above may therefore be unnecessary. The final
implementation must preserve the package's tightly controlled invariants and
avoid defending against impossible states on the hot or transition paths.

### Hot and cold work

Within an arch, each iteration performs:

1. increment the shared row index;
2. compare it to the shared active count;
3. load or store requested values with base-plus-index addressing; and
4. execute the caller's loop body.

When an arch ends, the enumerator performs cold transition work:

1. scan for the next active matching arch;
2. bind the aligned Ent and component columns;
3. cache their managed base refs;
4. cache the shared active count; and
5. resume at row zero.

No `Get<T,N>()`, selection recursion, presence test, arch scan, or column lookup
belongs in the within-arch row body.

## Release research method

The research harness is local and ignored by Git:

```text
out/ecs-row-research/EcsRowResearch.csproj
out/ecs-row-research/Program.cs
```

The measured setup used:

- `net10.0`;
- Release configuration only;
- 16,384 Ents;
- 128 passes per short sample;
- the median of five short samples;
- zero loop allocations verified with
  `GC.GetAllocatedBytesForCurrentThread()`;
- the real archetypal query and column storage;
- two `int` columns for the initial comparison; and
- eight distinct `int` columns for the wider comparison.

The harness is intentionally short so design iteration remains fast. It is not
a substitute for the permanent benchmark suite.

## Release results

### Default .NET 10 tiered compilation and dynamic PGO

Representative stable steady-state results were:

| Path | Approximate ns/Ent | Relative to spans | Loop allocation |
|---|---:|---:|---:|
| Two direct spans | 0.27-0.29 | 1.00x | 0 B |
| Current-element-ref row | 0.36-0.38 | 1.29x-1.32x | 0 B |
| Base-ref-plus-row `foreach` | 0.27-0.28 | 0.96x-0.98x | 0 B |
| Exact `while (MoveNext())` cursor | 0.27-0.28 | 0.96x-0.99x | 0 B |
| Eight direct spans | 1.09-1.13 | 1.00x | 0 B |
| Eight base-ref-plus-row fields | 0.63-0.74 | 0.57x-0.67x | 0 B |

The exact values move with tier transition timing and code alignment at these
sub-nanosecond scales. The controlling result is the generated inner-loop
assembly: the revised natural `foreach` and exact cursor reduce to the same
base-plus-scaled-index operations.

### Why eight selected fields can beat spans

The direct eight-span loop retained a separate bounds check for each span. The
JIT does not know from the types alone that all eight independent spans have the
same active length.

Its inner loop contained the equivalent of:

```text
check row against length0
load field0[row]
check row against length1
load field1[row]
...
check row against length7
load field7[row]
```

The generated row enumerator knows the ECS invariant:

```text
all selected columns have count rows
```

It performs one shared row/count check in `MoveNext` and then uses unchecked
typed `Unsafe.Add` access. Its inner body becomes eight direct loads using one
index.

The row design is therefore not merely a convenience wrapper. For sufficiently
wide scalar queries, it can be a faster scalar iteration interface than
independently indexing several spans.

### Writable-ref result

The prototype also exercised:

```csharp
foreach (var row in query.Rows())
{
    row.Position += row.Velocity;
    sum += row.Position;
    row.Position -= row.Velocity;
}
```

The Tier-1 assembly contained direct indexed loads and stores with no row,
accessor, or lookup calls. Short timing was more sensitive to tier transition
than the read-only sum, so the current report does not use one writable timing
number as an acceptance threshold. The permanent benchmark must isolate real
writes separately.

### Dynamic PGO disabled

With tiered compilation active but dynamic PGO disabled, representative results
were:

| Path | Approximate ns/Ent | Relative to spans |
|---|---:|---:|
| Two direct spans | 0.293 | 1.00x |
| Current-element-ref row | 0.560 | 1.91x |
| Base-ref-plus-row `foreach` | 0.453 | 1.54x |
| Exact `while (MoveNext())` cursor | 0.455 | 1.55x |
| Eight direct spans | 1.19 | 1.00x |
| Eight base-ref-plus-row fields | 1.20 | 1.01x |

The revised representation remains better than the original row, and the wide
query remains competitive, but the flattened two-column state machine does not
match the span loop without profile information.

This is the main unresolved performance item. Dynamic PGO is part of normal
modern .NET tiered execution and optimizes hot paths based on observed branch
behavior. The relevant runtime settings are documented in
[Compilation config settings](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/compilation).

### Tiered compilation disabled

Disabling tiered compilation entirely produced stable but substantially worse
flat-query code in this prototype. The no-tier result is diagnostic rather than
the expected game runtime configuration. It shows that the current flattened
state machine benefits materially from Tier-1 profile information and that AOT
behavior cannot be assumed from the default JIT result.

The permanent matrix must test:

- default tiered JIT and dynamic PGO;
- tiered JIT with PGO disabled;
- ReadyToRun startup and re-JIT steady state;
- NativeAOT without profile data; and
- NativeAOT or ReadyToRun with representative static profile data, if used by
  shipping applications.

## JIT observations

### Struct promotion makes the row disappear

The JIT's physical struct promotion, also called scalar replacement of
aggregates, can split struct fields into independent values. Microsoft describes
this optimization and its effect on struct enumerators in
[the .NET 8 physical-promotion announcement](https://devblogs.microsoft.com/dotnet/announcing-net-8-preview-7/).

In the revised Tier-1 loop:

- `Current` was inlined;
- the row value was not copied to stack memory;
- row property getters were inlined;
- base refs and the row index were tracked independently; and
- final loads used normal indexed address operands.

The row must therefore remain a small, exact, transparent value. Adding virtual
dispatch, generic lookup state, wrapper layers, optional dictionaries, or
opaque helper calls would make promotion less reliable.

### The design resembles the useful part of `Span<T>`

The runtime implementation of `Span<T>` stores a managed base ref and a length,
then uses `Unsafe.Add` after its bounds check. See the current
[`Span<T>` source](https://source.dot.net/System.Private.CoreLib/src/runtime/src/libraries/System.Private.CoreLib/src/System/Span.cs.html).

The row design applies the same base-ref addressing principle across several
aligned columns, but centralizes the count in the enumerator instead of carrying
one length per component in every row.

### Dynamic PGO recognizes the dominant branch

The flattened `MoveNext` contains two paths:

```csharp
if ((uint)next < (uint)count)
{
    row = next;
    return true;
}

return MoveNextArch();
```

For a large arch, the first path runs once per row and the second path runs once
per arch. Dynamic PGO observes that distribution and optimizes the common path.

This is desirable steady-state behavior, but the design should not hide the
unprofiled cost. The production implementation should attempt to make the cold
transition path less disruptive without causing the hot state to spill.

### Blindly preventing transition inlining was slower

An earlier cursor variant marked the arch-transition helper as non-inlineable
while passing the cursor state through the call. That forced hot cursor fields
to stack locations. The within-arch loop repeatedly loaded and stored row and
base state, making the cursor roughly 1.6x to 2x slower depending on the JIT mode.

Therefore the production implementation must not simply add
`MethodImplOptions.NoInlining` to `MoveNextArch` and assume the rare call is free.
The disassembly, not the source-level appearance, decides whether the split
worked.

### Wide-query code size

The Tier-1 two-field row method emitted approximately 765 bytes in the research
shape. The eight-field row method emitted approximately 2,489 bytes. The
corresponding eight-span method emitted approximately 2,210 bytes.

The eight-field row's inner loop remained compact. Most of the additional code
was arch scanning, selection matching, column-array lookup, and binding for eight
columns.

This is a cold-path and instruction-footprint concern, not evidence of per-row
work. It motivates a generated cold binding boundary, but that boundary must not
spill the hot row state.

## Cold binding design direction

### Problem with returning ref-containing binding state

A research attempt moved arch discovery into a non-inlineable helper and tried
to return a stack-only binding record containing component refs. C# correctly
rejected assigning refs from that returned local record into longer-lived
enumerator fields because the returned record had a narrower escape scope.

Similarly, returning an `EntArchChunk<A>` from a cold helper and then retaining
refs derived from its spans narrows the refs to the local chunk's scope under the
current public signatures.

The implementation must not apply `UnscopedRefAttribute` merely to silence that
diagnostic. The attribute is appropriate only when the API can prove that the
returned ref points to storage whose lifetime is independent of the scoped
receiver. Misusing it would weaken compiler-enforced lifetime safety.

### Promising internal boundary

A production-only cold helper can return an ordinary generated binding value
containing the already-owned arrays and the active count:

```csharp
internal readonly struct MotionArrays
{
    internal readonly EntMut[] Ents;
    internal readonly Position[] Positions;
    internal readonly Velocity[] Velocities;
    internal readonly int Count;
}
```

The caller can then bind managed base refs:

```csharp
MotionArrays arrays = FindNextArch(...);

ents = ref MemoryMarshal.GetArrayDataReference(arrays.Ents);
positions = ref MemoryMarshal.GetArrayDataReference(arrays.Positions);
velocities = ref MemoryMarshal.GetArrayDataReference(arrays.Velocities);
count = arrays.Count;
row = 0;
```

This creates no new arrays and no managed object. `MotionArrays` is a returned
value containing references to existing pooled arrays.

This direction needs a production prototype because:

- call-site generated code may live in a consuming assembly and cannot directly
  access current internal column storage;
- the low-level bridge must not expose arch IDs or arrays as normal public API;
- the helper boundary must keep the within-arch fields promoted; and
- a returned value with many array refs may itself affect the cold ABI and code
  size.

Possible homes for the bridge include generated code that composes an existing
library-owned binding primitive or a deliberately hidden low-level public
surface marked out of normal IntelliSense. No choice is accepted until Release
disassembly proves the hot state remains direct.

## Generator strategy

### Exact specialization is required for the primary path

The fastest row has one concrete field and one concrete ref property for every
selected component. The generator should know the complete closed query shape:

```text
A
required component names and types
property visibility for each selected component
whether aligned Ent access is exposed
```

It can then emit:

- one exact row type;
- one exact row enumerator;
- named component properties;
- exact base-ref fields;
- exact cold binding code; and
- no marker comparisons or recursive component lookup in the row body.

### Demand-driven generation

Generating every component subset would recreate the power-set problem that the
archetypal storage work is designed to avoid. If a group declares `n`
components, generating all query rows would require up to:

```text
2^n - 1
```

nonempty selections before considering ordering.

The generator should instead emit only query combinations actually used in
source, or only explicitly declared named query combinations.

A demand-driven call-site approach can inspect a `Rows()` invocation whose
receiver has a closed nested `EntArchQuery<A,S>` type. The generator can dedupe
that selection and emit an internal extension overload and exact cursor in the
consuming assembly.

The implemented pass resolves already-bound closed query receiver types
semantically. For component groups generated in the same compilation, where the
new symbols are not yet available to the generator's input semantic model, it
also follows direct and local-variable query chains syntactically back to
`QueryArchetypal<A>()`. This supports inline queries, stored local descriptors,
named `WithProperty()` calls, and raw `With<T, N>()` calls. Low-level handwritten
marker types must be visible from namespace-level generated code; ordinary
generated component markers already satisfy that requirement.

This requires a feasibility spike for:

- receiver type discovery through locals and fluent expressions;
- canonicalization of different `With` ordering;
- deduplication across files and projects;
- overload selection so the exact generated `Rows()` wins;
- incremental-generator stability;
- open generic helper methods; and
- generated visibility across assemblies.

### Open generic fallback

An open generic helper may not expose a closed query shape to the generator:

```csharp
static void Update<S>(EntArchQuery<MotionComponents, S> query)
    where S : struct, IEntArchSelect<MotionComponents>
{
}
```

Options are:

1. require a closed generated row API at the caller;
2. retain a generic recursive binding cursor as a fallback;
3. expose only chunk spans for open generic code; or
4. require explicit query-shape declaration.

The generic recursive prototype was close but not equal to the exact row. It was
typically around 8% to 11% slower in the original controlled two- and
eight-binding experiments. It is a viable fallback, not the primary hot path.

The current implementation does not generate a generic row fallback for an open
selection. Open generic query helpers continue to use chunk spans. A closed
caller may still use `Rows()` after the helper boundary when its exact receiver
type is visible.

### Canonicalization

These should identify one generated row shape:

```csharp
query.WithPosition().WithVelocity()
query.WithVelocity().WithPosition()
```

The generator needs a stable canonical component order independent of fluent
call order. The order may follow generated field IDs or another stable group
declaration order.

The current implementation accepts both orders and emits an exact shape for
each used closed receiver type. It does not emit unused combinations, but two
used orderings of the same component set currently produce two row shapes.
Sharing the row representation while retaining distinct receiver overloads is a
generated-code-footprint todo; it is not a row-loop correctness or speed issue.

Read/write intent is not part of the row shape. Selecting a component exposes
one writable ref property named after that component. This avoids duplicate
read/write members and lets equivalent selections share the same row shape once
selection-order canonicalization is implemented.

## Rejected and secondary designs

### Current-element-ref row

Shape:

```csharp
Current => new(
    ref Unsafe.Add(ref positionBase, row),
    ref Unsafe.Add(ref velocityBase, row));
```

Result:

- zero allocations;
- safe refs;
- natural `foreach`; but
- approximately 29% to 32% slower than spans in the two-field Tier-1 test.

Reason for rejection:

- materialized current addresses survive optimization;
- address arithmetic cannot be folded as cleanly into final loads; and
- the penalty appears even in a manual `for` loop that constructs the same row
  wrapper, proving it is not primarily the query scanner.

### Row containing one `Span<T>` per component

Shape:

```csharp
internal readonly ref struct MotionRow
{
    private readonly Span<Position> positions;
    private readonly Span<Velocity> velocities;
    private readonly int row;
}
```

Reason for rejection as the primary representation:

- every span carries a base ref and a length;
- row state grows by roughly two machine words per selected component;
- independent bounds checks may remain;
- the ECS already has one shared count invariant; and
- the prototype showed no stable advantage over direct spans and was sensitive
  to tiering and code layout.

Base refs plus one shared row/count pair express the actual invariant more
directly.

### Pointer-increment rows

Shape:

```csharp
position = ref Unsafe.Add(ref position, 1);
velocity = ref Unsafe.Add(ref velocity, 1);
remaining--;
```

Measured result:

- approximately 1.97x to 2.01x the span time in the two-field prototype.

Reason for rejection:

- every iteration mutates several managed refs;
- the JIT retains more loop-carried ref state;
- ref reassignment creates longer dependency chains; and
- one integer row index is cheaper and easier to optimize.

### By-ref `foreach` over a stored current row

Shape:

```csharp
foreach (ref readonly var row in query.Rows())
{
}
```

where the enumerator stores a `current` row field and updates it on every
`MoveNext`.

Measured result:

- approximately 2x the span time in the safe stored-current prototype.

Reason for rejection:

- the current row field must be rewritten each iteration;
- taking a ref to enumerator state makes stack placement more likely; and
- the syntax is less natural while being slower.

### Self-aliasing `ref readonly foreach`

A prototype returned `ref readonly this` from `Current`, making the loop variable
an alias of the enumerator itself. In favorable Tier-1 output it could match the
span loop.

Reason for rejection:

- it required `UnscopedRefAttribute` to let a ref to mutable enumerator stack
  state escape the property;
- lifetime reasoning becomes fragile;
- a retained row can dangle or observe a later row; and
- the safe base-ref-plus-index row now achieves the same hot-loop result without
  self-aliasing.

### Exact handwritten `while` cursor

Shape:

```csharp
var rows = moving.Rows();
while (rows.MoveNext())
{
    rows.Position.X++;
}
```

Measured result:

- approximately equal to spans after Tier-1 PGO.

Status:

- technically sound;
- useful as a benchmark oracle or lower-level fallback; but
- no longer required as the primary public API because the revised natural
  `foreach` reaches the same inner loop.

### Nested chunk and row-index iteration

Shape:

```csharp
foreach (var chunk in moving)
{
    Span<Position> positions = chunk.Get<Position, PositionName>();
    Span<Velocity> velocities = chunk.Get<Velocity, VelocityName>();

    foreach (int row in chunk.Rows)
        positions[row].Value += velocities[row].Value;
}
```

Measured result:

- approximately equal to the direct `for` span loop.

Status:

- valid secondary API;
- predictable without a flattened cross-arch state machine;
- useful for explicit chunk-aware algorithms; but
- does not satisfy the desired flat per-Ent syntax.

### Generic recursive binding state

Shape:

```text
Binding<T7,N7,Binding<T6,N6,...Binding<T0,N0>>>
```

Each node stores one base ref and delegates a generic marker lookup to the
previous node. The JIT can fold closed marker comparisons and inline the chain.

Measured result:

- roughly 8% to 11% slower than the exact two-field cursor in the original
  controlled runs;
- zero allocations; and
- no surviving recursive calls in the inner assembly when fully optimized.

Status:

- possible open-generic fallback;
- not the primary generated path due to larger state, more code pressure, and
  less consistent optimization.

### Static delegate callback

Shape:

```csharp
query.ForEach(AddPair);
```

Measured result:

- approximately 7.8x to 8x the span loop in the prototype.

Reason for rejection:

- the call remained in the row body;
- a static cached delegate avoids allocation but not the indirect call; and
- it prevents the caller's body from optimizing with component access.

### Generic struct action callback

Shape:

```csharp
query.Run(ref action);
```

where `action.Execute(ref position, in velocity)` is intended to inline.

Result:

- highly sensitive to tiering, inlining, and generic shape in the quick
  prototype;
- materially slower in the tested natural shape; and
- different from the requested `foreach` API.

Status:

- may remain useful for generated system execution or AOT-specific work;
- not selected as the public per-Ent iterator.

### Tuple or deconstruction wrappers

Reason for rejection:

- ordinary tuple elements are values;
- C# deconstruction outputs copies;
- `ValueTuple` cannot carry direct managed refs as normal elements;
- ref wrappers change assignment semantics and require `.Value`; and
- they do not improve the row-state machine.

### Interlaced component storage

Shape:

```text
[Ent, Position, Velocity]
[Ent, Position, Velocity]
...
```

Reason for rejection:

- damages the existing structure-of-arrays span path;
- makes single-component and SIMD iteration worse;
- complicates adding or removing columns;
- changes storage behavior far beyond the iterator API; and
- is unnecessary because the base-ref row works over current columns.

### Native pointer table

A row could hold a pointer to a table of unmanaged column bases plus a row
index.

Reason for rejection as the universal design:

- reference-containing component arrays require GC-tracked managed refs;
- a native pointer table cannot safely represent them;
- the table introduces another indirection;
- it complicates lifetime and movement; and
- exact managed base refs already compile to direct addressing.

An unmanaged-only specialization may still be researched later, but it is not
needed to make the general row API fast.

### Treat every unmanaged column as bytes

Casting reference-free columns to bytes can simplify some cold move/copy
operations. It does not by itself improve typed per-row access:

- the caller still needs a typed ref;
- element-size arithmetic remains;
- iteration should retain type-aware alias and alignment information; and
- reference-containing types still need a separate path.

This remains a separate storage/movement backlog item, not the row-iterator
design.

### Source rewriting, interceptors, and IL weaving

A compiler rewrite could transform the natural flat loop into nested chunk and
span loops.

Reason for rejection for the initial API:

- ordinary source generators add code but do not rewrite existing method bodies;
- C# interceptors remain experimental;
- IL weaving complicates debugging, tooling, NativeAOT, and build transparency;
- the safe generated row already reaches the desired Tier-1 loop; and
- a runtime/library solution is easier to understand and maintain.

## Optional components

The initial row implementation should support required fields only. A required
field exists for every matching arch and every active row.

A later optional-field design can bind presence once per arch:

```csharp
foreach (var row in query.Rows())
{
    if (row.HasAcceleration)
        row.Acceleration = default;
}
```

The optional presence test must be arch-stable and must not perform a component
lookup per row. Possible generated state is:

```text
hasAcceleration
accelerationBase
```

The property contract when `HasAcceleration` is false must be explicit. The
first implementation should not add nullable-ref wrappers or defensive hot-path
checks before the required-field path is complete and measured.

## Cross-group access

A row iterator targets exactly one archetype group `A`. It can select many
components from that group, but row indices are meaningful only within that
group's own dense storage.

Components from another group can have different membership, arch shape, and
row order. They cannot be appended to the same row merely because they belong
to the same Ent.

A future cross-group operation must be explicit, with one driving group and an
intentional point lookup or join for the other group. It is not part of the
initial row iterator and must not add hidden per-row joins to this hot path.

## Memory and allocation footprint

### Per enumeration

The enumerator is a stack-only `ref struct`. It contains:

- the arch query enumerator;
- one managed base ref per selected column;
- one Ent base ref;
- one row index; and
- one active count.

It allocates no managed object.

### Per row

At the C# level, `Current` returns a small row value with the same selected bases
and the row index. In optimized Tier-1 code, physical promotion removes that
materialized value from the inner loop.

There is no heap allocation per row and no pooled buffer per row iterator.

### Generated code footprint

Exact row types increase generated IL and native code for each used query shape.
Demand-driven generation is therefore important for both compile-time and
instruction footprint.

The implementation should track:

- number of generated row shapes;
- generated source bytes;
- Tier-1 native bytes per shape;
- duplicate shapes eliminated by canonicalization; and
- cold binding bytes versus inner-loop bytes.

## Safety properties

### Reference-containing components

Managed base refs remain GC-tracked. A `ref T` into `T[]` works whether `T` is
unmanaged or contains managed references. The row representation does not cast
reference-containing storage to native pointers or bytes.

Writing a managed reference still requires the normal GC write barrier. The row
API must preserve it. Direct typed `ref T` assignment does so.

### Row values cannot escape to the heap

The generated row is a `readonly ref struct`. It cannot be boxed, stored in a
normal heap object, captured by a closure, or used as an array element. These
language restrictions support the storage-lifetime contract.

### No ref to mutable enumerator internals

The accepted row stores refs to component-array elements at row zero, not a ref
to the enumerator or to an enumerator field. A row property therefore does not
depend on the stack address of the enumerator object.

### Dirty pooled data

The storage system intentionally may retain dirty data for reference-free arrays
when no references are present. The row iterator exposes only active rows and
does not read capacity beyond the active count. It does not add clearing work.

## Implementation plan

### Phase 1: exact two-field production spike (completed)

1. Generate one exact row and row enumerator for a known two-field query.
2. Keep public query and span APIs unchanged.
3. Bind Ent and component bases once per arch.
4. Use one shared row/count pair.
5. Return named writable ref properties.
6. Add no runtime lock, volatile operation, ownership counter, or defensive
   impossible-state check.
7. Verify the exact public syntax compiles in normal game code.

Acceptance:

- zero loop allocation;
- writes persist to the archetypal column;
- Ent and components remain aligned;
- no component lookup in the within-arch assembly; and
- default Tier-1 time is within 5% of the direct span baseline.

### Phase 2: cold binding boundary (implemented baseline; further tuning deferred)

The production implementation retained the existing
`EntArchQuery<A,S>.Enumerator` and `EntArchChunk<A>` as the cold boundary. The
generated `MoveNextArch()` obtains each selected span and assigns its managed
base ref directly into the row enumerator. This keeps the ref lifetime visible
to the compiler and introduces no new arrays, holders, runtime objects, or
public storage API.

The Tier-1 hot bases and row remain promoted, the within-arch loop contains no
stack reload cycle for the row abstraction, and enumeration allocates nothing.
Separating or shrinking the comparatively large arch-transition code remains a
code-size tuning task only if wider-query measurements justify it.

### Phase 3: demand-driven generator (completed)

1. Discover closed query shapes used with `Rows()`.
2. Canonicalize component order.
3. Emit one writable ref property per selected component.
4. Dedupe repeated uses.
5. Emit exact row and enumerator types.
6. Provide a defined behavior for open generic query helpers.
7. Add generated-output fixtures.

Acceptance:

- no power-set generation;
- stable deterministic output;
- no public arch or row IDs;
- exact properties chosen without reflection or runtime dictionaries; and
- incremental builds regenerate only affected shapes.

### Phase 4: correctness and lifetime tests (completed for the implemented surface)

Cover:

- empty query;
- one matching arch;
- several matching archs;
- nonmatching active archs between matches;
- Ent access;
- component reads;
- writable component access;
- all selected fields used;
- only the first or last selected field used;
- repeated enumeration;
- different allocs used from different owner threads;
- invalid structural mutation documented rather than checked in the hot path;
- component arrays that grow between separate enumerations; and
- reference-containing components retained and updated correctly.

### Phase 5: permanent short Release benchmark (baseline completed; matrix expansion remains)

The benchmark must remain quick enough for frequent iteration. Use short warmup
and measurement groups, then separate longer confirmation only when a design is
close to acceptance.

Required query widths:

- one field;
- two fields;
- four fields;
- eight fields;
- sixteen fields when the generated type remains practical.

Required access patterns:

- read one field;
- read all fields;
- write one field;
- read one and write one;
- write several fields;
- access Ent;
- do not access Ent; and
- realistic component arithmetic.

Required component layouts:

- 1-, 2-, 4-, and 8-byte scalar values;
- 12-, 16-, 24-, 32-, and 64-byte structs;
- naturally aligned and awkward-size structs;
- structs containing one or several managed references;
- pure reference types if allowed as archetypal components; and
- mixed component sizes in one query.

Required arch distributions:

- one large matching arch;
- several large matching archs;
- many small matching archs;
- many nonmatching archs;
- a sparse set of matching archs at high arch IDs; and
- several allocs sharing the same `A`.

Required runtime modes:

- default Release tiered JIT and dynamic PGO;
- PGO disabled;
- tiered compilation disabled;
- ReadyToRun when applicable;
- NativeAOT when applicable; and
- x64 and Arm64 when hardware is available.

### Phase 6: disassembly gate (completed for the default two-field path)

For the default two-field steady-state path, the accepted inner loop should have:

- no call instruction;
- no virtual or interface dispatch;
- no `Get<T,N>()`;
- no `ValuesAt` call;
- no selection-marker comparison;
- no hash or arch lookup;
- no independent component bounds checks;
- no ref reassignment per selected column;
- one shared row progression and loop condition; and
- direct typed loads/stores using base-plus-index addressing.

For wide queries, limited spills may be unavoidable once live bases exceed the
register set. The comparison must distinguish unavoidable register pressure from
row-abstraction overhead.

### Phase 7: final MethodImpl tuning (deferred)

Only after the representation and cold boundary are accepted, compare:

- no attribute;
- `AggressiveInlining`;
- `AggressiveOptimization`;
- both where legal; and
- explicit no-inline cold helpers.

Every variant must be tested in Release with timing and disassembly. Attributes
must not be used as a substitute for a representation the JIT can understand.

## Acceptance criteria

The per-Ent row iterator is accepted as a primary public API when all of the
following hold:

### API

- The natural `foreach (var row in query.Rows())` syntax works.
- Generated named writable ref properties are clear.
- Ent access is direct and aligned.
- The API exposes no arch ID, row ID, alloc ID, or storage array.
- Required selection supports multiple components.
- Cross-group access remains explicit and separate.

### Correctness

- Reads return the correct aligned values.
- Writes update the correct archetypal columns.
- Compaction and growth between enumerations preserve later correctness.
- Reference-containing components remain GC-safe.
- The documented structural-stability rule is sufficient.

### Performance

- The loop allocates zero bytes.
- Two-field default Tier-1 performance is within 5% of direct spans.
- Wider scalar queries are no slower than spans unless a documented register or
  code-size tradeoff justifies the difference.
- No per-row lookup or bounds-check multiplication survives.
- Point read/set performance outside queries is unchanged.
- Span query performance is unchanged.

### Footprint

- Only used or explicitly declared query shapes generate exact rows.
- No power-set expansion occurs.
- No managed object is created per row, arch, or enumeration.
- Generated native code size is recorded and bounded.

### Runtime modes

- Default JIT behavior is accepted.
- PGO-disabled behavior is understood and documented.
- ReadyToRun and NativeAOT behavior is either accepted or the span/nested-row
  alternative is explicitly recommended for those modes.

## Recommended final shape

The design to carry forward is:

```text
Query<A, selection>
    |
    | Rows()
    v
Exact generated row enumerator
    - arch scan state
    - Ent base ref
    - selected component base refs
    - shared row
    - shared count
    |
    | Current
    v
Exact generated readonly ref row
    - same base refs
    - row
    |
    +-- Ent                  -> Ent base + row
    +-- Position             -> Position base + row
    +-- Velocity             -> Velocity base + row
```

The central rule is:

> Cache column bases once per arch, carry one row index, and form each typed
> component ref only at the final property access.

That rule produced the best safe natural `foreach` result, preserves the
structure-of-arrays span interface, supports reference-containing components,
allocates nothing, and leaves room for a generated exact hot path without
exposing storage internals in the public ECS API.

## References

- [C# `foreach` language specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/statements#1395-the-foreach-statement)
- [C# `ref struct` and ref-field documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct)
- [C# `UnscopedRefAttribute` documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/general#unscopedref-attribute)
- [.NET physical struct promotion](https://devblogs.microsoft.com/dotnet/announcing-net-8-preview-7/)
- [.NET 10 performance improvements](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/)
- [.NET compilation and dynamic PGO settings](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/compilation)
- [Current `Span<T>` source](https://source.dot.net/System.Private.CoreLib/src/runtime/src/libraries/System.Private.CoreLib/src/System/Span.cs.html)
- [RyuJIT design tutorial](https://github.com/dotnet/runtime/blob/main/docs/design/coreclr/jit/ryujit-tutorial.md)
