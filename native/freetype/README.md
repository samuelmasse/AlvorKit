# AlvorKit.FreeType.Native

FreeType binaries for win-x64, win-arm64, linux-x64, linux-arm64, osx-x64 and osx-arm64

The package version is the upstream [FreeType](https://gitlab.freedesktop.org/freetype/freetype) release the binaries were built from (tag `VER-x-y-z`). Built with the internal zlib and without bzip2/png/harfbuzz/brotli, so the binaries have no dependencies. A 4th version segment (x.y.z.N) is an AlvorKit packaging revision: same upstream release, rebuilt packaging.

| RID | Binary |
| --- | --- |
| win-x64 | `freetype.dll` |
| win-arm64 | `freetype.dll` |
| linux-x64 | `libfreetype.so` |
| linux-arm64 | `libfreetype.so` |
| osx-x64 | `libfreetype.dylib` |
| osx-arm64 | `libfreetype.dylib` |
