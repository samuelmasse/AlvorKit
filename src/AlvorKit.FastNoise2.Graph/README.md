# AlvorKit.FastNoise2.Graph

Typed construction, managed node ownership, and allocation-free sampling for FastNoise2 1.1.1.

This package replaces raw metadata IDs, string matching, enum-option indexes, native-handle cleanup, and scalar sampling
arguments with discoverable C# enums, fluent node configuration, graph validation, AlvorKit vectors, and spans. It does
not replace FastNoise2's fused SIMD node graph; it makes that graph safer to construct and use.

The package is pinned to:

- FastNoise2 runtime metadata 1.1.1.
- AlvorKit FastNoise2 binding 1.1.1.3.
- Audited upstream FastNoise2 commit `903c1f2d2f9d53ddce94cd223f32727d9ab3aeaa` and its pinned FastSIMD commit.
- .NET 10 and the `AlvorKit` namespace.

## Installation

Applications that create the concrete native backend need both packages:

```powershell
dotnet add package AlvorKit.FastNoise2.Graph
dotnet add package AlvorKit.FastNoise2.Backend
```

Engine code should receive `FnGraph` from dependency injection and needs only the graph package directly. The host's
backend package selects and loads the matching native runtime.

## Complete example

```csharp
using AlvorKit;

var fn = new FnBackend();
var graph = new FnGraph(fn);

var simplex = graph.Create(FnNodeType.Simplex)
    .Float(FnFloatVariable.FeatureScale, 96f)
    .Integer(FnIntegerVariable.SeedOffset, 0)
    .Float(FnFloatVariable.OutputMinimum, -1f)
    .Float(FnFloatVariable.OutputMaximum, 1f);

var root = graph.Create(FnNodeType.FractalFbm)
    .Integer(FnIntegerVariable.Octaves, 5)
    .Float(FnFloatVariable.Lacunarity, 2f)
    .Hybrid(FnHybrid.Gain, 0.5f)
    .Hybrid(FnHybrid.WeightedStrength, 0.1f)
    .Source(FnSource.Source, simplex);

var output = new float[256 * 256];
Span<float> minMax = stackalloc float[2];
root.GenUniformGrid2D(output, (0f, 0f), (256, 256), (1f, 1f), 1337, minMax);
```

`FnGraph` retains a finalizable managed handle for every `Create` or `CreateEncoded` result. Copying `root` or `simplex` does not
clone the native node or duplicate its native reference; every node value keeps its graph alive. FastNoise2 releases the
references after the graph and all of its node values become unreachable. Callers do not clear or dispose the graph.

## Graph contract

- `FnGraph(Fn)` requests the fastest compiled FastSIMD implementation supported by the current CPU.
- `Create` resolves the exact, case-sensitive FastNoise2 metadata name and retains the returned native reference.
- `CreateEncoded` loads a complete Base64 node tree exported by the upstream Node Editor and manages its root reference.
- Every fluent setter resolves an exact metadata name, component, and member kind. Unsupported node/member combinations
  throw; no numeric metadata index is exposed.
- `Source` and node-valued `Hybrid` connections must use nodes from the same graph and must remain acyclic.
- Connect every required source before sampling. Sampling deliberately does not walk or revalidate the graph.
- Native references are released by the retained `SafeHandle` finalizers; `FnGraph` has no disposal protocol.

Graph creation and configuration are cold operations and are not thread-safe. Once a graph is complete and immutable,
the same root can be sampled concurrently into independent, nonoverlapping buffers. The hot path performs no metadata,
ownership, cycle, or graph-completeness checks. Batch overloads retain only constant-time bounds and overlap checks that
prevent invalid native buffer access.

## Configuration methods

| Method | Contract |
| --- | --- |
| `Float(FnFloatVariable, float)` | Sets a scalar float metadata variable. |
| `Integer(FnIntegerVariable, int)` | Sets an integer metadata variable. |
| `DistanceFunction(FnDistanceFunction)` | Sets Distance Function on point-distance and cellular nodes. |
| `CellularReturnType(FnCellularReturnType)` | Sets Return Type on `CellularDistance`. |
| `Interpolation(FnInterpolation)` | Sets the fade interpolation curve. |
| `ClampOutput(bool)` | Sets output clamping on `Remap`. |
| `RemovedDimension(FnRemovedDimension)` | Selects the axis omitted by `RemoveDimension`. |
| `RotationType(FnRotationType)` | Selects the 3D plane preset on `DomainRotatePlane`. |
| `VectorizationScheme(FnVectorizationScheme)` | Selects displacement-vector construction on simplex domain warps. |
| `Hybrid(FnHybrid, float)` | Supplies a constant value for a hybrid input. |
| `Hybrid(FnHybrid, FnGraphNode)` | Supplies a spatially varying node for a hybrid input. |
| `Source(FnSource, FnGraphNode)` | Connects a required node input. |
| `GetActiveFeatureSet()` | Reports the selected cumulative native FastSIMD mask. |

A hybrid node connection takes priority over its stored constant. FastNoise2 1.1.1 cannot detach that connection, so
the wrapper rejects a later constant assignment rather than silently updating a dormant constant. A node connection may
be replaced with another node. The C API cannot report the connections already present in an encoded root, so the
wrapper also rejects constant-valued hybrid mutation on that root. Required-source and node-valued hybrid replacement
remain available.

Metadata minimums and maximums are Node Editor guidance. Native setters do not generally enforce them. Use the ranges
and hazards below as part of the application contract.

## Enum reference

Except for `FnFeatureSet`, managed enum integers are wrapper implementation details. Do not persist them, cast them to
raw metadata indexes, or pass them to `Fn`. The wrapper resolves exact upstream names at runtime.

### Member enums

| Type | Values |
| --- | --- |
| `FnFloatVariable` | `AmplitudeScalingX/Y/Z/W`, `FeatureScale`, `Lacunarity`, `Maximum`, `Minimum`, `MultiplierX/Y/Z/W`, `OutputMaximum`, `OutputMinimum`, `Pitch`, `Roll`, `Scaling`, `ScalingX/Y/Z/W`, `StepCount`, `Value`, `Yaw` |
| `FnIntegerVariable` | `SeedOffset`, `ValueIndex`, `DistanceIndex0`, `DistanceIndex1`, `Octaves`, `Power` |
| `FnHybrid` | `Fade`, `FadeMaximum`, `FadeMinimum`, `FromMaximum`, `FromMinimum`, `Gain`, `GridJitter`, `Lhs`, `MinkowskiP`, `NewDimensionPosition`, `OffsetX/Y/Z/W`, `PingPongStrength`, `PointX/Y/Z/W`, `Power`, `Rhs`, `SizeJitter`, `Smoothness`, `ToMaximum`, `ToMinimum`, `Value`, `WarpAmplitude`, `WeightedStrength` |
| `FnSource` | `A`, `B`, `DomainWarpSource`, `Lhs`, `Lookup`, `Source`, `Value` |

The similarly named members are deliberately separate contracts:

- Float `Value` configures `Constant`; hybrid `Value` configures `PowFloat`; source `Value` configures `PowInt`.
- Integer `Power` maps to upstream `Pow` on `PowInt`; hybrid `Power` maps to `Pow` on `PowFloat`.
- `Maximum` and `Minimum` are the `ConvertRGBA8` input bounds. `OutputMaximum` and `OutputMinimum` remap coherent output.
- `Scaling` is uniform domain scaling. `ScalingX/Y/Z/W` are independent axis scaling.
- `AmplitudeScalingX/Y/Z/W` scale warp displacement, not input coordinates.
- `StepCount` is intentionally a float and must be finite, nonzero, and normally positive.

### Option enums

| Type and value | Exact behavior |
| --- | --- |
| `FnDistanceFunction.Euclidean` | Square root of the sum of squared axis distances. |
| `EuclideanSquared` | Sum of squared axis distances; faster, with a different numeric scale. |
| `Manhattan` | Sum of absolute axis distances. |
| `Hybrid` | Euclidean-squared plus Manhattan distance. |
| `MaximumAxis` | Greatest absolute distance on any axis. |
| `Minkowski` | Configurable p-norm using `FnHybrid.MinkowskiP`. |
| `FnCellularReturnType.Index0` | Selected distance rank 0. |
| `Index0Add1` | Selected rank 0 plus selected rank 1. |
| `Index0AbsoluteDifference1` | Absolute difference between the two selected ranks; upstream name `Index0Sub1`. |
| `Index0Multiply1` | Product of the two selected ranks. |
| `Index0Divide1` | Rank 0 divided by rank 1. |
| `FnInterpolation.Linear` | Uncurved normalized fade value. |
| `Hermite` | Cubic smoothstep: `3t² - 2t³`. |
| `Quintic` | Quintic smootherstep: `6t⁵ - 15t⁴ + 10t³`. |
| `FnRemovedDimension.X/Y/Z/W` | Coordinate omitted by `RemoveDimension`; Y is the constructed default. |
| `FnRotationType.ImproveXyPlanes` | Preset optimized for important XY planes. |
| `FnRotationType.ImproveXzPlanes` | Preset optimized for important XZ planes. |
| `FnVectorizationScheme.OrthogonalGradientMatrix` | Orthogonal gradient-matrix domain-warp vector construction. |
| `FnVectorizationScheme.GradientOuterProduct` | Gradient outer-product domain-warp vector construction. |

`FnVectorizationScheme` is a noise algorithm choice; it is unrelated to CPU SIMD selection.

### FastSIMD feature sets

`FnFeatureSet` uses the exact cumulative masks from the FastSIMD revision pinned by FastNoise2 1.1.1.

| Value | Native mask | Architecture |
| --- | ---: | --- |
| `Scalar` | 1 | Portable scalar |
| `Sse` | 6 | x86 SSE |
| `Sse2` | 14 | x86 SSE2 |
| `Sse3` | 30 | x86 SSE3 |
| `Ssse3` | 62 | x86 SSSE3 |
| `Sse41` | 126 | x86 SSE4.1 |
| `Sse42` | 254 | x86 SSE4.2 |
| `Avx` | 510 | x86 AVX |
| `Avx2` | 1022 | x86 AVX2 |
| `Avx512` | 16382 | x86 AVX-512 |
| `Neon` | 49152 | 32-bit ARM NEON |
| `Aarch64` | 114688 | AArch64 SIMD |
| `WasmSimd` | 131072 | WebAssembly SIMD |
| `Maximum` | 4294967295 | No requested upper limit; select the fastest available implementation |

An active feature set describes the compiled implementation, not noise quality. It can differ by CPU, operating system,
architecture, runtime identifier, and native package build.

## Complete node catalog

Notation: `F` = float variable, `I` = integer variable, `H` = hybrid constant-or-node input, `S` = required source,
and `E` = typed enum/bool option. Upstream names that differ from managed names are shown explicitly.

### Basic and coherent generators

| Node | Behavior | Configuration |
| --- | --- | --- |
| `Constant` | Returns one value everywhere; ignores position and seed. | F: `Value` |
| `White` | Deterministic, discontinuous white noise from seed and exact coordinates. | I: `SeedOffset`; F: `OutputMinimum/Maximum` |
| `Checkerboard` | Alternates output bounds by N-dimensional cell parity. | F: `FeatureScale`, `OutputMinimum/Maximum` |
| `SineWave` | Product of `sin(coordinate / FeatureScale)` across active axes; period is `2π * FeatureScale` per axis. | F: `FeatureScale`, `OutputMinimum/Maximum` |
| `Gradient` | Sum of `(coordinate + Offset) * Multiplier` across active axes. | F: `MultiplierX/Y/Z/W`; H: `OffsetX/Y/Z/W` |
| `DistanceToPoint` | Selected distance from the current domain position to a hybrid point. | E: `DistanceFunction`; H: `PointX/Y/Z/W`, `MinkowskiP` |
| `Simplex` | Seeded simplex-lattice gradient noise in 2D, 3D, and 4D. | F: `FeatureScale`, `OutputMinimum/Maximum`; I: `SeedOffset` |
| `SuperSimplex` | Smoother, more isotropic, and more expensive simplex gradient noise. | Same as `Simplex` |
| `Perlin` | Seeded grid-gradient noise with quintic interpolation. | Same as `Simplex` |
| `Value` | Seeded pseudorandom lattice values with Hermite interpolation; not gradient noise. | Same as `Simplex` |
| `CellularValue` | Hashed value of the selected nearest-distance rank, remapped to the output range. | F: `FeatureScale`, `OutputMinimum/Maximum`; I: `SeedOffset`, `ValueIndex`; E: `DistanceFunction`; H: `MinkowskiP`, `GridJitter`, `SizeJitter` |
| `CellularDistance` | Remapped distance rank or arithmetic combination of two selected ranks. | F: `FeatureScale`, `OutputMinimum/Maximum`; I: `SeedOffset`, `DistanceIndex0/1`; E: `DistanceFunction`, `CellularReturnType`; H: `MinkowskiP`, `GridJitter`, `SizeJitter` |
| `CellularLookup` | Evaluates `Lookup` at the closest jittered feature position in world space with seed + 1. | F: `FeatureScale`; I: `SeedOffset`; E: `DistanceFunction`; H: `MinkowskiP`, `GridJitter`, `SizeJitter`; S: `Lookup` |

Cellular indexes are clamped to 0 through 3. Grid jitter above 1 and nonzero size jitter can create search-grid artifacts.
`DistanceToPoint` is constructed with Euclidean-squared distance even though its 1.1.1 metadata advertises Euclidean;
configure it explicitly when the distinction matters.

### Fractals, warps, operators, and blends

| Node | Behavior | Configuration |
| --- | --- | --- |
| `FractalFbm` | Unnormalized sum of source octaves; coordinates scale by lacunarity, seed increments, and amplitude scales by gain and weighting. Upstream name `FractalFBm`. | I: `Octaves`; F: `Lacunarity`; H: `Gain`, `WeightedStrength`; S: `Source` |
| `FractalRidged` | Unnormalized sum of `1 - 2 * abs(Source)` octaves. | Same as `FractalFbm` |
| `PingPong` | Folds `Source * PingPongStrength` into a triangular waveform in `[0, 1]`. | H: `PingPongStrength`; S: `Source` |
| `DomainWarpSimplex` | Displaces source coordinates with simplex-derived vectors. | F: `FeatureScale`, `AmplitudeScalingX/Y/Z/W`; I: `SeedOffset`; E: `VectorizationScheme`; H: `WarpAmplitude`; S: `Source` |
| `DomainWarpSuperSimplex` | Smoother, more expensive SuperSimplex vector warp. | Same as `DomainWarpSimplex` |
| `DomainWarpGradient` | Faster interpolated-grid vector warp. | Same, without `VectorizationScheme` |
| `DomainWarpFractalProgressive` | Each warp octave receives the preceding warped position. | I: `Octaves`; F: `Lacunarity`; H: `Gain`, `WeightedStrength`; S: `DomainWarpSource` |
| `DomainWarpFractalIndependent` | Every octave starts from the original position and accumulated offsets are applied together. | Same as progressive |
| `Add` | Required LHS + hybrid RHS. | S: `Lhs`; H: `Rhs` |
| `Subtract` | Hybrid LHS - hybrid RHS. | H: `Lhs`, `Rhs` |
| `Multiply` | Required LHS * hybrid RHS. | S: `Lhs`; H: `Rhs` |
| `Divide` | Hybrid LHS / hybrid RHS; zero is not guarded. | H: `Lhs`, `Rhs` |
| `Modulus` | Floating-point remainder of hybrid LHS / RHS; zero is not guarded. | H: `Lhs`, `Rhs` |
| `Min` / `Max` | Hard minimum or maximum. | S: `Lhs`; H: `Rhs` |
| `MinSmooth` / `MaxSmooth` | Polynomial smooth min/max; absolute smoothness near zero becomes hard min/max. | S: `Lhs`; H: `Rhs`, `Smoothness` |
| `PowFloat` | `pow(max(abs(Value), FLT_MIN), Power)`; discards sign and does not return exact zero for zero input. | H: `Value`, `Power` |
| `PowInt` | Required value raised to an integer power; values below 2 still square in 1.1.1. | I: `Power`; S: `Value` |
| `Fade` | Clamps and eases normalized Fade between its bounds, then interpolates A to B; equal bounds select 50/50. | S: `A`, `B`; H: `Fade`, `FadeMinimum/Maximum`; E: `Interpolation` |

`DomainWarpSource` must be a domain-warp node. That warp node owns the final `Source` evaluated at the warped position.

### Modifiers and dimensional operations

| Node | Behavior | Configuration |
| --- | --- | --- |
| `Abs` | Absolute value of source. | S: `Source` |
| `SignedSquareRoot` | `sign(Source) * sqrt(abs(Source))`. | S: `Source` |
| `DomainScale` | Multiplies every active coordinate; values above 1 increase apparent frequency and zero collapses the domain. | F: `Scaling`; S: `Source` |
| `DomainOffset` | Adds a constant or spatially generated offset to every active coordinate. | H: `OffsetX/Y/Z/W`; S: `Source` |
| `DomainRotate` | Roll rotates X, pitch Y, and yaw Z, in radians. Yaw alone rotates 2D; pitch/roll promote 2D to 3D; 4D is unchanged. | F: `Yaw`, `Pitch`, `Roll`; S: `Source` |
| `DomainAxisScale` | Independently multiplies every active coordinate. | F: `ScalingX/Y/Z/W`; S: `Source` |
| `SeedOffset` | Adds `SeedOffset` to the seed passed through the complete child graph. | I: `SeedOffset`; S: `Source` |
| `ConvertRgba8` | Clamps source to Min/Max, scales to grayscale byte, sets alpha 255, and bit-casts packed RGBA8 into each float slot. Upstream name `ConvertRGBA8`. | F: `Minimum`, `Maximum`; S: `Source` |
| `GeneratorCache` | Caches only the last SIMD batch per thread, keyed by source, seed, and exact coordinates. | S: `Source` |
| `Remap` | Linear range mapping with optional output clamp; equal source bounds divide by zero. | H: `FromMinimum/Maximum`, `ToMinimum/Maximum`; E: `ClampOutput`; S: `Source` |
| `Terrace` | Quantizes in intervals of `1 / StepCount` with optional transition smoothing. | F: `StepCount`; H: `Smoothness`; S: `Source` |
| `AddDimension` | Appends `NewDimensionPosition` to 2D or 3D input; 4D passes through. | H: `NewDimensionPosition`; S: `Source` |
| `RemoveDimension` | Removes a selected coordinate from 3D or 4D; 2D passes through, and selecting W in 3D passes through. | E: `RemovedDimension`; S: `Source` |
| `DomainRotatePlane` | Fixed 3D anti-artifact rotation. 2D always promotes using XY mode; 4D rotates XYZ and preserves W. | E: `RotationType`; S: `Source` |

## Important constructed defaults

Configure important behavior explicitly. FastNoise2 constructs C++ objects directly; it does not replay every metadata
default through setters, and metadata min/max fields are not validation rules.

| Member | FastNoise2 1.1.1 constructed default |
| --- | ---: |
| `FeatureScale` | 100; zero is invalid because it is used as a divisor |
| coherent `OutputMinimum` / `OutputMaximum` | -1 / 1 |
| generator `SeedOffset` | 0 |
| `SeedOffset` node offset | 1 |
| fractal `Octaves`, `Lacunarity`, `Gain`, `WeightedStrength` | 3, 2, 0.5, 0 |
| cellular `GridJitter`, `SizeJitter`, `MinkowskiP` | 1, 0, 1.5 |
| cellular `DistanceIndex0`, `DistanceIndex1` | 0, 1 |
| warp `WarpAmplitude`, axis amplitude scale | 50, 1 per axis |
| smooth min/max `Smoothness` | 0.1 |
| terrace `Smoothness` | 0 |
| `FadeMinimum`, `FadeMaximum`, `Fade` | -1, 1, 0 |
| remap From range / To range | -1..1 / 0..1 |

FastNoise2 1.1.1 has inconsistent domain-warp `SeedOffset` behavior. Direct Simplex and Gradient warps apply the offset
twice, direct SuperSimplex applies it once, and fractal warp use applies it once for Simplex/Gradient but not for
SuperSimplex. Leave this value at zero unless the pinned behavior is intentional.

## Sampling methods

All batch methods have overloads with and without a final `Span<float> outputMinMax`. Index 0 receives minimum, index 1
receives maximum, and later elements are untouched. The range covers exactly the samples written. FastNoise2 1.1.1
computes it internally even when the span is omitted, so omission saves only the two-value copy. It is meaningful only
for finite numeric output, not `ConvertRgba8` packed pixels.

| API | Count and layout | Coordinate rule |
| --- | --- | --- |
| `GenUniformGrid2D` | `count.X * count.Y`; `output[y * count.X + x]` | `offset + index * step` |
| `GenUniformGrid3D` | `count.X * count.Y * count.Z`; X, then Y, then Z | `offset + index * step` |
| `GenUniformGrid4D` | `count.X * count.Y * count.Z * count.W`; X, Y, Z, W | `offset + index * step` |
| `GenPositionArray2D/3D/4D` | Exactly `output.Length`; caller order | `axis[i] + offset.Axis` |
| `GenTileable2D` | `size.X * size.Y`; X then Y | Maps the tile to a 4D hypertorus; no offset parameter |
| `GenSingle2D/3D/4D` | One value | Exact supplied vector |

Grid counts must be positive, their product must fit `Int32`, and `output` must hold the complete product. Extra output
elements are untouched. Zero or negative steps are valid and repeat or reverse coordinates. For a 3D/4D slice, use a
singleton Y, Z, or W axis instead of X for better SIMD utilization.

Position arrays use structure-of-arrays storage. `output.Length` is the count, every coordinate span must be at least
that long, extra coordinates are ignored, and empty output is rejected. Reusing positions can avoid repeated coordinate
construction and can be faster than uniform-grid generation.

Tileable sampling makes the first and last samples adjacent across the seam; the opposite edge samples are not normally
duplicates. The graph must behave meaningfully in 4D.

Single-sample methods substantially underutilize SIMD lanes. Use a batch method for more than one sample.

Input positions, output, and min/max storage must not overlap. Immutable graphs can be sampled concurrently only when
each call owns independent output and min/max buffers.

## Encoded trees and raw bindings

Use `CreateEncoded` for Base64 trees copied from the upstream Node Editor. Encoded trees contain runtime metadata IDs and
member indexes, so treat them as assets coupled to the pinned FastNoise2 version and reverify them during upgrades. The
AlvorKit binding can load encoded trees but cannot export them or connect to Node Editor IPC.

Use raw `Fn` only for binding work, metadata verification, or tooling that genuinely needs unknown runtime members. Raw
callers must resolve metadata IDs and exact names, distinguish float/integer/enum kinds, manage every `FnNode` reference,
avoid incomplete and cyclic graphs, and pass the same SIMD ceiling to every node in a tree.

The raw binding exposes every one of the 45 functions in FastNoise2 1.1.1's `FastNoise_C.h`. C++-only functionality is
not available: graph serialization and editable `NodeData`, current configured-value/connection introspection, custom
FastSIMD nodes, memory-pool tuning, SmartNode reference-count queries, metadata display-name/UI drag helpers, and Node
Editor IPC. The exact boundary and C-symbol inventory are recorded in the agent-readable catalog.

Native nodes use intrusive references and shared allocation pools that default to 64 KiB; FastNoise2 does not allocate
one pool per node. The C ABI does not expose `SmartNodeManager::SetMemoryPoolSize`, and ordinary authored graphs should
not need it.

## Agent-readable catalog and verification

The NuGet package includes `docs/fastnoise2-features.json`. It contains:

- Every managed enum and enum value, including exact upstream metadata spellings.
- Every public wrapper signature and its purpose.
- Every one of the 47 runtime nodes with all variables, required sources, hybrids, enum options, and showcase values.
- Every generation shape and exposed/unexposed binding capability.
- Every C API symbol and pinned upstream behavior that requires a wrapper or documentation response.
- Ownership, lifetime, hybrid-replacement, validation, sampling, and enum-ordinal contracts.

The repository verifier cross-checks the catalog against runtime metadata:

```powershell
dotnet run --project demos/AlvorKit.FastNoise2.Demo --configuration Release -- --verify
```

The wrapper's XML documentation build fails on any undocumented public type, method, or enum member.

## Upstream references

- [FastNoise2 repository and feature overview](https://github.com/Auburn/FastNoise2)
- [FastNoise2 1.1.1 C API](https://github.com/Auburn/FastNoise2/blob/v1.1.1/include/FastNoise/FastNoise_C.h)
- [Node graph architecture](https://github.com/Auburn/FastNoise2/wiki/Node-Graph-Architecture)
- [Noise type guide](https://github.com/Auburn/FastNoise2/wiki/Understanding-Noise-Types)
- [Node Editor](https://auburn.github.io/fastnoise2nodeeditor/)
