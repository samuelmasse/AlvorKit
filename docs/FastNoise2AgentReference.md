# FastNoise2 Agent Reference

This is the working reference for agents changing, debugging, or extending
FastNoise2 use in AlvorKit. It is not an upstream manual. It explains the parts
that matter inside this repo and links back to primary upstream documentation.

Research date: 2026-06-22.

## Mental Model

FastNoise2 is a node graph noise library. Instead of configuring one mutable
object and calling `GetNoise(x, y, z)`, build a tree of nodes and generate from
the root node. Simple coherent noise can be a single `Simplex`, `Perlin`, or
`Value` node. More useful procedural terrain usually wraps a source generator in
fractal, modifier, operator, or domain warp nodes.

The root matters. If the graph is:

```text
Simplex -> FractalFBm -> DomainWarp
```

call generation on `DomainWarp`, not on `Simplex` or `FractalFBm`.

Node graphs are the performance model. Upstream keeps combinations such as
fractal layers, blends, modifiers, and warps inside the SIMD pipeline instead
of generating several arrays and combining them later. That is why AlvorKit
should prefer a real FastNoise2 graph over several separate `Gen...` calls plus
C# scalar composition when the same result can be expressed as nodes.

## Local Package Shape

Relevant local paths:

- `native/fastnoise2`: native package description and pinned native artifact.
- `out/bindgen/AlvorKit.FastNoise2`: generated managed C API surface.
- `out/bindgen/AlvorKit.FastNoise2.Backend`: generated native entry points.
- `demos/AlvorKit.FastNoise2.Demo`: small visual testbed.

The generated managed entry point is `AlvorKit.FastNoise2.Fn`. Production code
normally instantiates `FnBackend` from the backend package:

```csharp
using AlvorKit.FastNoise2;

var fn = new FnBackend();
```

The API is intentionally close to upstream C. Node handles use the same boring
opaque-handle pattern as GLFW:

```csharp
public readonly record struct FnNode(nint Handle);
```

`FnNode` does not own lifetime and has no helper methods. Callers own the node
handles they create and should release them with `DeleteNodeRef`. Output buffers
are caller-owned and are only pinned for the duration of a managed span overload.

## Creating Nodes

There are two ways to create a graph:

- `NewFromMetadata(id, uint.MaxValue)` creates a node by runtime metadata ID.
- `NewFromEncodedNodeTree(encoded, uint.MaxValue)` loads an encoded graph from
  the FastNoise2 Node Editor.

Prefer metadata lookup by node name for small code-built graphs. Avoid hardcoded
metadata IDs; IDs are an implementation detail.

```csharp
static (FnNode Node, string Name) CreateNode(Fn fn, string wantedName)
{
    var count = fn.GetMetadataCount();
    for (var i = 0; i < count; i++)
    {
        fn.GetMetadataName(i, out var name);
        if (!string.Equals(name, wantedName, StringComparison.OrdinalIgnoreCase))
            continue;

        var node = fn.NewFromMetadata(i, uint.MaxValue);
        if (node != default)
            return (node, name ?? wantedName);
    }

    throw new InvalidOperationException($"FastNoise2 node not found: {wantedName}");
}
```

## Configuring Nodes

FastNoise2 metadata exposes three input categories:

- Variables: plain float, int, or enum fields set with `SetVariableFloat` or
  `SetVariableIntEnum`.
- Node lookups: required source node links set with `SetNodeLookup`.
- Hybrids: inputs that can be a constant float or a source node, set with
  `SetHybridFloat` or `SetHybridNodeLookup`.

Use metadata names to find indices:

```csharp
static int FindVariable(Fn fn, FnNode node, string variableName)
{
    var metadataId = fn.GetMetadataID(node);
    var count = fn.GetMetadataVariableCount(metadataId);
    for (var i = 0; i < count; i++)
    {
        fn.GetMetadataVariableName(metadataId, i, out var name);
        if (string.Equals(name, variableName, StringComparison.OrdinalIgnoreCase))
            return i;
    }

    throw new InvalidOperationException($"Variable not found: {variableName}");
}
```

For `FractalFBm` in the local v1.1.1 binding, the useful inputs are:

- `Source` node lookup
- `Octaves` int variable, default `3`
- `Lacunarity` float variable, default `2`
- `Gain` hybrid, default `0.5`
- `Weighted Strength` hybrid, default `0`

For `Simplex`, `SuperSimplex`, and `Perlin`, the useful variables are:

- `Feature Scale`, default `100`
- `Seed Offset`, default `0`
- `Output Min`, default `-1`
- `Output Max`, default `1`

## Generating Values

Use batch APIs for real work:

- `GenUniformGrid2D`: 2D textures and heightmaps.
- `GenUniformGrid3D`: voxel terrain, volumes, or a 2D slice through 3D noise.
- `GenUniformGrid4D`: animated or parameterized higher-dimensional fields.
- `GenPositionArray*`: arbitrary positions when the sample points are not a
  regular grid.
- `GenTileable2D`: seamless tileable textures.

Avoid `GenSingle*` in loops. The generated docs call these single-sample APIs
very slow because they underuse SIMD lanes.

Uniform grid offsets are world-space starting positions. Step sizes are the
world-space distance between adjacent samples. For 2D output, values are
row-major as `values[y * width + x]`. For 3D output, X is the innermost axis:

```text
values[(z * yCount + y) * xCount + x]
```

When taking a 2D slice through 3D noise, use `zCount = 1` rather than
`xCount = 1`; the generated docs warn that tiny X counts are bad for
performance because of how positions are generated.

## Feature Scale vs Step Size

Do not confuse node `Feature Scale` with grid `Step`.

- `Feature Scale` belongs to the generator node and controls the scale of the
  noise features inside that node.
- `Step` belongs to a generation call and controls how far apart the sampled
  world positions are.

Upstream explicitly treats grid step size as independent from node feature
scale. In practice:

- Use `Step = 1` when sampling one voxel or pixel per world unit.
- Change `Step` for camera zoom or lower-resolution previews.
- Change `Feature Scale` when porting a frequency setting or changing the
  intrinsic size of noise features.

FastNoiseLite's default frequency is `0.01`. The closest FastNoise2 graph-level
equivalent in this repo is a coherent source node with `Feature Scale = 100`.
That keeps features around one cycle per 100 world units while allowing grid
`Step` to stay at `1`.

## FastNoiseLite Migration Notes

FastNoiseLite style:

```csharp
var generator = new FastNoiseLite();
generator.SetFractalType(FastNoiseLite.FractalType.FBm);
generator.SetSeed(seed);
var value = generator.GetNoise(x, y, z);
```

FastNoise2 equivalent shape:

```text
Simplex -> FractalFBm
```

Suggested local configuration:

- Source node: `Simplex`
- Source `Feature Scale`: `100`
- Source `Seed Offset`: `0`
- Fractal node: `FractalFBm`
- Fractal `Octaves`: `3`
- Fractal `Lacunarity`: `2`
- Fractal `Gain`: `0.5`
- Fractal `Weighted Strength`: `0`
- Generation seed: same seed that FastNoiseLite received via `SetSeed`
- Generation positions: same `x`, `y`, `z` world coordinates

This matches settings, not necessarily exact bits. FastNoise2 and FastNoiseLite
are separate implementations and the closest algorithm names may not be
byte-identical. For gameplay migration, compare generated terrain statistically
and visually rather than requiring exact sample equality.

## Craftdig-Style 3D Terrain Recipe

The Craftdig native backend used FastNoiseLite with only:

```csharp
generator.SetFractalType(FastNoiseLite.FractalType.FBm);
generator.SetSeed(meta.Seed);
```

Terrain sampled `GetNoise(x, y, z) + 0.5f` and compared that against a vertical
bias. To reproduce that behavior with FastNoise2:

1. Build `FractalFBm(Simplex)` using the defaults listed above.
2. Generate a 3D grid with `Step = 1`.
3. Use the chunk or preview origin as `xOffset`, `yOffset`, `zOffset`.
4. Pass the world seed to the generation call.
5. Apply the same density transform and bias in game code unless the whole
   density expression is later moved into the FastNoise2 graph.

For a 2D visual preview of the terrain density near the surface center:

```csharp
fn.GenUniformGrid3D(
    rootNode,
    values,
    xOffset,
    yOffset,
    zSlice,
    width,
    height,
    1,
    1f,
    1f,
    1f,
    seed,
    minMax);
```

## Encoded Node Trees

For complex graphs, use the FastNoise2 Node Editor and copy an encoded node
tree. Load it with:

```csharp
var node = fn.NewFromEncodedNodeTree(encodedNodeTree, uint.MaxValue);
```

This is the best route for artistic iteration. It avoids hand-writing many
metadata lookups and keeps the graph editable in upstream tooling. Still keep
small, important gameplay graphs readable in code when reproducibility and code
review matter more than visual authoring.

## Memory and Ownership

FastNoise2 writes into caller-provided output memory. It does not own that
memory and must not retain the pointer after generation returns. AlvorKit's
generated span overloads:

1. Pin the span for the duration of the native call.
2. Pass the pinned pointer to the raw C API.

They do not validate that counts match span lengths. Callers must pass buffers
large enough for the native call.

This is why an existing `float[]`, pooled array, or section buffer can be passed
as `Span<float>` safely:

```csharp
var values = new float[xCount * yCount * zCount];
fn.GenUniformGrid3D(rootNode, values, x0, y0, z0, xCount, yCount, zCount, 1f, 1f, 1f, seed);
```

Do not allocate a fresh buffer inside every section or frame unless the code is
known to be cold. Reuse buffers for chunk generation, demo textures, and tests.

## Native Build Notes

Windows FastNoise2 native builds must compile with ClangCL. Upstream recommends
ClangCL because MSVC has SIMD compiler bugs that can cause incorrect FastNoise2
generation. The local `native/fastnoise2/conf/native-build.yml` manifest forces
both CMake compilers to `clang-cl` for Windows:

```yaml
windows:
  cmakeOptions:
    - -DCMAKE_C_COMPILER=clang-cl
    - -DCMAKE_CXX_COMPILER=clang-cl
```

The native build script is tailored to the GitHub-hosted Windows runners used by
`.github/workflows/native-packages.yml`: `windows-2025` for x64/x86 and
`windows-11-arm` for arm64. Those images include Visual Studio with the C++
toolset and `Microsoft.VisualStudio.Component.VC.Llvm.Clang`. The script asks
`vswhere` for one Visual Studio installation containing both components, then
launches that developer shell.

If a Windows machine lacks the Visual Studio ClangCL component, the build should
fail instead of searching custom local installs or falling back to MSVC.

FastNoise2 v1.1.1 also needs two local source patches during native package
builds:

- `src/CMakeLists.txt` is patched so the package manifest can pass explicit
  FastSIMD feature sets per RID.
- GNU builds only receive `-mno-vzeroupper` on x86 targets, because ARM GCC
  rejects that x86-only flag.
- `linux-arm` caps FastSIMD max/default features to `NEON` so v1.1.1 does not
  select AArch64-only intrinsics for 32-bit ARM.

Linux FastNoise2 builds intentionally allow the exact C++ runtime SONAMEs
reported by `readelf`, including `libstdc++.so.6` and `libgcc_s.so.1`.

## Agent Workflow

When changing FastNoise2 code in this repo:

1. Read this file.
2. Inspect the generated API in `out/bindgen/AlvorKit.FastNoise2`.
3. If you need node names or defaults, enumerate metadata at runtime using
   `FnBackend`; do not guess indices.
4. Prefer graph nodes over multiple separate native generation passes when the
   result can be represented in FastNoise2.
5. Build the relevant project after binding or graph changes.
6. For demos, verify visually with AlvorSense screenshots.
7. For scripted drag tests, remember that apps may reset drag state only during
   `Update`; after `mouse Left up`, send one `update 0.016` before starting a
   new scripted drag.

## Common Pitfalls

- Generating from an inner source node instead of the graph root.
- Forgetting to set required node lookups before generation.
- Treating `Step` as FastNoiseLite `frequency`.
- Hardcoding metadata IDs.
- Expecting FastNoiseLite and FastNoise2 to produce identical values for the
  same conceptual settings.
- Using `GenSingle*` loops for section or texture generation.
- Applying viewport-local min/max normalization when testing panning; that makes
  the image appear to change shape as the camera moves.
- Forgetting to release node handles on unload/dispose.

## Primary Sources

- FastNoise2 README: https://github.com/Auburn/FastNoise2
- FastNoise2 Getting Started: https://github.com/Auburn/FastNoise2/wiki/Getting-started-using-FastNoise2
- FastNoise2 Node Graph Architecture: https://github.com/Auburn/FastNoise2/wiki/Node-Graph-Architecture
- FastNoise2 Fractal node reference: https://github.com/Auburn/FastNoise2/wiki/Nodes%23-Fractal
- FastNoise2 Compiling guide: https://github.com/Auburn/FastNoise2/wiki/Compiling-FastNoise2
- FastNoise2 Node Editor IPC: https://github.com/Auburn/FastNoise2/wiki/Node-Editor-IPC
- FastNoiseLite Documentation, for migration defaults: https://github.com/Auburn/FastNoiseLite/wiki/Documentation
