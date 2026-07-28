# AlvorKit Engine Live Patch Demo

This real engine project proves the smallest useful ReJIT interception path.
`LivePatchTarget.SceneMode()` normally returns `0`. Pressing **Space** sends its
module MVID, metadata token, and replacement value `1` through the managed
interception API and native C ABI. The profiler supplies new IL to CoreCLR, and
the already-running render loop immediately changes from blue to magenta.
Pressing **R** calls `RequestRevert`, restoring the original method.

The window and UI use the engine's normal sizing and scaling. No dimensions are
hardcoded.

## Build and run on Windows x64

Follow `docs/Interception.md` to build the native package and generate its
binding. Build the demo with the local native package as a restore source:

```powershell
$nativePackages = "<repo>\bin\AlvorKit.Interception.Profiler.Native\Release"
dotnet build demos\AlvorKit.Engine.LivePatch.Demo\AlvorKit.Engine.LivePatch.Demo.csproj `
  -c Release "-p:RestoreAdditionalProjectSources=$nativePackages"
```

Then launch the built demo DLL directly so the profiler is loaded only into the
game process:

```powershell
$profiler = "<repo>\out\interception-profiler\win-x64\Release\AlvorKit.Interception.Profiler.Native.dll"
$demo = "<repo>\bin\AlvorKit.Engine.LivePatch.Demo\Release\AlvorKit.Engine.LivePatch.Demo.dll"

$env:CORECLR_ENABLE_PROFILING = "1"
$env:CORECLR_PROFILER = "{3840ACF7-5AF1-49EA-BF94-5F7086C57F57}"
$env:CORECLR_PROFILER_PATH_64 = $profiler
$env:ALVORKIT_INTERCEPTION_PROFILER_PATH = $profiler

dotnet $demo
```

Use `dotnet $demo --proof` to execute an automated original → replace → revert
check without opening a window. Both modes print every request and native
snapshot, including callback counts and HRESULTs.

The repository also includes matching **AlvorKit: Live Patch Demo** and
**AlvorKit: Live Patch Proof** VS Code launch configurations. Breakpoints work
normally because VS Code launches and debugs the same profiled process.
