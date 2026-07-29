# Agent Live Development

Use one ignored workspace to coordinate visual observation, scoped diagnostics,
structured bridges, and source-file updates against one exact running AlvorKit
development process.

The mechanisms have distinct roles:

- AlvorSense owns deterministic input, time, rendering, and screenshots.
- LiveCode inspects exact injection scopes and runs short-lived diagnostic
  commands or predefined bridges.
- Source Update compiles a unified diff to an original project `.cs` file and
  updates one existing method definition in the loaded module.

Source Update is the normal choice when the desired result is “this method in
the original file now has this body.” The replacement is ordinary compiler
output for the declaring type. Private fields, properties, methods, and captured
primary-constructor parameters therefore use their normal metadata tokens and
direct IL access. There is no handler ABI, reflection lookup, private-field
mapping, per-instance dispatch, or call-site redirection.

## Start an editable process

An editable target must be launched from an immutable Debug PE/PDB pair. Do not
run a normal project build behind a process and assume it is the same baseline.

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- start `
  --id source-demo `
  --editable-project demos\AlvorKit.Engine.SourceUpdate.Demo\AlvorKit.Engine.SourceUpdate.Demo.csproj
```

`--editable-project` builds with portable PDBs and optimizations disabled,
copies the complete output into the AlvorSense session, records PE/PDB hashes,
MVID, SDK identity, and CodeView path, and launches that immutable copy with
`DOTNET_MODIFIABLE_ASSEMBLIES=debug`.

The target must explicitly enable `RootLiveCode` and `RootSourceUpdate`.
`SourceUpdateHostOptions.FromEnvironment` rejects a normal launch without the
immutable manifest.

## Create and bind a workspace

Find the target's LiveCode session, then bind both process identities:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace init `
  --id source-demo `
  --purpose "Adjust the pulse service" `
  --session source-update-demo `
  --alvorsense source-demo

dotnet run --project scripts\AlvorKit.Script.LiveCode -- source start `
  --workspace source-demo
```

The detached coordinator owns the loaded Roslyn project, exact PE baseline, and
every acknowledged generation for the workspace. Its token-free manifest lives
at:

```text
tmp/live/<workspace-id>/source/coordinator.json
```

The LiveCode capability token remains only in process memory and per-user
discovery storage. Never copy it into a workspace, log, diff, or document.

## Edit the original file

Make a normal code edit to the real project file. Save a unified diff from the
last acknowledged source to that exact current file. Then submit both:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- source apply `
  --workspace source-demo `
  --source demos\AlvorKit.Engine.SourceUpdate.Demo\PulseService.cs `
  --diff path\to\pulse-update.diff `
  --update-id pulse-generation-1
```

The CLI copies the diff immutably beneath
`tmp/live/<workspace-id>/source/diffs/`. The compiler applies the diff to its
acknowledged source snapshot and requires the result to byte-match the current
file before emitting a delta.

Version 1 accepts exactly one existing ordinary method-body change. It rejects
declaration, signature, field, constructor, primary-constructor-capture,
attribute, base type, interface, generic-shape, async/iterator, unsafe, dynamic,
lambda, anonymous-function, and local-function changes.

An accepted command returns `queued` immediately. Advance the game through
AlvorSense so the target reaches its next safe-frame pump, then read status:

```powershell
'update 0.016' | dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send `
  --id source-demo --workspace source-demo

dotnet run --project scripts\AlvorKit.Script.LiveCode -- source status `
  --workspace source-demo
```

The coordinator advances its compiler generation only after the target returns
`applied`. Exact source and delta hashes plus method/type tokens are retained in:

```text
tmp/live/<workspace-id>/source/evidence/
```

If transport or runtime state becomes ambiguous, the status is
`restart-required`; do not emit another generation from that coordinator.

## Observe, diagnose, update, verify

Use this order:

1. Capture current visible behavior through AlvorSense.
2. Use LiveCode graph or a predefined bridge only when internal state is needed.
3. Edit the original `.cs` file and create its exact diff.
4. Submit with `source apply`.
5. Advance a safe frame and require terminal evidence.
6. Capture the visible result in the same AlvorSense session.

Use ordinary `lc/` submissions for diagnostic commands. Use `bridge/` and
`puppet/` for their respective recorded payloads. Source Update diffs belong
only under `source/diffs/`.

## Generation and cleanup rules

Every successful update is a forward runtime generation. A later edit may
restore the original source text, but it is still another forward generation,
not a runtime rollback.

An applied generation is tracked as `restart-required` in `session.json`.
Before closing:

1. Wait for every pending update to become terminal.
2. Stop the idle source coordinator.
3. Stop or intentionally restart the target process.
4. Resolve the Source Update intervention after process exit is proved.
5. Capture any required final screenshot and close the workspace.

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- source stop `
  --workspace source-demo

dotnet run --project scripts\AlvorKit.Script.AlvorSense -- stop `
  --id source-demo --workspace source-demo

dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace close `
  --workspace source-demo
```

See [`AlvorSense.md`](AlvorSense.md) for visual control and
[`LiveCode.md`](LiveCode.md) for scope, bridge, and frozen-inspection commands.
See [`SourceUpdate.md`](SourceUpdate.md) for the public method-update contract.
