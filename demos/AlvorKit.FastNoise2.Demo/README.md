# FastNoise2 Feature Demo

This project is both an interactive gallery and an exhaustive executable check
of the FastNoise2 surface exposed by AlvorKit.

## Interactive Gallery

```powershell
dotnet run --project demos/AlvorKit.FastNoise2.Demo --configuration Release
```

The window renders one typed FnGraph showcase for every supported node. Graphs are
created once on first selection and reused when navigating or reseeding.
Its title identifies the FastNoise2 version, node index, metadata groups, node,
generation mode, and seed.

- Left and Right select a node.
- Space cycles uniform 2D, a uniform 3D slice, a uniform 4D slice, and tileable
  2D generation.
- R changes the seed.
- F11 toggles fullscreen.
- Escape exits.

The console reports the selected node's purpose plus its variables, required
sources, and hybrid inputs.

## Exhaustive Verification

```powershell
dotnet run --project demos/AlvorKit.FastNoise2.Demo --configuration Release -- --verify
```

This nonvisual mode compares the versioned feature database with the live
runtime and exercises every node, variable setter, enum value, required source,
and both forms of every hybrid. It also covers uniform, position-array,
tileable, and single-value generation in every supported dimension; optional
batch min/max; encoded graph loading; packed RGBA8 output; SIMD reporting; and
concurrent use of one immutable graph. It also compares every typed gallery recipe
with its raw verification graph in all four preview shapes, requiring byte-identical
samples. Raw metadata and node handles are confined to this verification path.

See [`docs/AgentRules/FastNoise2.md`](../../docs/AgentRules/FastNoise2.md) for
the agent-facing design guide and
[`res/fastnoise2/features.json`](../../res/fastnoise2/features.json) for the
machine-readable feature database.
