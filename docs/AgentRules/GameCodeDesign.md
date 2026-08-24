# Game Code Design Policy

## Scope

Read this policy before designing or changing game source, runtime services,
composition, state, failure semantics, ownership, lifecycle, concurrency, or
hot-path representations. The shared C# policy remains cumulative.

## Hard Stops And Overrides

These are prescriptive game defaults, not instructions to copy nearby code.
Game accessibility rules override AlvorKit's curated-library preference where
stated. Repository invariants and approval gates remain in force.

## Rules

### Assembly Metadata

- Hand-authored `AssemblyInfo.cs` files are banned. Declare assembly metadata
  with SDK-style MSBuild properties and items in the owning project file,
  including `InternalsVisibleTo` items for friend assemblies.
- SDK-generated `<ProjectName>.AssemblyInfo.cs` files under intermediate output
  directories are expected and remain enabled.

### Accessibility

- Ordinary game-code types and collaborating members are public. Game projects
  are not curated library API surfaces; do not use `internal` or an `Internal/`
  directory as an implementation-hiding convention for game systems, state,
  commands, presentation types, or helpers.
- Keep details `private` when they are owned by one type. Use `internal` only
  when an explicit framework, generated-code, or deliberately curated assembly
  contract requires it.

### Services And Composition

- Put runtime behavior in injected instance classes by default. A directly
  constructed primary facade governed by `Facades.md` may own its runtime
  behavior and ordinary private collaborators without introducing injection.
  A service should remain an instance even when it currently has no fields.
- An injected game service is never partial. When its behavior has multiple
  concerns, split it into public constructor-injected collaborators with one
  class per file. Do not apply a facade's root-plus-`Internal/` partial layout
  inside a game project.
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

The shared C# policy remains canonical for source shape. Apply it cumulatively;
this policy does not restate or relax it.

Game-specific additions:

- Hold mutable game state behind the narrowest collaborator-facing contract.
  Prefer private storage over `internal` or `public` fields.
- Configuration types bound by the configuration binder still expose
  properties. Generated `[Components]` source remains exempt from
  hand-authored source-shape rules because the generator owns it.
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
