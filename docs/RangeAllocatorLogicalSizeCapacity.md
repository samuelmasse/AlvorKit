# Range Allocator Logical Size And Capacity Plan

## Context

`RangeAllocator.Alloc(ref handle, alignment, size)` currently has two different
meanings depending on whether the existing handle can satisfy the request.

When a handle is new or must move, the allocator stores the requested `size` in
`RangeAllocation.Size`. When a same-handle request is smaller than the existing
slot, `ReuseExistingOrFree` returns early and leaves the previous larger
`RangeAllocation.Size` in place.

That means the allocator behaves like a capacity-retaining allocator, but it
does not separately remember the latest logical request. A later `Pack()` can
compact holes between live ranges, but it cannot reclaim shrink slack inside a
live range because only the retained larger size remains in allocator state.

The desired behavior is:

- A shrink request should be recorded as the current logical payload size.
- A shrink request should not immediately free the reduced bytes.
- `Pack()` should reclaim retained shrink slack by packing to logical size.

## Proposed Model

Track two payload byte counts for each live allocation:

| Value | Meaning |
| --- | --- |
| Logical size | Latest requested payload size from `Alloc`. |
| Capacity size | Payload capacity currently reserved in the backing store. |

The free map and `Used` continue to describe physical reserved backing-store
memory. Logical size describes the caller-visible request.

`RangeAllocator` reserves backing-store index `0` by starting usable allocation
space at `FirstUsableIndex = 1`. This means `Used` has a permanent baseline of
`1`, even when there are no live allocations. Any total-used invariant must
include that sentinel byte:

```text
Used = 1 + sum(CapacitySize + max alignment padding for each live allocation)
```

After `Pack()` has reclaimed shrink slack, each packed allocation has
`CapacitySize == Size`, so the invariant becomes:

```text
Used = 1 + sum(Size + max alignment padding for each live allocation)
```

## Public Shape

Use `RangeAllocation.Size` for logical size, matching the current public
documentation that describes it as requested payload size.

Add a new field to `RangeAllocation`:

```csharp
public long CapacitySize;
```

`CapacitySize` is the retained payload capacity used to compute the actual
reserved backing-store footprint. The reserved footprint is:

```text
CapacitySize + maximum possible alignment padding
```

For the current allocator policy, maximum possible alignment padding is
`0` when alignment is `0` or `1`, otherwise `Alignment - 1`. Public docs should
spell this out instead of relying on the private `MaxPadding` helper name.

This is a public struct change, so docs and tests should make the new meaning
explicit. Downstream source that used `RangeAllocation.Size` as reserved
capacity must move to `CapacitySize`, and unsafe, binary, or serialized
consumers may observe the public struct layout change. If the name
`CapacitySize` feels too generic, use `ReservedPayloadSize`; the key is that it
must not be confused with the total reserved footprint including alignment
padding.

## Allocation Behavior

### New Handle

For a new non-zero allocation:

- Set `Size = requestedSize`.
- Set `CapacitySize = requestedSize`.
- Reserve `CapacitySize + max alignment padding`.
- Increase `Used` by the same reserved footprint.

### Same Handle, Request Fits Existing Capacity

When the handle is already live, alignment matches, and
`requestedSize <= CapacitySize`:

- Set `Size = requestedSize`.
- Keep `CapacitySize` unchanged.
- Keep `Index` unchanged.
- Do not modify the free map.
- Do not change `Used`.

This records shrink and grow-within-capacity requests without freeing or moving
the backing block.

### Same Handle, Request Exceeds Existing Capacity

When `requestedSize > CapacitySize` or alignment changes:

- Free the old block using its capacity footprint.
- Allocate a new block.
- Set `Size = requestedSize`.
- Set `CapacitySize = requestedSize`.

This preserves the existing replacement behavior for true growth.

### Zero-Byte Request

Keep the current zero-byte behavior:

- Free the handle if it is live.
- Reset the handle to `0`.

Preserve the current validation order: negative alignment or size must throw
before the zero-byte clear path runs. For example,
`Alloc(ref handle, -1, 0)` should not free an existing handle.

## Free Behavior

`Free(handle)` must return the full retained capacity footprint to the free map:

```text
CapacitySize + maximum possible alignment padding
```

It must not use logical `Size`, because shrink slack is still physically
reserved until pack or free. This is especially important when replacing a
previously shrunk allocation: the old block must be returned using
`CapacitySize`, not the smaller logical `Size`.

## Pack Behavior

`Pack()` should compact live ranges using logical size:

1. Iterate live allocations in rank order as today.
2. Capture `LastAllocationSlots` before moving each allocation.
3. Move each allocation to the next packed index.
4. Compute the packed footprint from logical `Size`.
5. Set `CapacitySize = Size`.
6. Advance the packed index by `Size + max alignment padding`.
7. Reset the free map to one tail block after the packed live range.

After pack, retained shrink slack is gone and `Used` reflects the sentinel byte
plus the sum of packed logical footprints.

## Callback And Relocation Semantics

`LastAllocationSlots` should preserve the pre-pack physical allocation state,
including the old `Index`, logical `Size`, and old `CapacitySize`.

`AllocationSlots` after pack should expose the new compacted allocation state:

- New `Index`.
- Same logical `Size`.
- `CapacitySize == Size`.

This lets relocation users see both where a range moved from and how large the
logical live payload now is. If a consumer needs to copy the old retained
capacity during relocation, it can read `LastAllocationSlots[handle].CapacitySize`.
If it should copy only live bytes, it can read `AllocationSlots[handle].Size`.

## Visualizer Updates

The visualizer should stop inferring retained shrink slack only from the latest
scripted command once allocator state can represent it directly.

Display these fields:

- Request/logical bytes: `RangeAllocation.Size`.
- Capacity bytes: `RangeAllocation.CapacitySize`.
- Retained extra bytes: `CapacitySize - Size`.
- Reserved bytes: `CapacitySize + maximum possible alignment padding`.
- Padding bytes: reserved bytes minus capacity bytes.

For the current operation, the visualizer may still highlight the latest request
argument, but the persisted logical size should come from allocator state.

Update the captured visual snapshot shape, not only rendered labels.
`AllocatorRangeVisual` needs separate logical size, capacity size, retained
extra, reserved size, and padding fields so drawn live ranges agree with
allocator `Used`. Add or update a scenario so the visualizer shows
same-handle shrink before and after an explicit `Pack()`.

## Benchmark Updates

Benchmark summary metrics should separate:

- Requested/logical bytes from `RangeAllocation.Size`.
- Capacity bytes from `RangeAllocation.CapacitySize`.
- Retained extra bytes from `CapacitySize - Size`.
- Reserved bytes from capacity plus actual or maximum padding, depending on the
  metric being reported.
- Padding bytes excluding retained payload slack.

Add a shrink-then-pack benchmark case before using benchmark output to measure
the benefit of reclaiming shrink slack.

## Tests

Add focused tests for the new model:

1. Shrinking a same-handle allocation updates `AllocationSlots[handle].Size`.
2. Shrinking does not change `AllocationSlots[handle].CapacitySize`.
3. Shrinking does not reduce `Used`.
4. Growing within retained capacity updates `Size` and does not change `Used`.
5. Growing beyond retained capacity replaces the block and resets
   `CapacitySize` to the new logical size.
6. Alignment changes replace the block and reset `CapacitySize` to the new
   logical size.
7. Free after shrink returns the full retained capacity footprint.
8. Zero-byte allocation after shrink clears the handle and returns the full
   retained capacity footprint.
9. Invalid zero-byte calls such as `Alloc(ref handle, -1, 0)` throw before
   clearing the handle.
10. Replacement after shrink frees the old block using `CapacitySize`, not the
    smaller logical `Size`.
11. Pack after shrink reduces `Used`, including the permanent sentinel-byte
    baseline.
12. Pack after shrink sets `CapacitySize == Size`.
13. Pack after shrink resets the free map to one tail block. Assert this with
    `FreeBlockCount == 1` and a follow-up allocation whose `Index` starts at the
    pre-allocation `Used`.
14. `LastAllocationSlots` exposes pre-pack capacity, and `AllocationSlots`
    exposes post-pack compacted capacity.
15. Alignment padding still matches existing address expectations, including a
    pack-after-shrink case with a misaligned original index.
16. New allocations initialize `CapacitySize == Size`.
17. `SharedVertexBuffer` integration still builds against the new public
    allocation shape.

## Verification

For Working Mode implementation, run focused checks that match the touched
surface:

1. Run focused `AlvorKit.Ranges` unit tests:

   ```powershell
   dotnet test tests\AlvorKit.Ranges.Test\AlvorKit.Ranges.Test.csproj --filter FullyQualifiedName~AlvorKit.Ranges.Test.RangeAllocatorTest
   ```

2. Build `AlvorKit.Engine` to protect `SharedVertexBuffer` integration:

   ```powershell
   dotnet build src\AlvorKit.Engine\AlvorKit.Engine.csproj
   ```

3. Build the visualizer demo when visualizer files change:

   ```powershell
   dotnet build demos\AlvorKit.Ranges.Demo.Visualizer\AlvorKit.Ranges.Demo.Visualizer.csproj
   ```

Commit Mode or explicitly requested verification can add:

- Focused coverage for `AlvorKit.Ranges`.
- Visualizer frame captures for same-handle shrink before and after pack.
- Release range benchmarks after the shrink-then-pack benchmark exists.

## Non-Goals

- Do not free shrink slack immediately.
- Do not change handle identity for same-handle shrink or grow-within-capacity.
- Do not add a wrapper allocator abstraction only to hide the semantic change.
- Do not make `Pack()` optional reclamation behavior ambiguous; pack should be
  the point where retained shrink slack is reclaimed.
