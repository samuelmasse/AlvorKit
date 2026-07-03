# Repository Instructions

## Scope

These instructions apply repo-wide. More specific `AGENTS.md` files under
`src/`, `scripts/`, `demos/`, `tests/`, and `res/templates/` add area-specific
rules for their scope and may narrow or relax repo-wide defaults when they say
so explicitly. Read the closest scoped instructions before working in those
areas.

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
editing. Preserve existing behavior unless the task asks for a behavior change,
and keep refactors cohesive and scoped to the touched project or directly
related tests unless a broader refactor is explicitly requested.

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

Use `scripts/AlvorKit.Script.AlvorEye` when the visual target is not wired for
AlvorSense, such as an arbitrary desktop window, external application, or demo
that needs real OS-level window discovery and input. Read `docs/AlvorEye.md`
before using or extending AlvorEye.

## Line Length

In Working Mode, treat the 170-character line limit as cleanup guidance that
should not block making changes work. In Commit Mode, keep hand-authored C#,
MSBuild XML, JSON, YAML, C, and header lines at or below 170 characters.

Prefer wrapping long argument lists, object and array literals, command strings,
interpolated messages, regex setup, and generated-code emission strings when
wrapping keeps the result readable. Generated output, build output, vendored
upstream source, minified files, lockfiles, and unavoidable long URLs or
external identifiers may exceed the limit. During Commit Mode, wrap existing
over-limit touched lines unless they are exceptions, but do not create unrelated
churn just to clean historical lines.

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
changed library, then build; consumers use the exact local generated project
under `out/bindgen` when it exists and otherwise use the pinned package. Do not
add `LOCAL_BINDINGS` or any other compile-time symbol to distinguish local
generated bindings from packaged bindings.

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

- A `.cs` file may live directly at the root of its project when that is the
  clearest home. Prefer one top-level type per `.cs` file; do not group multiple
  records, classes, structs, or interfaces in a protocol, model, command, or
  `Types` file just because they are small.
- Prefer repository-level and project-level global usings over ordinary
  file-level `using` directives. Before adding a file-level import, check
  implicit usings and existing `<Using Include="..." />` entries. Add broadly
  useful namespaces to the area `Directory.Build.props`; add project-only
  namespaces to the `.csproj`. Reserve file-level imports for aliases, rare
  conflicts, or one-off third-party APIs. `using var` and `using (...)`
  disposal statements are allowed and are not import directives.
- Prefer clean primary constructors when they express a type's dependency or
  value shape. Do not add mirrored private fields or properties solely to make
  primary constructor parameters `readonly`; use parameters directly unless a
  distinct member is needed for validation, transformation, API naming, or real
  mutable state. In partial types, first verify whether primary constructor
  parameters are already in scope.
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
  components one by one. Do not add `checked()` around routine vector or scalar
  boundary casts unless the user asks for it or the code already models that
  exact checked invariant.
- Treat AlvorKit maths types as first-class API shapes. Accept and pass vectors,
  matrices, quaternions, boxes, and related maths types instead of flattening
  true maths values into scalar overloads.
- Do not silently clamp, coerce, or normalize caller-provided values in property
  setters or state updates. Reject invalid values clearly, model the invariant
  in the type system, or clamp explicitly at a platform boundary.
- Do not create private nested classes for helper composition; prefer internal
  top-level helper types. Avoid partial classes for hand-authored code except
  for generated-code integration or unavoidable framework/tooling requirements,
  and mention the reason in the work summary.
- Avoid generic `Factory`, `Manager`, `Service`, and similarly broad suffixes
  when a constructor, static `Create`, delegate, or domain-specific type name is
  clearer. Generally avoid static helper types and methods in hand-authored
  code; reserve static members for constants, operators, pure domain functions
  with no collaborator dependency, and framework-required entry points.

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
exists. Fallback documentation is acceptable only when upstream has no usable
documentation, and it must describe the public API shape rather than the
generator or selection process. Every public binding documentation comment must
reference the original C symbol using exact native names in `<c>...</c>`. For
managed convenience overloads or helpers, inherit or point back to the
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
