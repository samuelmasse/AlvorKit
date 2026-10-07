# Maths benchmarks

This suite uses `AlvorKit.Bench` for timing, warmups, samples, comparisons,
managed allocation counters, and JSON. Each sample applies 256 passes to 4,096
prepared inputs. Setup and output retention are outside the clock; one operation
is one input value, not one entire pass.

```powershell
dotnet run --project bench/AlvorKit.Maths.Bench -c Release -- list
dotnet run --project bench/AlvorKit.Maths.Bench -c Release -- run "Vector3/**" --warmup 11 --samples 9
dotnet run --project bench/AlvorKit.Maths.Bench -c Release -- run "Conversion/**" "EqualityComparer/**"
dotnet run --project bench/AlvorKit.Maths.Bench -c Release -- run "Plane3/**" --json out/bench/planes.json
```

The suite retains the former vector arithmetic, scalar operations, interpolation,
bounds, normalization, cross product, value semantics, conversion, comparer,
swizzle, inlining-hint, vector-leaf, matrix, quaternion, and plane workloads.
Comparisons name their baseline explicitly in the catalog. Equivalent matrix
composition accounts for AlvorKit's column convention and System.Numerics' row
convention. Integer comparer cases compare direct versus default-comparer calls;
System.Numerics has no corresponding fixed-integer vector types.

The intrinsic swizzle candidates require SSE2 and fail explicitly without it.
They never substitute the regular implementation under an intrinsic label.
Correctness and special floating-point semantics belong in the maths tests.

The shared runner does not provide BenchmarkDotNet's disassembly diagnoser.
Use the runtime's `DOTNET_JitDisasm` diagnostics in a separate process when
inspecting code generation; do not compare diagnostic timings with ordinary runs.
The JIT-hint comparison still preserves each helper's original method attributes.

[The historical vector report](../../docs/Maths.VectorBenchmarkReport.html)
records earlier BenchmarkDotNet measurements. New numbers use a different harness
and batching protocol; reproduce both revisions with the same runner before
claiming a regression or improvement.
