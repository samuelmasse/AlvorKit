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

- Game C# source files may be up to 750 lines when a cohesive game system,
  state, menu, renderer, or simulation reads better together.
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

- Give a method one concern. Split multi-concern logic into named stages behind
  a short orchestrator; around fifty lines is the practical ceiling, while flat
  single-concern checklists and component initializers may run longer.
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
  private and expose a narrowly scoped `ref` property. Prefer that shape when
  get/set accessors would merely forward unrestricted access to the backing
  field. Keep get/set accessors when they enforce validation, transformation,
  restricted writes, or other behavior. Use an exposed field only for a
  specific framework, binary-layout, generated-code, or measured hot-path
  requirement.
- Organize every class and struct in the member order defined by AlvorKit's
  root `C# Defaults`: readonly fields; non-readonly fields; get-only
  properties; get/set or get/init properties; ref properties; constructors;
  then remaining members. Within every field and property category, use
  `private`, then `internal`, then `public` accessibility. A nontrivial
  multiline property implementation may be the final property block
  immediately before the constructor, or before methods when no constructor
  exists.
- Keep consecutive fields and simple properties compact. Do not put blank lines
  between members of the same category without a meaningful grouping reason.
- Default parameter values are banned. Require callers to provide every
  argument instead of hiding behavior behind an optional value.
- A multiline declaration parameter list keeps its closing `)` directly after
  the final parameter. Never place that parenthesis on its own line. Apply this
  to methods, constructors, primary constructors, records, delegates, and
  lambdas.
- The C# `checked` keyword is banned in all game code, including production
  source, scripts, tests, generated output, and templates. Express any required
  range contract without the keyword.
- A captured primary constructor may precede the normal member layout when it
  removes mechanical backing fields and assignments from a small value carrier
  or non-public implementation type. Keep an explicit constructor when its
  accessibility differs from the type or initialization performs behavior. A
  ref-like parameter that cannot be captured may initialize one explicit field
  inline without forcing an assignment-only constructor body.
- Expression-bodied computed and forwarding properties are fine; they expose
  behavior or controlled access rather than hiding owned storage.
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
