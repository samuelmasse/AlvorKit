# AlvorKit Maths Vector Benchmarks

This small BenchmarkDotNet project compares AlvorKit `Vec2`, `Vec3`, and
`Vec4` directly with `System.Numerics.Vector2`, `Vector3`, and `Vector4`.

Each pair traverses the same 4,096-element contiguous batch, writes to
preallocated output, and reports the mean per vector operation with AlvorKit as
the ratio baseline. The operation classes are add, subtract, pair multiply,
scalar scale, scalar divide, Lerp, collision-style bounds, Dot, Normalize,
equality/hash value semantics, and Vec3 Cross.

The mix comes from a semantic census of production C# in the current AlvorZone
repositories, excluding mirror checkouts. Vector operator sites were led by add
(177), multiply (144), subtract (76), and divide (33). Craftdig accounted for
108, 36, 26, and 7 of those sites respectively, and heavily uses fixed integer
vectors as hash keys and grid coordinates. `System.Numerics.Vector2/3/4` have
no shape-equivalent fixed-integer types, so the benchmark compares the
corresponding operation classes only where both libraries expose equivalent
float shapes.

Focused companion cases cover the most common Craftdig casts, direct versus
default-comparer equality/hash work for `Vec2i` and `Vec3i`, and representative
three- and four-lane swizzle getters against safe x86 shuffle candidates. They
also test JIT inlining/optimization hints and isolate the `Abs` and `Clamp`
leaves of the collision-style bounds workload.

```powershell
dotnet run --project demos/AlvorKit.Maths.Demo.Bench -c Release -- --job short --filter "*Vector*Benchmarks*"
```

Run only conversion, comparer, and swizzle cases:

```powershell
dotnet run --project demos/AlvorKit.Maths.Demo.Bench -c Release -- --job short --filter "*ConversionBenchmarks*" "*EqualityComparerBenchmarks*" "*SwizzleBenchmarks*"
```

Run the JIT-hint and bounds diagnostics:

```powershell
dotnet run --project demos/AlvorKit.Maths.Demo.Bench -c Release -- --job short --filter "*InliningHintBenchmarks*"
dotnet run --project demos/AlvorKit.Maths.Demo.Bench -c Release -- --job short --filter "*VectorLeafBenchmarks*"
```

Split float-vector equality from hashing:

```powershell
dotnet run --project demos/AlvorKit.Maths.Demo.Bench -c Release -- --job short --filter "*FloatValueSemanticsBenchmarks*"

dotnet run --project demos/AlvorKit.Maths.Demo.Bench -c Release -- --job short --filter "*SystemTypeParityBenchmarks*"
```

The consolidated interpretation and links to every raw result are in
[`docs/Maths.VectorBenchmarkReport.html`](../../docs/Maths.VectorBenchmarkReport.html).

Compare `Plane3` with `System.Numerics.Plane` across their overlapping operations:

```powershell
dotnet run --project demos/AlvorKit.Maths.Demo.Bench -c Release -- --job short --filter "*Plane3Benchmarks*"
```
