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

FastNoise2 builds pass `FASTNOISE2_STRICT_FP=ON` so the native package favors byte-stable noise output across SIMD feature sets
over the fastest relaxed floating-point mode. The expected fixture digests are pinned from a local strict ClangCL win-x64 release
build and should agree across CI RIDs unless a platform-specific code path still drifts. Linux and macOS builds also pass
`-ffp-contract=off` to keep GCC and Clang from introducing target-specific floating-point contraction in strict builds.
