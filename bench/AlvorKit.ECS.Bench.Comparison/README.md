# C# ECS comparison

Runs AlvorKit and nine external ECS frameworks through `AlvorKit.Bench` and writes
a self-contained HTML report alongside the complete measurement JSON.

The competitors are Arch, DefaultEcs, Entitas, Fennecs, Flecs.NET, Frent, Friflo,
Morpeh, and Svelto.ECS.

## Run

From the AlvorKit root:

```powershell
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release -- list
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release
```

Bare invocation measures all 200 cases: 134 runnable methods, with two padding
inputs for ordinary update scenarios. Defaults are 11 warmups and 5 retained
samples per case. The output path is printed after the run:
`out/bench/ecs-comparison/<UTC timestamp>/results.html` and `results.json`.

Build first, then run measurements without overlapping builds, tests, games, or
other benchmark processes. For a repeatable output location or a filtered run:

```powershell
dotnet build bench/AlvorKit.ECS.Bench.Comparison -c Release
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release --no-build -- `
    run --json out/bench/ecs-comparison/full.json
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release --no-build -- `
    run "Update2/**/Padding0/Scalar/**" --warmup 21 --samples 9 `
    --json out/bench/ecs-comparison/update2.json
dotnet run --project bench/AlvorKit.ECS.Bench.Comparison -c Release --no-build -- `
    report out/bench/ecs-comparison/full.json --output out/bench/ecs-comparison/share.html
```

Case IDs have the shape `Scenario/N100000/Padding0/Mode/Framework/Variant`.
Include/exclude globs and other runner switches are described in
[the benchmark guide](../README.md). `report` reads saved measurements without
rerunning them. `--json -` prints the enriched JSON to stdout, saves the same
JSON and HTML under the default output directory, and prints the report path
to stderr. A filtered report explicitly shows unmeasured representatives as
**Not run**; it does not substitute another measured variant.

## Read the report

The dark overview is a framework-by-workload heatmap with separate **AlvorKit /
Archetypal** and **AlvorKit / Sparse** rows: 11 comparison rows for ten frameworks.
Both storage models cover all seven scalar scenarios. Every cell shows median
nanoseconds per Ent operation and its ratio to the fastest measured overview
representative in that column. **1.00×** is the lowest measured median; **2×**
means twice that time. Green, yellow, and orange cells indicate increasing
multipliers. Comparisons share count, padding, scenario, and execution mode.

The workload selector sits directly above the detail charts farther down the
page. Selecting a workload updates that section in place without scrolling.
Column headings show the component count or arithmetic operation; the selected
workload gives a brief operation description and the number of update passes.
Mode and nonmatching-Ent filters select the compared data. Each report uses one
matching Ent count, displayed above the overview.

Details show sorted bars, observed sample ranges, every measured variant,
and managed allocation traffic. Bars always use a linear scale starting at zero.
Warmups do not contribute to these statistics. Sample ranges are
not confidence intervals, and a small median difference does not establish
statistical significance. There is deliberately no overall framework score.

Each measured overview cell has a **code** link, and variant names in the bars
and detail table link to their timed workload's `Run` method. **Setup / timing**
opens the measurement method, including fixture creation and timer boundaries.
These are native `vscode://file/...:line:column` links; VS Code must be installed,
and the browser may ask before opening it. No extension or report server is needed.

Links target the local checkout, with paths and line numbers resolved when the
HTML is generated. They do not restore the source revision used by an older run.
Regenerate the HTML after moving the checkout or editing source in ways that
shift method locations. Source navigation metadata is embedded in
the HTML separately from the original measurement JSON.

Package versions, machine details, raw samples, and run provenance remain in
the saved JSON. The HTML focuses on the comparison and source links.

Only scalar and SIMD workloads are included. Every supported scenario/mode has
both AlvorKit and external implementations. SIMD includes library vector APIs
and explicit vector loops; the selected API determines its vector width.
Sparse storage has no SIMD case in this suite. Missing implementations say
**Unavailable**, and missing measurements say **Not run**.

Overview representatives are fixed in `ComparisonCase`, independently of results:

| Framework | Scalar creation | Scalar updates | SIMD |
| --- | --- | --- | --- |
| AlvorKit / Archetypal | ArchetypalReusedBuilder | Rows | Query_SIMD |
| AlvorKit / Sparse | Sparse | SparseHandles | — |
| Arch | Default | Scalar; Default for Mixed | — |
| DefaultEcs | Default | ComponentSystem_Scalar for Update1; Scalar otherwise | — |
| Entitas | Default | GroupDirect | — |
| Fennecs | Default | ForEach | — |
| FlecsNet | Default | Iter | — |
| Frent | Default | QueryInline | Simd |
| FrifloEngineEcs | Default | Scalar | SIMD_Scalar |
| Morpeh | Stash | Stash | — |
| SveltoECS | Default | Default | — |

All other scalar and SIMD variants remain available in the detail table, labeled
by storage model. A representative describes a particular implementation of this
workload; it does not assert that a framework cannot be tuned further or that it
lacks APIs absent from this suite.

## Workload contracts

| Scenario | Timed operation | Fixture |
| --- | --- | --- |
| Create1 / Create2 / Create3 | Create 100,000 Ents with one, two, or three zero-initialized integer components | Fresh world per sample |
| Update1 | Increment the first component | 100,000 matching Ents; first starts at 0 |
| Update2 | Add second to first | First starts at 0; second at 1 |
| Update3 | Add second and third to first | First starts at 0; second and third at 1 |
| Mixed | Add second to first across four signatures | Each Ent has the two data components and one of four marker components |

Updates execute 64 passes per sample and count all 6,400,000 Ent updates.
Creation executes one pass. For Update1–3, padding is either zero or ten
nonmatching, marker-only Ents per matching Ent. Creation and Mixed do not have
padded variants. Padding does not accidentally join a one-component query.

Measurements prepare a fresh fixture, start the timer, invoke one stateless
workload, stop the timer, retain the fixture, and dispose it. Creation times
include the workload's component registration, capacity reservation/growth,
command playback, and required submission. World construction and disposal are
outside timing. Update query preparation follows the source approach: some
variants retain a prepared query while others acquire one in each timed pass.
This distinction is part of the measured API usage, not a claim about a pure
arithmetic kernel. Frameworks retain their own storage, component representation,
and scheduling strategies.

Entitas uses its published NuGet runtime with the default SafeAERC reference
counting. Creation obtains components through `CreateComponent<T>`, initializes
them, and calls `AddComponent`; pools start empty in each fresh context.
Creation contexts declare only the one, two, or three measured component slots.
Update contexts additionally declare the padding slot; Mixed declares two data
slots, one unused third-data slot, and four distinct marker slots.
Updates retain a matching group and prepare its cached `GetEntities()` array
outside timing. Each timed pass calls `GetEntities()`, reads components through
`GetComponent`, and changes their integer fields directly. These cases measure
in-place arithmetic, without component replacement or reactive notifications.
No SIMD implementation is supplied for Entitas.

AlvorKit creation measures six access patterns at every component count:

| Storage | Variant | Timed creation path |
| --- | --- | --- |
| Sparse | Sparse | Allocate, then set each component |
| Sparse | SparseMutator | Allocate, then initialize through the generated fluent `Mutate()` API |
| Archetypal | ArchetypalSetters | Allocate, then set each component, entering intermediate shapes |
| Archetypal | ArchetypalMutator | Allocate, then initialize through fluent `Mutate()`, also entering intermediate shapes |
| Archetypal | ArchetypalFinalShape | Build the complete shape for each Ent and call `Create()` once |
| Archetypal | ArchetypalReusedBuilder | Build the complete shape once per workload invocation, then reuse its `Create()` for all Ents |

The reusable builder's construction remains inside timing. Both final-shape
paths enter the final archetype directly. Fluent mutation performs individual
writes; it does not combine them into a single structural change. The overview
uses the reusable builder for archetypal creation and direct setters for sparse
creation, chosen before measurement. Every alternative remains visible for
comparison; the report never automatically promotes whichever variant wins a run.

Sparse updates iterate a prepared array of matching Ent handles. Archetypal
`DenseHandles` provides the equivalent access pattern in the same detail table;
archetypal `Rows` remains its overview representative. Preparing these handle
sets is outside timing. Thus the overview compares complete selected traversal
strategies, while the handle variants allow a closer storage-access comparison.
Sparse padding and the four mixed-composition markers are sparse components too.
These cases use base ECS storage without Indexed hooks or bag maintenance.

The report's allocation figures are median managed bytes per operation from the
timed region on the measuring thread. They exclude setup/teardown, native
allocations, and allocations on workers. JSON also preserves the runner's
separate whole-measurement counters;
these must not be interpreted as timed-workload allocations or retained memory.

## Dependencies

Package versions are declared in the project file. The build records those
references for the report, so saved results retain the versions measured.
After updating packages, rebuild and run the correctness tests before measuring.

`Svelto.Common` stays at 3.6.0 because 3.7.x reports assembly version 0.0.0.0
and cannot satisfy `Svelto.ECS` 3.5.2's reference to assembly version 3.4.0.0.

Every saved result includes the AlvorKit revision, dirty-tree status, runtime,
OS, architecture, processor count, Windows CPU model, package versions, and
representative choices. Preserve uncommitted source changes alongside a run to
reproduce a dirty checkout. HTML is fully
self-contained and requires no service or CDN.

## Correctness checks

```powershell
dotnet test tests/AlvorKit.ECS.Bench.Comparison.Test -c Release
```

Tests execute every runnable method with small and odd counts, inspect actual
framework component values and matching populations, verify padding counts,
and cover mixed-composition distribution, report validation, and safe JSON
embedding. Correctness assertions are outside benchmark workloads.
