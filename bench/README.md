# AlvorKit benchmarks

`AlvorKit.Bench` owns the shared CLI, catalog, sampling, JSON, and memory reporting.
The reusable library lives in `src/AlvorKit.Bench`; executable suites live here.
SHROOM uses the same runner and supplies its own `AllocLayer` snapshot integration.

```powershell
dotnet run --project bench/AlvorKit.ECS.Bench -c Release -- list
dotnet run --project bench/AlvorKit.ECS.Indexed.Bench -c Release -- list
dotnet run --project bench/AlvorKit.ECS.Indexed.Bench -c Release -- `
    run "Scalar*" "*Bags" --warmup 11 --samples 9 --json out/bench/indexed.json
```

Bare invocation runs the entire suite with 11 warmups and 5 retained samples.
Quoted filters support `*` within one path segment, `**` across segments, and `?`
for one character. Include patterns are unioned; repeatable `--exclude` patterns
are applied afterward. `--show-samples` prints retained samples. `--json -` writes
only JSON. `--memory-tree` prints supplied native allocation trees.

## Suites

- `AlvorKit.ECS.Bench` distills the AlvorKit cases from `Ecs.CSharp.Benchmark`:
  sparse writes, three-component creation, archetypal handle access, rows,
  chunk spans, and Vector256 traversal. Creation includes storage growth;
  traversal excludes fixture preparation and teardown.
- `AlvorKit.ECS.Indexed.Bench` measures sparse writes, nested scalar/array dirty
  reactions, equal writes, gated bags, maintained keys, wide values, Clear,
  and individual Dispose. Mutation loops use 1,024 live Ents and 4,096 passes;
  teardown uses 131,072 prepared Ents. It measures consumer shapes, not a
  complete Craftdig frame or rendering workload. Scalar tracking uses `OnChange`;
  arrays and the wide-value reader use `OnWrite`. Separate wide changed/equal
  cases measure `OnChange` with a 64-byte value. The nested dirty fixtures
  deliberately keep their unconditional marker writes for comparison with the
  original hooks. Craftdig persistence already coalesces its marker; replication
  now does so as well. These fixtures are not an exact persistence measurement.
- `AlvorKit.ECS.Bench.Query` compares ordered and shuffled sparse/archetypal
  access, one/two/eight-column rows and spans, and discovery through 2,047
  materialized archetypes with one active match.
- `AlvorKit.ECS.Bench.Archetypal` contains point access at five signature widths,
  concrete/generic class/struct call sites, membership kernels, structural
  changes, and concurrent arena owners. Cold catalog and growth cases run each
  sample in a fresh child process. Child startup is outside the reported time;
  JIT preparation uses a separate archetype group. Other cases use the runner's
  ordinary in-process warmups. The ideal-direct membership case is a synthetic
  precomputed-table bound, not an alternative production ECS implementation.
- `AlvorKit.Hashing.Bench` compares the approved table-hash and additive-checksum
  helpers with assembly-local implementations of the same algorithms. It also
  compares 32/64-bit epoch indexes with retained dictionaries at three sizes.
  One epoch operation is a complete clear/fill/read cycle, not one lookup.
- `AlvorKit.Maths.Bench` compares vectors, matrices, quaternions, and planes with
  System.Numerics and includes conversions, equality, swizzles, and JIT hints.
  See its [guide](AlvorKit.Maths.Bench/README.md) for comparisons and diagnostics.
- `AlvorKit.Ranges.Bench` covers all 13 allocator scenarios at small and large
  input sizes: churn, growth, handle reuse, fragmentation, and packing. Packing
  samples batch independent prepared fixtures (64 for shrink packing, eight
  for fragmented packing); their operation counts include every fixture.
  Existing-handle reuse repeats 32 passes over the prepared handle.
  The simulated-copy case visits relocation metadata and sums logical byte
  counts; it does not copy payload bytes or measure memory bandwidth.

The seven sparse component/lifetime cases formerly in the ECS demo also live
under `Lifetime` in `AlvorKit.ECS.Bench`. Allocation/disposal cycles include
those operations; existing-component cases exclude fixture setup and cleanup.
All benchmark executables live under `bench/` and share `list`, `run`, filters,
warmups, samples, and schema-9 JSON. The former demo CLIs and BenchmarkDotNet
dependencies have been removed. Range output uses the common timing/allocation
columns instead of the former allocator-specific counters.

## Allocation boundaries

Use `BenchTimer.Stop(operationCount, unit)` immediately after the workload.
It captures elapsed time first and then current-thread managed allocation bytes.
`WorkloadAllocatedBytes` excludes fixture preparation, observation, and disposal.
A missing value means unmeasured, not zero. Cross-thread allocations need a
separate measurement; this counter covers synchronous workloads only.
Concurrent archetypal cases explicitly leave this counter unmeasured. For
isolated cases the workload counter comes from the child, while the complete
measurement allocation counter describes the parent process driver, including
process/JSON overhead. It is not a child-fixture allocation measurement.
The concurrent cases' complete measurement counter also excludes allocations
performed on owner threads; it describes the calling thread only.

The runner also captures `AllocatedBytes` for the complete measurement method,
including fixture preparation and cleanup. Optional CoreCLR profiler object counts
cover that same complete measurement. Do not treat these totals as hot-path costs.
The runner never installs or launches a profiler automatically.

Inspect exact byte totals in JSON before claiming zero allocation. Rates use
significant digits so small nonzero values remain visible. For attribution,
use the existing [CoreCLR allocation profiler](../docs/Interception.md#managed-allocation-capture)
in a separate run; profiled timing is not comparable to ordinary timing. Its
process-wide object count can include finalizer/runtime work on other threads.

JSON schema 9 contains both allocation scopes under `allocations` (and
`meanAllocations`), warmups, every retained sample,
means, runtime information, and any supplied native memory trees. Byte counters
measure allocation traffic; native memory trees measure retained storage.

The Indexed suite also supports a separate retained-memory experiment:

```powershell
dotnet run --project bench/AlvorKit.ECS.Indexed.Bench -c Release -- --footprint
```

It reports handle size/reference content and the managed heap delta for a warmed
131,072-Ent fixture. This is a process-level heap estimate, not exact object
ownership accounting. Run it separately from timing.

The archetypal suite has separate low/high-occupancy storage snapshots. Run each
in a fresh process; these include the internal catalog/storage counters and a
managed-heap delta, not just logical component payload bytes:

```powershell
dotnet run --project bench/AlvorKit.ECS.Bench.Archetypal -c Release -- --footprint low
dotnet run --project bench/AlvorKit.ECS.Bench.Archetypal -c Release -- --footprint high
```

## Comparisons

Build Release first. Run old and new binaries in separate processes, sequentially,
with identical inputs and normal runtime settings. Retain source revisions,
uncommitted diffs, runtime/machine details, and raw JSON together. Repeat the pair
to check noise; inspect raw samples and medians as well as means. Avoid overlapping
builds, tests, games, or other benchmarks with timed runs.

Verify that warmups reach steady state under tiered compilation. A fixed count
can be insufficient for short cases: Indexed context registration continued
changing after 11 warmups on .NET 10. The redesign comparison uses 51 warmups
and 15 samples for the main cases, and 201 warmups and 15 samples for registration
and wide values. These counts describe that experiment, not a universal minimum.

An earlier implementation must perform comparable work to justify a speed ratio.
In particular, old Indexed teardown could recreate dirty components; a benchmark
of that incorrect behavior is not evidence that correct teardown should match it.
The teardown comparisons use plain components and maintained bags without dirty
reactions. Behavioral regressions belong in tests, outside the timing loop.

## Adding a suite

`Program.cs` selects an `IBenchSuiteProvider` through `BenchHost`. A suite declares
IDs, descriptions, inputs, and comparisons. Measurements prepare fixtures, start a
timer, invoke one stateless `Workloads/...Run` method, stop the timer, retain a
result, and clean up. Keep every measured loop in the workload; do not introduce
delegate or interface dispatch merely to share timed paths. Count complete cycles
explicitly when one operation contains several writes.
