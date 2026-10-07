# Benchmark Instructions

These instructions apply to every project under `bench/`.

AlvorKit benchmarks use the shared `AlvorKit.Bench` harness for setup, timing,
sampling, comparison, and console output.

Shared runner changes belong in `src/AlvorKit.Bench`. Game-specific fixtures remain in their games.

## Purpose

Benchmarks measure performance; they do not test behavior.

- Keep assertions, correctness probes, and equivalence checks in tests.
- Retain a result, count, checksum, or handle after timing when needed to keep
  measured work observable. Do not validate it in the benchmark.
- Benchmark behavior only after the corresponding contract is covered by
  tests.

## Required project structure

- `Program.cs` selects the suite through `BenchHost`.
- `<Domain>Benchmarks.cs` declares groups, descriptions, inputs, approaches,
  and baselines.
- `<Domain>Measurements.cs` prepares fixtures, controls the timer, retains
  results, and disposes fixtures.
- `Workloads/` contains every complete timed path as a parameterized, stateless
  `Run` method.
- `Approaches/` contains benchmark-only alternative implementations.

Align workload paths with the displayed hierarchy. For example, displayed
`Traverse` > `FacePointers` maps to
`Workloads/Traverse/TraverseFacePointers.cs`. Input sizes and distributions are
suite nodes or descriptions, not duplicate approach types.

## Hot-path isolation

Every measurement must have this shape:

```text
prepare fixture
start timer
invoke one Workloads/...Run method
read elapsed time immediately
retain result and dispose fixture
```

- Put the complete measured operation in `Workloads/`, including its loops,
  batching, and production or facade calls.
- Never implement a measured operation directly in `Measurements`, even when
  it is only one call.
- Between timer start and elapsed-time capture, perform only the single workload
  call and any assignment of its return value.
- Keep fixture construction, input generation, result queries, formatting, and
  disposal outside the timed region unless one is explicitly the operation
  being measured.
- When construction or lazy initialization is intentionally measured, put that
  operation in the workload and state the choice in the suite description.
- Do not add delegate, interface, or generic dispatch to deduplicate timed
  approach paths.

## Sampling

- Count every completed operation in `BenchResult`. Include all maps, batches,
  and repeated passes in the operation count.
- Make each sample long enough to dominate clock and scheduling noise. Batch
  prepared fixtures or repeat read-only passes without changing the intended
  data scale.
- Do not move setup into the timed region merely to lengthen a sample.
- Read elapsed time after the whole workload returns, then dispose fixtures.

## Running

Run benchmark projects directly in Release:

```powershell
dotnet run --project bench/AlvorKit.ECS.Indexed.Bench --configuration Release
```

Bare invocation runs every benchmark with the default warmup and sample counts.
The console shows means only by default; use `--show-samples` to print every
retained sample.

List benchmark IDs and descriptions without running measurements:

```powershell
dotnet run --project bench/AlvorKit.ECS.Indexed.Bench --configuration Release -- list
```

Use quoted benchmark ID globs to select runs. `*` matches within one path
segment, `**` crosses path segments, and `?` matches one character. Multiple
include patterns are unioned before repeatable `--exclude` patterns are
applied. Omitting include patterns selects all benchmarks.

Each benchmark defaults to 16 total measurements: 11 warmups followed by 5
retained samples. Use `--warmup` and `--samples` to replace those counts. Use
`--json <path>` to export a versioned result document or `--json -` to write
JSON to standard output. See `bench/README.md` or pass `--help` for the complete
interface.

Use normal .NET runtime settings unless a suite explicitly compares and
documents runtime modes.
