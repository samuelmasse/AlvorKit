# AlvorKit.GLFW.Native

GLFW binaries for win (x64, x86, arm64), linux (x64, arm, arm64) and osx (x64, arm64)

The package version is the upstream [GLFW](https://github.com/glfw/glfw) tag the binaries were built from. A 4th version segment (x.y.z.N) is an AlvorKit packaging revision: same upstream release, rebuilt packaging.

The Linux binaries are built X11-only (Wayland disabled) to keep the runtime dependency set small.

| RID         | Binary           |
| ----------- | ---------------- |
| win-x64     | `glfw3.dll`      |
| win-x86     | `glfw3.dll`      |
| win-arm64   | `glfw3.dll`      |
| linux-x64   | `libglfw3.so`    |
| linux-arm   | `libglfw3.so`    |
| linux-arm64 | `libglfw3.so`    |
| osx-x64     | `libglfw3.dylib` |
| osx-arm64   | `libglfw3.dylib` |
