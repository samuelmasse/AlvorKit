# Windows Dark Title Bar

Give every AlvorKit GLFW window a dark title bar on Windows. Always-on,
best-effort, invisible to app code: no theme detection, no preference API, no
events, no facade changes, no cross-platform work. If anything fails, the
window keeps the default light title bar and nothing else happens.

Enabled by `AlvorKit.GLFW` ≥ 3.4.14, whose `glfw3native.h` coverage added
`Glfw.GetWin32Window(GlfwWindow)` (`[SupportedOSPlatform("windows")]`).

## Design

- One internal helper in `src/AlvorKit.Windowing.Glfw`:
  `GlfwWindowsDarkMode.TryEnable(Glfw glfw, GlfwWindow window)`. A static
  partial class because `[LibraryImport]` requires one (same shape as the
  AlvorEye `WindowsNative` imports); `TryEnable` is a pure best-effort
  function with no state.
- `TryEnable` returns immediately unless `OperatingSystem.IsWindows()`; the
  Windows path is a `[SupportedOSPlatform("windows")]` member so the CA1416
  analyzer is satisfied without suppressions.
- Windows path: `Glfw.GetWin32Window` for the HWND, then
  `DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
  ref one, 4)`. The whole body sits in a `try`/`catch` that swallows
  everything; a zero HWND or non-zero HRESULT is ignored the same way.
  Failure modes covered: GLFW not on the Win32 platform, pre-2004 Windows 10
  rejecting attribute 20, a `glfw3` build without the native-access export
  (`EntryPointNotFoundException`).
- Single call site: the `GlfwWindowHost` constructor, before callers show the
  window. Demos create windows hidden and show them later, so the dark title
  bar is in place before first paint — no light flash. `AgentGlfwWindowHost`
  inherits it; agent captures are framebuffer-only, so determinism is
  unaffected.
- Runs once per window at construction. No per-frame work, no allocations.

## Steps

- [x] 1. Add `GlfwWindowsDarkMode.cs` (import + `TryEnable`) and call it from
      the `GlfwWindowHost` constructor. Build
      `demos/AlvorKit.Windowing.Demo` against the local `out/bindgen`
      3.4.14 projects.
- [ ] 2. Verify by running the Windowing demo: title bar renders dark. The
      title bar is non-client area, so use eyes or an AlvorEye OS-window
      screenshot — AlvorSense framebuffer captures cannot show it.
- [x] 3. Commit Mode: scoped lint; the new file follows the existing
      `[ExcludeFromCodeCoverage]` policy for GLFW platform glue
      (`AlvorKit.Windowing.Glfw` also sets `IsCoverageSourceProject=false`).
- [ ] 4. Bump `GlfwBindVer` to 3.4.14 in `AlvorKit.Packages.props` once CI
      publishes the package. Until then this project only builds where
      `out/bindgen` provides the local 3.4.14 binding; the 3.4.13 package
      fallback lacks `GetWin32Window`.

## Native Reference

| Item | Value |
| --- | --- |
| Attribute | `DWMWA_USE_IMMERSIVE_DARK_MODE` = `20` (`dwmapi.dll`) |
| Payload | `int` 1 = dark; ignore the returned HRESULT |
| HWND source | `Glfw.GetWin32Window(window)` |

## Explicitly Out Of Scope

Following the OS light/dark setting (registry), a theme preference API,
`ThemeChanged` events, live re-checks, palette exposure to Blend, and
macOS/Linux behavior. If any of that is ever wanted, it layers on top of this
helper without undoing it: detection decides a value, `TryEnable` grows a
`dark` parameter, and the call site stays the host constructor.
