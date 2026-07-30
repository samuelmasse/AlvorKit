# Repository Instructions

## Scope

These instructions apply repo-wide. More specific `AGENTS.md` files under
`src/`, `scripts/`, `demos/`, `tests/`, and `res/templates/` add area-specific
rules for their scope and may narrow or relax repo-wide defaults when they say
so explicitly. Read the closest scoped instructions before working in those
areas.

## Development Status And Compatibility

AlvorKit and every game repository inheriting these instructions are unshipped
development projects. No repository-owned API, ABI, command line,
configuration, save format, serialized data, protocol, generated output, or
existing behavior is a compatibility surface merely because it already exists,
is public, has a version, or has consumers.

Breaking changes are welcome when they produce the cleanest current design. Do
not add backward- or forward-compatibility code, migrations, deprecation shims,
legacy aliases or overloads, adapters, dual-read or dual-write paths, version
bridges, or retained old implementations. Change repository-owned producers and
consumers together, update affected tests, documentation, examples, and
generated output, and delete the superseded design.

A game becomes shipped only when its own root `AGENTS.md` explicitly declares
that status. The declaration must identify the exact compatibility surfaces and
policy. Do not infer shipping or compatibility requirements from package
versions, public accessibility, existing data, or historical behavior.

## Fallback Designs Are Banned

Fallback design is banned. Do not handle edge cases, incomplete correctness, or
uncertainty by pairing a preferred, fast, or new path with a slower, legacy,
approximate, reduced-fidelity, best-effort, default-result, or catch-and-retry
fallback. Do not retain the previous implementation as a safety net, silently
disable part of a feature, or return stale, default, partial, or knowingly
inferior results.

Implement one correct design for the supported contract. Make that design cover
all supported inputs or strengthen the representation and invariants. When the
supported contract requires a product decision, ask for that decision instead
of inventing a fallback.

A separately specified platform, backend, or interoperability mode is a
first-class requirement, not a fallback. Agents must not introduce an alternate
path merely to recover from an incomplete primary implementation.

## Game Repository Instructions

Sibling AlvorKit game repositories keep a small root `AGENTS.md` that routes to
[docs/GameRepositoryInstructions.md](docs/GameRepositoryInstructions.md). That
document is the authoritative shared policy for game-repository work; keep
game-specific exceptions in the game's local router instead of copying the
shared policy.

## Working Mode And Commit Mode

Agents operate in **Working Mode** by default. Make the requested change or
investigation without treating it as cleanup, commit, PR, or release ready
unless the user explicitly asks for Commit Mode.

In Working Mode:

- Do not create, refresh, or require advisory leases unless the user asks for
  lease-backed coordination.
- Do not run lint, coverage, broad test gates, or final verification solely
  because work is ending.
- Targeted builds, tests, visual checks, or generated-output checks are allowed
  when useful for the specific change or question.
- Style, documentation, line-length, file-size, and final-review rules guide
  good work but should not block making Working Mode changes work.
- Do not stage, commit, push, open a PR, or describe work as ready to commit
  unless the user asks for Commit Mode.
- In the handoff, list skipped final checks such as lint, coverage, broad tests,
  visual verification, staging, or commit when those would normally be expected
  in Commit Mode.

For code changes, review the requested files and nearby collaborators before
editing. Preserve unrelated existing behavior unless the task asks for a
broader behavior change, and keep refactors cohesive and scoped to the touched
project or directly related tests unless a broader refactor is explicitly
requested. This scope constraint is not a compatibility requirement for an API,
format, or design that the task intentionally changes.

Use **Commit Mode** only when the user explicitly asks for cleanup, final
verification, staging, committing, pushing, opening a PR, or making work ready to
commit. In Commit Mode, inventory the intended scope with status and diffs, then
read the relevant changed files before editing or staging. Preserve concurrent
work: do not revert others' edits, do not use destructive git commands, and ask
when ownership or intent is unclear.

When staging or committing is requested, identify the exact files or globs,
inspect status and diffs first, stage only those paths, recheck status and the
staged diff, and pause or clearly separate the work if unrelated changes appear
in the same paths. Avoid broad commands such as `git add .`.

## Coordination

See [docs/AgentCoordination.md](docs/AgentCoordination.md) for lease commands,
conflict notes, complaint filing, and staging details.

Advisory leases under `out/agents/` are available only when the user explicitly
asks for lease-backed coordination. They are hints, not hard locks. Do not
create leases in Working Mode unless the user asks. Read-only exploration does
not need a lease.

When lease-backed coordination is requested, use the lease helper instead of
hand-editing JSON. Check active leases before claiming paths, claim precise
repository-relative paths or globs, refresh long-running leases, and delete the
lease with `done` when finished. If an active lease overlaps your intended write
paths, avoid the overlap when practical or leave a short conflict note.

For tool complaints or disruptive concurrent-work complaints, write one concise
Markdown note under `out/complaints/` with a descriptive filename.

## Visual Automation

Use `scripts/AlvorKit.Script.AlvorSense` first when an agent needs to see, drive,
or verify an AlvorKit game wired through `AgentGlfwWindowHost` from
`AlvorKit.Windowing.Agent`. Read `docs/AlvorSense.md` before using or extending
AlvorSense, share important screenshots in chat, describe key input/update
batches, and continue in one live session when practical.

AlvorSense does not drive arbitrary desktop windows, external applications, or
real OS-level input. Verify those behaviors manually or with purpose-built
external tooling.

## Live Runtime Debugging

Read `docs/AgentLiveDevelopment.md` before using LiveCode, Source Update, or a
combined AlvorSense and LiveCode workflow. Treat AlvorSense as the visible
source of truth and LiveCode as the scoped debugger: observe through normal
input first, inspect the exact live scope when behavior is surprising, then
return to AlvorSense to verify any intervention.

Create agent-authored live submissions only beneath
`tmp/live/<workspace-id>/lc/`, `bridge/`, `puppet/`, or `source/diffs/`. Use
the workspace-aware CLI options so the target identity, exact inputs, outputs,
source hashes, and persistent interventions are recorded. Do not place
disposable submissions in production or demo source directories; the
intentional original `.cs` edit is not a disposable submission.

Clean up persistent LiveCode effects and stop or restart a Source Update target
before closing the workspace. Never record capability tokens in workspace
files, chat, logs, or documentation.

## Line Length

Use conventional, readable C# formatting. Do not compress code into cryptic
one-liners or pack unrelated constructs together merely to reduce vertical
space. Conversely, when a cohesive declaration, call, condition, initializer,
or expression reads clearly on one line and fits within 120 characters, keep
it on one line instead of breaking it prematurely into vertical fragments.
Treat 120 characters as the preferred C# wrapping point and 140 characters as
the hard maximum for agent-authored C# in both Working Mode and Commit Mode.
No closer `AGENTS.md` may relax this rule. This does not change automated
checks, which retain their existing 170-character failure threshold.

## Generated Output Checks

Read [docs/GeneratedOutputChecks.md](docs/GeneratedOutputChecks.md) before
changing a code generator, generator configuration, generated binding output, or
generated binding documentation, and whenever the user asks for generated-output
checks.

When changing a code generator or generator configuration, capture generated
output before and after the change in Commit Mode or when the user asks for
generated-output checks. In Working Mode, do this only when useful. Regenerate
only the binding library whose inputs, configuration, or source project changed;
use `all` only when the change intentionally affects every generated binding
project, and say why in the handoff.

Do not embed generated source, project files, scripts, or other multi-line
output directly inside C# string literals. Put emitted text in a template under
`res/templates/` and render it with the repository template helper.

When doing generated-output checks, read the generated source and project-file
diff carefully, use focused fixtures when full binding output is too large, and
summarize meaningful generated-code changes before handoff. Delete disposable
`out/bindgen-review/` snapshots before a Commit Mode handoff unless the user
asks to keep them; in Working Mode, list skipped generated-output checks or
cleanup.

Do not wire bindgen into normal restore or build targets. Run bindgen for the
changed library, then build. The bindgen default writes to non-active
`out/generated/bindgen`; pass `--setup-local` only when the task intentionally
needs consumers to use local generated binding projects. Consumers use the exact
local generated project under `out/bindgen` when it exists and otherwise use the
pinned package. Do not add `LOCAL_BINDINGS` or any other compile-time symbol to
distinguish local generated bindings from packaged bindings.

Native package builds are intended for CI. Agents must never run
`scripts/AlvorKit.Script.NativeBuild`, invoke native runtime package builds, or
install native build dependencies on a developer machine unless the user
explicitly asks for that work and grants permission for that run.

## Generated Native Test Doubles

Generated native binding API projects emit an abstract API class plus
`<ApiClass>Noop` and a forwarding `<ApiClass>Wrapper`. For tests that need a
native library double, subclass the generated noop and override only observed or
construction-needed calls; use the wrapper when most calls should forward to a
real backend. Keep native-library test doubles in tests. Do not add alternate
runtime constructors, ownership flags, or native-free product special cases just
to avoid native calls in tests.

## Package Version Properties

Keep version properties in `AlvorKit.Packages.props` limited to generated
binding packages, generated-package roots, and similarly pinned generated
inputs. Ordinary hand-authored project dependencies, including script utilities
and runtime helper packages, should declare package versions directly in the
project file unless there is a clear non-generated repo-wide reason to
centralize them.

## C# Defaults

These defaults are unconditional. A closer `AGENTS.md` may add stricter
requirements but must not relax them.

- Omit braces when an `if`, `else`, `for`, `foreach`, `while`, or similar
  control-flow body contains exactly one statement and that complete statement
  occupies one physical line. A statement split across multiple physical lines
  requires braces even when it is syntactically one statement. This restriction
  applies to control-flow bodies, not expression-bodied members, lambdas, or
  other `=>` expressions. Braces are for multiline or multi-statement bodies,
  not single-line bodies. An unbraced `else` with exactly one statement must put
  that statement on the same physical line as `else`, as in
  `else DoOtherThing();`. Placing that single statement on the following line
  is banned. If the complete `else` statement cannot stay readable within the
  line-length limit, use a braced body.

  ```csharp
  if (condition)
      DoThing();
  else DoOtherThing();

  for (var index = 0; index < count; index++)
      Process(index);

  foreach (var value in values)
  {
      PublishTransformedValue(value, sourceConfiguration, destinationConfiguration, transformationContext,
          validationContext, diagnosticContext);
  }
  ```

- Strongly prefer guard clauses and early `return`, `continue`, or `break`
  statements that keep the main path flat. Avoid complicated `if`/`else`
  chains and nested conditionals when their exceptional or terminal cases can
  be handled first.
- Keep each mathematical computation and boolean expression on one physical
  line whenever it remains readable within the hard line-length limit. Prefer
  that single-line form even when it exceeds the preferred wrapping point. If
  the logic cannot remain clear on one line, split it into named intermediate
  variables or sequential steps whose individual computations and boolean
  expressions each stay on one line. Do not wrap one operator chain or
  parenthesized computation across multiple lines.
- Standalone discard assignments such as `_ = expression;` are banned. Do not
  create a fake assignment to silence an unused-value warning. Remove a
  no-effect expression, invoke a method directly when only its side effect
  matters, or name and use the result when it matters. Required interface,
  delegate, or framework parameters may remain unused without assigning them
  to `_`. This rule does not ban `out _`, deconstruction discards, or `_`
  patterns.
- Keep the input expression and `switch` keyword of a switch expression on the
  same physical line as the declaration, assignment, `return`, or expression
  arrow when the line fits within the length limit. Put the opening brace on
  the next line at the declaration's normal indentation and indent switch arms
  one level, just like a normal method body. Do not put `switch` on a separately
  indented continuation line.

  ```csharp
  private static Result Convert(Value value) => value switch
  {
      Value.First => Result.First,
      Value.Second => Result.Second,
      _ => throw new UnreachableException(),
  };
  ```

- Use binary literals (`0b...`) for bit masks, packed flags, and other bitwise
  constants whose meaning depends on the position, adjacency, or grouping of
  individual bits. Group binary digits with `_` when that makes fields easier
  to read. Hexadecimal literals remain valid when hexadecimal communicates the
  value more clearly, including powers of two, full-byte or all-ones patterns
  such as `0xFF` and `0xFFFF`, and values defined by an external hexadecimal
  contract. Do not use hexadecimal merely to shorten a positional bit layout.
- A `.cs` file may live directly at the root of its project when that is the
  clearest home. Prefer one top-level type per `.cs` file; do not group multiple
  records, classes, structs, or interfaces in a protocol, model, command, or
  `Types` file just because they are small.
- Use of the C# `sealed` keyword in repository-owned declarations is banned. Do
  not use it on classes, records, overrides, or any other declaration,
  including generated output, examples, demos, scripts, and tests.
- Use of the C# `checked` keyword is banned in repository-owned code. Do not use
  checked expressions or blocks, including in generated output, templates,
  examples, demos, scripts, and tests. Express any required range contract
  without the keyword.
- Organize class and struct members in this exact category order:
  1. readonly fields;
  2. non-readonly fields;
  3. properties with only a `get` accessor;
  4. properties with both read and write access, including `set` or `init`;
  5. `ref` and `ref readonly` properties;
  6. constructors;
  7. all remaining members.
     Within each field or property category, order accessibility as `private`,
     then `internal`, then `public`. Constants and static readonly fields belong
     with readonly fields. Static and instance members do not create separate
     categories. Keep overloads and closely related members together only within
     these ordering constraints. A multiline property with nontrivial accessor
     logic may be placed after all simple properties as the final property block
     immediately before the constructor, or before the remaining members when
     there is no constructor.
- Keep consecutive fields and simple properties compact. Strongly prefer no
  blank lines between members of the same category; add vertical space only
  when it marks a meaningful category boundary or isolates a nontrivial
  multiline property implementation.
- Default parameter values are banned. Every caller must supply every argument;
  use a distinctly named method or overload only when it represents a genuinely
  different operation rather than recreating an implicit default.
- In every multiline declaration parameter list, put the closing `)` directly
  after the final parameter. A closing parenthesis on its own line is banned.
  This applies to methods, constructors, primary constructors, records,
  delegates, and lambdas.
- An injected service enters a collaborator only through constructor injection.
  Never pass an injected service through an ordinary method, local function,
  delegate, command, record, or other operation parameter. A type is either a
  scope-owned injected service or an explicitly passed ordinary object, never
  both. Per-call parameters contain operation data, not hosted collaborators.
- Strongly prefer private fields for storage. Expose state through the
  narrowest useful property instead of an `internal` or `public` field. When a
  caller genuinely needs by-reference access, keep the backing field private
  and expose a narrowly scoped `ref` or `ref readonly` property. When both
  accessors would only forward unrestricted reads and writes to one backing
  field, prefer a `ref` property; keep get/set accessors when they validate,
  transform, restrict, or otherwise own behavior. Use an exposed field only
  when a framework, binary layout, generated-code contract, or measured
  hot-path requirement specifically demands one.
- Auto accessors are banned in hand-authored classes and non-record structs. A
  property must compute its value or use explicit accessors over private backing
  fields. Records are the only hand-authored exception: auto-properties and
  positional records are allowed when they clearly express the record's value
  shape. Generated code is also exempt because its source shape belongs to the
  generator. Accessor-only declarations on interfaces and abstract members are
  contracts rather than stored auto-properties and are allowed.
- Avoid the static `Array` API for operations on existing contiguous storage,
  including `Array.Clear`, `Array.Fill`, `Array.Copy`, `Array.IndexOf`,
  `Array.Reverse`, and `Array.Sort`. Obtain an appropriate `Span<T>` or
  `ReadOnlySpan<T>` view and use span-based operations instead.
- Prefer repository-level and project-level global usings over ordinary
  file-level `using` directives. Before adding a file-level import, check
  implicit usings and existing `<Using Include="..." />` entries. Add broadly
  useful namespaces to the area `Directory.Build.props`; add project-only
  namespaces to the `.csproj`. Reserve file-level imports for aliases, rare
  conflicts, or one-off third-party APIs. `using var` and `using (...)`
  disposal statements are allowed and are not import directives.
- Prefer clean primary constructors when captured parameters eliminate
  mechanical backing fields and assignment-only constructor bodies. This is an
  allowed exception to the normal constructor position for a small value
  carrier or a non-public implementation type. Keep an explicit constructor
  when constructor accessibility must differ from the type, when initialization
  has behavior, or when ref-returning access requires a declared backing field.
  A ref-like parameter that the compiler cannot capture may initialize one
  explicit ref-like field inline while the remaining parameters stay captured.
  In partial types, first verify whether primary constructor parameters are
  already in scope.
- Trust nullable reference type analysis for non-null contracts. Do not add
  manual null guards or asserts just to recheck a non-nullable value.
- Prefer file-scoped namespaces, nullable-aware code, collection expressions,
  and the style already enforced by `.editorconfig`. Avoid new production
  dependencies unless the task clearly needs them and the tradeoff is explained.
- Prefer functional style where it improves clarity: pure helpers, immutable
  values, small transformations, explicit inputs and outputs, and minimal shared
  mutable state.
- Prefer tuple literals for repository vector types such as `Vec2`, `Vec3`, and
  `Vec4` when the target type is clear. Use constructors when the constructor is
  the point, such as scalar splats, composition constructors, conversion tests,
  or expressions with no target vector type.
- Prefer repository vector casts such as `(Vec2u)image.Size` over converting
  components one by one.
- Treat AlvorKit maths types as first-class API shapes. Accept and pass vectors,
  matrices, quaternions, boxes, and related maths types instead of flattening
  true maths values into scalar overloads.
- Do not silently clamp, coerce, or normalize caller-provided values in property
  setters or state updates. Model the invariant in the type system, or clamp
  explicitly at a platform boundary.
- In AlvorKit's curated library projects, do not create private nested classes
  for helper composition; prefer internal top-level helper types when they are
  intentionally outside the public API. Game repositories override this and
  prefer public game-code types and collaborating members. Avoid partial classes
  for hand-authored code except for generated-code integration or unavoidable
  framework/tooling requirements, and mention the reason in the work summary.
- Avoid generic `Factory`, `Manager`, `Service`, and similarly broad suffixes
  when a constructor, static `Create`, delegate, or domain-specific type name is
  clearer. Generally avoid static helper types and methods in hand-authored
  code; reserve static members for constants, operators, pure domain functions
  with no collaborator dependency, and framework-required entry points.

## Project Split Model

Before creating or reorganizing game projects, package boundaries, frontend,
backend, server, protocol, or menu packages, read
[docs/ProjectSplitModel.md](docs/ProjectSplitModel.md). Keep dependency
direction consistent with the pure/frontend/menu/backend/server split.

## Game Scope Organization

Before creating or reorganizing game dependency-injection scopes,
root/game/world/level/player services, loader scopes, or state transitions, read
[docs/GameScopeOrganization.md](docs/GameScopeOrganization.md). Keep scope
prefixes, attributes, service names, and constructor dependency ordering
consistent with that guide.

## Game Ents And ECS

AlvorKit game templates and game repositories must use AlvorKit ECS for game
Ents. Use `Ent` in every context. The word `Entity` is banned; use `Ents` for
the plural. This applies to prose, code identifiers, type and member names,
parameters and locals, filenames, directories, labels, and compound names.

Before creating or significantly changing generated component declarations,
Ent handles or arenas, Indexed contexts, hooks, bags, indexes, or Ent lifetime,
read [docs/ECS.md](docs/ECS.md). Keep game behavior in injected systems and
services, keep Ent state in components, and follow the guide's ownership,
registration, mutation, iteration, and teardown contracts.

## GL Object Ownership

Before creating, deleting, or wiring the lifetime of any GL object (buffers,
textures, vertex arrays, programs), read [docs/GlOwnership.md](docs/GlOwnership.md).
`GlLayer` is hierarchical: objects belong to the scope node that created them,
and a scope-lifetime object must not get its own `IDisposable` or `Delete*`
teardown — disposing the scope's node reclaims everything it owns in one sweep.

## UI Menu Authoring

Before creating or significantly changing an AlvorKit UI menu, read
[docs/MenuAuthoring.md](docs/MenuAuthoring.md). Menu classes should follow that
single-`Create`-method shape unless a more specific `AGENTS.md` explicitly
allows a different local pattern.

## Documentation

Write public documentation for a reader who only sees the published API, tool,
or document. Avoid meta descriptions that only make sense to the author, an
agent, or a generator maintainer unless the generation process is itself the
subject. Prefer domain wording and concrete examples of the public things the
documentation describes.

Before changing generated C binding documentation, read
`docs/CBindingDocumentation.md`. Use its audit checklist against generated
output when doing generated-output checks or Commit Mode checks; in a Working
Mode handoff, list that audit if it was skipped.

For generated native bindings, use original upstream documentation whenever it
exists. When upstream has no usable documentation, author documentation from
the public API shape rather than describing the generator or selection process.
Every public binding documentation comment must reference the original C symbol
using exact native names in `<c>...</c>`. For managed convenience overloads or
helpers, inherit or point back to the
native-shaped member and keep the underlying C symbol visible. For enum groups
synthesized from macros, document the public grouping rule or native API use.

## Runtime Allocation Discipline

Avoid managed allocations in runtime, render-loop, resource lifetime,
validation, bind/unbind, delete/dispose cleanup, polling, and other hot-path
code unless the allocation is explicitly intended and documented. This includes
arrays, `List<T>`, LINQ, iterator blocks, closures, params arrays, boxing,
string formatting, and defensive copies. Treat teardown and delete paths as
allocation-sensitive unless the user explicitly says otherwise.

When a native API passes a pointer and count for handles, ids, state values, or
other blittable data, do not copy it into a managed array just to validate,
track, delete, or forward it. Prefer `Span<T>`/`ReadOnlySpan<T>` over native
memory, `stackalloc`, caller-owned buffers, or a no-allocation scan. If a stable
snapshot is truly required, document why the allocation is acceptable and keep
it outside hot paths when possible.

When fixing an allocation-sensitive bug, solve the stated contract directly. Do
not introduce helper abstractions, diagnostic string construction, broader
validation policy, or extra state while fixing a narrow span, pointer, upload,
bind, delete, or lifetime contract. For byte-count contracts, prefer
`MemoryMarshal.AsBytes`, validate the byte count, and forward the resulting span
without allocation. For low-level runtime changes, scan touched code for
allocation constructs when practical; in a Working Mode handoff, list this scan
if it was skipped.

## Tests And Verification Gates

Test files may be up to 750 lines when keeping related scenarios together
improves readability. Do not apply normal source, script, or config file-size
targets to test files.

Read [docs/AgentVerification.md](docs/AgentVerification.md) for lint,
unit-test timing, coverage, command examples, artifact paths, and report-reading
workflow.

Do not run lint by default in Working Mode. Run the repository linter when the
user asks for Commit Mode or linting, and prefer scoped linting over repo-wide
linting. Do not use an unfiltered `git diff --name-only` in a dirty shared
worktree because it may include other agents' changes.

Do not run broad unit-test timing gates by default in Working Mode. Targeted
builds or tests are allowed when useful. When the user asks for Commit Mode or a
broad unit-test gate, run direct unit test commands through the timing guard.
The coverage tool already enforces the same timing budget from its own TRX
output.

Do not run coverage by default in Working Mode. Use the coverage tool when the
user asks for Commit Mode or a coverage signal for C# source or unit-test
changes. The repository coverage gate is 95% line, 85% branch, and 95% method
coverage; Commit Mode work should still aim for meaningful 100% coverage in
touched source modules.

Run repo-wide lint, full timing gates, full coverage gates, or other broad
verification only when explicitly requested, for CI parity checks, or as part of
user-requested broad Commit Mode work. If focused checks pass but broader checks
fail on unrelated files or projects, report that instead of fixing unrelated
work.
