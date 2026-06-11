# AlvorKit.MiniAudio.Native

miniaudio binaries for win-x64, linux-x64 and osx-arm64

The package version is the upstream [miniaudio](https://github.com/mackron/miniaudio) tag the binaries were built from, compiled with `MA_DLL` plus `alvorkit_sizeof_*` helpers for the caller-allocated types. A 4th version segment (x.y.z.N) is an AlvorKit packaging revision: same upstream release, rebuilt packaging.

| RID | Binary |
| --- | --- |
| win-x64 | `miniaudio.dll` |
| linux-x64 | `libminiaudio.so` |
| osx-arm64 | `libminiaudio.dylib` |
