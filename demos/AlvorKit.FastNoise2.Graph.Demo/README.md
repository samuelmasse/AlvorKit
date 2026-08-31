# FastNoise2 graph wrapper comparison

This code-only demo builds the same `FractalFBm(CellularValue)` graph twice:

- `OldNoisePattern.cs` uses the raw `Fn` metadata API, string and index discovery, manual native-handle lifetime, and
  scalar generation arguments. It is preserved only as a migration comparison; do not copy it into production code.
- `TypedNoisePattern.cs` uses `AlvorKit.FastNoise2.Graph` enums, fluent configuration, graph ownership and validation,
  AlvorKit vectors, and span sampling.

Both paths sample the same 4 x 3 x 2 grid with seed 4242. The program requires byte-identical output before printing it.

Run it from the repository root:

```powershell
dotnet run --project demos/AlvorKit.FastNoise2.Graph.Demo --configuration Release
```

The first line of successful output is:

```text
FastNoise2 raw metadata pattern and typed graph pattern produced identical output.
```

The old path demonstrates the work the wrapper removes: metadata enumeration, exact-name comparison, raw enum option
lookup, separate float/integer/hybrid/source setters, explicit `FnNode` tracking, reverse-order deletion, and twelve
individual scalar grid arguments. The typed path still creates the real FastNoise2 SIMD graph and produces the same
native output.
