# AlvorKit Game Repository Instructions

## Scope

These shared instructions apply to an AlvorKit game repository whose root
`AGENTS.md` routes here. Paths and commands in this document assume the current
directory is the game repository root and AlvorKit is checked out at
`../AlvorKit`.

Read the game repository's `README`, solution, project files, scripts, and
nearby source before assuming its local layout. Do not assume other sibling
game repositories exist.

## Development Status

This game is unshipped unless its own root `AGENTS.md` explicitly declares it
shipped and names its compatibility contract. Until then, the development-status
and breaking-change rules in `../AlvorKit/AGENTS.md` apply without relaxation.
The no-fallback rule remains unconditional even after a game ships.

## AlvorKit Relationship

The game consumes AlvorKit from `../AlvorKit` through relative project
references. AlvorKit owns the engine, UI, injection, windowing, GL lifetime,
maths, generated bindings, script tools, demos, documentation, and AlvorSense.

Do not treat AlvorKit as a fixed external dependency. We own that code too. If
the clean solution needs an engine API, UI primitive, script tool, harness
capability, math helper, resource abstraction, or docs update, make the right
change in `../AlvorKit` instead of forcing a game-local workaround. Keep truly
game-specific behavior in the game repository.

Engine and tooling changes must follow `../AlvorKit/AGENTS.md` and any closer
scoped AlvorKit `AGENTS.md`. Keep game-repository and AlvorKit status, staging,
and commits separate.

## Working Rules

Read `../AlvorKit/AGENTS.md` before non-trivial work. Its Working Mode, Commit
Mode, C# defaults, line-length rule, allocation discipline, visual automation,
generated-output, coordination, and verification rules apply to the game
repository. This document or a closer game `AGENTS.md` may override them only
where the originating AlvorKit rule permits an override.

Game-specific overrides:

- Game C# source files must not exceed 350 physical lines. Split files by
  responsibility before they reach the ceiling; cohesion is not an exception
  to the limit.
- Test files may also be up to 750 lines when related scenarios read better
  together.
- Do not create, add, or expand unit-test projects or unit tests for
  game-repository code unless the user explicitly requests tests. AlvorKit
  unit-test and coverage requirements apply only to changes made in
  `../AlvorKit`.
- Verify game behavior with targeted builds and, when appropriate, AlvorSense
  smoke checks. Do not propose unit tests as the default verification strategy
  for game features.
- Run existing game tests only when explicitly requested or when diagnosing a
  failure in those existing tests.
- Keep hot paths allocation-sensitive: update, render, input polling,
  emulation and simulation, resource lifetime, validation, bind and unbind,
  and teardown.
- Follow the runtime allocation discipline in `../AlvorKit/AGENTS.md` and
  `../AlvorKit/src/AGENTS.md`: avoid managed allocations and GC pressure in
  per-frame, per-tick, polling, render, simulation, resource, and teardown
  paths unless the cost is intentional and accepted. Watch for arrays,
  `List<T>`, LINQ, closures, iterator blocks, boxing, `params`, string
  formatting, async state machines, and defensive copies.
- Use AlvorKit shapes directly: scopes, controls, vectors, maths types,
  `GlLayer`, UI menus, and engine lifecycle APIs.
- For the maths type inventory, naming scheme, and usage rules, read
  `../AlvorKit/docs/Maths.md`; per-family members are in
  `../AlvorKit/docs/MathsReference.md`. The concrete maths structs are
  generated into the `AlvorKit.Maths.Primitives` package and have no source
  under `../AlvorKit/src/`.
- AlvorKit maths types and `ScalarMath` are mandatory for maths values. Do
  not model positions, sizes, or ranges as `(int, int, int)`-style tuples,
  parallel scalar members, or game-local vector/box/range types, and do not
  re-implement clamp/lerp/min/max-style helpers the maths surface already
  provides. A project missing the maths reference gains the reference; that
  is never a reason to invent a local shape.
- Use managed `System.IO.Hashing.XxHash3` for general-purpose
  non-cryptographic hashing, stable content fingerprints, and deterministic
  procedural sampling. Encode structured inputs explicitly with
  `BinaryPrimitives`; do not add native xxHash bindings, injected hash
  services, or game-local general-purpose hash implementations. Hash seed
  domains use `long`, matching the managed API. The C# `unchecked` keyword is
  forbidden. Keep a local integer mixer only for fixed-table slot selection,
  or a cheap rolling signature inside measured benchmark work, when that
  narrower operation is named and documented as such.

## VS Code Launch Configurations

Whenever an agent creates a project that can be launched directly, the same
change must add a checked-in VS Code launch configuration for it under
`.vscode/launch.json`. This requirement is unconditional and applies to
executables, games, demos, tools, and runnable fixtures in both Working Mode and
Commit Mode. Add any corresponding `.vscode/tasks.json` build task referenced
by `preLaunchTask`, and include the working directory, arguments, and
environment required for the launch configuration to exercise the project's
supported launch contract.

## Game Ents And ECS

Game Ents must use AlvorKit ECS. Use `Ent` in every context. The word `Entity`
is banned; use `Ents` for the plural. This applies to prose, code identifiers,
type and member names, parameters and locals, filenames, directories, labels,
and compound names.

Model players, enemies, projectiles, items, chunks, and other mutable simulated
objects with generated `[Components]`, ECS handles and arenas, and
`AlvorKit.ECS.Indexed` when their component writes maintain bags, hooks, or
indexes. Do not introduce a parallel game Ent hierarchy, bespoke component
store, or alternate ECS.

Keep behavior in injected services and systems and keep Ent state in
components. Services, commands, configuration, assets, protocol records, and
ordinary value objects are not game Ents and should remain normal C# types.

Before creating or significantly changing game Ents, component declarations,
Ent handles or arenas, Indexed contexts, hooks, bags, indexes, or Ent lifetime,
read `../AlvorKit/docs/ECS.md`. Follow its ownership, registration, mutation,
iteration, and teardown contracts.

## Code Design Style

These are prescriptive defaults, not merely instructions to copy nearby code.
Apply them in new projects and packages even when no local precedent exists.

### Assembly Metadata

- Hand-authored `AssemblyInfo.cs` files are banned. Declare assembly metadata
  with SDK-style MSBuild properties and items in the owning project file,
  including `InternalsVisibleTo` items for friend assemblies.
- SDK-generated `<ProjectName>.AssemblyInfo.cs` files under intermediate output
  directories are expected and remain enabled.

### Accessibility

- Prefer `public` over `internal` for game-code types and collaborating members.
  Game projects are not curated library API surfaces; assembly boundaries
  should not hide ordinary game systems, state, commands, or helpers.
- Keep details `private` when they are owned by one type. Use `internal` only
  when a deliberately small, curated assembly API is a real design
  requirement.

### Services And Composition

- Put runtime behavior in injected instance classes. A service should remain an
  instance even when it currently has no fields.
- Do not make a class or method static merely because it is stateless or
  because an analyzer recommends it.
- Use constructor injection. Do not introduce service locators, ambient
  containers, or hidden global dependencies.
- An injected service enters another service only through constructor
  injection. Never thread an injected service through an ordinary method,
  local function, delegate, command, record, or other operation parameter. A
  type is either a scope-owned service or an explicitly passed ordinary object,
  never both. Method parameters carry per-call data rather than hosted
  collaborators.
- Keep composition in scopes, loaders, and entry points. Each loader should
  initialize its own layer instead of absorbing or replacing another loader's
  responsibilities.
- Keep domain services focused. Do not give them unrelated loading,
  persistence, rendering, protocol, or presentation responsibilities.
- Prefer a valid neutral initial state during normal binding over nullable
  placeholders and defensive access paths.

### Method And Component Shape

- Give a method one concern. Every hand-authored method, constructor, and local
  function must stay at or below fifty physical lines, counted from its
  declaration through its closing brace or expression-body semicolon; XML
  documentation and attributes above the declaration do not count. This is a
  hard ceiling, including for flat checklists and component initializers. Aim
  for fewer than twenty-five lines in ordinary methods, and split longer logic
  into named stages behind a short orchestrator well before it reaches the
  ceiling.
- Decompose a large subsystem into single-concern components of roughly one to
  two hundred lines driven by one orchestrator. Route cross-component effects
  through the orchestrator as return values, not as callbacks between
  components.
- Avoid long parameter lists and stacked `out` parameters. Pass a small input
  struct, or return a named tuple or value-or-null result.
- Group a project's files into domain folders; keep the project root for
  boot-level and configuration types.

### State In Fields

- Hold object-owned state in explicit private fields, not auto-properties.
  Expose only the access collaborators need through get-only, get/set, `ref`,
  or `ref readonly` properties. Strongly prefer this over `internal` or
  `public` fields, including for mutable game state.
- Auto accessors are banned in hand-authored classes and non-record structs.
  Every stored property on those types uses explicit accessors over a private
  backing field. Records are the only hand-authored exception: auto-properties
  and positional records are allowed when they clearly express the record's
  value shape. Interface and abstract accessor declarations are contracts, not
  stored auto-properties, and remain valid.
- If mutation by reference is genuinely required, keep the backing field
  private and expose a narrowly scoped `ref` property. Do not disguise
  unrestricted field access behind trivial
  `get => field; set => field = value;` forwarding; use a writable `ref`
  property for that contract. Keep get/set accessors only when they enforce
  validation, transformation, restricted writes, or other behavior. Use an
  exposed field only for a specific framework, binary-layout, generated-code,
  or measured hot-path requirement.
- Prefer a `readonly struct` when none of its instance members mutate retained
  state. In a non-readonly struct, explicitly mark every hand-authored instance
  member that does not mutate retained state as `readonly`, including
  expression-bodied properties, get-only properties, ordinary methods, and
  non-mutating getters on behavioral get/set properties. Use an accessor-level
  `readonly get` when the setter must remain mutating. A member that returns a
  writable `ref`, or otherwise mutates the receiver, must remain non-readonly.
- Organize every class and struct in the member order defined by AlvorKit's
  root `C# Defaults`: constants; readonly fields; non-readonly fields; get-only
  properties; get/set or get/init properties; ref properties; constructors;
  then remaining members. Constants always precede instance members and use
  `public`, then `internal`, then `private` accessibility. Within every field
  and property category, use `private`, then `internal`, then `public`
  accessibility. Static readonly fields remain readonly fields. A nontrivial
  multiline property implementation may be the final property block
  immediately before the constructor, or before methods when no constructor
  exists.
- Keep consecutive fields and simple properties compact. Do not put blank lines
  between members of the same category without a meaningful grouping reason.
- Constants and fields are distinct member categories. Put exactly one blank
  line between the final constant and the first field below it; never declare
  constants and fields as one compact block.
- Keep every class and struct at no more than eight directly retained instance
  fields. Constants and static fields do not count; auto-property backing
  storage and positional-record members do count. When cohesive private state
  would exceed the limit, group it into one or more private nested carrier
  structs. An embedded state-carrier struct must never be `internal` or
  `public`, must not escape its containing type, and must not be returned or
  passed by value. Its fields use `public` PascalCase names so the containing
  type can access them; the carrier's private accessibility keeps those fields
  effectively private. This is the sole exception to the ordinary
  private-field rule. A passive carrier declares no constructor, including no
  primary constructor. Initialize its fields explicitly where the owning state
  is established. Add a carrier constructor only when construction enforces a
  real invariant rather than merely copying values into fields. A standalone
  `internal` passive carrier follows the same shape with `internal` PascalCase
  fields; do not recreate private backing fields and forwarding properties
  inside it. Do not make the carrier `readonly` merely to force
  constructor-based population; the owner may retain the fully initialized
  carrier in a `readonly` field instead.
- A private embedded carrier must reduce the containing type's access surface,
  not merely its direct field count. Never re-expose a carrier by mirroring its
  fields through a block of one-to-one forwarding properties or methods. A
  value that needs its own forwarding member is not private carrier state. Keep
  such values directly on a deliberately small contract, or define a
  standalone collaboration or snapshot type at the narrowest required
  accessibility and expose the cohesive group as one member.
- Default parameter values are banned. Require callers to provide every
  argument instead of hiding behavior behind an optional value.
- A multiline declaration parameter list keeps its closing `)` directly after
  the final parameter. Never place that parenthesis on its own line. Apply this
  to methods, constructors, primary constructors, records, delegates, and
  lambdas.
- The C# `checked` keyword is banned in all game code, including production
  source, scripts, tests, generated output, and templates. Express any required
  range contract without the keyword.
- Use a primary constructor by default for every class or behavioral struct
  that receives constructor parameters, including public types, facades,
  injected services, and stateful implementation types. New declarations must
  use this shape, and materially edited types should convert assignment-only
  explicit constructors. Passive field-carrier structs are the exception: they
  receive no constructor merely to populate fields and are initialized
  explicitly by their owner.
- Refer to captured parameters directly when no named field is required. Do not
  introduce mirrored private fields and an assignment-only constructor body.
  When a parameter needs named storage, validation, or derived initialization,
  retain the primary constructor and initialize the field or property inline,
  using a focused static helper when necessary. Additional constructor
  overloads must chain to the primary constructor and are not a reason to
  abandon it.
- Use an explicit constructor only when the required contract cannot be
  expressed clearly with a primary constructor, such as when constructor
  accessibility must differ from type accessibility or initialization
  inherently requires statement-level control flow or ordered side effects.
  Ordinary dependency capture, validation, derived values, base-constructor
  arguments, and named backing fields are not exceptions. A ref-like parameter
  that the compiler cannot capture may initialize one explicit ref-like field
  inline while the remaining parameters stay captured.
- Individual expression-bodied computed and forwarding properties are fine
  when they form a deliberately small contract. A forwarding block that
  reconstructs an embedded carrier's field surface is banned.
- Configuration types bound by the configuration binder still expose
  properties, but non-record hand-authored implementations use explicit
  accessors over private fields. Generated `[Components]` source is exempt
  because the generator owns its source shape.
- Delete write-only state and members that only tests read.

### Static Members And Constants

- Do not create static service classes, static mutable state, or broad
  collections of static helpers.
- Reserve static members for operators, extension methods, framework-required
  entry points, compile-time values, and pure value operations that are
  unambiguously owned by the type.
- Magic numbers are banned. Give representation widths, shifts, masks,
  sentinels, domain bounds, algorithmic costs, fixed capacities, and other
  meaningful numeric values descriptive constants on the type that owns their
  meaning.
- Define one canonical origin for related constants and derive the others from
  it. For example, derive a size and mask from one shift, a maximum from one
  bit width, or a first valid identifier from its empty sentinel. Do not repeat
  literals whose agreement is an unstated invariant.
- Do not manufacture constants for ordinary arithmetic identities and visibly
  self-explanatory local values, such as zeroing a counter, incrementing by one,
  loop origins, or the `-1`, `0`, and `1` components of an explicitly named
  direction delta.
- Do not promote one-off literals or runtime policy to global constants. Use
  injected configuration or instance state for values that can vary by runtime
  or composition.
- Disable analyzer rules that recommend making instance members static when
  they conflict with these conventions.

### Hot-Path Data Layout

- Never cache, precompute, or retain a value that is obtainable through a
  simple independent mathematical formula from already-held IDs, indices,
  coordinates, and constants. Compute it where needed. In particular, do not
  add full-volume companion arrays for row-major addressing, chunk addressing,
  bit positions, masks, or similar arithmetic derivations. If the formula is
  awkward at the use site, improve the representation or move the calculation
  to the appropriate cold boundary instead of materializing a lookup cache.
- Do not retain a small fixed lookup array or list when a closed mapping from
  an index, enum, coordinate delta, or similarly compact key can be expressed
  directly. Use a switch expression or a simple formula, and expose a named
  count plus an indexed operation when callers must iterate the mapping.
  Derive reverse, opposite, and related mappings from the same named
  representation instead of adding a second lookup or linear search. Retained
  tables are appropriate only when the values are authored or configured data,
  can change at runtime, or measured performance justifies the storage.
- For a dense grid whose hot loop repeatedly visits a fixed neighborhood,
  strongly prefer a blocked sentinel border around the retained data. Convert
  public coordinates or unpadded addresses to padded row-major indices in cold
  API, loading, and synchronization paths. The hot loop should queue indices,
  apply fixed row and layer offsets, and reconstruct through indexed state
  without converting back to coordinates.
- Sentinel padding removes per-neighbor world-boundary branches because every
  fixed offset lands on valid retained storage and border cells reject entry
  through the ordinary data check. Preserve the unpadded external contract and
  account for the padded capacity in every index-addressed companion array.
- Padding guarantees semantic index safety; it does not by itself prove that
  the runtime removed managed-array range checks. Inspect generated code or
  measure before adding lower-level access, and use unsafe access only for a
  demonstrated remaining bottleneck with layout invariants that make it safe.

### Failure Semantics

- Internal code assumes its contracts are satisfied. Do not add redundant
  range checks, custom guard exceptions, or debug assertions for states that
  should never occur.
- The repository-wide fallback-design ban applies to edge cases and performance
  design as well as invalid states. Do not add an alternate slower, older,
  approximate, partial, or best-effort implementation when the intended design
  does not cover its supported contract.
- Let invalid internal or authoritative data fail naturally at the operation
  that cannot handle it.
- Validate external input only when validation is part of a real security,
  externally imposed interoperability, or recoverable protocol boundary.
- Every exception in production game code is fatal to the process. Let it
  propagate unchanged. The program is not expected to remain usable, preserve
  mutable state, or clean up after an exception.
- Do not add `try`, `catch`, `finally`, catch filters, or exception-driven
  retry, rollback, cleanup, logging, translation, or state restoration to
  production game code. Catching and rethrowing is not useful.
- Perform required normal-path cleanup explicitly after successful work.
  Structure operations so an expected rejection is decided before state is
  committed, or represent it as an ordinary result or reason value. A condition
  from which the game must continue is not exceptional.
- Ordinary `using` declarations and statements remain valid when a resource
  requires disposal during normal successful execution. Do not introduce one
  solely to perform otherwise-unneeded exception cleanup.
- Tests, benchmarks, and orchestration tools outside `src/` may catch only when
  their explicit contract is to supervise independent runs and continue after
  one fails. This exception does not apply to runnable game hosts and is not
  precedent for game code.
- Expected rejections on local guard paths return a reason value, such as a
  conflict string or a small enum, that the caller logs verbatim. Do not throw
  an exception only to catch it a few frames up in the same flow.
- Log messages state the precise cause in plain language; do not funnel
  distinct causes into one vague label.
- Keep failure taxonomies only as granular as the distinct responses they
  enable.

### Ownership And Lifecycle

- Every mutable resource should have an obvious owner and one understandable
  allocation, replacement, clearing, and teardown path.
- Express lifecycle with domain operations such as `Load`, `Clear`, `Fill`,
  `Stop`, or `Unload` when those names describe the real operation.
- Use `IDisposable` only for types that participate in a genuine disposal
  contract, not as a generic marker for resetting state or returning storage.
- Similar resources should follow similar lifecycle patterns.
- Do not add wrapper properties or methods that merely expose information
  already publicly available.

### Concurrency

- Design concurrency from actual ownership and access paths. Prefer
  thread-owned, worker-owned, or scope-owned state over shared mutable state.
- Reuse per-worker buffers when their lifetime naturally matches a worker.
- Add locks only around state that is genuinely accessed concurrently and
  requires synchronization.
- Do not add volatile publication, snapshots, defensive copies, or additional
  locks for hypothetical races unsupported by the lifecycle.
- Do not assign unusual thread priorities without a measured scheduling
  requirement.

### Design Restraint

- Implement the smallest coherent design that satisfies the current system. Do
  not add speculative extensibility, defensive infrastructure, or future
  abstraction layers.
- Do not create a feature-specific protocol, synchronization channel, or side
  system when the concern belongs to a general system that has not been built
  yet.
- Keep package roles strict. Pure simulation, backend persistence, frontend
  presentation, protocol, and executable composition remain separate.
- Put derived presentation values in frontend packages instead of pure
  simulation packages.
- Give each class one clear responsibility. Move initialization, persistence,
  and presentation derivations to their respective owners instead of
  accumulating convenience methods on a domain object.
- Preserve unrelated behavior. A subsystem change should not also alter menus,
  loading presentation, scheduling policy, or other user-visible behavior.
- Prefer direct, readable code over infrastructure justified only by
  theoretical robustness.
- Prefer fixed-cadence recomputation over dirty flags, change counters, and
  deadline prediction when the recomputation is cheap; bounded staleness beats
  invalidation machinery.
- An abstraction must remove duplication that already exists. Prefer a small
  toolkit that keeps call sites explicit over a framework that hides them.

## Facade Projects

A project containing a project-root `FACADE.md` is a facade project. Read that
file before changing the project, its paired `.Debug` project, or a consumer.
The facade is the project's single injected entry point: it presents a small
public contract while hiding the cooperating services and retained state that
implement that contract. Keep `FACADE.md` brief and stable: describe the
business capability, identify the facade, and declare its concise PascalCase
type prefix without cataloguing implementation details.

### Established Public APIs — Facade Projects Only

The approval requirement in this subsection applies only to a project that
contains a project-root `FACADE.md`. It does not apply to a project without
`FACADE.md`, even when that project exposes public repository-owned APIs. For a
project without `FACADE.md`, follow the unshipped-development rule: breaking
repository-owned APIs needs no special approval, and producers and consumers
should be changed together.

Within a facade project, once the facade's public API exists in source or
documentation, treat it as an established design boundary. Changing it is a
**BIG** design decision, even while the repository is unshipped. This
facade-only rule overrides the shared default that breaking repository-owned
APIs needs no special approval.

Before changing an established facade public API, always stop and ask the user
for explicit permission for that exact change. Do not edit the API or its
consumers until the user replies affirmatively. The permission request must:

- show the current and proposed C# declarations;
- explain why the current contract is insufficient and why the change is
  necessary;
- identify the affected consumers and expected update scope; and
- ask directly whether the specific public API change is approved.

Adding, removing, renaming, retyping, or changing the accessibility of a public
member or contract type counts as an API change, as does changing documented
public behavior. A general request to implement a feature, perform a refactor,
or improve an algorithm is not permission to change an established facade API.
Internal implementation changes that preserve the public contract do not
require this approval.

Use a request shaped like this:

> I need permission to change the established `PathFacade` API.
>
> Current:
>
> ```csharp
> public PathResult FindPath(PathRequest request);
> ```
>
> Proposed:
>
> ```csharp
> public PathResult FindPath(PathRequest request, PathSearchOptions options);
> ```
>
> Reason: the new search policy is caller-selected and cannot be represented by
> `PathRequest` without giving that request unrelated responsibilities. This
> requires updating the path scheduler and pathfinding tests. May I make this
> specific public API change?

Approval is a design gate, not a compatibility requirement. After approval,
change producers and consumers together and remove the superseded API without
adding compatibility overloads, aliases, adapters, or deprecation shims.

### Layout, Composition, And Tests

- Every type owned by a facade project uses the prefix declared in its
  `FACADE.md`, including public contract types, injected services, internal
  machinery, and nested implementation types. The single root injectable class
  is named `<Prefix>Facade`. A paired debug facade declares its own prefix,
  normally `<CorePrefix>Debug`, and names its root `<DebugPrefix>Facade`. Types
  imported from another project retain the prefix of their owning project.
- Exactly one injectable class lives at the project root beside the project
  file and `FACADE.md`. Public static classes may live there at the same level.
- Other public instantiated classes, structs, enums, and interfaces live in
  `Classes/`, `Structs/`, `Enums/`, and `Interfaces/` respectively.
- Injected implementation services and internal static classes live directly
  in `Internal/`. Injected services are the hosted singleton collaborators of
  the root facade.
- Non-injected internal implementation types are grouped by kind:
  `Internal/Classes/`, `Internal/Structs/`, `Internal/Enums/`, and
  `Internal/Interfaces/`. `Internal/Classes/` is specifically for instantiated
  non-singleton helper objects, not injected services or static classes.
- The facade and its implementation graph use constructor injection. A facade
  must not construct its own services.
- Composition roots create the graph with `Host<TFacade>()` in the scope that
  owns it, then resolve the facade normally. Tests must exercise the same
  hosted composition rather than bypassing it with direct construction.
- Dedicated unit tests are mandatory for every facade project. A new facade,
  facade behavior change, or facade implementation change is incomplete until
  its unit tests cover the public contract and the affected internal invariants
  through the real hosted composition graph. This facade-specific requirement
  overrides the Working Mode default that otherwise prohibits adding or
  running game unit tests without an explicit user request.
- Facade tests must assert behavior, values, state transitions, failure
  semantics, or representation requirements that matter at runtime.
  Reflection-based API surface locks, exported-type or member-name inventories,
  and tests whose purpose is merely to prove that a declaration exists or has
  particular accessibility are banned. Compilation of behavioral call sites
  already proves that required declarations exist.

### Debug Facades

A facade project is paired with a `<FacadeProject>.Debug` facade project when
internal diagnostics are required. The debug project follows the same layout
rules, contains its own single injected facade, and may receive friend-assembly
access to the production facade's internals. Host both facades in the same
scope so the debug facade observes that scope's production facade instance.
The debug facade may inject and use the core project's internal services
directly; it does not need to depend on or route internal access through the
production facade.

Keep production-facade implementation types and members private unless another
core implementation type must use them. Use `internal` only for genuine
assembly collaboration or deliberate access from the paired debug assembly.
Friend-assembly access is not a reason to expose the whole implementation.
Debug-owned coordination, storage, transformation, and presentation belong in
the debug project. Retain an opt-in hook in the core only when information must
be recorded at the exact point where the production algorithm executes.

The debug facade is an observation and control surface over the actual hosted
core machinery. It may expose captured state, counters, traces, visualization
data, and explicit diagnostic controls already supported by that machinery. It
must not implement an alternative algorithm, reference or oracle
implementation, fallback path, duplicated state model, or extra domain behavior
that the core facade does not perform. When higher-level debug tooling needs to
observe an operation, run the real core operation and inspect its captured
internals. Put genuinely higher-level visualization or comparison orchestration
in the owning higher-level `.Debug` project rather than manufacturing new
behavior inside the lower-level debug facade.

Core game-logic projects may depend only on a production facade's public
contract. They must never import its internal machinery or reference its paired
debug project. The debug facade is for higher-level diagnostic, visualization,
testing, and benchmark projects only.

### Facade Benchmarks

Every production facade project `<FacadeProject>` must have one executable
benchmark project named `<FacadeProject>.Bench`. Add only `.Bench`; names such
as `<FacadeProject>Benchmark` and `<FacadeProject>.Benchmark` are banned. A
paired debug facade is hosted and exercised by the production facade's bench;
it does not receive a separate `.Debug.Bench` project.

Each game repository must also have one common `<Game>.Bench` library for
facade-benchmark infrastructure. Individual facade bench projects reference
that common project for allocation capture, report formatting, and other
genuinely shared harness code instead of copying it. Non-facade benchmark
runners may also reuse this library; doing so does not make them facade
benchmarks or subject their project names to the facade-bench naming rule.

Every facade bench must support both modes below from the game repository root:

```powershell
# Authoritative performance run without allocation callbacks.
dotnet run -c Release --project scripts\<FacadeProject>.Bench -- `
    --json-output out\facade-benchmarks\<FacadeProject>-timing.json

# Diagnostic run with exact process-wide managed-object counts.
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.TestInterception -c Release -- `
    --exec-project scripts\<FacadeProject>.Bench\<FacadeProject>.Bench.csproj `
    --configuration Release `
    --allocation-profiling `
    --module <FacadeProject>.Bench `
    --timeout-seconds 300 -- `
    --json-output out\facade-benchmarks\<FacadeProject>-objects.json

# Diagnostic lower/high/extreme allocation-scaling matrix.
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.TestInterception -c Release -- `
    --exec-project scripts\<FacadeProject>.Bench\<FacadeProject>.Bench.csproj `
    --configuration Release `
    --allocation-profiling `
    --module <FacadeProject>.Bench `
    --timeout-seconds 300 -- `
    --allocation-matrix `
    --json-output out\facade-benchmarks\<FacadeProject>-scaling.json
```

Allocation tracking is opt-in because profiler callbacks affect timing. Use the
ordinary untracked run for speed conclusions, and the tracked run for allocation
counts; never compare tracked timing directly with untracked timing. The report
must state whether tracking is on.

The allocation matrix must include lower-bound, high, and extreme cases. Judge
object counts by growth with game-data dimensions such as chunks, cells,
entities, or requests. Fixed composition, singleton, host, service, profiler
capture, and process objects establish the lower-bound baseline and must not be
treated as scaling regressions. Capture cold lifecycle allocation and warmed
steady-state work separately so background or retained-path allocations remain
visible even when the narrow hot loop is allocation-free.

Every facade bench must accept `--json-output <path>` for its normal catalog
and allocation matrix. The versioned JSON artifact retains execution and CI
metadata, settings and selection, every preparatory and measured sample, raw
durations and allocation bytes, exact object counts for each captured lifecycle
or measured phase, per-operation object counts, validation signatures,
diagnostics, and aggregate totals. Represent unavailable untracked object
counts explicitly as null. Do not replace detailed samples with console
medians or summaries.

Game CI runs every facade bench on each push and pull-request commit. For each
facade it archives three separate JSON artifacts: the profiler-free catalog for
authoritative timing, the profiler-enabled catalog for exact process-wide
object counts, and the profiler-enabled lower/high/extreme allocation matrix
for scaling analysis. Include commit and CI-run metadata in each file. Do not
merge tracked and untracked timing into one comparison because allocation
callbacks change execution cost.

## Documentation Router

Open the matching guide under `../AlvorKit/docs/` instead of re-inventing local
rules:

- `AlvorSense.md`: hidden, engine-native visual harness for AlvorKit games.
- `AgentLiveDevelopment.md`: combined AlvorSense, LiveCode, and Source Update
  workflow, recording, approval, and cleanup contract.
- `ECS.md`: required game Ent components, handles, arenas, Indexed hooks and
  bags, iteration, ownership, and teardown.
- `ProjectSplitModel.md`: pure, frontend, menu, backend, server, protocol, and
  executable package split.
- `GameScopeOrganization.md`: DI scopes, scope prefixes, loader scopes, states,
  controls, seeding, and constructor ordering.
- `GlOwnership.md`: hierarchical `GlLayer` ownership and GPU object lifetime.
- `MenuAuthoring.md`: `AlvorKit.UI` menus and the one-public-`Create` shape.
- `Logging.md`: application logging in standard engine-loop and custom headless hosts.
- `AgentVerification.md`: lint, timing, coverage, artifacts, and report reading.
- `AgentCoordination.md`: leases, conflicts, complaints, staging discipline.
- `GeneratedOutputChecks.md`: generator and generated-output review workflow.

Use `../AlvorKit/demos/` as runnable examples for engine APIs. Other docs and
design references exist under `../AlvorKit/docs/`; open them only when the task
touches that area.

Before adding a `ProjectReference`, make sure it preserves the package's role.
It is vitally important that a project does not take on a dirty dependency in
the `.csproj` that defeats the purpose of the split. Pure packages must not
reference UI, GL, frontend, menu, audio, or windowing packages. Frontend
packages may depend on `AlvorKit.Engine`, but should not depend on
`AlvorKit.Engine.Loop`; loop ownership belongs in the executable, menus, or
another composition package.

## Visual Checks

Prefer AlvorSense when the game uses `RootLoop.RunGlfw`,
`AgentGlfwWindowHost`, or supports `ALVORKIT_WINDOWING_AGENT=1`. Read
`../AlvorKit/docs/AlvorSense.md`, run the CLI from the game repository root,
and pass `--workdir .` so artifacts land under that repository's ignored
`out/`.

```powershell
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- start --id <game-id> --project <runnable-project-or-script> --workdir .
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- send --id <game-id> --command "render" --command "screenshot out\shots\<name>.png"
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- stop --id <game-id>
```

When a visual check reveals surprising behavior, keep that same target alive
and follow `../AlvorKit/docs/AgentLiveDevelopment.md`. Use AlvorSense for normal
user-visible input and evidence, LiveCode for exact scoped inspection, and
Source Update for a normal edit to one existing method body. Put
agent-authored submissions and immutable diffs beneath the game repository's
ignored `tmp/live/<workspace-id>/` workspace and use workspace-aware commands
so another agent can audit and clean up the session.

An executable that supports Source Update must explicitly compose
`RootSourceUpdate` and be launched through AlvorSense `--editable-project`.
Normal launch and release profiles remain ordinary non-editable game runs.

AlvorSense does not drive real desktop windows or OS-level input and focus.
Verify those behaviors manually or with purpose-built external tooling.

## Verification

Use the game repository's solution, `README`, scripts, and CI workflow for
normal build commands. AlvorKit tools usually accept `--repo-root .`; read
`../AlvorKit/docs/AgentVerification.md` first when the task requires its
workflows.

In Working Mode, prefer targeted builds or an AlvorSense smoke only when they
directly support the requested change. Do not add or run game unit tests unless
the user explicitly requests them or an existing game test must be diagnosed.
Lint, coverage, broad timing gates, generated-output checks, staging, and
commits are explicit-request or Commit Mode work.
