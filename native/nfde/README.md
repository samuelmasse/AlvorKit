# AlvorKit.NFDe.Native

Native File Dialog Extended binaries for win (x64, x86, arm64), linux (x64, arm, arm64) and osx (x64, arm64).

The package version is the upstream [NFDe](https://github.com/btzy/nativefiledialog-extended) release. A fourth version segment is an AlvorKit packaging revision of the same upstream release.

Linux uses NFDe's xdg-desktop-portal backend and requires the system D-Bus library plus a working desktop portal. The Linux source build normalizes `nfdpathsetsize_t` to C `unsigned long` so the generated cross-platform binding can represent the type as `CULong` on every target.

| RID           | Binary         |
| ------------- | -------------- |
| `win-x64`     | `nfd.dll`      |
| `win-x86`     | `nfd.dll`      |
| `win-arm64`   | `nfd.dll`      |
| `linux-x64`   | `libnfd.so`    |
| `linux-arm`   | `libnfd.so`    |
| `linux-arm64` | `libnfd.so`    |
| `osx-x64`     | `libnfd.dylib` |
| `osx-arm64`   | `libnfd.dylib` |
