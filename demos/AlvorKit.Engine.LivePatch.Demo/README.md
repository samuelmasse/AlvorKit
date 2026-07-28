# AlvorKit Engine Live Patch Demo

This real engine project proves the smallest useful ReJIT interception path.
`LivePatchTarget.SceneMode()` normally returns `0`. Pressing **Space** sends its
module MVID, metadata token, and replacement value `1` through the managed
interception API and native C ABI. The profiler supplies new IL to CoreCLR, and
the already-running render loop immediately changes from blue to magenta.
Pressing **R** calls `RequestRevert`, restoring the original method.

The window and UI use the engine's normal sizing and scaling. No dimensions are
hardcoded.

## Build and run

Build normally. The CoreCLR adapter uses a local generated profiler binding when
present and otherwise restores the published binding. The matching native
runtime package supplies the startup library:

```powershell
dotnet build demos\AlvorKit.Engine.LivePatch.Demo\AlvorKit.Engine.LivePatch.Demo.csproj `
  -c Release
```

Launch through the isolated child host so it selects the restored runtime asset
and enables the profiler only in the game process:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestInterception -- `
  --exec-project demos\AlvorKit.Engine.LivePatch.Demo\AlvorKit.Engine.LivePatch.Demo.csproj `
  --configuration Release `
  --module AlvorKit.Engine.LivePatch.Demo -- `
  --no-build --no-restore
```

Add `--proof` after the final `--` to execute an automated original → replace →
revert check without opening a window. Both modes print every request and native
snapshot, including callback counts and HRESULTs.

The repository also includes matching **AlvorKit: Live Patch Demo** and
**AlvorKit: Live Patch Proof** VS Code launch configurations. Breakpoints work
normally because VS Code launches and debugs the same profiled process using the
restored runtime asset from the demo's Release output.
