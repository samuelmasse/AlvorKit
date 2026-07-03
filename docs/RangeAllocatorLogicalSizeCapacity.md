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
CapacitySize + MaxPadding(Alignment)
```

This is a public struct change, so docs and tests should make the new meaning
explicit. If the name `CapacitySize` feels too generic, use
`ReservedPayloadSize`; the key is that it must not be confused with the total
reserved footprint including alignment padding.

## Allocation Behavior

### New Handle

For a new non-zero allocation:

- Set `Size = requestedSize`.
- Set `CapacitySize = requestedSize`.
- Reserve `CapacitySize + MaxPadding(alignment)`.
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

## Free Behavior

`Free(handle)` must return the full retained capacity footprint to the free map:

```text
CapacitySize + MaxPadding(Alignment)
```

It must not use logical `Size`, because shrink slack is still physically
reserved until pack or free.

## Pack Behavior

`Pack()` should compact live ranges using logical size:

1. Iterate live allocations in rank order as today.
2. Capture `LastAllocationSlots` before moving each allocation.
3. Move each allocation to the next packed index.
4. Compute the packed footprint from logical `Size`.
5. Set `CapacitySize = Size`.
6. Advance the packed index by `Size + MaxPadding(Alignment)`.
7. Reset the free map to one tail block after the packed live range.

After pack, retained shrink slack is gone and `Used` reflects the sum of packed
logical footprints.

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
- Reserved bytes: `CapacitySize + MaxPadding(Alignment)`.
- Padding bytes: reserved bytes minus capacity bytes.

For the current operation, the visualizer may still highlight the latest request
argument, but the persisted logical size should come from allocator state.

## Tests

Add focused tests for the new model:

1. Shrinking a same-handle allocation updates `AllocationSlots[handle].Size`.
2. Shrinking does not change `AllocationSlots[handle].CapacitySize`.
3. Shrinking does not reduce `Used`.
4. Growing within retained capacity updates `Size` and does not change `Used`.
5. Growing beyond retained capacity replaces the block and resets
   `CapacitySize` to the new logical size.
6. Free after shrink returns the full retained capacity footprint.
7. Pack after shrink reduces `Used`.
8. Pack after shrink sets `CapacitySize == Size`.
9. Pack after shrink resets the free map to one tail block.
10. `LastAllocationSlots` exposes pre-pack capacity, and `AllocationSlots`
    exposes post-pack compacted capacity.
11. Alignment padding still matches existing address expectations.
12. `SharedVertexBuffer` integration still builds against the new public
    allocation shape.

## Verification

For implementation:

1. Run focused `AlvorKit.Ranges` unit tests.
2. Run focused coverage for `AlvorKit.Ranges`.
3. Build the visualizer demo.
4. Capture visualizer frames for same-handle shrink before and after pack.
5. Build `AlvorKit.Engine` to protect `SharedVertexBuffer` integration.
6. Run Release range benchmarks to measure the cost of recording logical size
   and the benefit of pack reclaiming shrink slack.

## Non-Goals

- Do not free shrink slack immediately.
- Do not change handle identity for same-handle shrink or grow-within-capacity.
- Do not add a wrapper allocator abstraction only to hide the semantic change.
- Do not make `Pack()` optional reclamation behavior ambiguous; pack should be
  the point where retained shrink slack is reclaimed.
