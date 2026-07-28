# AlvorKit Interception

`AlvorKit.Interception` is the runtime-neutral managed API for changing an
ordinary managed method in a live optimized CoreCLR process. Target methods do
not need a LivePatch attribute, proxy, source edit, or pre-build rewrite.

## Layers

1. `native/interception-profiler` is the native CoreCLR profiler loaded at
   process startup.
2. `native/interception-profiler/include/alvorkit_interception_profiler.h` is
   the versioned C ABI.
3. `AlvorKit.Script.Bindgen` generates the raw managed API and P/Invoke backend
   under `out/bindgen`.
4. `src/AlvorKit.Interception` provides runtime-neutral identities,
   capabilities, plans, physical claims, leases, collisions, and backend
   contracts. It does not reference CoreCLR or native profiler assets.
5. `src/AlvorKit.Interception.CoreClr` provides the optional CoreCLR backend,
   exact delegate/trampoline generation, patch handles, and structured
   completions. Loaded-IL decoding and immutable-generation planning are the
   advanced `AlvorKit.Interception.CoreClr.Advanced` surface in that package.
6. `src/AlvorKit.Mocking.Emit` is the profiler-free typed IL substrate shared
   by Dynamic proxies and operation wrappers.
7. `src/AlvorKit.Mocking.Interception` owns Mocking's exact operation wrappers
   and depends only on neutral Interception, Mocking, and Mocking.Emit.
8. `src/AlvorKit.LivePatch` adds receiver selection and injector lifecycle
   over the neutral backend contract.
9. `src/AlvorKit.Engine.LivePatch` composes the CoreCLR backend and exposes the
   service through LiveCode at the
   engine safe-frame boundary.

## Exact dispatch

An instance target such as:

```csharp
public int Calculate(int value, ref int observed)
```

has one exact handler ABI:

```csharp
public int Run(
    ExactTarget receiver,
    int value,
    ref int observed)
```

The native wrapper loads the real receiver and declared arguments, asks one
stable managed resolver for the selected handler pointer, then uses a
signature-correct managed `calli`. A miss falls through to a copy of the
original IL. The wrapper preserves the original local signature and exception
regions.

This design does not box arguments, allocate `object[]`, use `DynamicInvoke`,
or require one dispatch method for every return type. The warm exact
handler/lease path has an executable zero-managed-allocation test.

Static targets omit the receiver and require the explicit global selector at
the LivePatch layer.

## Runtime flow

```text
LiveCode request
    -> game safe-frame queue
    -> LivePatch selector/lease
    -> exact Interception dispatch plan
    -> generated binding and versioned C ABI
    -> profiler-owned CLR-initialized worker
    -> RequestReJITWithInliners
    -> GetReJITParameters
    -> future calls use wrapper or original fallback
```

Commands are queued because the game thread is not a safe place to invoke
profiler APIs that may suspend the runtime. The native worker calls
`InitializeCurrentThread` before submitting ReJIT and revert requests.

The target identity is:

```text
module MVID + MethodDef token + FNV-1a hash of the exact metadata signature
```

Display names are diagnostic only. The profiler validates the signature again
inside the target process before changing IL.

## Loading

The profiler must be present at startup:

```text
CORECLR_ENABLE_PROFILING=1
CORECLR_PROFILER={3840ACF7-5AF1-49EA-BF94-5F7086C57F57}
CORECLR_PROFILER_PATH_64=<absolute profiler DLL>
ALVORKIT_INTERCEPTION_PROFILER_PATH=<the same path>
ALVORKIT_INTERCEPTION_MODULES=<semicolon-separated managed module allowlist>
```

The current supported profiler target is Windows x64. The native project does
not retain dormant build paths or runtime assets for other RIDs. The CI-first
support and expansion gates are recorded in
[`InterceptionProfilerPlatformPlan.md`](InterceptionProfilerPlatformPlan.md).

`InterceptionProfiler.Connect` negotiates ABI/capabilities with the library
CoreCLR already loaded. It does not attach the profiler after startup.

For tests and isolated executables, use the child-only launcher instead of
setting profiler variables in the parent shell:

```powershell
dotnet run --project scripts/AlvorKit.Script.TestInterception -- `
    --test-project tests/MyGame.Test/MyGame.Test.csproj `
    --module MyGame.Test -- --no-build --no-restore
```

The launcher creates a unique temporary runsettings file, starts only the
selected VSTest child with the profiler enabled, enforces a bounded timeout,
and removes the file afterward. `--exec-project` provides the equivalent
isolated executable mode. The coverage command's `--interception` option uses
the same asset resolver and child-only settings.

Profiler-enabled launches disable ReadyToRun/native images because ReJIT
requires IL-backed methods. This adds cold-start JIT work. It does not disable
normal Release optimization globally. Unprofiled launches have no profiler
code or branch in their methods.

The launcher has an executable isolation acceptance: two profiled VSTest
children observe the native module, followed by an ordinary VSTest child that
observes neither profiler variables nor the native module.

Before starting a child, the launcher guard rejects missing opt-in, partial or
conflicting profiler variables, unsupported host/runtime/architecture
combinations, and mismatched profiler assets with structured failure kinds.
CoreCLR module identity is read from the PE metadata MVID and validated before
it participates in an exact target.

## Current contract

Implemented:

- raw replacement IL and exact managed handler dispatch;
- ordinary static and reference-type instance methods;
- exact value, reference, `ref`, and `out` arguments and exact returns;
- original IL and exception-region fallback;
- multiple patched methods and multiple scope handlers on one method;
- atomic managed handler replacement without another ReJIT;
- immediate managed deactivation plus asynchronous original-IL restoration;
- existing-inliner ReJIT and revert;
- neutral exception propagation, explicit LivePatch containment, and
  collectible handler release;
- structured request/callback/HRESULT/timing evidence;
- immutable loaded-body snapshots, exact SHA-256 body identity, typed IL
  operands and control-flow targets, prefix classification, and small/fat
  exception-region decoding;
- Cecil-free call/construction/field recognition with stable site identities
  and pristine structured rejections;
- constructor base/`this` split planning with branch and exception-region
  crossing validation, plus exact remainder delegate extraction and executable
  preserved-prefix ABI-v3 generation for supported same-module routes; stack,
  prefix-cycle, cross-split-local, token-bearing signature, generic-route, and
  static-constructor hazards fail closed while `InitLocals` is preserved, and
  optimized no-locals bodies receive the canonical empty dynamic local
  signature before the extracted remainder executes;
- arbitrary-caller construction lowering that validates one recognized
  `newobj`, rewrites only its fixed-size opcode/token to an exact same-module
  route, and preserves the complete loaded body and IL map;
- immutable multi-site symbolic caller composition with baseline labels,
  branch/switch and exception-region preservation, IL maps, and explicit
  local/helper/signature relocation requests; and
- source-to-body targeting for synchronous, async, iterator, and
  async-iterator methods while retaining source diagnostics and authoritative
  generated `MoveNext` body identity;
- code-first preparation preview by exact caller/source body, canonical member
  signature, stable site or occurrence, and expected loaded-body identity,
  with composition only after a complete successful preview;
- Mocking route preparation with exclusive reservations, a shared
  all-or-nothing publication gate, actionable failure categories, and
  reverse-order rollback that leaves Dynamic proxy capability independent;
- a public typed owned-instance caller binder used by both the AlvorKit
  profiler fixture and Shroom without exposing internal site descriptors or reflecting
  over internal adapter code; and
- a standalone profiler-launched Release performance fixture that records
  cold install/remove and warm direct/inert/active routes while asserting
  zero managed allocation for warmed inert, active, and swapped scalar routes;
  the swap evidence also proves the profiler request ID, pending-request
  count, and active-patch count remain unchanged.

The caller-first migration proof additionally exercises the real startup
profiler with scalar/reference/wide/void signatures, `in`/`ref`/`out`,
ref-struct ingress, live mutable and readonly struct data, managed-reference
alias identity, construction-specific generic routes, original fallback,
`callvirt` null behavior, ordinary exceptions, and removal while a routed
invocation is in flight. Warm inert fallback and active scalar replacement
both have zero-allocation assertions.

ABI 3 and native package/bindings version 0.4.0 add bounded method generations,
worker-acquired loaded-body snapshots, SHA-256 baseline identity, StandAloneSig,
TypeSpec, MemberRef, and MethodSpec relocation, original-to-instrumented IL
maps, target ReJITID, per-relocation results, and structured failure stages.
The 16-test caller-proof fixture includes true late-metadata tests. After their
callers have JITted, they prove the requested rows do not exist, then create and
execute a Cdecl `int(int)` StandAloneSig plus one generation containing a
closed generic TypeSpec, a custom-modifier-bearing private MemberRef, an
internal MemberRef, and a closed generic MethodSpec. Replacement reuses every
exact token, and a separate case rejects a stale baseline.

The Mocking migration fixture passes 39/39 in one Windows x64 Release
startup-profiler process. It contains all 34 named legacy Mocking behavior
rows, one narrower end-to-end integration proof, and four route-binding
lifetime tests. The same complete fixture passes under the interception-aware
Coverlet host; the July 27, 2026 zero-threshold run recorded 66.01% line,
54.01% branch, and 72.38% method coverage for `AlvorKit.Mocking`.

Collectible generic arguments, signature types, and modifiers now select the
collectible module as the weak owner for Mocking's method registry, callback
delegate, exact wrapper, receiver-free method, and typed trampoline caches.
Retirement releases the active wrapper after acquired calls drain while
retaining the exact original fallback. That fallback cannot yet be cleared or
safely rebound: an old rewritten code version can enter the shared
generationless gateway after removal completes. Closing that last lifetime
edge requires generation/code-version correlation or an inline-original miss
path.

The production exact trampoline also supports mutable and readonly
managed-reference returns with alias identity and ref-struct value returns.
Managed-reference routes require the propagation policy because there is no
safe default reference for containment. Ref-to-ref-struct, pointer,
function-pointer, and open element shapes remain rejected. Fully closed generic
methods and methods on fully closed generic declaring types are accepted with
their exact constructed signatures; open or unsafe generic arguments are
rejected. Native construction-specific code-version correlation remains a
separate activation requirement.

This is a tested ABI-v3 foundation, not a frozen general operation planner.
`VAR`/`MVAR` signatures, cross-module access, constructor/field MemberRefs, and
collectible unload remain outside the executable relocation proof. The managed
symbolic planner also does not yet lower every selected operation into an
executable generation, and generation completion does not yet report
observed-inliner, site, or constructed-generic correlation.

Run the standalone performance evidence through the isolated executable host:

```powershell
dotnet run --project scripts/AlvorKit.Script.TestInterception -- `
    --exec-project tests/AlvorKit.Interception.Performance.Fixture/AlvorKit.Interception.Performance.Fixture.csproj `
    --configuration Release `
    --module AlvorKit.Interception.Performance.Fixture -- `
    --no-build --no-restore
```

The fixture emits a compact JSON record. Wall-clock values are observations,
not pass/fail thresholds; only deterministic allocation and handler-swap
invariants fail the run. A July 27, 2026 Windows x64 Release v2 sample recorded
9.279 ms cold install, 3.04 ns/call warm direct, 6.26 ns/call warm inert,
6.27 ns/call warm active exact, 6.27 ns/call after a managed handler swap, and
10.589 ms cold remove. Warm inert, active, and swapped routes each allocated
0 B over 100,000 calls after tier warmup. The handler swap retained request ID
1, zero pending requests, and one active patch, proving it caused no profiler
request or patch transition.

Remaining fail-closed boundaries include:

- open generic targets and unsafe generic construction arguments;
- varargs, function pointers, pointers, and ref-to-ref-struct shapes;
- unsupported custom-modifier and general symbolic-lowering cases; and
- unproven constructed-code-version correlation and collectible-unload
  lifetime.

Closed constructed generics, explicit mutable/readonly value receivers,
propagating managed-reference returns, and ref-struct value returns have
managed executable coverage. These accepted shapes do not weaken the remaining
native correlation and lifetime gates.

The complete receiver-selection and LiveCode interface is documented in
[`LivePatch.md`](LivePatch.md).
