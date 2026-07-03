# Range Allocator Optimization Plan

## Context

`AlvorKit.Ranges` owns a logical range allocator extracted from Engine buffer
management. The allocator is currently optimized for correctness and simple
integration with `SharedVertexBuffer`, but the benchmark shows clear room for
hot-path improvement.

Release benchmark baseline from July 3, 2026:

| Benchmark | Best | Mean | Managed allocation |
| --- | ---: | ---: | ---: |
| `alloc-free-reuse` | 13.22M alloc/s | 8.33M alloc/s | 96.0 B/alloc |
| `rolling-window-fragmentation` | 13.21M alloc/s | 12.84M alloc/s | 50.7 B/alloc |
| `pack-fragmented-ranges` | 0.94M alloc/s | 0.94M alloc/s | 267.8 B/alloc |

The first two rows are useful allocation-churn signals. The pack row is an
end-to-end scenario that includes allocation, freeing, packing, address reads,
and cleanup; it should not be interpreted as pure allocation throughput.

## Goals

- Improve allocation and free throughput without weakening allocator
  correctness.
- Reduce managed allocation in allocator hot paths.
- Separate benchmark scenarios so each result describes the work it measures.
- Preserve `SharedVertexBuffer` integration, including relocation data exposed
  through `AllocationSlots`, `LastAllocationSlots`, and `Allocations`.
- Keep runtime code readable and allocation-sensitive; do not introduce vague
  wrapper abstractions only to satisfy source-size targets.

## Current Hot Spots

The main cost is `RangeFreeBlockMap`. It uses two `SortedList<long, ...>`
indexes and `SortedSet<long>` buckets. That gives useful correctness properties,
but each allocation/free can pay for sorted-list array shifts, tree operations,
and `SortedSet` node allocation.

Other likely costs:

- `Alloc` removes a free block and re-adds the remainder on every successful
  allocation.
- `Free` searches both neighbors and then removes/re-adds blocks to coalesce.
- `FindBestFit` and `TakeBestFit` duplicate index lookups.
- Alignment currently reserves `allocSize + alignment`, which can over-reserve
  and increase pack/resize pressure.
- `RangeBoundarySearch` is generic even though the allocator uses `long` keys.
- The handle free-list uses `Queue<int>`, which is simple but not the lightest
  option for hot churn.

## Phase 1: Benchmark Clarity

1. Rename current scenarios to describe their actual work.
   - `alloc-free-reuse` -> `single-range-alloc-free`
   - `rolling-window-fragmentation` -> `steady-window-churn`
   - `pack-fragmented-ranges` -> `fragmented-pack-scenario`

2. Split setup, measured body, and cleanup.
   - Do not include handle-array allocation in steady-state timing.
   - Do not include teardown frees unless the metric says it is an end-to-end
     scenario.

3. Add targeted scenarios.
   - `linear-alloc-no-resize`: pre-sized allocator, allocate N ranges, no frees.
   - `linear-alloc-with-resize`: default allocator, allocate N ranges, report
     resize count/time.
   - `same-handle-reuse-hit`: repeated calls that should hit the
     same-allocation fast path.
   - `same-handle-grow-replace`: repeated growth through one handle.
   - `fragmented-same-size-holes`: isolate same-size free bucket behavior.
   - `fragmented-distinct-size-holes`: isolate sorted size-search pressure.
   - `pack-only-fragmented`: setup outside timing, measure only `Pack()`.
   - `pack-callback-simulated-copy`: approximate `SharedVertexBuffer`
     relocation callback work without requiring GL.

4. Expand benchmark output.
   - Pack count and pack time.
   - Resize count and resize time.
   - Final free block count and distinct free size count.
   - Live range count.
   - Bytes reserved, bytes requested, and estimated padding.

## Phase 2: Low-Risk Allocator Optimizations

1. Add free-map fast paths.
   - Consume directly from a sole free block.
   - Consume directly from the tail block when it is the best fit.
   - Merge back into a sole free block without full neighbor bookkeeping.

2. Combine free-map operations.
   - Replace `FindBestFit` plus `TakeBestFit` with one `TryTakeBestFit`.
   - Remove known blocks by known `(index, size)` during merge instead of
     looking them up again.

3. Specialize boundary search for `long`.
   - The helper does not need generic numeric operators for current use.
   - Keep the public allocator API unchanged.

4. Tighten alignment accounting.
   - Fix exact-alignment behavior if the current policy advances when it should
     not.
   - Reserve actual padding or maximum needed padding deliberately.
   - Update tests because `Used` and address expectations may change.

## Phase 3: Managed Allocation Reduction

1. Replace `SortedSet<long>` buckets.
   - Pooling the set object does not avoid per-node allocation.
   - Consider allocator-owned arrays or nodes for equal-size block indexes.

2. Replace general-purpose `Queue<int>` for free handles only if measured.
   - A simple stack or custom ring buffer may reduce overhead.
   - Preserve or intentionally change handle reuse order; current tests expect
     reuse behavior.

3. Add optional capacity reservation.
   - Allow benchmark and engine users to pre-size allocation slots.
   - Avoid array growth in pack-heavy or large linear-allocation scenarios.

## Phase 4: Larger Data-Structure Redesign

If Phase 2 and Phase 3 are not enough, replace `RangeFreeBlockMap` with a
dedicated allocator index:

- Store free blocks in pooled arrays of structs.
- Maintain one index ordered by address for coalescing.
- Maintain one index ordered by size for best-fit search.
- Reuse node slots without managed allocation.

This is the highest-upside path but also the highest-risk path. It needs a
strong invariant test suite before implementation.

## Validation Plan

For each optimization:

1. Run the focused range tests.
2. Run focused coverage for `AlvorKit.Ranges`.
3. Run the benchmark in Release with JSON output before and after.
4. Compare scenario-specific metrics, not only aggregate alloc/s.
5. Build `AlvorKit.Engine` to protect `SharedVertexBuffer` integration.

Success criteria:

- No correctness regressions in packing, resizing, coalescing, and address
  alignment tests.
- Reduced managed bytes per allocator operation in churn scenarios.
- Higher allocations/sec in `single-range-alloc-free` and
  `steady-window-churn`.
- Separate pack-only improvements reported as ranges/sec or bytes/sec, not
  allocation throughput.

