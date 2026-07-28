# AlvorKit Engine LiveCode Demo

This is a real `RootLoop` engine project: a living, interactive mycelial
observatory whose three animated colonies are three simultaneously active
`ColonyScope` instances. Their colors, motion, population, layout, atmosphere,
and graph relationships all come from dependencies inside those exact scopes.
The colonies continuously orbit a rendered central sun; neither the simulation
nor the responsive UI uses a hardcoded window size.

Start the game:

```powershell
dotnet run --project demos\AlvorKit.Engine.LiveCode.Demo
```

The normal engine frame loop continuously pumps LiveCode before state updates.
The window is also AlvorSense-compatible because it uses the standard
`RootLoop.RunGlfw` startup path.

This demo advertises both built-in and game-owned structured bridges while
retaining the arbitrary scoped-C# path:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- bridges `
    --session mycelial-observatory
```

The response includes `alvorsense` with an `exclusiveInput` lease and the
demo-owned `observatory` bridge with its versioned JSON schema.

## Launch with LivePatch

Build the native profiler and the Release game:

```powershell
$env:ALVORKIT_CORECLR_SOURCE = "<dotnet-runtime v10.0.9 checkout>"

dotnet run --project scripts\AlvorKit.Script.NativeBuild -- `
  build interception-profiler --rid win-x64
dotnet build demos\AlvorKit.Engine.LiveCode.Demo -c Release
```

Then use the checked-in VS Code profile:

```text
AlvorKit: Observatory (Release + LivePatch)
```

It launches the actual Release game DLL under the `coreclr` debugger with the
profiler and exact module allowlist. Breakpoints remain attached to the game
process. A normal `dotnet run` or Visual Studio launch omits the profiler and
runs the same project without LivePatch.

Confirm the live capabilities:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- `
  patch capabilities --session mycelial-observatory
```

The current response advertises ReJIT, existing-inliner repair, revert, raw IL,
multiple patches, signature validation, exact dispatch, and all four selectors.

The checked-in `Submissions/` files are durable teaching examples for this
demo. Agent-authored experiments against a running process belong in an ignored
`tmp/live/<workspace-id>/lc/` or `tmp/live/<workspace-id>/lp/` workspace as
described by [`AgentLiveDevelopment.md`](../../docs/AgentLiveDevelopment.md);
do not add one-off debugging submissions to this directory.

## Replace one ordinary method

The target is the ordinary unannotated
`ColonyGarden.Update(double)` method used by every colony. The exact source sent
to the external compiler is checked in as
[`Submissions/FasterOrbit.cs`](Submissions/FasterOrbit.cs). Its handler receives
the real `ColonyGarden` receiver and `delta`, while its constructor receives the
selected scope's own `ColonySky`:

```csharp
public sealed class FasterOrbit(ColonySky sky)
{
    [LivePatchHandler]
    public void Run(ColonyGarden receiver, double delta)
    {
        receiver.Phase += delta * 8.5;
        receiver.SolarAngle += delta * 1.65;
        receiver.SolarRadius =
            0.27f + MathF.Sin((float)receiver.Phase * 0.23f) * 0.055f;
        receiver.Anchor =
        (
            0.5f + MathF.Cos((float)receiver.SolarAngle) * receiver.SolarRadius,
            0.5f + MathF.Sin((float)receiver.SolarAngle) *
                receiver.SolarRadius *
                ColonyGarden.SolarVerticalScale
        );
        receiver.SolarAngle = Math.Atan2(
            (receiver.Anchor.Y - 0.5f) / ColonyGarden.SolarVerticalScale,
            receiver.Anchor.X - 0.5f);
        receiver.Primary =
        (
            0.55f + MathF.Sin((float)receiver.Phase) * 0.35f,
            0.18f,
            1f,
            1f
        );
        receiver.Secondary = (0.08f, 1f, 0.86f, 1f);
        receiver.OrbitRadius =
            108f + MathF.Sin((float)receiver.Phase * 0.7f) * 28f;
        receiver.SporeCount = 58;
        receiver.Form = "live-patched solar helix";
        sky.Warp =
            0.42f + MathF.Sin((float)receiver.Phase * 0.31f) * 0.32f;
        sky.Weather = "agent-authored chromatic storm";
    }
}
```

Install it only for `Moon Garden`:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch install `
  --session mycelial-observatory `
  --scope "Moon Garden" `
  --selector exact-scope `
  --target "AlvorKit.Engine.LiveCode.Demo.ColonyGarden::Update" `
  --target-assembly AlvorKit.Engine.LiveCode.Demo `
  --name "Moon solar helix" `
  --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\FasterOrbit.cs
```

The logical wire request is a normal authenticated LiveCode bridge request.
The `assembly` and `symbols` values are the base64 Roslyn outputs for the exact
source above:

```json
{
  "kind": "bridge",
  "bridge": "livepatch",
  "bridgeVersion": 1,
  "payload": {
    "operation": "install",
    "executorScopeId": 4,
    "selector": { "kind": "exactScope", "scopeId": 4 },
    "target": {
      "assembly": "AlvorKit.Engine.LiveCode.Demo",
      "type": "AlvorKit.Engine.LiveCode.Demo.ColonyGarden",
      "method": "Update"
    },
    "entryType": "FasterOrbit",
    "assembly": "<base64 PE>",
    "symbols": "<base64 portable PDB>",
    "name": "Moon solar helix"
  }
}
```

On the measured Release run, install reached `Active` in 28.484 ms. The Moon
scope changed color, morphology, population, atmosphere, and solar motion while
its two sibling receivers continued running the original method.

Replace the active handler atomically:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch replace `
  --session mycelial-observatory `
  --patch 1 `
  --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\ReverseOrbit.cs
```

The response explicitly reports:

```text
LivePatch 1 published its new handler atomically; no ReJIT was required.
```

Remove it:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch remove `
  --session mycelial-observatory `
  --patch 1
```

Managed dispatch stops immediately. The measured native original-IL restoration
completed in 1.437 ms, after which every colony was again executing the
original `ColonyGarden.Update`.

## Prove failure containment

Install
[`Submissions/ExplodingOrbit.cs`](Submissions/ExplodingOrbit.cs) for one scope.
It deliberately throws from the handler. The observed status was:

```json
{
  "state": "failed",
  "nativeRequestId": 2,
  "nativeOperation": "remove",
  "nativeState": "removed",
  "rejitElapsed": "00:00:00.0017910",
  "hResult": 0,
  "failure": "System.InvalidOperationException: The demo patch deliberately crossed a solar singularity.",
  "submissionContext": { "state": "collected" }
}
```

The game remained responsive, the original orbit resumed on subsequent frames,
the failure stayed visible in the right panel, and the submitted assembly
context became collectible.

Capture the exact live frame at any point through the existing structured
AlvorSense bridge:

```powershell
@'
screenshot out\observatory-livepatch.png
state
'@ | dotnet run --project scripts\AlvorKit.Script.LiveCode -- puppet `
  --session mycelial-observatory
```

## Interact normally

- Click or press `Tab` to select a colony executor.
- Drag or use the arrow keys to move the selected colony.
- Right-click or press `Space` to pulse the selected colony.
- Press `B` to bloom every colony and `L` to intensify their links.
- Press `F` to deliberately freeze the game loop for out-of-band inspection.

The right panel is the current injection scope graph. It shows stable IDs,
parentage, the selected exact executor, graph revision, active scopes, and ended
lifetime tombstones.

## Inspect the deliberately frozen game

The demo enables a dedicated frozen-inspection thread with a one-second stale
frame threshold. It still runs ordinary constructor-injected
`ILiveCodeCommand` classes; only the execution thread changes.

Request the freeze through the normal game-thread lane:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope mycelial-observatory `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\FreezeForInspection.cs
```

After one second, verify that the frame number stopped:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- frozen status `
    --session mycelial-observatory
```

Inspect the exact `Tide Archive` scope without pumping another game frame:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- frozen exec `
    --session mycelial-observatory `
    --scope "Tide Archive" `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\InspectFrozenColony.cs
```

The exercised Release run reported game thread `2`, inspector thread `4`,
scope `3`, engine tick `13367`, the exact orbit state, spore count, form, and
weather while the frame number remained `13369`.

Release the same blocked game thread from another frozen command:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- frozen exec `
    --session mycelial-observatory `
    --scope mycelial-observatory `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\ReleaseFrozenGame.cs
```

`frozen status` then returns `isFrozen: false` and an advancing frame number.
Submitting `frozen exec` while the game is already advancing is rejected rather
than racing ordinary gameplay.

## Let an agent rewrite the running game

Use a predefined domain bridge when the operation is already part of the
game's stable agent surface:

```powershell
@'
{
  "action": "transfigure",
  "colony": "Tide Archive",
  "spores": 88,
  "orbitRadius": 154,
  "rotationSpeed": -2.2,
  "weather": "bridge-born electric rain",
  "warp": 0.78,
  "burst": 3.4
}
'@ | dotnet run --project scripts\AlvorKit.Script.LiveCode -- bridge `
    --session mycelial-observatory `
    --name observatory `
    --version 1
```

Drive exact interactions and return a current-frame screenshot without writing
C#:

```powershell
@'
key Tab down
update 0.016
key Tab up
update 0.016
move 420 650
mouse Left down
update 0.016
mouse Left up
update 0.016
key B down
update 0.016
key B up
update 0.016
screenshot out\livecode-puppet.png
state
'@ | dotnet run --project scripts\AlvorKit.Script.LiveCode -- puppet `
    --session mycelial-observatory
```

The puppet batch owns input exclusively for its short transaction and returns
the screenshot bytes to the CLI, which writes the requested caller-side path.

Use full C# for novel inspection or mutation that no predefined bridge covers.

Inspect the graph from another terminal:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- graph `
    --session mycelial-observatory
```

Make only `Moon Garden` expand into a high-energy magenta singularity. The
command also opens and ends a nested `ProbeScope`, so its lifecycle tombstone
appears in the panel:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope "Moon Garden" `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\InspectAndAwaken.cs
```

Recompose every colony and create a new central sibling executor:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope "mycelial-observatory" `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\RewriteConstellation.cs
```

Smaller root examples can independently create `Agent Aurora` or end
`Tide Archive`:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope "mycelial-observatory" `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\CreateColony.cs

dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope "mycelial-observatory" `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\RetireColony.cs
```

Resize the current native window, change `RootScale`, bloom the colonies, and
attach a live-compiled animated overlay to `RootScripts`:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope "mycelial-observatory" `
    --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\ResizeAndRescale.cs
```

Unlike the one-shot commands, the overlay deliberately remains referenced by
the root script list, so its collectible load context stays alive until the
script is removed or the process exits.

All changes happen in the same uninterrupted game process and are visible in
the next frame. The reusable integration and security model are documented in
[`docs/LiveCode.md`](../../docs/LiveCode.md).
