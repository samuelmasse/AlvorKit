# Facade Policy

## Scope

Read this policy before creating or materially expanding a facade, or changing
a project containing `FACADE.md`, its paired debug project, consumers, tests,
benchmarks, proposal, or documented public behavior.

## Approval Gates And Overrides

**FACADE-API-001** is an approval gate. A game-local proposal gate may also
apply before facade creation or material expansion. One approval satisfies both
only when it explicitly approves the exact API and implementation/cutover scope.
**FACADE-TEST-OVERRIDE-001** overrides the ordinary game-test default within
the scope stated below.

## Facade Projects

A project containing a project-root `FACADE.md` is a facade project. Read that
file before changing the project, its paired `.Debug` project, or a consumer.
The facade is the project's primary public entry point: it presents a small
public contract while hiding the implementation and retained state behind that
contract. A facade may be directly constructed, dependency-injection composed,
or created through another explicit production-owned composition path. Keep
`FACADE.md` brief and stable: describe the business capability, identify each
exact primary facade type, and declare its concise PascalCase type prefix
without cataloging implementation details.

Facade organization consumes the whole project boundary. Never embed a
facade-shaped partial class, `Internal/` implementation layout, or secondary
facade inside an ordinary game, demo, presentation, or composition project. If
a capability warrants a facade, give it a dedicated project with a project-root
`FACADE.md`; otherwise implement it as ordinary collaborating classes using the
host project's normal layout. A class does not become a facade merely because
it coordinates several collaborators.

### Multi-Facade Projects

A facade project may contain multiple primary facades only when they form one
cohesive contract family over shared low-level representation or retained
machinery. Its `FACADE.md` must explicitly call it a multi-facade project and
list every exact primary facade type with its own type prefix. Do not use this
exception to collect unrelated capabilities or avoid giving independently
owned behavior a dedicated project.

Each facade-owned public or internal top-level type uses its owning facade's
declared prefix. A public contract type owned by one listed facade may also be
used by another listed facade without changing its prefix. Genuinely shared
implementation types used by at least two listed facades may instead use a
concise role-based name, but they must remain internal and live under
`Internal/`. This exception does not permit an unprefixed public type, a
facade-specific unprefixed helper, or an undeclared secondary facade.

Multi-facade projects may keep implementation types directly under `Internal/`
when those types are shared across facade boundaries or their declared prefix
already identifies the owning facade. They may still use the ordinary
kind-based internal directories when that organization is clearer. Tests and
the one project benchmark must cover every listed primary facade through its
supported production construction path.

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

> I need permission to change the established `Paths` API.
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

- Every public or internal top-level type owned by an ordinary facade project
  uses the prefix declared in its `FACADE.md`, including public contract types,
  composition-owned services, and internal machinery. Multi-facade projects
  use the ownership and shared-implementation rules above. Private nested
  implementation types need not repeat the prefix because their containing
  type already supplies that context. Types imported from another project
  retain the prefix of their owning project.
- Each primary facade class uses the exact domain name declared in `FACADE.md`.
  It does not require a `Facade` suffix or a name derived mechanically from its
  type prefix. A paired debug facade likewise declares its exact primary type
  and its own prefix rather than deriving its name from the core facade.
- Every declared primary facade class lives at the project root beside the
  project file and `FACADE.md`. An ordinary facade project declares exactly one;
  a multi-facade project declares the exact cohesive set permitted above. Each
  root source file presents that facade's public contract and construction or
  lifetime surface. Public static classes may live there at the same level.
- Other public instantiated classes, structs, enums, and interfaces live in
  `Classes/`, `Structs/`, `Enums/`, and `Interfaces/` respectively.
- Each primary facade is partial. Its implementation-only partial declarations
  live directly in `Internal/`, following the shared C# public-class layout.
  Keep each root file focused on its public surface and use purpose-named
  internal partial files to organize implementation concerns.
- Composition-owned implementation services, when present, and internal static
  classes live directly in `Internal/`. Injected services are the retained
  singleton collaborators of an injector-composed facade. These services are
  ordinary non-partial classes; the declared primary facades are the only
  partial classes in this layout unless generated or framework-owned code
  requires another.
- Except for the multi-facade organization permitted above, non-injected
  internal implementation types are grouped by kind: `Internal/Classes/`,
  `Internal/Structs/`, `Internal/Enums/`, and `Internal/Interfaces/`.
  `Internal/Classes/` is specifically for instantiated non-singleton helper
  objects, not injected services or static classes.
- Dependency injection is optional. When a facade or implementation service is
  injector-composed, injected collaborators enter only through constructor
  injection and the composition root publishes the resolved facade to its
  owner. Directly constructed facades may directly own ordinary private
  implementation objects. Do not introduce service location, ambient
  containers, or mixed ownership merely to support more than one construction
  style.
- Tests exercise the supported production construction path. Directly
  constructed facades are constructed directly in tests; injector-composed
  facades use their real injector composition rather than bypassing it.
- Dedicated unit tests are mandatory for every facade project. A new facade,
  facade behavior change, or facade implementation change is incomplete until
  its unit tests cover the public contract and the affected internal invariants
  through the supported production composition. This facade-specific
  requirement overrides the Working Mode default that otherwise prohibits
  adding or running game unit tests without an explicit user request.
- Facade tests must assert behavior, values, state transitions, failure
  semantics, or representation requirements that matter at runtime.
  Reflection-based API surface locks, exported-type or member-name inventories,
  and tests whose purpose is merely to prove that a declaration exists or has
  particular accessibility are banned. Compilation of behavioral call sites
  already proves that required declarations exist.

### Debug Facades

A facade project is paired with a `<FacadeProject>.Debug` facade project when
internal diagnostics are required. The debug project follows the same layout
rules, contains its own primary debug facade, and may receive friend-assembly
access to the production facade's internals. Compose the debug facade against
the same production facade instance and implementation state that consumers
use. When the production facade uses an injector, resolve both facades from the
same injector. The debug facade may receive and use the core project's internal
services directly; it does not need to route internal access through the
production facade.

Keep production-facade implementation types and members private unless another
core implementation type must use them. Use `internal` only for genuine
assembly collaboration or deliberate access from the paired debug assembly.
Friend-assembly access is not a reason to expose the whole implementation.
Debug-owned coordination, storage, transformation, and presentation belong in
the debug project. Retain an opt-in hook in the core only when information must
be recorded at the exact point where the production algorithm executes.

The debug facade is an observation and control surface over the actual composed
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
paired debug facade is resolved and exercised by the production facade's bench;
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
entities, or requests. Fixed composition, singleton, service, profiler
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

Facade CI integration follows the game repository's existing benchmark
orchestration and cadence. When CI uses a shared facade catalog, project list,
or matrix, register a new facade through that shared mechanism. Do not add a
facade-specific command block, filename branch, duplicated profiler invocation,
or workflow trigger merely to satisfy this policy.

A task scoped to one facade does not authorize changing repository-wide CI
triggers, benchmark cadence, or the modes run for every facade. If the shared
orchestration cannot express a desired benchmark mode uniformly, keep the
required mode available through the facade bench command and leave the CI
orchestration unchanged until a separately requested repository-wide change
introduces the reusable mechanism.

When CI runs more than one mode for a facade, archive each mode as a separate
JSON artifact: the profiler-free catalog for authoritative timing, the
profiler-enabled catalog for exact process-wide object counts, and the
profiler-enabled lower/high/extreme allocation matrix for scaling analysis.
Include commit and CI-run metadata in each file. Do not merge tracked and
untracked timing into one comparison because allocation callbacks change
execution cost.
