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
