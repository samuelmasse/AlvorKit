# AlvorKit.FastNoise2.Native

This directory contains the native package metadata and build configuration for
FastNoise2.

The native-build script downloads the pinned upstream FastNoise2 release from
GitHub, configures it with CMake, and copies each built shared library into the
NuGet runtime layout:

```text
runtimes/<rid>/native/<library>
```

Expected library names:

- Windows: `FastNoise.dll`
- Linux: `libFastNoise.so`
- macOS: `libFastNoise.dylib`

The upstream source tree is not committed here. Keep this package as build
metadata plus built runtime artifacts only.

The native package workflow runs `dotnet run --project scripts/AlvorKit.Script.NativeBuild -- verify fastnoise2 --rid <rid>`
after each runtime build. The verifier dynamically loads the produced FastNoise2 library, checks deterministic noise fixtures,
writes `out/native-verify/fastnoise2/<rid>/report.json`, and uploads that directory as the `fastnoise2-verify-<rid>` artifact.

The first verifier pass intentionally uses the current non-strict FastNoise2 build flags. Do not add `FASTNOISE2_STRICT_FP=ON`
until the non-strict verification results have shown whether the shipped RID outputs drift.
