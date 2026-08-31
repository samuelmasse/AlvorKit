# FastNoise2 Capability Guide

## Scope

Read this guide before designing or changing coherent procedural noise, noise
node graphs, fractal terrain, cellular fields, domain warping, procedural
textures, or arbitrary-position noise sampling in AlvorKit or an inheriting
game repository.

This guide covers the FastNoise2 1.1.1 C API exposed by AlvorKit. The structured
agent database is [`res/fastnoise2/features.json`](../../res/fastnoise2/features.json).
The database is an executable coverage contract: the FastNoise2 demo compares
its node, group, variable, enum, required-source, and hybrid-input inventory
against runtime metadata. Schema 3 also inventories all 45 C symbols, every
wrapper enum/value and method family, binding boundaries, ownership rules,
sampling contracts, and pinned upstream behavior that needs special care. The package's
canonical human-readable reference is
[`src/AlvorKit.FastNoise2.Graph/README.md`](../../src/AlvorKit.FastNoise2.Graph/README.md).

## Required Mental Model

FastNoise2 is a graph evaluator, not a collection of unrelated scalar noise
functions. Build one graph, call a generation method on its outermost node, and
let FastNoise2 fuse the entire graph into one SIMD pipeline. Do not generate
separate arrays for nodes that can be composed inside the graph.

Every configurable member is one of three kinds:

- **Variable:** a float, int, or enum that is constant across the generation
  call. Per-dimension variables appear once for each of X, Y, Z, and W.
- **Required source:** a node connection that must be wired before generation.
  An unwired source is an invalid graph, not a request for a default value.
- **Hybrid:** either a constant float or a node connection. Use the float form
  for a uniform parameter and the node form when the parameter should vary over
  space. Connecting a node makes it active instead of the stored constant.
  FastNoise2 1.1.1 cannot detach it; the wrapper rejects a later constant
  assignment instead of silently updating a dormant value. It also rejects
  float mutation on an encoded root because the C API cannot reveal that
  root's preexisting hybrid connections.

Use `AlvorKit.FastNoise2.Graph` for authored graph construction. Its enums keep
node types, float variables, integer variables, enum choices, required sources,
and hybrids out of application-owned string tables. It validates those semantic
keys against exact runtime metadata and rejects cyclic wrapper-built graphs
during cold configuration. The graph retains a finalizable handle per
`Create` or `CreateEncoded` result.

```csharp
var graph = new FnGraph(fn);

var source = graph.Create(FnNodeType.CellularValue)
    .Float(FnFloatVariable.FeatureScale, 112f)
    .DistanceFunction(FnDistanceFunction.EuclideanSquared)
    .Hybrid(FnHybrid.GridJitter, 1f);

var root = graph.Create(FnNodeType.FractalFbm)
    .Integer(FnIntegerVariable.Octaves, 5)
    .Hybrid(FnHybrid.Gain, 0.5f)
    .Source(FnSource.Source, source);
```

The package uses `Vec2`/`Vec3`/`Vec4` offsets and steps, integer vectors for
grid counts, caller-owned spans, and typed overloads for every generation
shape. Every `FnGraphNode` value keeps its graph and retained `SafeHandle`s
alive. Their finalizers release external native references after the graph and
all node values become unreachable. `FnGraph` does not require clearing or
disposal. Nodes from different graphs cannot be connected. Connect every
required source before sampling: the hot generation path intentionally does
not revalidate graph completeness or ownership.

Use the raw `Fn` metadata surface only in bindings, exhaustive metadata
verification, dynamic tooling that genuinely needs unknown runtime members, or
the old-pattern half of
[`AlvorKit.FastNoise2.Graph.Demo`](../../demos/AlvorKit.FastNoise2.Graph.Demo).
Raw callers use `SetVariableIntEnum` for enum entries and the database's
`integerVariableNames`, and `SetVariableFloat` for every other variable. They
must release every handle obtained from `NewFromMetadata` or
`NewFromEncodedNodeTree` with `Fn.DeleteNodeRef`.

## Coordinate, Scale, And Seed Rules

- `Feature Scale` is feature size in world units. It is the inverse of
  frequency: `featureScale = 1 / frequency`. A larger value makes larger,
  smoother features. It is separate from generation step size.
- Generation offsets choose the sampled world-space origin. Step sizes choose
  the distance between adjacent samples. Reuse these rather than rebuilding a
  graph to pan or change sampling density.
- The generation seed changes the whole graph. A generator's `Seed Offset`
  changes that generator without changing sibling seeds. The dedicated
  `SeedOffset` modifier instead changes the seed passed through its complete
  child graph. Use generator offsets to decorrelate sibling sources.
- Keep `Seed Offset` at zero on domain-warp nodes unless pinned 1.1.1 behavior
  is deliberate. Direct Simplex and Gradient warps apply it twice, direct
  SuperSimplex applies it once, and fractal warp use applies it once for
  Simplex/Gradient but not for SuperSimplex.
- Coherent generators default to an output range of `[-1, 1]`. Their `Output
  Min` and `Output Max` variables rescale inside the SIMD graph. Fractals,
  operators, distance nodes, and modifiers can exceed that range.
- Per-dimension metadata entries share a name and carry a dimension index. Match
  both. In the database, `Multiplier.X` means the X entry named `Multiplier`.

## Choosing A Source

| Need | Start with | Important configuration |
| --- | --- | --- |
| General natural terrain or masks | `Simplex` | Set `Feature Scale`; this is the default coherent source. |
| Maximum smoothness and isotropy | `SuperSimplex` | Higher quality and slower than `Simplex`. |
| Classic grid-gradient character | `Perlin` | Expect some grid-direction character. |
| Smooth lattice-value variation | `Value` | Hermite-interpolated random values rather than gradient vectors. |
| Independent per-position randomness | `White` | No spatial continuity; do not use as a terrain height field. |
| Voronoi cell identities | `CellularValue` | Choose distance function, cell value index, and jitter. |
| Borders, caves, and distance fields | `CellularDistance` | Choose distance indexes and how they combine. |
| Custom value at each cell | `CellularLookup` | Wire `Lookup`; it is evaluated at the closest jittered cell position. |
| Multiple spatial scales | `FractalFBm` or `FractalRidged` | Wire `Source`; set octaves, lacunarity, gain, and weighted strength. |
| Organic coordinate distortion | A `DomainWarp*` node | Wire `Source`; set feature scale, warp amplitude, and axis amplitude scaling. |

Prefer `FractalFBm` for natural multi-octave detail and `FractalRidged` for
sharp ridges. Domain-warp fractals are different: their required `Domain Warp
Source` must be a `DomainWarp` node such as `DomainWarpSimplex`, and that warp
node must itself have its output `Source` wired.

## Complete Node Inventory

The runtime exposes 47 nodes. The exact member lists and enum values are in the
structured database; the table below is the decision index.

### Basic generators

| Node | Use and configuration |
| --- | --- |
| `Constant` | Uniform scalar source. Set `Value`; useful for graph wiring and hybrid equivalents. |
| `White` | Uncorrelated seeded values. Configure seed offset and output range. |
| `Checkerboard` | Alternating N-dimensional cells sized by `Feature Scale`. |
| `SineWave` | Periodic wave sized by `Feature Scale`; useful as a signal or mask. |
| `Gradient` | Sum of `(position + Offset) * Multiplier` per dimension. Offsets are hybrids. |
| `DistanceToPoint` | Distance from a configurable X/Y/Z/W point in the current domain. Select one of six distance functions; `Minkowski P` matters only for Minkowski. |

### Coherent noise

| Node | Use and configuration |
| --- | --- |
| `Simplex` | Recommended general coherent noise; high quality, fast, and low in directional artifacts. |
| `SuperSimplex` | Smoother, more isotropic coherent noise at higher cost. |
| `Perlin` | Classic grid-gradient noise with recognizable Perlin character. |
| `Value` | Interpolated grid values; fast, soft, and more grid-visible. |
| `CellularValue` | Value of the Nth closest cell. Configure `Value Index`, distance function, and jitter. |
| `CellularDistance` | Distance to or combination of two nearest-cell indexes. Five return modes are available. |
| `CellularLookup` | Evaluates required `Lookup` at the closest jittered cell position, using the incoming seed + 1. |

All cellular nodes expose `Minkowski P`, `Grid Jitter`, and `Size Jitter` as
hybrids. `Grid Jitter = 0` produces a uniform grid; values above 1 can introduce
grid artifacts. Size jitter varies cell sizes and can also create artifacts.

### Fractals and domain warps

| Node | Use and configuration |
| --- | --- |
| `FractalFBm` | Natural layered detail. Wire `Source`; configure octaves, lacunarity, gain, and weighted strength. |
| `FractalRidged` | Inverted layered detail for peaks, ridges, and canyons. Same inputs as FBm. |
| `DomainWarpGradient` | Fast gradient-grid coordinate warp. Wire the noise being warped as `Source`. |
| `DomainWarpSimplex` | Higher-quality simplex coordinate warp with two vectorization schemes. |
| `DomainWarpSuperSimplex` | Smoothest built-in warp and the most expensive of the three. |
| `DomainWarpFractalProgressive` | Each warp octave receives the previous octave's warped position. Wire a `DomainWarp` node. |
| `DomainWarpFractalIndependent` | Every octave warps the original position and the offsets accumulate. Wire a `DomainWarp` node. |

`Warp Amplitude` is maximum displacement in world units and may itself be a
node. `Amplitude Scaling.X/Y/Z/W` changes displacement per axis. Progressive
warping usually looks more twisted; independent warping preserves a clearer
relationship to the original domain.

### Operators, blends, and value modifiers

| Node | Use and configuration |
| --- | --- |
| `Add` / `Multiply` | Required `LHS`, hybrid `RHS`; combine fields without intermediate arrays. |
| `Subtract` / `Divide` / `Modulus` | Both operands are hybrids. Avoid zero divisors for divide and modulus. |
| `Min` / `Max` | Required `LHS`, hybrid `RHS`; hard union/intersection-like combinations. |
| `MinSmooth` / `MaxSmooth` | Smooth min/max with hybrid `Smoothness`; useful for soft field transitions. |
| `PowFloat` | Hybrid value and exponent. FastNoise2 powers the absolute value, so it does not preserve the input sign. |
| `PowInt` | Required value and integer exponent of at least 2; faster than `PowFloat`. |
| `Fade` | Required A/B sources, hybrid fade signal/range, and Linear/Hermite/Quintic easing. A zero fade range resolves to the midpoint. |
| `Abs` | Absolute value of required `Source`. |
| `SignedSquareRoot` | Square root of absolute source magnitude while preserving sign. |
| `PingPong` | Reflects a scaled source between extremes to create flowing contour-like patterns. |
| `Remap` | Maps a source from one hybrid range to another, optionally clamping output. Avoid a zero source range. |
| `Terrace` | Quantizes a source by `Step Count`; hybrid `Smoothness` softens transitions. Use a positive, nonzero step count. |
| `SeedOffset` | Changes the seed passed to a required child graph. |
| `ConvertRGBA8` | Packs clamped grayscale RGBA8 bits into each output float. Reinterpret bits; the numeric float value and reported min/max are not meaningful. |
| `GeneratorCache` | Thread-local cache for repeated evaluation of the same child at identical position and seed within a graph. It is not a field or chunk cache. |

### Domain modifiers

| Node | Use and configuration |
| --- | --- |
| `DomainScale` | Uniformly scales coordinates before the required source. This changes apparent frequency, not output amplitude. |
| `DomainAxisScale` | Per-axis X/Y/Z/W coordinate scale for stretching or compressing features. |
| `DomainOffset` | Per-axis hybrid coordinate offsets without changing dimensionality. |
| `DomainRotate` | Roll rotates X, pitch Y, and yaw Z, in radians. In 2D, yaw alone rotates; 4D passes through unrotated. |
| `DomainRotatePlane` | Faster preset rotation for improving XY or XZ plane quality in 3D. A 2D call is promoted to a rotated 3D plane. |
| `AddDimension` | Appends a hybrid coordinate to 2D or 3D input; 4D input remains 4D. Useful for slices and time parameters. |
| `RemoveDimension` | Drops a selected axis from 3D/4D input; a 2D call remains 2D. |

## Generation Surface

| API | Use |
| --- | --- |
| `GenUniformGrid2D` | Row-major textures and height fields; X is the inner loop. |
| `GenUniformGrid3D` | Volumes and voxels; X, then Y, then Z. Use Y or Z count 1 for a slice rather than X count 1. |
| `GenUniformGrid4D` | 4D fields and animated 3D slices; X, Y, Z, then W. |
| `GenPositionArray2D/3D/4D` | Arbitrary or reusable caller-owned positions. For repeated grids, cached position arrays can be faster than rebuilding grid coordinates. |
| `GenTileable2D` | Seamless 2D output by mapping the domain onto a 4D hypertorus. The root graph must behave correctly for 4D evaluation. |
| `GenSingle2D/3D/4D` | A genuinely isolated sample only. Per sample, these are much slower because SIMD lanes are underused. |

Batch methods can fill a two-float min/max span. The pinned native build computes
that range on every batch call; omitting the span avoids only copying the two
values. Use it for normalization only when the graph returns numeric scalar
values; it is not valid for `ConvertRGBA8`. Caller-owned spans must be large
enough for the complete output and all position arrays must contain at least
`count` entries.

All generation calls are thread-safe on an immutable node tree. The same tree
may generate into independent output buffers concurrently. Do not change node
variables or connections while another thread is generating.

## Practical Graph Recipes

- **Natural height field:** `FractalFBm(Source = Simplex)`. Start with feature
  scale 100, 4-6 octaves, lacunarity 2, gain 0.5, weighted strength 0.
- **Mountain ridges:** `FractalRidged(Source = Simplex)` followed by `Remap` or
  `Terrace`. Normalize after generation only if the consumer requires it.
- **Organic terrain:** `DomainWarpSimplex(Source = FractalFBm(Simplex))` when
  one warp scale is enough, or a domain-warp fractal whose warp node's source is
  the terrain graph when multi-scale distortion is required.
- **Cell borders or caves:** `CellularDistance` with
  `FnCellularReturnType.Index0AbsoluteDifference1` (upstream `Index0Sub1`), optionally
  transformed with `Abs`, `Remap`, `Terrace`, or a thresholding consumer.
- **Biome blend:** `Fade(A, B, Fade = low-frequency Simplex)` with an explicit
  fade range and interpolation curve.
- **Seamless material:** build a graph that supports 4D and call
  `GenTileable2D`; do not fake seams with edge copying.
- **Animated 2D field:** use `AddDimension` to append time and generate 2D, or
  sample a 3D/4D slice with time in Z/W.
- **Spatially varying roughness:** connect a low-frequency node to fractal
  `Gain` or `Weighted Strength` instead of generating a second array and
  combining it afterward.

## Binding Boundaries

AlvorKit's current C binding exposes all 45 symbols in `FastNoise_C.h`: graph
loading, handle lifetime, all 10 generation functions and batch range output,
active SIMD reporting, all runtime metadata, dynamic node creation, variables,
required sources, and both forms of hybrid input.

Upstream features not exposed by this binding are programmatic graph encoding,
editable `NodeData`, current value/connection introspection, custom C++/FastSIMD
nodes, `SmartNodeManager::SetMemoryPoolSize`, metadata display-name formatting,
metadata UI drag-speed hints, SmartNode reference-count queries, and Node
Editor IPC. Encoded trees exported by the upstream Node Editor can
still be loaded with `NewFromEncodedNodeTree`. Do not claim that a binding-only
consumer can export, inspect, or live-edit a graph unless the binding is
deliberately extended.

Native nodes are intrusive-reference-counted and allocated from shared pools
that default to 64 KiB. This is not 64 KiB per node. The C ABI has no pool-size
control, and ordinary authored graphs do not need one.

AlvorKit native packages build FastNoise2 with strict floating-point behavior
for byte-stable output across compiled SIMD feature sets. `FnFeatureSet.Maximum`
requests the fastest supported compiled feature set; pass a lower typed feature
set only when a deterministic deployment contract requires it. Inspect
`GetActiveFeatureSet` when diagnostics need the selected cumulative native mask.

## Verification And Maintenance

Run the exhaustive managed feature check after changing the FastNoise2 version,
binding, catalog, or demo:

```powershell
dotnet run --project demos/AlvorKit.FastNoise2.Demo --configuration Release -- --verify
```

The verifier covers all 47 nodes, 93 variable entries, 11 enums and all 44 enum
values, 32 required sources, 59 hybrids in both constant and node-backed form,
all generation shapes, encoded loading, min/max reporting, active SIMD
reporting, packed RGBA8 output, and concurrent generation. It also requires the
schema-3 C-symbol/binding/behavior inventories, all 34 public managed signatures,
and all 12 managed enum inventories. Wrapper tests additionally prove reverse
coverage: every runtime node, variable, hybrid, required source, enum, and enum
option has an exact typed representation.

When upgrading FastNoise2, regenerate only the FastNoise2 binding through the
normal bindgen workflow, update the catalog from the new runtime metadata, and
resolve every catalog mismatch deliberately. Never hide a newly exposed node or
member behind a count exception.

Primary upstream references:

- [FastNoise2 repository and feature overview](https://github.com/Auburn/FastNoise2)
- [FastNoise2 1.1.1 C API](https://github.com/Auburn/FastNoise2/blob/v1.1.1/include/FastNoise/FastNoise_C.h)
- [FastNoise2 node graph architecture](https://github.com/Auburn/FastNoise2/wiki/Node-Graph-Architecture)
- [FastNoise2 noise-type guide](https://github.com/Auburn/FastNoise2/wiki/Understanding-Noise-Types)
- [FastNoise2 Node Editor](https://auburn.github.io/fastnoise2nodeeditor/)
