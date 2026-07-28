# Agent Live Development

This guide is the operational contract for agents using AlvorSense, LiveCode,
predefined bridges, and LivePatch against a running AlvorKit development
process.

These capabilities complement each other:

- AlvorSense is the user-visible source of truth. It drives normal input and
  captures rendered evidence.
- LiveCode is the scoped debugger. It inspects or deliberately mutates injected
  runtime state without restarting.
- LivePatch is live surgery. It temporarily replaces one existing method for an
  explicit receiver set.

Use the loop:

```text
AlvorSense observe
    -> LiveCode inspect the exact scope
    -> optionally invoke a bridge, run LiveCode, or install LivePatch
    -> AlvorSense reproduce and verify
    -> clean up and prove restoration
```

## Choose The Surface

Use AlvorSense alone for acceptance checks, controls, layout, rendering, and
behavior that should be judged strictly from a normal user's perspective.

When AlvorSense reveals surprising behavior and the cause is not already
obvious, keep that same session alive and use LiveCode next. Inspect the exact
scope and runtime dependencies before adding logging, editing normal source, or
restarting the game.

Use a predefined LiveCode bridge when its typed operation already matches the
task. Use arbitrary LiveCode for novel inspection or a deliberate scoped state
change. Use LivePatch only when the experiment specifically requires replacing
an existing method body without rebuilding.

Use frozen LiveCode when the normal frame heartbeat stalled. AlvorSense cannot
advance a blocked game thread; return to AlvorSense after releasing or
restarting the target to verify user-visible recovery.

## Create A Workspace

Agent-authored live work belongs beneath the current game repository:

```text
tmp/live/<workspace-id>/
```

Initialize it only after the target has advertised a LiveCode session:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace init `
    --id orbit-debug `
    --purpose "Explain and correct the selected colony orbit" `
    --session mycelial-observatory `
    --alvorsense observatory-debug
```

When running from a game repository, use the sibling AlvorKit project path:

```powershell
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.LiveCode -- workspace init `
    --id orbit-debug `
    --purpose "Explain and correct the selected colony orbit" `
    --session MyGame.Dev `
    --alvorsense my-game-debug
```

The initializer resolves the immutable LiveCode session ID and process
identity, captures the scope graph, bridge descriptors, and optional LivePatch
capabilities, and creates:

```text
SESSION.md
session.json
baseline/
bridge/
events/
evidence/
lc/
lp/
puppet/
```

`session.json` is the machine-readable identity and cleanup ledger.
`SESSION.md` is the human-readable observation, next action, and handoff.
Never copy the LiveCode capability token into either file.

Check the association before resuming another agent's work:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace status `
    --workspace orbit-debug
```

If the recorded process is gone or its identity changed, create a new
workspace. Do not silently retarget an old workspace to the newest process with
the same display name.

## Keep Exact Inputs

Write every agent-authored C# submission to an immutable numbered file:

```text
tmp/live/orbit-debug/lc/001-inspect-orbit.cs
tmp/live/orbit-debug/lc/002-adjust-orbit.cs
tmp/live/orbit-debug/lp/001-replace-update.cs
tmp/live/orbit-debug/lp/002-reverse-update.cs
```

Do not overwrite an executed submission. A replacement gets a new number so
the event record continues to identify the exact source that ran.

Pass `--workspace` to record logical request and result JSON beneath `events/`.
Workspace-recorded C# must use `--file`; stdin submissions are rejected. The
tool also rejects `lc` or `lp` source paths outside the corresponding workspace
directory and records the source SHA-256.

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope 4 `
    --file tmp\live\orbit-debug\lc\001-inspect-orbit.cs `
    --workspace orbit-debug
```

Prefer numeric scope IDs after capturing the graph. Labels and short type names
are discovery conveniences and can become ambiguous.

Record AlvorSense input and evidence in the same event stream:

```powershell
@'
render
screenshot tmp/live/orbit-debug/evidence/001-before.png
state
'@ | dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send `
    --id observatory-debug `
    --workspace orbit-debug
```

The `--workspace` association rejects a different AlvorSense session ID.
LiveCode `puppet`, graph, bridge, frozen, and LivePatch commands support the
same option.

## Coordinate A Paused Deterministic Loop

AlvorSense deliberately stops the game clock between command batches. A
LiveCode operation that must enter a game-thread scope can therefore compile
and reach the target, then wait for the next update. This is a healthy paused
target, not a reason to restart it.

Dispatch that LiveCode command with the agent shell's background or yielded
process facility, send one recorded zero-delta update, then collect the
LiveCode result:

```powershell
# Start this without blocking the agent's next shell action.
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session mycelial-observatory `
    --scope 4 `
    --file tmp\live\orbit-debug\lc\001-inspect-orbit.cs `
    --workspace orbit-debug

dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send `
    --id observatory-debug `
    --workspace orbit-debug `
    --command "update 0 0 0"
```

Wait for and report the original LiveCode command after the update completes.
Do not launch duplicate submissions just because the first command is waiting;
the first may still execute on the next frame. Use a positive deterministic
delta only when reproducing behavior intentionally requires time to advance.

## Explain Before Mutating

Before a mutation, update `SESSION.md` and tell the user:

- the exact running process and scope;
- the submission or bridge payload;
- the expected visible or diagnostic effect;
- whether the effect is one-shot or persistent;
- the exact cleanup or restart requirement.

For a showcase, pause for approval between meaningful mutations. A normal task
that already authorizes an in-scope runtime change does not require redundant
approval, but the agent must still state the action and cleanup.

Compile failures occur outside the game process. Report them, keep the failed
source file, create a new numbered revision, and continue in the same running
session.

## Track Persistent Effects

LivePatch commands automatically track their patch ID and observed lifecycle
when `--workspace` is supplied.

Track a persistent LiveCode or bridge effect explicitly:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace add-intervention `
    --workspace orbit-debug `
    --id spatial-overlay `
    --kind livecode `
    --description "RootScripts contains the live spatial observatory overlay" `
    --source tmp\live\orbit-debug\lc\002-enable-overlay.cs `
    --cleanup "exec lc/099-remove-overlay.cs in root scope"
```

Use `--state restart-required` only when no narrower cleanup exists. A
state-changing one-shot command still needs an intervention when the changed
state persists after the command assembly unloads.

LivePatch install and replacement source belongs under `lp/`:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch install `
    --session mycelial-observatory `
    --scope 4 `
    --selector exact-scope `
    --target "AlvorKit.Engine.LiveCode.Demo.ColonyGarden::Update" `
    --target-assembly AlvorKit.Engine.LiveCode.Demo `
    --name "Moon orbit experiment" `
    --file tmp\live\orbit-debug\lp\001-replace-update.cs `
    --workspace orbit-debug
```

After removal, query status with the same workspace until the patch reports a
terminal restored state:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch remove `
    --session mycelial-observatory `
    --patch 1 `
    --workspace orbit-debug

dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch status `
    --session mycelial-observatory `
    --patch 1 `
    --workspace orbit-debug
```

For an explicitly tracked non-patch effect, mark it resolved only after
observing its cleanup:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace resolve-intervention `
    --workspace orbit-debug `
    --id spatial-overlay
```

## Verify And Close

Always return to the user-visible surface after an intervention. Repeat the
relevant input through AlvorSense, capture a screenshot, and describe what
changed. Internal values alone do not prove that a visual or interaction bug
is fixed.

Close only after every persistent intervention is resolved:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace close `
    --workspace orbit-debug
```

Closing rejects unresolved or restart-required interventions. Stop AlvorSense
sessions the agent started. Leave user-owned visible applications and sessions
running unless the user asks to stop them.

Live changes are experiments, not repository implementation. If the result
should become permanent, implement it separately in normal source, then build
and verify it through the repository's ordinary workflow.

## Technical References

- [`LiveCode.md`](LiveCode.md): host composition, commands, bridges, scopes, and
  frozen inspection.
- [`LivePatch.md`](LivePatch.md): exact handler ABI, selectors, ReJIT, failure,
  replacement, and unloading behavior.
- [`AlvorSense.md`](AlvorSense.md): deterministic user input, time, screenshots,
  and session ownership.
- [`AlvorEye.md`](AlvorEye.md): visible desktop fallback when the target is not
  wired for AlvorSense.
