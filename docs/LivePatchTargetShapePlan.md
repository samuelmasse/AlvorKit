# LivePatch Target-Shaped Source Plan

## Status

This is the implementation plan for letting an agent write a LivePatch
submission in the shape of the target service instead of writing the current
explicit handler ABI.

The intended experience is:

```csharp
namespace MyGame;

public sealed class WorldService(RenderClock clock, WorldState state)
{
    private double phase;

    public void Update(double delta)
    {
        phase += delta;
        state.Advance(clock.Now, phase);
    }
}
```

The constructor parameters and field declarations are projections of state
that must already exist on the live production object. The submission does not
construct another `WorldService`, add fields, run field initializers, or
re-resolve the projected dependencies from Injection.

The compiler lowers the source to the existing exact handler ABI:

```csharp
public sealed class GeneratedWorldServicePatch
{
    [LivePatchHandler]
    public static void Invoke(WorldService receiver, double delta)
    {
        // Generated exact accesses against receiver.
    }
}
```

LivePatch continues to use its current receiver selectors, exact trampoline,
ReJIT wrapper, failure containment, atomic replacement, original-IL fallback,
and collectible submission lifecycle.

This feature is named **target-shaped source**. It is deliberately not called
a literal `MethodDef` replacement. Runtime reflection, stack-frame identity,
caller-info defaults, generated state-machine types, and debugger binding can
observe the generated handler boundary even though ordinary member behavior
uses the real receiver.

## Decision Summary

The implementation will:

1. Keep the existing handler-shaped authoring mode as the default compatibility
   surface.
2. Add `--shape target` to `patch install`; `patch replace` will normally infer
   the installed shape and may accept the option as an assertion.
3. Compile target-shaped submissions outside the game with a staged
   Roslyn-and-metadata pipeline.
4. Treat projected fields and retained primary-constructor parameters as aliases
   for existing production storage.
5. Lower private access to generated `.NET 10` `UnsafeAccessor` methods.
6. Emit a static `[LivePatchHandler]` method with the real receiver as its hidden
   first parameter for the supported instance target.
7. Validate the exact running target and every generated accessor before
   publishing the patch.
8. Reuse the current managed and native runtime path without changing the native
   profiler ABI, C header, generated bindings, or ReJIT wrapper format.
9. Demonstrate the feature in the existing visual
   `AlvorKit.Engine.LiveCode.Demo` observatory.

The first implementation will not:

- add fields or otherwise change the layout of an existing object;
- run projected constructors or field initializers;
- resolve target-shaped constructor parameters from the selected executor
  scope;
- promise physical replacement of the target `MethodDef`;
- support static target methods; static members used by an instance target are
  supported through their separate accessor shape;
- support async, iterator, generic, value-type, volatile-field, fixed-buffer,
  or inaccessible-signature target shapes before their specific semantics are
  proven;
- provide `Before`, `After`, or `Around` behavior; or
- change the semantics of existing `[LivePatchHandler]` submissions.

## Motivation

The current handler contract is mechanically exact but pushes runtime
machinery into ordinary experimental code:

```csharp
public sealed class FasterOrbit(ColonySky sky)
{
    [LivePatchHandler]
    public void Run(ColonyGarden receiver, double delta)
    {
        receiver.Phase += delta * 8.5;
        sky.Weather = "chromatic storm";
    }
}
```

This has three ergonomic and semantic costs:

- the agent must name and pass the receiver explicitly;
- private receiver state is not available through ordinary C# member syntax;
  and
- constructor dependencies are resolved into a separate handler object, which
  need not be reference-identical to dependencies already retained by a
  particular receiver.

Target-shaped source should let the agent reason about one ordinary production
object. An unqualified field, property, method, primary-constructor capture, or
`this` expression must mean the corresponding member of that selected object.

## Existing Runtime Baseline

The current architecture already contains the execution machinery needed by
the lowered result:

- `LiveCodeCompiler.CompilePatch` emits a collectible handler assembly.
- `LivePatchSubmissionLoader` finds the single `[LivePatchHandler]` method and
  resolves an exact target overload.
- `LivePatchInstaller` asks the interception backend for an exact trampoline.
- `LivePatchRuntime` selects a trampoline for the real receiver.
- the ReJIT wrapper calls the selected exact managed function pointer and
  executes untouched original IL on a miss;
- `LivePatchLease.Replace` atomically publishes a new managed trampoline
  without another ReJIT; and
- removal, failure, or scope end deactivates dispatch and eventually releases
  the collectible submission.

A generated static handler already fits the trampoline factory. Native code sees
the same receiver-plus-declared-arguments signature it sees for an explicit
handler. No new native dispatch form is required.

## Authoring Contract

### Shape selection

Existing behavior remains:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch install `
    --shape handler `
    --session my-game `
    --scope 4 `
    --selector exact-scope `
    --target "MyGame.WorldService::Update" `
    --target-assembly MyGame `
    --file tmp\live\world-debug\lp\001-handler.cs `
    --workspace world-debug
```

Target-shaped behavior is explicit:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch install `
    --shape target `
    --session my-game `
    --scope 4 `
    --selector exact-scope `
    --target "MyGame.WorldService::Update" `
    --target-assembly MyGame `
    --file tmp\live\world-debug\lp\001-update.cs `
    --workspace world-debug
```

`handler` remains the default initially so existing scripts and checked-in
submissions remain source-compatible. Capabilities will advertise:

```json
{
  "submissionShapes": ["handler", "target"]
}
```

### Target-shaped source

One target-shaped submission contains:

- one top-level, non-generic reference class with the exact target namespace and
  type name;
- the matching primary-constructor parameter list when the method uses retained
  constructor captures;
- declarations for the existing private fields the method directly uses;
- exactly one target method with the exact declared signature; and
- ordinary C# method code with no receiver parameter, handler attribute,
  accessor declaration, reflection, or unsafe code.

Example:

```csharp
namespace MyGame;

public sealed class WorldService(RenderClock clock, WorldState state)
{
    private double phase;
    private string mode;

    public void Update(double delta)
    {
        phase += delta;
        AdvanceMode();
        state.Advance(clock.Now, phase);
        mode = "TARGET SHAPE";
    }
}
```

The projection may omit unused target fields and members. The compiler supplies
analysis-only descriptions of existing methods and properties from the exact
target metadata so calls such as `AdvanceMode()` bind normally.

Projected fields:

- must already exist on the exact declaring type;
- must match name, field type, static/instance shape, readonly shape, and
  relevant custom modifiers;
- cannot have an initializer;
- do not create storage; and
- read or mutate the current field value on the selected production receiver.

An unknown, mistyped, newly initialized, or otherwise mismatched field is a
compile-time LivePatch diagnostic.

The target method itself must be an instance method on that reference class in
v1. A global selector may still select every receiver of an instance method; it
does not imply support for a static target.

### Constructor parameters

A primary-constructor parameter can be used only when the loaded production
type already retains it in a uniquely verified field.

For example, `clock` may map to a compiler-generated capture such as
`<clock>P` only when the compiler proves:

- the selected production constructor parameter has the same position, name,
  and exact type;
- one existing compiler-generated field has the expected exact signature; and
- the production constructor directly stores that parameter into that field.

The field name alone is not sufficient proof.

`LivePatchTargetResolver` must prove this from the exact loaded module's IL,
using `PEReader`/`MetadataReader` against the MVID-verified file. For every used
primary parameter, inspect the matching production constructor and require one
unambiguous direct parameter-to-instance-field store with the exact field type.
The initial recognizer accepts only `ldarg.0`, the exact parameter load, optional
`nop` instructions, and `stfld`; it rejects conversions, helper calls, branches,
or transformed values until separately proven. Secondary constructors must
chain into that initialization path. The resulting capture `FieldDef` token is
part of `requiredMembers`. Do not infer capture solely from Roslyn naming
conventions, reflection field order, or a field whose name happens to resemble
the parameter.

If the original class used a constructor parameter only during construction,
the live object has no retained value for a later method to read. Using that
parameter in a target-shaped patch is therefore rejected. Resolving a new value
from the executor scope would violate per-receiver identity and the
"as-written-on-this-object" contract.

An ordinary explicit-constructor parameter is not an instance member in normal
C#. Target-shaped code should declare and use the actual existing field or
property that retained it.

### State and cleanup semantics

Removal restores future execution of the original method. It does not roll back
mutations that the patch already made to fields, injected dependencies, or
other runtime state.

The demo and public documentation must make this distinction visible. A field
returns to an original-looking value only if later original code writes that
value.

## Exact Semantic Boundary

The following behaviors are required in the first supported target shape:

- `this` means the real target receiver.
- A declared projected field means the exact existing production field.
- A verified retained primary parameter means its existing production capture.
- Existing public instance members dispatch on the real receiver.
- Existing private field reads, writes, compound assignments, increments,
  decrements, and supported `ref` uses preserve field semantics.
- Existing private property getters/setters and private method calls work after
  their focused lowering proofs.
- Locals, parameters, patterns, lambdas, and local functions retain ordinary
  lexical shadowing.
- `nameof` is replaced with the compile-time constant obtained from the
  analysis model; its projected alias syntax is not emitted into the final
  handler.
- Omitted caller-info arguments are materialized from the target-shaped semantic
  model so generated method names do not leak into ordinary calls.
- Return type, parameter types, `ref`/`in`/`out`, and required/optional custom
  modifiers exactly match the target method. Copy only parameter flags proven
  to be required by the exact handler ABI; do not copy arbitrary custom
  attributes onto the generated handler.

The following observable differences remain documented:

- `MethodBase.GetCurrentMethod()` observes the generated handler.
- Stack traces contain the generated handler method, with the submission source
  path supplied through PDB mapping.
- Caller introspection that deliberately examines the physical method can see
  the generated boundary.
- Closures and local functions are owned by the collectible submitted assembly.
- A closure stored into production state can intentionally keep that assembly
  alive after patch removal.

Async and iterator target methods are deferred. An exception thrown after an
`await` or during later enumeration does not cross the current exact trampoline,
so the existing containment contract would otherwise be misleading.

## Target Identity And Metadata Snapshot

### Reference manifest

Extend `LiveCodeReferenceManifest` additively with module identity records while
retaining `AssemblyPaths` and `GlobalUsings`:

```csharp
public sealed record LiveCodeReferenceModule(
    string AssemblyName,
    string AssemblyFullName,
    string Path,
    Guid ModuleMvid);
```

The running process supplies the authoritative loaded MVID for each file-backed
module. Before compiling, the CLI reads the file at `Path` and verifies that its
PE MVID still equals the loaded MVID. If the file was rebuilt or replaced after
process launch, target-shaped compilation stops with an actionable stale-file
diagnostic.

Target-shaped v1 requires one stable, readable PE image for every referenced
module involved in target or private-member resolution. Reject dynamic,
locationless, single-file-only, missing, and unreadable images. Also reject
duplicate loaded candidates for the same MVID/token identity instead of
choosing one by load order. Handler-shaped mode keeps its existing reference
behavior.

The target type and every type in the target/accessor signatures must also be
shareable with `LivePatchSubmissionLoadContext`. For v1, require those
assemblies to be loaded in `AssemblyLoadContext.Default`; otherwise the current
load-context resolver cannot give the submission the exact production type.

### Target descriptor

The compiler creates a pinned target descriptor containing:

- target module MVID;
- MethodDef token;
- exact raw metadata signature hash;
- assembly and declaring-type identity;
- method name, static/instance shape, return, parameters, attributes, and custom
  modifiers;
- eligible target restrictions; and
- a schema fingerprint for every production member required by the generated
  handler.

Each private-member requirement records:

- member kind;
- declaring module MVID and exact declaring type;
- FieldDef or MethodDef token when applicable;
- metadata name;
- raw signature;
- static/instance and relevant flags; and
- the generated accessor that consumes it.

Each retained-constructor capture additionally records the constructor
MethodDef/signature, a hash of its exact method body, the parameter position,
the capture FieldDef, and the accepted direct-store proof. The target process
repeats that proof rather than trusting the client's claimed field mapping.

The target process resolves tokens again and recomputes the schema fingerprint
before installing or replacing a patch. The game is authoritative even when
the client compiled against a path that looked valid.

### Replacement identity

`LivePatchSubmittedPatch` will retain the resolved target descriptor and
submission shape. Patch status will return them.

For target-shaped replacement, the CLI first obtains the installed descriptor
from status and compiles against that exact identity. It must not rediscover a
target from an unqualified type or method name. An optional `--shape target` on
replace is an assertion; omitting it uses the installed shape.

Cross-shape replacement is deferred initially. A handler-shaped patch is
replaced by another handler-shaped patch, and a target-shaped patch by another
target-shaped patch.

## Compiler Architecture

### Compiler ownership

Add a target-shaped compiler beside the current external LiveCode compiler in:

```text
scripts/AlvorKit.Script.LiveCode/Compilation/
```

Suggested cohesive types:

```text
LivePatchTargetSourceCompiler
LivePatchTargetResolver
LivePatchTargetDescriptor
LivePatchTargetProjection
LivePatchTargetProjectionValidator
LivePatchTargetSemanticRewriter
LivePatchTargetAccessorEmitter
LivePatchTargetDiagnostic
```

Split common parse/reference/emit code out of `LiveCodeCompiler` rather than
duplicating current reference-manifest and portable-PDB setup.

### Stage 1: production module and type resolution

Create a reference-only Roslyn compilation with:

- the exact manifest assembly paths;
- `MetadataImportOptions.All`;
- the target session's global usings; and
- the current preview language version.

Resolve the production assembly explicitly by the selected assembly identity.
Resolve the target type through that assembly symbol rather than calling
`GetTypeByMetadataName` after adding the target-shaped source, because the
projection intentionally has the same fully qualified type name.

Do not select an overload yet. At this stage, pin the unique file-backed module
and production type that the source projection must mirror.

### Stage 2: analysis projection and overload pinning

Parse the target-shaped source using its real `lp/*.cs` path.

Require:

- one eligible top-level class;
- the exact target namespace and metadata type name;
- no unsupported type parameters, record shape, nested target, or layout
  change;
- a matching constructor projection when primary captures are used;
- exactly one matching target method; and
- no source member intended to create new runtime state.

Build an analysis-only surrogate:

- retain the user-declared field aliases;
- add stubs for existing target members needed for semantic binding;
- add synthesized stubs for supported public inherited members without
  pretending the surrogate is the production base hierarchy;
- omit the target method stub because the user supplies it; and
- keep a map from every surrogate symbol to one authoritative production
  symbol.

After obtaining the projected method symbol, map every surrogate self-reference
back to the production type and compare its exact return, parameters, ref kinds,
and custom modifiers with the production overloads. Require one match, then pin
its MVID, MethodDef, and raw signature. This ordering avoids guessing semantic
types from syntax before aliases and global usings have bound.

Use Roslyn semantic symbols and operations for rewriting. Do not rewrite
identifier text heuristically. Text-only rewriting cannot safely distinguish
fields from locals, parameters, pattern variables, lambda captures, local
functions, overloads, or shadowed member names.

Reject source diagnostics before generating a handler. Suppress only
projection-specific diagnostics that the compiler intentionally creates, such
as duplicate target-type warnings and uninitialized alias fields.

### Stage 3: semantic lowering

Generate one uniquely named public handler container with:

- a concrete sealed type that is never instantiated in target-shaped mode;
- generated private static accessors;
- one public static `[LivePatchHandler]` method;
- the real target type as the first `receiver` parameter; and
- the exact target method's declared parameters and return.

The generated type may use a private constructor to make accidental
construction impossible. The loader must not impose the handler-shaped public
constructor contract when `submissionShape` is `target`.

Lower:

- `this` to `receiver`;
- public instance fields/properties/methods to receiver member access;
- non-public fields to ref-returning generated field accessors;
- retained primary parameters to their verified field accessors;
- private methods to exact generated method accessors;
- private property reads and writes to accessor calls;
- supported static members to their exact static form; and
- implicit caller-info arguments to explicit constants from the analysis
  operation.

Rewrite recursively inside supported lambdas and local functions. Preserve
local and parameter shadowing through the semantic model.

Initial fail-closed diagnostics cover unsupported:

- static target methods;
- `base` access and protected virtual base-call semantics;
- non-public inherited members;
- private method groups and delegates;
- property patterns over inaccessible members;
- generic private helpers;
- explicit-interface calls;
- async, iterator, and async-iterator target methods;
- target or signature types inaccessible to the final handler compilation;
- volatile and fixed fields;
- ref-like, pointer, function-pointer, open-generic, and other existing
  LivePatch-ineligible target shapes; and
- any symbol the rewriter cannot map uniquely.

Expand these cases only after a focused semantic and executable proof.

### Accessor generation

Use runtime-supported `UnsafeAccessor` methods. For a field:

```csharp
[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "phase")]
private static extern ref double Phase(WorldService receiver);
```

The generated accessor uses the member's exact declaring type. The runtime
does not walk the type hierarchy when matching an `UnsafeAccessor`, so an
inherited member must name its actual owner.

Field accessors return `ref`; this preserves reads, writes, compound assignment,
increment/decrement, and supported ref usage without per-call reflection or
allocation.

Method accessors reproduce the exact return, parameters, generic construction
when later supported, and constraints required by `.NET 10`.

For `UnsafeAccessorKind.StaticField` and `StaticMethod`, emit the required first
parameter typed as the exact owning type; its value is ignored by the runtime
and the generated call passes `null`. This owner parameter is unrelated to the
hidden receiver of the supported instance target. Target-shaped v1 does not
patch static methods.

The relevant runtime matching contract is documented by Microsoft:

<https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.unsafeaccessorattribute?view=net-10.0>

### Final compilation and diagnostics

Compile only the generated handler and accessor container against the running
target's reference manifest. Do not emit the analysis surrogate.

Compile target-shaped v1 deterministically with `OptimizationLevel.Release`
because service update methods can be hot. A future debug/release choice must
be an explicit CLI and manifest contract rather than an inferred session
property. Continue emitting a portable PDB.

Add `#line` mapping around transplanted user expressions and statements, and
hide generated scaffolding. Runtime exceptions and debugger sequence points
should name the numbered workspace submission, not a synthetic compiler file
or the production source file.

PDB tests must verify the mapped submission document name and checksum. Do not
assume that a `#line` directive alone carries the original source checksum.

Introduce stable `LPxxxx` diagnostics for:

- wrong target class or method;
- ambiguous overload;
- constructor projection mismatch;
- unretained primary parameter;
- missing or mismatched field alias;
- forbidden field initializer;
- readonly write;
- unsupported symbol or language construct;
- stale target module;
- stale member schema;
- generated accessor mismatch; and
- final compilation failure.

Translate final-pass diagnostics back to user source spans. Save generated
source only as an event artifact when diagnosing a compiler defect; it is not
an authoring surface.

## Runtime And Bridge Changes

### Protocol

Keep bridge version 1 backward compatible with additive fields:

```json
{
  "operation": "install",
  "submissionShape": "target",
  "targetDescriptor": {},
  "requiredMembers": [],
  "entryType": "...",
  "assembly": "...",
  "symbols": "..."
}
```

Legacy requests without `submissionShape` remain `handler`.

Capabilities add `submissionShapes`. Status and list include the shape and
resolved target identity. The bridge request remains bounded by the existing
compiled submission size policy because the final output is a small generated
handler assembly, not a rebuilt copy of the game assembly.

### Submission loading

Split target-shaped validation out of `LivePatchSubmissionLoader` when needed
to keep responsibilities clear.

For a target-shaped install:

1. Load the generated assembly into the current collectible load context.
2. Find its single static `[LivePatchHandler]` method.
3. Resolve the exact running target from the pinned MVID, MethodDef, and
   signature.
4. Recompute and compare every required-member fingerprint.
5. Inspect every generated `UnsafeAccessor` method against the authoritative
   owner, name, kind, and exact signature.
6. Structurally validate every accessor and force a non-mutating binding proof
   for every generated method body that can call one, including generated
   closure and local-function methods, so a missing or ambiguous member fails
   before publication.
7. Pass `handlerInstance: null` and the generated static method to
   `LivePatchSession.InstallReplace`.

Do not call `CreateHandler` for target-shaped submissions. In particular, do
not resolve projected constructor parameters from the executor scope.

The executor scope still determines:

- the exact-scope or descendant selector;
- the exact instance resolved for `exact-instance`; and
- ordinary patch ownership and lifecycle.

It does not supply target-shaped service state.

### Dispatch, failure, and removal

No changes are planned in:

- `AlvorKit.LivePatch` receiver matching;
- `LivePatchRuntime.ResolveHandler`;
- exact trampoline acquisition and in-flight drain;
- native exact-dispatch wrapper generation;
- native ReJIT/install/revert requests;
- existing-inliner repair; or
- collision claims.

A lowered handler exception follows the current containment policy: the first
failing invocation is contained, its handler deactivates, and subsequent calls
fall through to original IL. This guarantee applies only to synchronous work
that crosses the trampoline.

Atomic target-shaped replacement creates and prevalidates the next generated
handler before `LivePatchLease.Replace` publishes it. It must keep the same
patch ID, native patch ID, profiler request ID, active-patch count, and native
ReJIT count.

### Submission lifetime evidence

Generalize bridge registration ownership so it can report:

- current retained generation;
- every retired generation still unloading;
- collected retired generations when retained for diagnostic history; and
- source shape, source hash, target descriptor, and executor scope.

The current single `unloaded[patchId]` entry can lose observability during rapid
multiple replacements. Replace it with generation-aware retirement records
rather than overwriting one weak reference per patch.

Retired-generation records may keep only weak load-context references and
non-collectible value/string diagnostics. They must not retain submitted
`Type`, `MethodInfo`, delegates, or exception objects. Keep every generation
that is still unloading, but prune or bound already-collected diagnostic
history so repeated replacement cannot grow cold-path state without limit.

Prove that generated `UnsafeAccessor` methods do not keep a collectible
submission alive after:

- atomic replacement;
- explicit removal;
- automatic scope end;
- handler failure; and
- bridge/session disposal.

An intentionally escaped closure may keep an assembly alive; status and
documentation should describe that as retained submitted state rather than
pretending cleanup completed.

## Package And File Map

Expected script changes:

```text
scripts/AlvorKit.Script.LiveCode/AlvorKit.Script.LiveCode.csproj
scripts/AlvorKit.Script.LiveCode/Compilation/LiveCodeCompiler.cs
scripts/AlvorKit.Script.LiveCode/Compilation/LivePatchTargetSourceCompiler.cs
scripts/AlvorKit.Script.LiveCode/Compilation/LivePatchTargetResolver.cs
scripts/AlvorKit.Script.LiveCode/Compilation/LivePatchTargetProjection.cs
scripts/AlvorKit.Script.LiveCode/Compilation/LivePatchTargetProjectionValidator.cs
scripts/AlvorKit.Script.LiveCode/Compilation/LivePatchTargetSemanticRewriter.cs
scripts/AlvorKit.Script.LiveCode/Compilation/LivePatchTargetAccessorEmitter.cs
scripts/AlvorKit.Script.LiveCode/LivePatchCli.cs
scripts/AlvorKit.Script.LiveCode/LivePatchCommandTree.cs
```

Expected LiveCode metadata changes:

```text
src/AlvorKit.LiveCode/Execution/LiveCodeReferenceManifest.cs
src/AlvorKit.LiveCode/Execution/LiveCodeReferenceCatalog.cs
```

Expected engine bridge changes:

```text
src/AlvorKit.Engine.LivePatch/LivePatchBridgeProtocol.cs
src/AlvorKit.Engine.LivePatch/LivePatchLiveCodeBridge.cs
src/AlvorKit.Engine.LivePatch/LivePatchSubmissionLoader.cs
src/AlvorKit.Engine.LivePatch/LivePatchBridgeRegistrations.cs
```

Create additional cohesive loader/validation/registration types instead of
allowing these files to accumulate compiler-schema, lifetime, and protocol
responsibilities.

No source or generated-output change is expected beneath:

```text
native/interception-profiler/
src/AlvorKit.Interception/
src/AlvorKit.Interception.CoreClr/
```

If implementation reveals a required change there, stop and review the design
before expanding scope. A native change would invalidate the managed-only risk
and verification assumptions of this plan.

## Vertical Feasibility Spike

Before building the full compiler, complete one small executable spike.

The fixture production type must contain:

- one private value field;
- one private reference field;
- one retained primary-constructor dependency;
- one unretained primary-constructor parameter used only during construction;
- one ordinary instance method; and
- a second same-scope dependency instance with distinguishable identity.

The spike must prove:

1. target-shaped source compiles into a static exact handler;
2. private value and reference fields read and write the real receiver;
3. the retained dependency is reference-identical to the receiver's field;
4. the unretained dependency is rejected before loading;
5. accessor binding is proven before patch publication;
6. the generated handler plugs into the existing trampoline;
7. warmed active dispatch allocates zero bytes;
8. disposal releases the generated handler assembly after acquired calls drain;
   and
9. no native source, ABI header, binding, or profiler request changes are
   needed.

Do not proceed to the broad semantic rewriter if the retained-capture proof or
collectible-accessor proof fails. Investigate a separately reviewed
`DynamicMethod` or physical IL-relocation design instead.

## Automated Test Plan

### New script compiler tests

Create:

```text
tests/AlvorKit.Script.LiveCode.Test/
```

Add `InternalsVisibleTo` from the script project and add the test project to
`AlvorKit.slnx`.

Suggested test areas:

```text
LivePatchTargetSourceCompilerTest
LivePatchTargetMemberBindingTest
LivePatchTargetCaptureTest
LivePatchTargetDiagnosticTest
LivePatchTargetPdbTest
LivePatchTargetCommandLineTest
```

Required compiler coverage:

- legacy handler compilation remains unchanged;
- exact target assembly/type/overload resolution;
- instance `this`;
- public members;
- private value and reference field reads/writes;
- assignment, compound assignment, increment/decrement, and supported `ref`
  operations;
- readonly field reads and compile-time write rejection;
- retained primary captures and exact reference identity;
- unretained and ambiguous capture rejection;
- private property getter/setter and private method invocation;
- exact overload selection including `ref`, `in`, and `out`;
- source locations through first-pass and generated-pass diagnostics;
- PDB document path and checksum for the numbered submission;
- caller-info argument materialization;
- supported lambdas/local functions and lexical shadowing;
- wrong field name/type/static/readonly shape;
- field initializer and new-field rejection;
- volatile, fixed, base, private method-group, generic, async, and iterator
  fail-closed diagnostics;
- stale on-disk module MVID;
- dynamic, locationless, non-default-ALC, and duplicate-MVID target rejection;
- stale target/member schema; and
- an invalid accessor referenced only by a generated lambda/local-function body
  fails preflight;
- deterministic generated output for the same descriptor and source, excluding
  intentionally unique assembly identity.

### New engine bridge tests

Create:

```text
tests/AlvorKit.Engine.LivePatch.Test/
```

Add `InternalsVisibleTo` from `AlvorKit.Engine.LivePatch` and add the project to
the solution.

Required runtime coverage:

- target-shaped static handlers are never constructed, including a generated
  handler whose constructor deliberately throws;
- every accessor is validated and prepared before install;
- a stale descriptor fails before `InstallReplace`;
- the in-process constructor-body/capture proof rejects a changed or forged
  parameter-to-field mapping;
- exact production fields are mutated by direct handler invocation;
- source install, replacement, remove, failure, and status records retain the
  intended shape and target identity;
- rapid replacements retain unload evidence for every retired generation;
- generated submission contexts collect after replace, remove, scope end,
  failure, and bridge disposal;
- failures before publication unload the rejected context; and
- source-shaped replacement cannot silently change target or submission shape.

### Existing test projects

Continue using:

- `tests/AlvorKit.LivePatch.Test` for exact-instance, exact-scope, descendants,
  global selection, collision, scope end, and session lifecycle;
- `tests/AlvorKit.Interception.Test` for exact trampoline signature, exception,
  in-flight, reference, and allocation contracts;
- `tests/AlvorKit.LiveCode.Test` for additive reference-manifest identity data;
  and
- `tests/AlvorKit.Script.LiveWorkspace.Test` for recorded shape, source hash,
  target identity, intervention resolution, and workspace closure.

Add a focused zero-allocation test for warmed generated field-access dispatch.
The compiler and install paths are cold and may allocate; the per-invocation
path may not add reflection, arrays, boxing, closures, or other managed
allocation beyond allocations intentionally written by the submitted method.

### Profiled integration proof

The feature-level profiled integration proof is the profiler-enabled,
AlvorSense-owned observatory showcase described below. It is the one proof that
crosses target-shaped compilation, bridge validation, exact-scope selection,
managed dispatch, and native ReJIT in the same process.

Run the existing isolated low-level proof separately as an inliner,
original-fallback, and profiler regression:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestInterception -- `
    --exec-project demos\AlvorKit.Engine.LivePatch.Demo\AlvorKit.Engine.LivePatch.Demo.csproj `
    --configuration Release `
    --module AlvorKit.Engine.LivePatch.Demo -- `
    --proof
```

Both runs use the restored native runtime asset. Do not run the native build
tool or install native dependencies during normal implementation or
verification.

The profiled proof must show:

- target-shaped exact-scope install reaches `Active`;
- an unselected sibling continues through original IL;
- atomic replacement changes only the managed handler and creates no new native
  request;
- contained failure returns once and future calls execute original IL;
- immediate managed removal falls through to original IL;
- asynchronous native restoration reaches `Removed`;
- existing inliner and original-fallback behavior remain unchanged; and
- profiler request ID, native patch ID, pending-request count, and active-patch
  count retain current invariants.

A repository diff audit must confirm that no native ABI header, native source,
generated profiler binding, or generated binding version changed.

## Visual Demo Plan

### Demo selection

Extend:

```text
demos/AlvorKit.Engine.LiveCode.Demo/
```

Do not create another profiler demo. The existing observatory already provides:

- three simultaneous colony scopes;
- one receiver per scope;
- LiveCode and LivePatch bridge composition;
- AlvorSense-compatible deterministic rendering and input;
- a visible patch-status panel; and
- checked-in teaching submissions excluded from normal compilation.

Keep `AlvorKit.Engine.LivePatch.Demo` as the low-level interception/ReJIT proof.

### Target service

Patch `ColonySimulation.Update(double)`. It already has two same-scope injected
primary-constructor dependencies:

```csharp
public sealed class ColonySimulation(ColonyGarden garden, ColonySky sky)
```

Evolve the production demo service to contain meaningful existing private
state and helpers:

```csharp
private double atmospherePhase;
private string mode = "ORIGINAL";

public string Mode => mode;

private float AtmosphereWave =>
    MathF.Sin((float)atmospherePhase);

private void AdvanceAtmosphere(double delta, float rate)
{
    atmospherePhase += delta * rate;
    sky.Warp = Math.Clamp(
        0.25f + AtmosphereWave * 0.2f,
        0.02f,
        0.85f);
}
```

The original `Update` uses the retained `garden` and `sky` dependencies, the
private phase, property, method, and mode. It writes `mode = "ORIGINAL"` each
invocation.

Render `colony.Simulation.Mode` beside each colony's existing form/population
detail so source install, replacement, failure fallback, and original
restoration are visible in screenshots.

### Checked-in target-shaped submissions

Add:

```text
demos/AlvorKit.Engine.LiveCode.Demo/Submissions/SourceShapedOrbit.cs
demos/AlvorKit.Engine.LiveCode.Demo/Submissions/SourceShapedReverseOrbit.cs
demos/AlvorKit.Engine.LiveCode.Demo/Submissions/SourceShapedExplodingOrbit.cs
```

Each source file uses the target class name and constructor projection, declares
only the existing private field aliases it directly accesses, and defines the
ordinary `Update(double)` method.

The first sample:

- reads and writes `atmospherePhase`;
- uses both `garden` and `sky` retained dependencies;
- reads `AtmosphereWave`;
- calls `AdvanceAtmosphere`;
- visibly changes color, orbit, population, atmosphere, and mode; and
- writes `mode = "SOURCE / PRIVATE"`.

The second sample reverses the motion and writes
`mode = "REPLACED / PRIVATE"`.

The third sample writes a temporary failure marker and throws synchronously.
The following original invocation writes `ORIGINAL`, visibly proving fallback
without pretending that the earlier private-state mutation was rolled back.

Keep the existing `FasterOrbit`, `ReverseOrbit`, and `ExplodingOrbit` handler
samples as the low-level/compatibility ABI examples.

### Recorded showcase

AlvorSense must own the one Release observatory process; a VS Code launch and
AlvorSense cannot both own the same target. Resolve the already-restored
profiler beside the Release demo and launch that exact assembly with the
complete profiler environment:

```powershell
$gameDll = (Resolve-Path `
    "bin\AlvorKit.Engine.LiveCode.Demo\Release\AlvorKit.Engine.LiveCode.Demo.dll").Path
$profilerDll = (Resolve-Path `
    "bin\AlvorKit.Engine.LiveCode.Demo\Release\runtimes\win-x64\native\AlvorKit.Interception.Profiler.Native.dll").Path

dotnet run --project scripts\AlvorKit.Script.AlvorSense -- start `
    --id livepatch-target-shape `
    --assembly $gameDll `
    --env "CORECLR_ENABLE_PROFILING=1" `
    --env "CORECLR_PROFILER={3840ACF7-5AF1-49EA-BF94-5F7086C57F57}" `
    --env "CORECLR_PROFILER_PATH=$profilerDll" `
    --env "CORECLR_PROFILER_PATH_64=$profilerDll" `
    --env "ALVORKIT_INTERCEPTION_PROFILER_PATH=$profilerDll" `
    --env "ALVORKIT_INTERCEPTION_MODULES=AlvorKit.Engine.LiveCode.Demo" `
    --env "DOTNET_ReadyToRun=0"
```

After that process advertises the LiveCode session, initialize the workspace
against both immutable identities:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace init `
    --id livepatch-target-shape `
    --purpose "Prove target-shaped private service editing" `
    --session mycelial-observatory `
    --alvorsense livepatch-target-shape
```

Record:

1. `evidence/001-baseline.png` after deterministic updates.
   Ember, Tide, and Moon show `ORIGINAL`.
2. Copy the first checked-in sample to numbered workspace source
   `lp/001-source-shaped-orbit.cs`.
3. Install it with `--shape target` and `exact-scope` for Moon Garden.
   If the paused deterministic loop blocks the bridge, keep the command running
   and send one workspace-recorded zero-delta update.
4. Advance deterministic time and capture
   `evidence/002-source-active.png`.
   Moon shows `SOURCE / PRIVATE`; its siblings remain `ORIGINAL`.
5. Copy the replacement to `lp/002-source-shaped-reverse.cs`, replace the same
   patch, and capture `evidence/003-source-replaced.png`.
   Status shows the same patch/native identity and no new ReJIT request.
6. Remove the patch, wait for terminal native restoration, advance once, and
   capture `evidence/004-source-restored.png`.
   All receivers show `ORIGINAL`.
7. Install the numbered failing source as a fresh patch. Trigger at least two
   updates and enough additional deterministic updates for the observatory
   status panel to refresh.
8. Capture `evidence/005-source-failure-fallback.png`.
   The game remains responsive, Moon again shows `ORIGINAL`, and status shows
   `Failed`, the deliberate exception, terminal native removal, no current
   generation retained by LivePatch, and `unloading` or best-effort `collected`
   submission evidence.
9. Query both terminal patches. Confirm terminal native restoration,
   `Unload()` requested for every retired generation, and release of every
   LivePatch-owned strong reference.
10. Resolve and close the workspace without waiting indefinitely for GC, then
    stop only the AlvorSense session started for the showcase.

The README will include the exact commands, expected status fields, screenshots,
the state-not-rolled-back warning, and the distinction between deterministic
ownership release and nondeterministic ALC collection. Bounded-GC weak-reference
tests, not workspace closure, prove eventual collection.

### Demo files

Expected demo changes:

```text
demos/AlvorKit.Engine.LiveCode.Demo/ColonySimulation.cs
demos/AlvorKit.Engine.LiveCode.Demo/ObservatoryRenderer.cs
demos/AlvorKit.Engine.LiveCode.Demo/Submissions/SourceShapedOrbit.cs
demos/AlvorKit.Engine.LiveCode.Demo/Submissions/SourceShapedReverseOrbit.cs
demos/AlvorKit.Engine.LiveCode.Demo/Submissions/SourceShapedExplodingOrbit.cs
demos/AlvorKit.Engine.LiveCode.Demo/README.md
```

The demo project already excludes `Submissions/**/*.cs` from compilation, so no
project-file change should be necessary.

At implementation time, inventory and preserve unrelated or concurrent demo
edits before touching these paths.

## Documentation Deliverables

Update:

- `docs/LivePatch.md`: target-shaped mode first, handler ABI as the advanced
  explicit surface, exact private/capture rules, limitations, lifecycle, and
  state restoration semantics.
- `docs/AgentLiveDevelopment.md`: numbered target-shaped submissions, shape
  selection, compile failures outside the game, safe-frame coordination,
  replacement, terminal cleanup, and visual verification.
- `docs/LiveCode.md`: additive reference-module identity, compiler negotiation,
  capabilities, diagnostics, and status schema.
- the observatory README: complete target-shaped visual walkthrough.

`docs/Interception.md` should need no semantic change. If the implementation
touches the interception or native boundary, update that document only after
the design expansion is reviewed.

## Implementation Phases

### Phase 0: vertical proof

1. Add the private value/reference/capture fixture.
2. Lower one target-shaped method to a static exact handler.
3. Generate and preflight field accessors.
4. Invoke it against the real receiver.
5. prove identity, zero warm allocation, in-flight disposal, and collection.
6. Confirm the native boundary is unchanged.

Exit gate: every vertical-spike criterion passes.

### Phase 1: target identity and projection model

1. Add loaded module identities to the reference manifest.
2. Implement exact target resolution from assembly/type/method plus projected
   signature.
3. Define target/member descriptors and fingerprints.
4. Implement field-alias and primary-capture validation.
5. Add stable diagnostics for stale and impossible shapes.

Exit gate: target and schema mismatches fail before final compilation.

### Phase 2: field-complete compiler lowering

1. Build the analysis surrogate and symbol map.
2. Lower `this`, public members, explicit private fields, and retained primary
   captures.
3. Support reads, writes, compound operations, increments, and supported refs.
4. Emit the static exact handler and accessors.
5. Preserve source diagnostics and portable PDB mapping.

Exit gate: the requested private-field and injected-parameter experience works
without receiver/accessor syntax.

### Phase 3: private collaborators and semantic edges

1. Add private property getter/setter lowering.
2. Add private method invocation and overload matching.
3. Materialize caller-info defaults.
4. Support tested lambdas and local functions.
5. Add explicit fail-closed diagnostics for every deferred construct.

Exit gate: the observatory target-shaped samples compile without special
machinery and all supported constructs have focused tests.

### Phase 4: bridge and lifecycle integration

1. Add `submissionShape` and pinned schema to install/replace payloads.
2. Prevalidate and prepare generated accessors in the game.
3. Skip handler construction and Injection resolution for target shape.
4. Persist exact target/shape/source metadata in registrations and status.
5. Make retired-context tracking generation-aware.
6. Exercise install, replace, failure, scope end, remove, and disposal.

Exit gate: invalid generated code never becomes active; every retired
generation releases LivePatch-owned strong references, receives `Unload()`, and
remains weakly observable while unloading. Bounded-GC tests prove collection
when user code has not intentionally retained submitted state.

### Phase 5: CLI and workspace productization

1. Add `--shape` parsing and generated help.
2. Preserve handler as the default.
3. Infer target shape and exact target descriptor on replacement.
4. Record shape, source SHA-256, target identity, and generated-source
   diagnostic artifacts in workspace events.
5. Keep numbered `lp/` source immutability and existing cleanup ledger rules.

Exit gate: the full workflow can be driven only through documented CLI
commands with an auditable workspace.

### Phase 6: demo and executable evidence

1. Add private state/helpers and visible mode to `ColonySimulation`.
2. Add the three target-shaped samples.
3. Build the Release demo.
4. Run the existing isolated low-level profiler regression.
5. Run the profiler-enabled AlvorSense/workspace feature proof and capture
   evidence.
6. Prove terminal patch cleanup and workspace closure.

Exit gate: the requested authoring experience and every important lifecycle
transition are visible in one uninterrupted game process.

### Phase 7: documentation and Commit Mode verification

1. Update public and agent documentation.
2. Run focused coverage for each changed source project.
3. Run scoped lint over the intended files.
4. Re-read changed source, tests, scripts, demos, and docs.
5. Audit source file size, XML docs, line length, hot-path allocation, and
   concurrent-work preservation.
6. Confirm no native/generated-output change.

## Verification Commands

Focused development checks:

```powershell
dotnet test tests\AlvorKit.Script.LiveCode.Test\AlvorKit.Script.LiveCode.Test.csproj
dotnet test tests\AlvorKit.Engine.LivePatch.Test\AlvorKit.Engine.LivePatch.Test.csproj
dotnet test tests\AlvorKit.LiveCode.Test\AlvorKit.LiveCode.Test.csproj
dotnet test tests\AlvorKit.LivePatch.Test\AlvorKit.LivePatch.Test.csproj
dotnet test tests\AlvorKit.Interception.Test\AlvorKit.Interception.Test.csproj `
    --filter "FullyQualifiedName~InterceptionHandlerTrampoline"
dotnet build demos\AlvorKit.Engine.LiveCode.Demo\AlvorKit.Engine.LiveCode.Demo.csproj `
    -c Release
```

Commit Mode coverage:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- `
    --agent `
    --source-project AlvorKit.Script.LiveCode `
    --test-project AlvorKit.Script.LiveCode.Test

dotnet run --project scripts\AlvorKit.Script.TestCoverage -- `
    --agent `
    --source-project AlvorKit.Engine.LivePatch `
    --test-project AlvorKit.Engine.LivePatch.Test

dotnet run --project scripts\AlvorKit.Script.TestCoverage -- `
    --agent `
    --source-project AlvorKit.LiveCode

dotnet run --project scripts\AlvorKit.Script.TestCoverage -- `
    --agent `
    --source-project AlvorKit.LivePatch
```

Run the low-level profiler regression through the repository's isolated
launcher. Run the target-shaped feature proof through the profiler-enabled
AlvorSense launch specified in the demo plan. Both use the restored runtime
asset. Do not invoke `AlvorKit.Script.NativeBuild` or install native build
dependencies unless the user separately requests and authorizes that work.

The visual AlvorSense run is an acceptance showcase rather than an ordinary
unit test. Record the exact input/update batches, screenshots, status, removal,
ownership release, unload request, best-effort collection state, and workspace
closure.

## Risk Register

### Primary-constructor capture ambiguity

Risk: a parameter name appears source-visible but the live object contains no
field, or more than one candidate field exists.

Mitigation: require direct constructor-store proof and reject ambiguity. Never
fall back to executor-scope resolution.

### Semantic rewriting drift

Risk: textual rewriting changes shadowing, overload, ref, property, lambda, or
caller-info behavior.

Mitigation: use Roslyn symbols/operations, map every surrogate symbol to one
production symbol, and reject unsupported operations.

### Late accessor failure

Risk: `UnsafeAccessor` throws only on the first live invocation.

Mitigation: compare exact schema and force non-mutating accessor binding/JIT
preparation before publication.

### Collectible-context retention

Risk: runtime-generated accessor stubs or escaped closures retain a submitted
assembly.

Mitigation: prove every lifecycle path with weak references and bounded
collection loops; report intentionally escaped closures as retained state.

### Private property and method edge cases

Risk: getters/setters, overloads, inheritance, virtual dispatch, generic
constraints, or method groups do not match original C# semantics.

Mitigation: stage them after field support, test exact signatures, use the real
declaring owner, and keep unsupported forms fail-closed.

### Physical-method identity mismatch

Risk: documentation implies that reflection/debugging sees the target method
body itself.

Mitigation: consistently call the feature target-shaped source, document the
generated handler boundary, and reserve literal body replacement for a
separate IL-relocation design.

### Async failure containment

Risk: exceptions after an async method returns its task bypass synchronous
trampoline containment.

Mitigation: reject async and iterator target methods initially.

### Hot-path regression

Risk: generated private access allocates or adds material per-frame overhead.

Mitigation: use ref-return accessors, keep current exact calli dispatch, add
zero-allocation assertions, and record a representative warmed timing
observation without turning wall-clock values into brittle gates.

### Concurrent repository work

Risk: the existing LiveCode/LivePatch demos or documentation contain unrelated
in-progress edits.

Mitigation: inventory status and exact diffs before every implementation phase,
preserve existing work, and stage only explicitly owned paths in Commit Mode.

## Completion Criteria

The work is complete when:

- `--shape target` accepts an ordinary target-shaped service projection with no
  explicit receiver, handler attribute, accessor, reflection, or unsafe code;
- projected private fields read and mutate the selected production receiver;
- verified constructor captures use the receiver's exact stored dependencies;
- uncaptured parameters and new or mismatched fields fail before entering the
  game;
- existing private property and method forms used by the demo have focused
  semantic tests;
- source diagnostics and exception sequence points identify the numbered
  workspace source;
- exact-instance, exact-scope, descendants, and global selector behavior remain
  correct;
- target-shaped atomic replacement uses no additional ReJIT request;
- failure, removal, scope end, and disposal restore original fallback and
  release all non-escaped submission generations;
- warmed generated field access adds zero managed allocation;
- the observatory demo visibly proves install, scope isolation, replacement,
  failure fallback, restoration, and cleanup;
- existing handler-shaped commands and teaching submissions still work;
- the workspace ledger closes after terminal native restoration, release of all
  LivePatch-owned strong references, and an unload request for every retired
  generation; it does not wait indefinitely for nondeterministic GC;
- public and agent documentation state the exact semantic boundary;
- focused tests, coverage, demo build, profiled proof, visual evidence, and
  scoped lint pass in Commit Mode; and
- the final diff contains no native ABI, native profiler, bindgen, or generated
  binding change.

## Independent Validation

Three independent reviews challenged this plan:

- the compiler review confirmed that a staged semantic Roslyn lowering with
  generated `UnsafeAccessor` methods is viable, while identifying primary
  captures, caller-info defaults, source mapping, and unsupported semantic
  forms as mandatory fail-closed work;
- the runtime review confirmed that a static lowered handler fits the existing
  exact trampoline and selector path with no native ABI change, while requiring
  install-time schema/accessor preflight and generation-aware unload evidence;
  and
- the demo review selected `ColonySimulation.Update` in the existing LiveCode
  observatory and defined the exact visual install, replace, remove, failure,
  restoration, and workspace-cleanup sequence.

All three reviews agreed that the feature must be described as target-shaped
source rather than a physical target `MethodDef` replacement.
