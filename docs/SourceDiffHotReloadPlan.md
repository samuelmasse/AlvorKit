# Source-Diff Hot Reload Plan

## Status

This plan supersedes `LivePatchTargetShapePlan.md`.

The requested experience is a normal edit to the real C# source file:

```diff
--- a/demos/AlvorKit.Engine.SourceUpdate.Demo/ColonySimulation.cs
+++ b/demos/AlvorKit.Engine.SourceUpdate.Demo/ColonySimulation.cs
@@
     public void Update(double delta)
     {
-        atmospherePhase += delta;
-        garden.SporeCount++;
+        atmospherePhase += delta * 8;
+        garden.SporeCount += 10;
+        sky.Weather = "chromatic storm";
     }
```

The agent edits that original file and records the exact diff. Roslyn recompiles
the method in its real declaring type and emits an Edit-and-Continue delta. The
running process applies the metadata, IL, and PDB deltas to the original loaded
assembly.

This is a **global source update**, not a receiver-selected LivePatch handler.
It updates the original `MethodDef`; it does not redirect the method to a
generated class.

## Validated Decision

Implement a new opt-in feature called **Source Update**:

- input is the original project, original `.cs` path, exact previous source
  hash/generation, and an immutable unified diff;
- the agent applies the same diff to the real source file, so the worktree
  remains the authoritative implementation;
- the compiler loads the exact project and uses public Roslyn
  `EmitBaseline`/`Compilation.EmitDifference` APIs;
- one session-owned compiler coordinator keeps the Roslyn baseline and
  compilation chain alive for exactly as long as the editable process;
- the target applies the resulting delta with
  `MetadataUpdater.ApplyUpdate`;
- v1 accepts exactly one existing ordinary method-body edit per generation;
- the update affects every receiver of that method;
- restoration is another forward source edit and delta, not patch removal;
- Source Update and ReJIT-based interception are separate process/module modes;
  and
- after Source Update passes its acceptance gates, remove the current LivePatch
  product. LivePatch is migration scaffolding, not a retained optional surface.

Do not implement the proposed target-shaped handler compiler or generated
`UnsafeAccessor` layer for the normal source-edit workflow.

## User Experience

### Session launch

Start one source-editable Debug process through an explicit AlvorSense mode:

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- start `
    --id source-update-demo `
    --editable-project `
        demos\AlvorKit.Engine.SourceUpdate.Demo\AlvorKit.Engine.SourceUpdate.Demo.csproj
```

`--editable-project` is a new launch contract. It performs the exact build,
copies the complete runnable output to a session-private immutable directory,
and launches the copied DLL. It does not use `dotnet run`, `dotnet watch`, a
managed debugger, or the CoreCLR interception profiler.

The development executable must explicitly reference and compose
`RootSourceUpdate`. A launch flag cannot inject the bridge into an arbitrary
process. AlvorSense verifies that the expected bridge capability is present and
that only the target modules recorded in the editable launch manifest are
eligible.

Initialize the ordinary LiveCode workspace against that process:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace init `
    --id source-update-demo `
    --purpose "Edit ColonySimulation.Update in the running game" `
    --session source-update-demo `
    --alvorsense source-update-demo
```

When initialization discovers an editable AlvorSense launch, it starts one
session-owned compiler coordinator. That process loads generation 0 before any
source edit and owns the `MSBuildWorkspace`, `Solution`, `Compilation`, and
`EmitBaseline` chain. One-shot `source` commands are clients of that
coordinator. Losing the coordinator after generation 1 makes the session
restart-required; a fresh command cannot recreate the live EnC identity chain.

### Agent edit

The agent makes a normal repository edit to:

```text
demos/AlvorKit.Engine.SourceUpdate.Demo/ColonySimulation.cs
```

It saves the exact unified diff under:

```text
tmp/live/source-update-demo/source/001-update-colonies.diff
```

The source file itself contains the new code. The numbered diff is the immutable
live-session input and audit record.

### Apply

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- source apply `
    --workspace source-update-demo `
    --project demos\AlvorKit.Engine.SourceUpdate.Demo\AlvorKit.Engine.SourceUpdate.Demo.csproj `
    --source demos\AlvorKit.Engine.SourceUpdate.Demo\ColonySimulation.cs `
    --diff tmp\live\source-update-demo\source\001-update-colonies.diff
```

The command:

1. verifies the live process, module, SDK, project, PE, PDB, source, and
   generation identities;
2. sends the diff once to the session compiler coordinator, which applies it
   to the previous source snapshot in memory;
3. verifies that the result exactly equals the current real source file;
4. proves that exactly one supported method body changed;
5. compiles and emits one metadata/IL/PDB delta;
6. submits it through the authenticated Source Update bridge;
7. records the target acknowledgment before advancing the compiler baseline;
   and
8. returns the new generation and source/delta hashes.

Because Source Update is applied on the normal pre-update safe-frame lane, an
AlvorSense session that is paused between batches needs one explicit update
after the apply request is in flight. The agent starts `source apply` as a
yielded/background command, waits for its workspace-recorded
`queued-for-safe-frame` acknowledgment (also visible as the pending update ID
in `source status`). This acknowledgment means the exact update ID has been
accepted into the target's pre-update queue, not merely sent by the compiler
coordinator. The agent then sends one workspace-recorded `update 0 0 0` batch
and collects the original apply result. The readiness acknowledgment prevents
the update batch from racing ahead while Roslyn is still compiling. This is
coordination of the update operation, not per-method dispatch machinery.

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send `
    --id source-update-demo `
    --workspace source-update-demo `
    --command "update 0 0 0"
```

### Subsequent edit

The agent edits the same real source file again and writes:

```text
tmp/live/source-update-demo/source/002-adjust-atmosphere.diff
```

The next `source apply` requires generation 1 and produces generation 2.
Updates are serialized and linear per module.

### Forward restoration

To restore the earlier method body, the agent writes and applies a normal
inverse source diff:

```text
tmp/live/source-update-demo/source/003-restore-update.diff
```

This creates generation 3. It is called **forward restoration**, not removal or
rollback. Only restarting the process returns to pristine generation 0.

## Exact Semantics

For a supported method-body edit:

- Roslyn binds the code inside the original declaring type.
- `this` is the real existing object.
- Existing private fields compile to normal field access.
- Existing private properties and methods bind normally.
- Existing captured constructor dependencies retain their exact object
  identity.
- The constructor does not rerun.
- Existing object state remains in place.
- The original method metadata identity is retained.
- Future invocations use the new body.
- Invocations already executing continue with the body they entered.
- Every instance of the type uses the new method body.

There is no receiver parameter, handler attribute, generated accessor,
reflection lookup, private-field projection, handler allocation, or per-call
dispatch introduced by Source Update.

### Primary-constructor dependencies

An already-captured primary-constructor parameter works with normal C#
semantics. The baseline object already has storage for it.

An edit that newly causes an uncaptured primary-constructor parameter to become
captured is restart-required. Existing objects have no stored constructor value
that an update could recover, and their constructors cannot be rerun.

### State and exceptions

Source restoration changes future execution only. It does not undo state
mutations performed by earlier generations.

An exception from the updated method is an ordinary production exception.
Source Update does not catch it, deactivate the generation, return a default
value, or automatically fall back. The agent must correct the source and apply
another forward generation, or restart the process.

## V1 Supported Edit

V1 accepts:

- one physical, source-authored C# document;
- one pre-existing non-constructor instance or static method;
- one block-body or expression-body replacement;
- ordinary statements, expressions, locals, calls, and access to existing
  members; and
- a method whose declaration, containing type, and capture topology are
  otherwise unchanged.

V1 requires unchanged:

- assembly, module, namespace, and containing type identity;
- type kind, base types, interfaces, layout, and generic arity;
- primary-constructor and ordinary-constructor declarations;
- fields, properties, events, methods, and nested types;
- target method name, accessibility, modifiers, attributes, generic shape,
  return type, parameters, ref kinds, and custom modifiers; and
- all other project documents and generated inputs.

V1 rejects:

- new, removed, or changed declarations;
- constructor edits;
- field or property initialization changes;
- newly captured or no-longer-captured primary parameters;
- async, iterator, or async-iterator methods;
- lambdas, anonymous methods, and local functions;
- `dynamic` operations and anonymous-object creation;
- generic target methods or targets in generic types;
- unsafe code, pointers, function pointers, `stackalloc`, and ref-like
  state-machine shapes;
- generated documents;
- edits spanning more than one method or file;
- source generator, analyzer configuration, reference, package, project, or
  MSBuild property changes; and
- any edit whose metadata delta/EnC log and map add a type, field, method,
  property, event, call-site cache, delegate cache, static-data holder, or
  anonymous type rather than updating only the selected existing MethodDef.

These cases can expand only through focused compiler and runtime proofs.

## Why Public Roslyn APIs

Use:

- `Microsoft.Build.Locator`;
- `Microsoft.CodeAnalysis.Workspaces.MSBuild`;
- `ModuleMetadata`;
- `EmitBaseline`;
- `EditAndContinueMethodDebugInformation`;
- `SemanticEdit`;
- `Compilation.EmitDifference`; and
- `EmitDifferenceResult.Baseline`, `UpdatedMethods`, and `ChangedTypes`.

Do not embed Roslyn's `ExternalAccess.HotReload` service. Its orchestration and
rude-edit analyzer are internal friend-assembly APIs used by `dotnet watch`.
Pinning the SDK does not make that an appropriate public dependency.

Do not make `dotnet watch` own the AlvorSense target. It owns launch/restart and
project-history behavior, while Source Update needs an exact existing process,
an authenticated workspace, deterministic AlvorSense input, and an explicit
ordered-diff ledger.

`dotnet watch` remains a useful behavioral oracle during the vertical spike.

## Compiler Pipeline

### Toolchain pinning

The repository currently has no `global.json`, and its LiveCode tool references
a Roslyn package independently of the installed SDK compiler. Source Update
cannot accept that ambiguity.

For the isolated Phase 0 spike:

1. select a supported .NET 10 SDK;
2. invoke that exact SDK explicitly and use matching Roslyn
   Workspaces/compiler packages in the spike;
3. record the SDK, compiler, MSBuild, target framework, configuration, runtime,
   and global properties in every editable session; and
4. fail closed when the launch and compiler toolchains differ.

After the spike proves that exact combination, review and add the repo-wide
`global.json` pin before product implementation. This keeps toolchain selection
strict without making a repo-wide pin part of the experiment that is choosing
the toolchain.

### Authoritative build

The editable launch command builds the target once with:

- `Configuration=Debug`;
- `Optimize=false`;
- portable PDBs and debug symbols;
- no trimming;
- no ReadyToRun;
- no single-file publishing; and
- deterministic build inputs.

It then copies the complete runnable output, including the target DLL, PDB,
dependencies, `.deps.json`, and `.runtimeconfig.json`, to a session-private
immutable artifact directory and launches from that copy. The generation-0 PE
and PDB never come from mutable `bin/Debug` paths after launch.

It records:

- project path, target framework, configuration, and global MSBuild properties;
- SDK/compiler versions;
- immutable target DLL/PDB paths and SHA-256 hashes;
- module MVID and PE CodeView/PDB identity;
- every source document path, bytes, checksum, and encoding;
- additional documents and analyzer-config inputs;
- the full project graph and the paths, identities, and hashes of every metadata
  reference, analyzer, and generator binary;
- source-generator inputs and generation-0 generated-document checksums; and
- the complete generation-0 project/compilation identity.

The same operation launches that exact DLL with:

```text
DOTNET_MODIFIABLE_ASSEMBLIES=debug
```

The target must report `MetadataUpdater.IsSupported == true`.

Reject Release/optimized, trimmed, ReadyToRun, single-file, shadow-copied,
dynamic, locationless, stale, or unmatched PE/PDB/source configurations.

### Initial baseline

Load the exact generation-0 project through `MSBuildWorkspace` before the first
agent source edit. Keep that loaded `Solution`/`Compilation` alive in the
session coordinator. Preserve the exact generation-0 source bytes separately;
never try to recover old documents by reopening the now-edited worktree.

Verify that its compilation recreates the launched module identity and PDB
document checksums. Build the public `EmitBaseline` from:

- the exact baseline PE through `ModuleMetadata`;
- local-signature handles read from each MethodDef body through `PEReader`;
- portable-PDB Edit-and-Continue debug information; and
- the exact old `Compilation`.

Fail on any `MSBuildWorkspace` load failure, skipped project, unresolved
reference, workspace diagnostic, or project-graph/input hash mismatch. Prove
the deterministic generation-0 compilation maps to the launched MVID,
MethodDefs, CodeView/PDB identity, and document checksums; use a scratch emit
comparison where the selected toolchain supports an exact comparison.

The PDB reader and local-signature provider need focused fixtures. Do not
substitute empty or guessed metadata merely because v1 does not preserve active
locals.

### Diff and semantic validation

Read and hash the numbered unified diff once; it includes exactly one source
path. Apply it to the previous generation's stored source bytes in memory and
require:

```text
SHA256(in-memory result) == SHA256(current real source file)
```

Load the new source into the prior Roslyn solution. Compare old and new syntax
trees and semantic models.

Resolve the same old/new `IMethodSymbol` and prove the v1 edit boundary. Detect
new synthesized fields, closure/state-machine methods, or changed surrounding
symbols and reject them.

Run the configured generators as part of the new compilation and require every
generated-document path and checksum to remain identical to generation 0. A
method-body edit that changes generated output is unsupported even if generator
configuration and inputs appear unchanged.

Rehash the real source after compilation and immediately before submission. A
change observed before submission rejects the operation. Re-read after target
acknowledgment: if the file then differs from the just-applied result, the
generation remains successfully applied and the workspace records
`worktreeAhead` for the next edit. The protocol detects editor races; it does
not claim a portable exclusive lock over the real file.

### Delta emission

Construct one public `SemanticEdit` of kind `Update` from the exact old and new
method symbols, with no syntax map or local preservation. Keep this call behind
a tiny adapter for the Roslyn version selected in Phase 0; do not document or
bind to a constructor shape from a different package version.

Require:

- no compilation or emit errors;
- `EmitDifferenceResult.Success`;
- exactly one updated MethodDef;
- the expected original MethodDef token;
- parsed metadata delta and EnC log/map evidence showing no added TypeDef,
  FieldDef, MethodDef, Property, or Event definitions and only the expected
  existing MethodDef update, plus the narrowly allowed reference, signature,
  and heap records;
- bounded metadata, IL, and PDB delta sizes; and
- recorded hashes and EnC ID/base-ID evidence for the exact emitted payload.

Delta bytes are not reproducible across compiler processes because Roslyn
generates a fresh EnC ID. The recorded SHA-256, EnC ID, and EnC base ID describe
the one payload actually submitted; they are not a promise that a later
compiler can regenerate it.

Use the returned `EmitDifferenceResult.Baseline` only after the target confirms
successful application. Then atomically advance the coordinator's in-memory
baseline, `Solution`, `Compilation`, and acknowledged source snapshot together,
and persist the acknowledged source/delta evidence as the next producer
generation.

### Session compiler coordinator

Public Roslyn delta emission creates a fresh EnC ID for every delta and chains
the next delta to it. `EmitBaseline` and the updated compilation state expose no
supported serialization or caller-supplied EnC ID. Therefore a new one-shot
process cannot replay history and continue a live target's chain, even with
identical inputs.

One long-lived coordinator owns the compiler state for one editable process:

1. it starts from the immutable generation-0 PE/PDB, loaded project, and stored
   source snapshots and owns their `ModuleMetadata`, PE/PDB readers, and
   `MSBuildWorkspace`;
2. it accepts authenticated local requests from one-shot LiveCode commands;
3. it serializes one pending generation per module;
4. it holds both the current and proposed baseline/compilation until the target
   acknowledgment is resolved;
5. after an applied acknowledgment, it retains the returned `EmitBaseline`,
   advances the in-memory `Solution`/`Compilation` chain, and writes the
   evidence ledger;
6. after a compile/rejection response, it discards the proposed state; and
7. after target apply ambiguity, coordinator loss, or target/coordinator
   identity mismatch, it marks the session restart-required.

The coordinator is update-time tooling, not a method-call proxy. It is stopped
on workspace close and adds no dispatch or field-access cost to the updated
method. Bound generation count, delta sizes, memory, and session lifetime.
Never serialize private Roslyn implementation objects or pretend a restarted
coordinator can attach to a generation greater than zero.

## Runtime Bridge

Create an opt-in `AlvorKit.Engine.SourceUpdate` composition package. It exposes
one authenticated LiveCode bridge and runs only through the normal pre-update
safe-frame lane.

Do not apply source updates through frozen inspection.

### Two-phase LiveCode transport

The current bridge request returns only after `LiveCodeHost.Pump` executes it,
so it cannot acknowledge queued work while AlvorSense is paused. Add an opt-in
two-phase bridge transport without changing the existing one-response `Bridge`
API:

1. `BridgeEnqueue` authenticates and bounds the request, validates the bridge
   name/version, atomically reserves the unique update ID by creating its
   bounded thread-safe `Pending` record, inserts a
   `LiveCodePendingBridge` linked to that record into the target pre-update
   queue, and only then returns `Accepted/queued-for-safe-frame`;
2. `BridgeOperationStatus` is served directly by `LiveCodeHostServer`, never
   through the game-thread queue, and returns pending/running/completed status
   plus the terminal result for that operation ID; and
3. `LiveCodeHost.Pump` executes the existing `LiveCodePendingBridge`, atomically
   publishes its terminal result to the operation record, and preserves the
   normal safe-frame execution contract.

Authentication, duplicate-ID checks, result retention bounds, cleanup, and
terminal status are part of this host-level protocol. `source apply` enqueues
and then waits/polls through the direct status request. A separate
`source status` command reads the same thread-safe record, so it remains usable
while the game loop is paused. Neither status path executes user or bridge code.
If queue insertion fails after ID reservation, the host atomically publishes a
terminal enqueue-failure result instead of deleting or reusing the ID. The pump
transitions the pre-existing record `Pending` to `Running` to `Completed`
exactly once, so it cannot outrun record creation on an already-running target.

### Capabilities

The bridge reports:

- protocol version;
- `MetadataUpdater.IsSupported`;
- runtime version;
- process identity;
- explicitly allowlisted target modules from the editable launch manifest, with
  assembly name, immutable path, MVID, DLL/PDB identity, and current generation;
- supported edit shape (`existing-method-body`);
- maximum delta sizes;
- tainted/restart-required state; and
- Source Update versus ReJIT session mode.

### Apply request

The request contains:

- module MVID;
- expected current generation;
- update ID;
- previous/result source hashes;
- expected MethodDef token;
- expected changed TypeDef tokens;
- metadata, IL, and PDB deltas;
- individual and aggregate delta hashes; and
- project/build identity hash.

The two-phase host transport:

1. authenticates and bounds the request, atomically reserves its unique pending
   operation record, and enqueues exactly one linked request for the normal
   pre-update lane;
2. publishes a terminal enqueue failure if insertion fails, or records
   `queued-for-safe-frame` with the pending update ID before the client may
   advance AlvorSense.

On that safe frame, the Source Update bridge:

1. resolves exactly one loaded assembly/module by MVID;
2. verifies its baseline identity and expected generation;
3. rejects duplicate update IDs and out-of-order generations;
4. serializes all operations for that module;
5. calls `MetadataUpdater.ApplyUpdate`;
6. immediately records the new generation in the process-identity-scoped target
   ledger after `ApplyUpdate` returns successfully;
7. resolves the validated `EmitDifferenceResult.ChangedTypes` tokens against the
   updated module;
8. invokes metadata-update handlers exactly once, containing and reporting
   discovery, resolution, and individual handler exceptions as
   `AppliedWithHandlerWarnings`; and
9. returns the exact applied hashes and generation.

`MetadataUpdater.ApplyUpdate` applies metadata and IL only; it does not discover
or invoke `[MetadataUpdateHandler]` methods. Source Update performs the managed
Hot Reload notification contract after committing the generation: rediscover
handlers across loaded assemblies on each cold-path apply, order dependency
assemblies before dependents, invoke every valid static
`ClearCache(Type[]?)` before any valid static `UpdateApplication(Type[]?)`, and
pass the resolved changed types. V1 requires `ChangedTypes` to contain exactly
the selected method's declaring TypeDef. Validate token kinds and module
identity before apply, then resolve the corresponding `Type` after apply.

The compiler coordinator advances for both `Applied` and
`AppliedWithHandlerWarnings`. A handler warning is not an apply failure because
the metadata update has already committed. It does mark the target
restart-required because application/cache notification may be incomplete; the
same delta is never retried. The disk evidence also records the acknowledgment,
but a restarted target is always generation 0 and never reattaches an old
process ledger.

Retry asks for status by update ID. It never blindly resubmits a delta.

If `ApplyUpdate` throws, is interrupted, or returns an ambiguous result, mark
the module/session tainted and restart-required. Do not guess whether the delta
partially applied, do not advance the producer baseline, and do not attempt an
automatic inverse delta.

## Process And Interception Modes

Source Update v1 is a separate Debug editable-process mode:

```text
Debug + MetadataUpdater + no debugger + no profiler/ReJIT
```

Legacy Scoped LivePatch exists only during migration and uses a separate
optimized interception mode:

```text
Release + CoreCLR profiler/ReJIT + handler dispatch
```

Do not enable both modes for the same target module. CoreCLR treats
Edit-and-Continue and ReJIT modules as incompatible, and the existing LivePatch
also caches/restores an original IL body that becomes stale after a metadata
update.

V1 uses process-wide separation rather than attempting disjoint-module
coexistence. Do not productize a later coexistence mode: the target end state
removes Scoped LivePatch.

During migration, editable mode reports `mode=source-update` and
`rejitAvailable=false`, and `patch install` returns
`mode-conflict: editable-source-process` before making a profiler request.
After retirement, the `patch` command and LivePatch capability disappear
entirely rather than remaining as an unavailable mode.

A managed debugger is also outside the custom Source Update v1 path. Debugger
Hot Reload uses its own update control plane.

## What Survives LivePatch Retirement

### Keep unconditionally

Keep:

```text
src/AlvorKit.Interception/
src/AlvorKit.Interception.CoreClr/
native/interception-profiler/
src/AlvorKit.Mocking/
src/AlvorKit.Mocking.Dynamic/
src/AlvorKit.Mocking.Emit/
src/AlvorKit.Mocking.Interception/
```

Mocking uses `AlvorKit.Interception` directly for concrete/static methods,
fields, constructors, constructor bodies, structs, caller-site routing,
passthrough, and verification. It does not use `AlvorKit.LivePatch`.

Global Source Update cannot replace Mocking's setup matching, invocation
history, per-instance isolation, caller-site routing, or rollback transaction
semantics.

### Keep only during migration

Keep the current:

```text
src/AlvorKit.LivePatch/
src/AlvorKit.Engine.LivePatch/
LiveCode patch CLI and bridge
```

only long enough to compare behavior, prove Source Update, and establish that
Mocking remains intact. The legacy product provides:

- different behavior for two instances of the same method;
- exact-instance, exact-scope, and descendant selection;
- automatic deactivation at injector-scope end;
- multiple disjoint receiver registrations over one method;
- atomic managed handler replacement without another ReJIT;
- handler exception containment and automatic original fallback;
- collectible submitted assemblies; and
- optimized Release experiments.

Do not attempt to reproduce these receiver-selected semantics in Source Update.
They are intentionally retired product behavior, not requirements blocking
LivePatch removal. While migration is in progress, keep the existing explicit
handler ABI unchanged and do not build the target-shaped private-access
compiler.

### Required LivePatch retirement

After Source Update passes the vertical spike, automated, demo, and surviving
Mocking/Interception gates, remove:

```text
src/AlvorKit.LivePatch/
src/AlvorKit.Engine.LivePatch/
tests/AlvorKit.LivePatch.Test/
demos/AlvorKit.Engine.LivePatch.Demo/
scripts/AlvorKit.Script.LiveCode/LivePatchCli.cs
scripts/AlvorKit.Script.LiveCode/LivePatchCommandTree.cs
docs/LivePatch.md
docs/LivePatchTargetShapePlan.md
```

Also remove:

- LivePatch project references, global usings, solution entries, and
  `InternalsVisibleTo` entries;
- LivePatch bridge registration, protocol, workspace patch tracking, CLI help,
  and capability advertisement;
- LivePatch composition from `AlvorKit.Engine.LiveCode.Demo`, migrating any
  still-useful source-update teaching behavior to the dedicated Source Update
  demo; and
- agent documentation and skill instructions for `lp/`, patch
  install/replace/remove, and LivePatch cleanup.

Retain historical rationale in this Source Update plan and ordinary version
control; do not keep a dead public product or CLI compatibility shim.

Do not remove `AlvorKit.Interception`, its CoreCLR backend, the native profiler,
or any Mocking package. They form Mocking's independent runtime path.

## Workspace Contract

Add:

```text
tmp/live/<workspace-id>/source/
```

Every successful or attempted generation records:

- create-once numbered unified diff and its SHA-256 hash;
- exact previous and resulting source snapshots;
- source encodings and SHA-256 hashes;
- old/new method identity and MethodDef token;
- changed TypeDef tokens;
- compiler diagnostics;
- metadata/IL/PDB delta hashes, EnC ID, and EnC base ID;
- request and response JSON;
- expected and observed generation;
- queued/pending update ID and `queued-for-safe-frame` acknowledgment;
- apply status;
- coordinator and target process identities;
- `worktreeAhead`, handler-warning, tainted, and restart-required state;
- visible evidence; and
- whether the process is healthy or restart-required.

The real source file remains edited. That is intentional: Source Update mirrors
a normal repository edit into the running process.

A compile/rude-edit rejection does not mutate the process; the source edit
remains in the worktree for correction or ordinary restart/build.

An apply failure leaves the source edit in the worktree and marks the process
restart-required.

Workspace closure requires:

- no update request in flight;
- the last target acknowledgment recorded;
- the runtime generation/source hash reconciled with the ledger, or an explicit
  restart-required record;
- all temporary bridge operations terminal; and
- a clear statement whether the source diff remains intentionally in the
  worktree.

It does not revert ordinary source edits automatically.

## Package And File Plan

Expected new runtime package:

```text
src/AlvorKit.Engine.SourceUpdate/
    AlvorKit.Engine.SourceUpdate.csproj
    RootSourceUpdate.cs
    RootSourceUpdateScript.cs
    SourceUpdateBridge.cs
    SourceUpdateProtocol.cs
    SourceUpdateModuleLedger.cs
    SourceUpdateModuleIdentity.cs
```

Expected two-phase host protocol changes:

```text
src/AlvorKit.LiveCode/
    LiveCodeClient.cs
    LiveCodeHostServer.cs
    LiveCodeHost.cs
    LiveCodePendingBridge.cs
    LiveCodeBridgeOperation.cs
    LiveCodeBridgeOperationStore.cs
src/AlvorKit.LiveCode/Protocol/
    LiveCodeWireRequestKind.cs
    LiveCodeWireRequest.cs
    LiveCodeWireResponse.cs
    LiveCodeBridgeEnqueueRequest.cs
    LiveCodeBridgeEnqueueResponse.cs
    LiveCodeBridgeOperationStatusRequest.cs
    LiveCodeBridgeOperationStatusResponse.cs
```

The existing `Bridge` request/response behavior remains compatible. Source
Update is the first client of the two-phase operations.

Expected compiler/CLI additions:

```text
scripts/AlvorKit.Script.LiveCode/SourceUpdate/
    SourceUpdateCli.cs
    SourceUpdateCommandTree.cs
    SourceUpdateCompilerCoordinator.cs
    SourceUpdateCoordinatorClient.cs
    SourceUpdateCoordinatorProtocol.cs
    SourceUpdateSession.cs
    SourceUpdateProjectBaseline.cs
    SourceUpdateDiff.cs
    SourceUpdateEditValidator.cs
    SourceUpdateDeltaCompiler.cs
    SourceUpdatePdbReader.cs
    SourceUpdateGeneration.cs
```

Expected AlvorSense/workspace additions:

```text
scripts/AlvorKit.Script.AlvorSense/
    editable-project launch option, immutable runnable artifact copy,
    and exact target-process identity
scripts/AlvorKit.Script.LiveWorkspace/
    coordinator/process identity, source generation, worktreeAhead,
    handler-warning, and restart-required records
docs/AgentLiveDevelopment.md
    source-update workflow and tmp/live/<workspace-id>/source/ contract
.agents/skills/alvorkit-live-debug/SKILL.md
    source submission routing, apply orchestration, evidence, and cleanup
```

Expected tests:

```text
tests/AlvorKit.LiveCode.Test/
tests/AlvorKit.Script.LiveCode.Test/
tests/AlvorKit.Engine.SourceUpdate.Test/
tests/AlvorKit.Script.AlvorSense.Test/
tests/AlvorKit.Script.LiveWorkspace.Test/
```

Expected demo:

```text
demos/AlvorKit.Engine.SourceUpdate.Demo/
```

Add `global.json` and matching Roslyn/MSBuild package versions only after the
vertical spike proves the selected SDK/toolchain combination. Because
`global.json` affects the entire repository, review that change separately
before productizing the spike.

## Vertical Feasibility Spike

Do this before general CLI or demo work.

Create a small Debug fixture containing:

- one service instance created before the update;
- a private mutable value field;
- a private reference field;
- an already-captured primary-constructor dependency;
- an uncaptured primary-constructor parameter;
- two instances with distinguishable identities; and
- one ordinary method with locals.

Prove:

1. the exact PE/PDB and project compilation produce an initial public
   `EmitBaseline`;
2. a one-method `SemanticEdit` emits exactly the expected MethodDef;
3. `MetadataUpdater.IsSupported` is true in the launched fixture;
4. the target applies the delta through the authenticated bridge;
5. the same pre-existing objects use the new body;
6. private field and captured dependency access are ordinary and correct;
7. both instances change, proving global semantics;
8. an already-running invocation completes its old body;
9. a second ordered generation applies;
10. an inverse source edit restores behavior as another forward generation;
11. stale source, wrong MVID/PDB, duplicate ID, and out-of-order generation are
    rejected before apply;
12. newly capturing the uncaptured parameter is restart-required;
13. optimized/Release launch is rejected;
14. an apply failure poisons the session instead of guessing rollback; and
15. Source Update and ReJIT/Scoped LivePatch are not allowed on the same target
    module;
16. two chained deltas use the same live coordinator state and a fresh
    one-shot compiler is refused rather than allowed to invent a new EnC chain;
17. overwriting ordinary `bin/Debug` output after launch cannot change the
    immutable baseline PE/PDB or loaded runnable artifact; and
18. loss of the coordinator after an applied generation makes the session
    restart-required.

Exit gate: all proofs pass with public APIs and no dependency on Roslyn
`ExternalAccess.HotReload` internals.

If exact initial baseline construction or chained public deltas cannot be made
stable inside one coordinator under the selected toolchain, stop. The fallback
is to let `dotnet watch` own the whole process and accept its restart/process
semantics, not to revive target-shaped private-member lowering.

## Automated Tests

### Diff and identity

- one-file unified diff applies to the exact previous source;
- stale base hash, wrong path, mismatched current file, malformed hunks, and
  path traversal fail closed;
- pre-existing dirty source is captured as generation 0 rather than confused
  with repository `HEAD`;
- an edit observed after validation but before submission is rejected;
- an edit observed after acknowledgment preserves the applied generation and
  records `worktreeAhead`;
- SDK, project, configuration, TFM, global properties, DLL, PDB, MVID, CodeView
  identity, and source checksums are exact.

### Edit validation

- one block-body and one expression-body update succeed;
- private field/property/method access succeeds;
- existing captured dependency identity is preserved;
- ordinary locals work in new invocations;
- declaration, signature, attribute, field, constructor, primary-constructor,
  base/interface, and additional-document edits fail;
- new capture topology, lambda/local function, async/iterator, generic,
  unsafe/stackalloc, `dynamic`, anonymous-object, generated-file, and
  multi-method edits fail;
- generator output must remain byte-identical to generation 0; and
- `UpdatedMethods` and the parsed metadata delta/EnC log-map must prove only
  the selected existing MethodDef changed and no definitions were added.

### Generation/runtime

- generation 0 to 1 and 1 to 2 apply in order;
- the coordinator retains and advances the public Roslyn EnC chain in memory;
- a fresh one-shot process cannot reconstruct or replay a generation greater
  than zero;
- coordinator loss after generation 1 is restart-required;
- duplicate update ID is idempotently reported, not reapplied;
- stale/out-of-order requests fail;
- `BridgeEnqueue` acknowledges `queued-for-safe-frame` without calling
  `LiveCodeHost.Pump`;
- `BridgeOperationStatus` reports pending and terminal states without entering
  the game-thread queue;
- one later pump executes the accepted update exactly once;
- authentication, duplicate operation IDs, bounded retention, terminal cleanup,
  and existing one-response `Bridge` compatibility are covered;
- the operation record is reserved before enqueue, enqueue failure is terminal,
  and a concurrent pump cannot outrun or double-transition it;
- a paused target exposes that accepted state before the one AlvorSense update
  batch is sent;
- producer baseline advances only after acknowledgment;
- `AppliedWithHandlerWarnings` still advances producer and target generations;
- changed TypeDef tokens are validated before apply and resolved afterward;
- metadata-update handlers run dependency-first, with every `ClearCache` before
  any `UpdateApplication`;
- handler discovery, resolution, and invocation failures are contained and
  recorded;
- active invocation finishes old IL and later invocation uses new IL;
- forward restoration creates the next generation;
- compile failure leaves runtime untouched;
- apply ambiguity marks restart-required;
- process restart returns generation 0 from the current built source;
- Debug editable capability succeeds;
- only launch-manifest allowlisted target modules are eligible;
- missing explicit `RootSourceUpdate` composition fails capability validation;
- changing ordinary build outputs cannot change the immutable launched
  PE/PDB/artifact set;
- Release/optimized, missing PDB, wrong MVID, debugger-owned, and profiler/ReJIT
  modes fail closed; and
- payload and history bounds are enforced.

### Retirement and surviving products

- before deletion, run the legacy Scoped LivePatch tests once to distinguish
  pre-existing failures from retirement work;
- after deletion, the solution contains no LivePatch projects, tests, demos,
  namespaces, references, bridge registrations, capability fields, or CLI
  commands;
- Mocking proxy and Interception suites remain and pass through their existing
  lower-level path;
- Source Update startup does not reference or initialize LivePatch;
- an editable source module cannot acquire a ReJIT mutation;
- a ReJIT-owned process/module cannot enable Source Update; and
- no target-shaped compiler or `UnsafeAccessor` output is generated.

## Visual Demo

Create a dedicated Debug `AlvorKit.Engine.SourceUpdate.Demo`.

The visible scene contains at least two live simulations of the same concrete
service type. Each displays:

- stable object identity;
- stable injected-dependency identity;
- private phase/state;
- current behavior label; and
- current Source Update generation.

Record:

1. baseline screenshot with generation 0 and both receivers in original
   behavior;
2. the ordinary source edit and numbered diff;
3. generation-1 apply status;
   this includes the pending update ID and `queued-for-safe-frame` evidence
   before the AlvorSense batch;
4. screenshot proving both pre-existing receivers changed without recreation;
5. a second source edit and generation-2 screenshot;
6. inverse source edit and generation-3 forward-restoration screenshot;
7. screenshot proving restored code did not roll back previously mutated state;
8. stale-hash and unsupported-edit diagnostics;
9. restart and screenshot proving the newly built current source is pristine
   generation 0; and
10. final `source status` reporting `mode=source-update` and
    `rejitAvailable=false`, with no LivePatch capability, plus CLI help proving
    that the `patch` command has been removed.

Use AlvorSense for deterministic updates and screenshots and record every
source/diff/apply/evidence event in the same workspace.

## Implementation Phases

### Phase 0: public-API spike

Select one SDK/package combination explicitly, then prove exact initial baseline
construction, one private-state method update, a two-generation in-memory EnC
chain, forward restoration, and mode exclusion.

### Phase 1: toolchain and launch identity

After the spike passes, separately review and pin the SDK/toolchain. Add
`--editable-project`, build and copy the exact immutable Debug artifact, record
target process/PE/PDB/source identities, require explicit `RootSourceUpdate`
composition, and advertise only allowlisted module capabilities.

### Phase 2: strict source/diff compiler

Implement the session compiler coordinator, one-file diff verification, exact
old/new compilations, conservative method-body validation, public
`EmitDifference`, and stable diagnostics. Reject coordinator restart/replay
after generation 0.

### Phase 3: runtime bridge and ledger

Implement safe-frame apply, per-module serialization, idempotent update IDs,
generation checks, immediate post-apply target ledger advancement,
exactly-once metadata-update handler notification with warning containment, and
restart-required poisoning. Add the opt-in two-phase `BridgeEnqueue` and
non-game-thread `BridgeOperationStatus` transport to `AlvorKit.LiveCode`, with
bounded thread-safe operation state and compatibility tests for existing bridge
calls.

### Phase 4: CLI and workspace

Add `source apply`, `source status`, generation/source/delta evidence, current
worktree reconciliation, paused AlvorSense apply orchestration, coordinator
lifecycle, the `queued-for-safe-frame` readiness signal, and forward-restoration
documentation. Update
`docs/AgentLiveDevelopment.md` and the `alvorkit-live-debug` skill to authorize
and clean up `tmp/live/<workspace-id>/source/` artifacts.

### Phase 5: demo and integration evidence

Build the dedicated Debug demo and capture the full global-update sequence.
Before retirement, run the existing Scoped LivePatch tests once as a migration
baseline. Run the surviving Mocking and Interception regression proofs in their
separate profiler-enabled test process.

### Phase 6: retire LivePatch

Delete both LivePatch product projects, their tests and demo, LiveCode patch
commands and bridge, solution/project references, friend-assembly entries,
capabilities, workspace patch tracking, and product/agent documentation.
Remove LivePatch composition from the existing LiveCode demo. Prove by
repository search that no product reference remains, while Mocking still uses
Interception/CoreClr and its profiler tests still pass.

### Phase 7: documentation and Commit Mode verification

Document Source Update as the sole method-body live-edit product, run focused
tests/coverage/lint, audit the pinned SDK impact, and verify that neither
LivePatch nor a target-shaped compiler/private-access layer remains.

## Completion Criteria

The work is complete when:

- an agent makes a normal diff to the original `.cs` file;
- the tool proves that diff against the exact live generation;
- Roslyn emits a delta for the original existing method;
- the runtime updates that original MethodDef;
- existing private members and already-captured injected state work normally;
- every pre-existing receiver observes the global change;
- reflection/stack identity remains the original method;
- multiple generations and forward restoration follow one verified in-memory
  EnC chain;
- stale, conflicting, unsupported, or ambiguous updates fail closed;
- apply ambiguity marks the session restart-required;
- worktree source and workspace history remain auditable;
- source races are rejected before submit or reported as `worktreeAhead` after
  acknowledgment;
- immutable session artifacts preserve the exact loaded baseline;
- coordinator loss after a live generation fails closed;
- a paused target acknowledges queue acceptance through the two-phase host
  protocol before AlvorSense advances;
- editable-source and ReJIT modes cannot collide;
- Mocking continues through Interception without a LivePatch dependency;
- `AlvorKit.LivePatch`, `AlvorKit.Engine.LivePatch`, their bridge/CLI, tests,
  demo, documentation, capabilities, and repository references are removed;
- no compatibility shim or dormant Scoped LivePatch product remains;
- the Debug visual demo proves update, global scope, state preservation,
  restoration, and restart; and
- the implementation uses public Roslyn/runtime APIs only.

## Independent Validation

Three independent reviews validated this direction with the conditions captured
above:

- compiler validation selected the public `EmitDifference` route, required
  exact PE/PDB/project baselines, found that EnC IDs make one-shot replay
  invalid, required the long-lived coordinator and metadata-table guards, and
  rejected dependence on internal Roslyn Hot Reload orchestration;
- runtime validation required an explicit non-optimized Debug editable launch,
  immutable runnable artifacts, serialized forward-only generations, manual
  post-apply handler notification, safe-frame application, taint-on-apply
  ambiguity, and strict separation from debugger/ReJIT modes; and
- Mocking validation found no `AlvorKit.LivePatch` dependency, confirmed that
  Mocking needs the lower-level Interception stack, and confirmed that removing
  the higher-level LivePatch product does not break Mocking. Scoped
  receiver-selection and failure-containment behavior are intentionally retired
  rather than carried into Source Update. Its demo review required the observable
  `queued-for-safe-frame` acknowledgment before advancing a paused AlvorSense
  target.
