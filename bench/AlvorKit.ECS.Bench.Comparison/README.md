# ECS comparison

Compares AlvorKit archetypal and sparse storage with Arch, DefaultEcs, Entitas,
Fennecs, Flecs.NET, Frent, Friflo, Morpeh, and Svelto.ECS.

## Coverage

The suite contains the original seven workloads at 100,000 matching Ents:

- Create1, Create2, and Create3: create one, two, or three zero-valued integer components.
- Update1: increment the first component.
- Update2: add the second component to the first.
- Update3: add the second and third components to the first.
- Mixed: update across four fixed component compositions.

Update workloads compare zero or ten nonmatching Ents per matching Ent.
Scalar and SIMD implementations remain separate. Existing setters, mutators,
final-shape builders, row loops, queries, and other access variants remain in
these workloads. Creation includes registration, storage growth, and required
submission; world construction is excluded. Update preparation and query
construction are outside timing. Updates default to 64 passes.

## Run

```powershell
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release -- list
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release -- `
    study "Update2/N100000/Padding0/Scalar/**" `
    --output out/bench/ecs-comparison/update2
```

Independent-process studies default to five launches, 201 warmups, and nine
retained samples per launch. A discarded pilot chooses one shared pass count
for implementations with the same workload, mode, and input. It targets 2 ms
for the fastest implementation while limiting the slowest batch to ten times
that target, subject to the original minimum pass count. Fixed-scale creation
is not repeated to inflate its duration. Short samples are flagged.

Use `--pilot-ms`, `--warmup`, `--samples`, `--launches`, and `--seed` to configure
the protocol. The output directory must be empty. Child measurements and logs
are retained; failed children fail the study. `--cold` measures the first timed
invocation after fixture construction with no warmup. Cold and warmed results
remain separate. Bare invocation uses the shared harness's in-process protocol
of 11 warmups and five retained samples. See [the harness guide](../README.md).

## Report

The original heatmap is published at
`out/bench/ecs-comparison/alvorkit-vs-frameworks.html`, with its raw data in the
adjacent JSON. Click a workload or result to inspect all variants and the source
captured with that measurement. Cells show time, allocation, and quality labels.

```powershell
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release --no-build -- `
    combine out/bench/ecs-comparison/creation/results.json `
    out/bench/ecs-comparison/updates/results.json `
    --output out/bench/ecs-comparison/alvorkit-vs-frameworks.json

dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release --no-build -- `
    report out/bench/ecs-comparison/alvorkit-vs-frameworks.json
```

Combined inputs must share environment and package versions. Later inputs
replace duplicate cases without pooling samples. Rows retain the original
protocol, calibrated passes, runtime assembly identities, and source snapshots.
Ratios require matching passes and sampling protocols. `report --baseline`
compares a saved previous run only when environment and contracts agree.

Independent-process estimates are means of launch means; 95% intervals use
between-launch variance and Student-t critical values. In-process results use
the sample median and min-max range. Overlapping intervals and changes below
3% are unresolved. The report flags single-process measurements, samples under
1 ms, and launch intervals with half-width above 10%. Warmup drift compares the
last three warmups with retained medians per launch. No overall framework score
or universal performance ranking is implied.

Timed allocation counts measuring-thread managed bytes inside the workload.
Value results are retained after timing without boxing. Peak process memory
includes the runtime, JIT, fixtures, and native libraries; it is not owned ECS
memory. Owned native bytes and hardware counters are not measured.

The HTML embeds retained samples, source snapshots, and the exact warmup tail
used for drift, omitting duplicate run measurements and unused warmup history.
Charts and source inspection work offline. Keep the linked JSON beside the HTML
to retain access to every warmup and complete contributing run. Package versions
are pinned; Svelto.Common 3.6.0 satisfies the selected Svelto.ECS assembly reference.
