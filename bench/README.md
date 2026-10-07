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
- Existing ECS diagnostic demos retain their specialized structural, concurrency,
  and storage-footprint studies. Their CLI and result format are separate.

## Allocation boundaries

Use `BenchTimer.Stop(operationCount, unit)` immediately after the workload.
It captures elapsed time first and then current-thread managed allocation bytes.
`WorkloadAllocatedBytes` excludes fixture preparation, observation, and disposal.
A missing value means unmeasured, not zero. Cross-thread allocations need a
separate measurement; this counter covers synchronous workloads only.

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
