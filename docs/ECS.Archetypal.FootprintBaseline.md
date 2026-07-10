# Archetype Footprint Reduction: AFR-01 and AFR-02 Baseline

> Captured July 10, 2026 from the current dense archetypal implementation.
> This is a comparison baseline, not a performance contract.

## Verification Baseline

AFR-01 adds 14 focused tests covering:

- Singleton entry and exit.
- Exact signature identity across different add orders.
- Add/remove inverse-edge reuse.
- Reduction without copying removed fields back into dst.
- First, middle, and last-row swap-back compaction.
- Moved-Ent `loc` repair.
- Cleared reference tails and intentionally dirty reference-free tails.
- Collection of removed class values and structs containing references.
- Arch and field-capacity growth.
- Independent groups and allocs.
- Cold shared-catalog resolution and warm point access from different alloc
  owners.

The focused Release test run passed all 76 `AlvorKit.ECS.Test` tests with no
warnings. Forced hash-collision coverage belongs to AFR-11 because the current
implementation has no signature hash path to force.

## Measurement Environment

| Property | Value |
| --- | --- |
| Runtime | .NET 10.0.9 |
| Process | X64 |
| Operating system | Microsoft Windows 10.0.26200 |
| GC | Workstation |
| Logical processors | 32 |
| Stopwatch frequency | 10,000,000 Hz |
| Isolated samples per case | 3 |
| Point operations per sample | 1,000,000 |
| Footprint arch target | 128 |
| Structural/occupancy row target | 256 |
| Concurrent alloc owners | 4 |

Every raw sample ran in a fresh child process. Setup, JIT warmup, GC, footprint
capture, formatting, and JSON serialization remained outside timed point loops.

The complete machine-readable report is generated at:

`out/ecs-archetypal/afr02-current-quick.json`

The `out` directory is intentionally ignored. Recreate the report with:

```powershell
dotnet run -c Release `
  --project demos/AlvorKit.ECS.Demo.Bench `
  -- --suite archetypal --quick --label afr02-current `
  --json out/ecs-archetypal/afr02-current-quick.json
```

## Point Operations by Signature Width

Times are medians in nanoseconds per operation. All listed point operations
reported zero managed bytes allocated per operation.

| `K` | Get present | Get absent | Has present | Has absent | Set existing | Logical bytes | Owned objects | Row slack |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 12.82 | 11.36 | 10.93 | 10.90 | 13.41 | 19,456 | 28 | 15 |
| 4 | 12.91 | 11.38 | 10.94 | 11.00 | 13.27 | 20,896 | 49 | 63 |
| 8 | 12.71 | 11.32 | 10.92 | 10.90 | 13.82 | 23,712 | 91 | 127 |
| 16 | 12.96 | 11.27 | 10.92 | 10.88 | 13.84 | 41,248 | 239 | 255 |
| 32 | 12.81 | 11.29 | 10.93 | 10.91 | 13.64 | 107,040 | 727 | 511 |

The point timings are nearly independent of `K` because the current dense
transition row provides direct field presence. Its footprint is not independent
of `K`: constructing the width incrementally materializes each prefix arch and
retains sixteen rows of storage for every visited alloc-local state.

The baseline therefore records both sides of the tradeoff that later sparse
membership work must preserve or improve: direct access is fast, while retained
storage and object count grow sharply.

## Value Shapes at `K = 8`

| Shape | Get median ns | Set median ns | Allocated B/op | Logical bytes | Owned objects |
| --- | ---: | ---: | ---: | ---: | ---: |
| Wide reference-free value | 12.96 | 13.88 | 0 | 24,672 | 91 |
| Reference | 12.74 | 15.14 | 0 | 23,776 | 91 |
| Struct containing references | 13.32 | 15.18 | 0 | 23,904 | 91 |

Reference values were allocated before timing. The reference-containing cases
measure only archetypal point access, not object construction.

## Structural Operations

Times are medians in nanoseconds per moved Ent.

| Case | Median ns | Allocated B/move | Materialized arches | Active rows | Row slack |
| --- | ---: | ---: | ---: | ---: | ---: |
| Cached add, pre-sized dst | 484.38 | 0 | 8 | 256 | 352 |
| Cached add with dst growth | 448.44 | 78.375 | 8 | 257 | 607 |
| Unknown add | 9,000.78 | 976.625 | 441 | 128 | 6,928 |
| Cached remove, pre-sized dst | 501.56 | 0 | 8 | 256 | 352 |
| Unknown remove | 10,437.50 | 768.000 | 466 | 128 | 7,328 |
| Compact first row | 601.56 | 0 | 8 | 256 | 352 |
| Compact middle row | 624.61 | 0 | 8 | 256 | 352 |
| Compact last row | 425.00 | 0 | 8 | 256 | 352 |

The pre-sized cases isolate cached movement and reusable capacity. The growth
case deliberately begins with insufficient dst capacity. Unknown cases use a
different base signature for each operation so every measured edge is cold;
their cost includes signature search, arch creation where required, catalog
growth, and activation of new row/column storage.

The quick profile is too short to infer that allocating growth is intrinsically
faster than reuse from the medians above. These cases exist to keep allocation
and retained-capacity changes attributable during the epic.

## Sparse and Dense Occupancy

| Case | Median ns/unit | Arches | Active rows | Row slack | Logical bytes | Estimated managed bytes | Owned objects |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Gray-code unique creation | 37,913.28/arch | 128 | 1 | 2,047 | 211,872 | 233,208 | 889 |
| Low occupancy | 30,677.34/arch | 128 | 128 | 1,920 | 211,872 | 233,208 | 889 |
| High occupancy | 3,059.77/row | 16 | 256 | 384 | 53,216 | 58,952 | 239 |

The Gray-code and low-occupancy cases have the same retained archetypal layout:
the current implementation keeps row and column arrays for every visited arch
even when only one final row remains active. This is the principal AFR memory
target.

## Alloc-Owner Concurrency

Concurrent timings are aggregate throughput normalized by the total operation
count, not single-thread latency.

| Case | Median ns/op | Alloc owners | Active rows | Logical bytes | Owned objects |
| --- | ---: | ---: | ---: | ---: | ---: |
| Get, one alloc | 15.16 | 1 | 1 | 23,712 | 91 |
| Get, four allocs | 3.88 | 4 | 4 | 37,824 | 250 |
| Set existing, one alloc | 17.15 | 1 | 1 | 23,712 | 91 |
| Set existing, four allocs | 4.39 | 4 | 4 | 37,824 | 250 |
| Concurrent unknown resolution | 16,936.72/move | 4 | 256 | 1,328,064 | 10,517 |

The concurrency cases obey the ownership contract: one worker exclusively
mutates one alloc's archetypal storage, while different workers share only the
group-global catalog and exact arch definitions.

## Metric Semantics

- Logical bytes are exact retained array-element payload for the current
  backend. They exclude managed headers, alignment, runtime type data, JIT data,
  and GC fragmentation.
- Estimated managed bytes add platform-sized array/object overhead and payload
  alignment. They remain an estimate rather than a profiler heap total.
- Owned objects count graph arrays, row/column arrays, component buffers,
  column-operation instances, and the group synchronization object. Runtime
  shared empty arrays are excluded.
- Catalog used and slack bytes, Ent used and slack bytes, component used and
  slack bytes, capacities, edge counts, and all raw samples are retained in the
  JSON report even when the compact console tables omit them.
- Process-wide retained-heap delta is a secondary noisy value. It includes
  harness state and runtime effects, may be negative, and must not replace the
  deterministic archetypal snapshot.

This schema is intended to survive the storage redesign. Later tasks should
change how each semantic metric is accumulated, not replace the comparison
surface.
