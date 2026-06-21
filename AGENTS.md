# Repository Instructions

## Scope

These instructions apply repo-wide. More specific `AGENTS.md` files under
`src/`, `scripts/`, and `demos/` add area-specific rules.

## Agent Complaints

When an agent has a complaint about the tools it uses, or believes another
agent is working at the same time in a way that disturbs its task, write a
concise complaint under `out/complaints/`. Use a descriptive Markdown filename
when possible. These complaints are later input for agent quality-of-life
improvements.

## Agent Coordination

Use advisory leases under `out/agents/` to make concurrent agent work visible.
These leases are coordination hints, not hard locks. If an active lease overlaps
your intended write paths, avoid the overlap when practical, or leave a short
conflict note explaining why the overlap is unavoidable.

Read-only exploration does not need a lease unless it runs expensive, broad, or
disruptive commands. Create a lease before editing files, generating code,
running repo-wide or broad scoped formatters, refreshing generated bindings,
performing cleanup, staging files, or doing other work that could disturb
another agent.

Before claiming paths, run `list` or `check` to inspect active non-expired leases.
Use the lease helper instead of hand-editing JSON:

```powershell
dotnet run --project scripts\AlvorKit.Script.AgentLease -- start --agent <id> --task "Short task" --path "src/Foo/**"
dotnet run --project scripts\AlvorKit.Script.AgentLease -- touch --agent <id>
dotnet run --project scripts\AlvorKit.Script.AgentLease -- list
dotnet run --project scripts\AlvorKit.Script.AgentLease -- check --agent <id> --path "src/Foo/**"
dotnet run --project scripts\AlvorKit.Script.AgentLease -- conflict --agent <id> --task "Short task" --path "src/Foo.cs" --reason "Brief reason"
dotnet run --project scripts\AlvorKit.Script.AgentLease -- done --agent <id>
```

You may set `ALVORKIT_AGENT_ID` instead of passing `--agent` on every command.
When starting without an explicit agent id, the helper generates one and prints
it; reuse that id for `touch`, `check`, `conflict`, and `done`.

Lease files are JSON at `out/agents/<agent-id>.json`. Use repository-relative
paths and globs such as `src/Foo.cs`, `scripts/AlvorKit.Script.Lint/**`,
`*.slnx`, `*`, or `repo-wide`. Keep the path list specific enough for overlap
checks to be useful. Valid modes are `write`, `generate`, `format`, `test`,
`cleanup`, and `review`.

Leases expire five minutes after their last update by default. Refresh your lease
before editing again after any long-running command, and refresh it once in a
while during longer work. Use `--timeout-minutes <n>` when a longer-running
operation needs a larger stale window. Delete the lease with `done` when work
finishes; stale leases expire automatically if cleanup is missed.

Before staging, run `check` for the exact files or globs you intend to stage and
confirm your current lease still covers them. If overlap is unavoidable, write a
conflict note; the helper stores it under `out/agents/conflicts/`.

## Visual Automation

Use `scripts/AlvorKit.Script.AlvorSense` first when an agent needs to see, drive,
or verify an AlvorKit game that is wired through `AgentGlfwWindowHost` from `AlvorKit.Windowing.Agent`.
AlvorSense is preferred over AlvorEye for those targets because it runs without a
visible window, does not move the user's real mouse, and gives the agent exact
control over simulated time, update counts, input, rendering, and screenshots.

Read `docs/AlvorSense.md` before using or extending AlvorSense. That guide
explains session startup, command batches, screenshot capture, exact-time input
control, result artifacts, and when to fall back to AlvorEye.

When using AlvorSense, keep the chat user oriented: share important screenshots
in chat, briefly describe the key input/update batches and observed changes, and
continue in one live game session whenever practical instead of restarting after
each observation.

Use `scripts/AlvorKit.Script.AlvorEye` when the visual target is not wired for
AlvorSense, such as an arbitrary desktop window, external application, or demo
that still requires real OS-level window discovery and input. Prefer AlvorEye
over ad hoc screenshot scripts for those desktop targets.

Read `docs/AlvorEye.md` before using or extending AlvorEye. That guide explains
scenario files, JSONL sessions, result artifacts, handoff behavior, visual
verification patterns, and desktop automation gotchas.

## Line Length

Keep hand-authored code and config lines at or below 170 characters.

This applies to C#, MSBuild XML, JSON, YAML, C, and header files. Prefer
wrapping long argument lists, object and array literals, command strings,
interpolated messages, regex setup, and generated-code emission strings when
wrapping keeps the result readable.

Generated output, build output, vendored upstream source, minified files,
lockfiles, and unavoidable long URLs or external identifiers may exceed this
limit.

When touching an existing over-limit line, wrap it if it is not one of those
exceptions. Do not create unrelated churn just to clean historical lines.

## Generated Code Review

When changing a code generator or generator configuration, capture generated
output before and after the change whenever feasible. Prefer the helper, which
adds a random five-character suffix to avoid collisions and removes the review
directory when `finish` completes:

```powershell
dotnet run --project scripts\AlvorKit.Script.BindgenReview -- start <library> --case <case>
dotnet run --project scripts\AlvorKit.Script.BindgenReview -- finish <review-root-printed-by-start>
```

When adding or changing generated source, project files, scripts, or other
multi-line output, never embed that emitted code directly inside C# string
literals. Put the emitted text in a template under `res/templates/` and render
it with the repository template helper, passing only the dynamic values needed
by the template. Short identifiers, resource paths, and single-expression
replacement values are fine; full methods, declarations, XML/project fragments,
or script bodies belong in templates.

If you capture snapshots manually, use an ignored directory under `out/` and
append a random five-character suffix to the case directory:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library> --output-root out\bindgen-review\<case>-<suffix>\before
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library> --output-root out\bindgen-review\<case>-<suffix>\after
git diff --no-index -- out\bindgen-review\<case>-<suffix>\before out\bindgen-review\<case>-<suffix>\after
```

Regenerate only the binding library whose generator inputs, configuration, or
source project changed. Use `all` only when the change intentionally affects
every generated binding project, and say why in the handoff. Review the
generated source and project-file diff carefully. Use focused fixtures under
`out/bindgen-review/` when a full binding output is too large, and summarize the
meaningful generated-code changes before handing off. Delete disposable
`out/bindgen-review/` snapshot directories before finishing the task unless the
user explicitly asks to keep them for follow-up inspection.

Do not wire bindgen into normal restore or build targets. Run bindgen for the
changed library, then build; consumers automatically use the exact local
generated project when it exists under `out/bindgen`, and otherwise use the
pinned package.

Do not add `LOCAL_BINDINGS` or any other compile-time symbol to distinguish
local generated bindings from packaged bindings. C# source and tests must
compile the same way whether MSBuild selects a project reference or a package
reference.

## Generated Native Test Doubles

Generated native binding API projects emit an abstract API class plus
`<ApiClass>Noop` and a forwarding `<ApiClass>Wrapper`. For tests that need a
native library double, subclass the generated noop and override only the calls
the test observes or that construction needs. Use the wrapper when most calls
should forward to a real backend and only a few need interception or recording.

Keep native-library test doubles in tests. Do not add alternate runtime
constructors, ownership flags, or native-free special cases to product classes
just to avoid native calls in tests. Use the generated backend only when the
test intentionally exercises the real native library.

## Package Version Properties

Keep version properties in `AlvorKit.Packages.props` limited to generated
binding packages, generated-package roots, and similarly pinned generated
inputs. Ordinary hand-authored project dependencies, including script utilities
and runtime helper packages, should declare their package versions directly in
the project file unless there is a clear non-generated repo-wide reason to
centralize them.

## C# File Placement

A `.cs` file may live directly at the root of its project when that is the
clearest home. Do not create a subdirectory solely because the file is C#.

Prefer one top-level type per `.cs` file. Do not group multiple records,
classes, structs, or interfaces in a single protocol, model, command, or
`Types` file just because they are small. Use a folder with one file per type
when several related data shapes belong together.

## C# Using Directives

Prefer repository-level and project-level global usings over ordinary file-level
`using` directives.

Before adding a `using` directive to a `.cs` file, first check whether the
namespace is already available through implicit usings or an existing
`<Using Include="..." />` entry. If the namespace is broadly useful across an
area, add it to that area's `Directory.Build.props`. If it is only useful for one
project, add it to that project's `.csproj`.

Ordinary file-level `using` directives should be reserved for genuinely narrow
cases where a global using would make the project less clear, such as aliases,
rare namespace conflicts, or one-off third-party APIs. `using var` declarations
and `using (...)` disposal statements are not import directives and are allowed.

## C# Style

Prefer clean primary constructors when they express a type's dependency or
value shape. Do not add mirrored private fields or properties solely to make
primary constructor parameters `readonly`; use the parameters directly unless a
distinct member is needed for validation, transformation, API naming, or real
mutable state. In partial types, first verify whether the primary constructor
parameters are already in scope before adding mirror state for another file.

Trust C# nullable reference type analysis for non-null contracts. Do not add
`ArgumentNullException.ThrowIfNull`, manual `if (x is null)` guards, or
`Debug.Assert(x is not null)` checks just to recheck a value whose static type is
non-nullable. Express possible null with `?` and handle it, or let an invalid
caller fail at the first real use when the static contract says non-null.

Prefer tuple literals when declaring or assigning repository vector types such
as `Vec2`, `Vec3`, `Vec4`, and their scalar-family variants, as long as the
target vector type is clear. Use constructors when the constructor itself is the
point, such as scalar splats, composition constructors, conversion tests, or
expressions where no target vector type is available before an operator runs.

Treat AlvorKit maths types as first-class API shapes. When a value is truly a
vector, matrix, quaternion, box, or related maths type, accept and pass that
type instead of adding overloads with multiple scalar arguments or preferring
scalar parameters at call sites.

Do not silently clamp, coerce, or normalize caller-provided values in property
setters or state updates. If a value is invalid, reject it with a clear error or
model the invariant in the type system. If a platform boundary requires clamping,
perform it explicitly at that boundary and keep the original state contract
obvious.

Do not create private nested classes for helper composition. Prefer internal
top-level helper types when a class needs composed collaborators.

Avoid partial classes for hand-authored code. Do not use partial declarations as
a file organization technique for parser sections, command groups, protocol
types, or file-size compliance. Use partial only for generated-code integration
or unavoidable framework/tooling requirements, and mention the reason in handoff.

Avoid Java-style C# design and naming. Do not introduce generic `Factory`,
`Manager`, `Service`, or similarly broad suffixes when a constructor, static
`Create`, delegate, or domain-specific type name would express the intent more
clearly.

Generally avoid static helper types and static helper methods in hand-authored
code. Prefer small instance collaborators with explicit dependencies, even when
they are stateless today. Reserve static members for constants, operators,
pure domain functions with no collaborator dependency, and framework-required
entry points.

## Documentation Voice

Write public documentation for a reader who only sees the published API, tool,
or document. Avoid meta descriptions that only make sense to the author, an
agent, or a generator maintainer, such as "generated type", "shared by generated
types", "this file emits", or "configured scalar family", unless the generation
process is itself the subject being documented. Prefer domain wording and
concrete examples of the public things the documentation describes.

## Runtime Allocation Discipline

Avoid managed allocations in runtime, render-loop, resource lifetime, validation,
bind/unbind, delete/dispose cleanup, polling, and other hot-path code unless the
allocation is explicitly intended and documented. This includes hidden
allocations from arrays, `List<T>`, LINQ, iterator blocks, closures, params
arrays, boxing, string formatting, and defensive copies.

When a native API passes a pointer and count for handles, ids, state values, or
other blittable data, do not copy it into a managed array just to validate,
track, delete, or forward it. Prefer `Span<T>`/`ReadOnlySpan<T>` over native
memory, `stackalloc`, caller-owned buffers, or a no-allocation scan. If a stable
snapshot is truly required because the source memory cannot remain valid or the
collection must be mutated while enumerating, document why the allocation is
acceptable and keep it outside hot paths whenever possible.

When fixing an allocation-sensitive bug, solve the stated contract directly.
Keep constants, locals, and validation near the code that needs them unless they
are reused or express a real shared concept. Do not introduce helper
abstractions, diagnostic string construction, broader validation policy, or
extra state while fixing a narrow span, pointer, upload, bind, delete, or
lifetime contract. For byte-count contracts, prefer reinterpreting existing
blittable spans with `MemoryMarshal.AsBytes`, validating the byte count, and
forwarding the resulting span without allocation.

Before finishing low-level runtime changes, scan the touched code for allocation
constructs and remove accidental allocations from hot paths. Treat teardown and
delete paths as allocation-sensitive unless the user explicitly says otherwise.

## Test File Size

Test files are allowed to be larger than normal code files. A cohesive test file
may be up to 750 lines when keeping related scenarios together improves
readability. Do not apply the normal source, script, or config file-size targets
to test files.

## Linting

Run the repository linter before handing off changes that touch code, workflow
configuration, JSON, or Markdown. For agent work, prefer scoped linting over
repo-wide linting. Pass only files, directories, or globs you edited:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint -- --include "path/or/glob"
```

Repeat `--include` for multiple paths. Use formatter write mode with the same
scoped includes when you need to fix supported formatting issues in touched
files:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint -- --fix --include "path/or/glob"
```

Do not use an unfiltered `git diff --name-only` in a dirty shared worktree,
because it may include other agents' changes. Run the repo-wide linter only for
broad cross-repo changes, CI parity checks, or when explicitly requested. If
scoped lint passes but repo-wide lint fails on unrelated files, report that
instead of fixing unrelated work.

## Code Coverage

Use the coverage tool whenever a change touches C# source or unit tests. The
tool writes agent-readable and human-readable artifacts under an isolated
`out/coverage/runs/<run-id>/` directory by default. The console output prints
the exact paths for the current run.

The repository coverage gate is 95% line, 85% branch, and 95% method coverage,
but agents should still aim for meaningful 100% coverage in touched source
modules. Use the 85% branch gate as a practical floor to avoid ridiculous or
low-value test setups for defensive branches, compiler-shaped branch artifacts,
platform probes, and integration boundaries. Treat missing lines in touched files
as test candidates unless the code is a CLI entry point, external process
wrapper, native-host probe, report generator integration, or other integration
boundary that is better marked with `ExcludeFromCodeCoverage`.

Quick commands:

- Agent focused coverage gate for a touched source project:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild`
- Agent focused coverage gate with an explicit test project:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild --test-project AlvorKit.Script.NativeBuild.Test`
- Agent report-only targeted run:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild --threshold 0`
- Agent generated binding report:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --binding xxhash --threshold 0`
- Agent full coverage gate:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent`

For focused work, gate coverage on the source project or projects you changed:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild
```

For generated binding test work, gate coverage on the binding library name:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --binding xxhash --threshold 0
```

The binding coverage path reads `native/<library>/conf/bindgen.yml`, measures
both the generated API project and its `.Backend` project, and selects matching
test projects by package or project reference. If this fails because
`out/bindgen` is missing, run bindgen for that library first. Inspect the
reported missing lines and methods; omit `--threshold 0` only when you intend to
enforce the coverage gate for generated binding modules.

The `--source-project` value may be a source project name, project file name, or
repository-relative project directory or file path. Repeat `--source-project`
for multiple touched source projects. When `--test-project` is omitted, the tool
runs test projects that reference the selected source projects.

Use `--test-project` only when you need to further narrow or explicitly choose
the tests to run:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild --test-project AlvorKit.Script.NativeBuild.Test
```

The `--test-project` value may be a test project name, project file name, or
repository-relative path. Repeat `--test-project` to run more than one test
project. When `--source-project` is present, the gate expects only those source
modules, even if the selected tests also reference helper projects.

Run the full coverage gate only before finishing broad or cross-project work,
for CI parity checks, or when explicitly requested:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent
```

To generate reports without enforcing the coverage percentage, use
`--threshold 0`:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild --threshold 0
```

Agent optimization notes:

- Prefer `--agent` for automated checks. It keeps the same pass/fail gate but
  skips ReportGenerator, Cobertura, and LCOV so the tool writes only the JSON
  data needed for the run-scoped `coverage-summary.json` and compact Markdown
  summary.
- The tool runs multiple selected test projects concurrently by default after a
  prebuild step. Use `--max-parallel 1` only when diagnosing order-sensitive
  build or test issues on a busy machine.
- Omit `--agent` only when the user explicitly asks for the browser HTML report,
  raw Cobertura XML, raw LCOV artifacts, or when debugging the coverage reporting
  pipeline itself. When you do generate a full report, use the `HTML report:`
  path printed by the coverage tool.
- Use `--run-id <name>` when you want a stable run directory, or
  `--output-root out/coverage/agents/<agent-id>` to place isolated runs under an
  agent-specific parent directory.

Generated artifacts:

Fast agent runs with `--agent` write:

- `out/coverage/runs/<run-id>/coverage-summary.json`: agent-oriented summary
  with `passed`, `testProjectFilters`, `sourceProjectFilters`, `totals`,
  `modules`, `unmeasuredModules`, missing line details, and test logs.
- `out/coverage/runs/<run-id>/coverage-summary.md`: compact human-readable
  summary.
- `out/coverage/runs/<run-id>/projects/<test-project>/coverage.json`: raw
  Coverlet JSON for each executed test project.
- `out/coverage/runs/<run-id>/projects/<test-project>/dotnet-test.log`:
  captured test output.
- `out/coverage/latest-run.json`: non-authoritative pointer to the latest run.
  It is useful for manual navigation but can be overwritten by concurrent work.

Full reports without `--agent` additionally write:

- `out/coverage/runs/<run-id>/html/index.html`: browser-readable ReportGenerator
  report.
- `out/coverage/runs/<run-id>/reportgenerator.log`: captured ReportGenerator
  setup and execution output.
- `out/coverage/runs/<run-id>/projects/<test-project>/coverage.cobertura.xml` and
  `coverage.info`: raw Cobertura XML and LCOV reports.

Agent workflow:

- Do not parse the HTML, Cobertura, LCOV, or test logs unless debugging the
  coverage tool itself. Start with the `Agent report:` path printed by the
  current coverage command.
- Check `passed`. Confirm `sourceProjectFilters` contains only the source
  projects you meant to gate. If it is empty during focused work, rerun with
  `--source-project` before chasing unrelated failures.
- If `passed` is `false`, inspect `unmeasuredModules` first; those are source
  projects that did not produce coverage at all.
- Inspect `files`, which is sorted with the most missing coverage first. Each
  entry has `path`, `missingLines`, `missingBranches`, `missingMethods`,
  `missingLineNumbers`, `missingBranchLineNumbers`, and `missingMethodNames`.
- For focused work, filter `files` to the source paths touched in the change.
  Read only those source files and nearby tests; do not scan the raw coverage
  reports by hand.
- Use the `Human report:` path printed by the current coverage command for a
  quick human-readable ranked list when deciding which file to tackle next.
- After adding tests, rerun the same targeted coverage command until the touched
  files have no missing lines, branches, or methods.

If the coverage gate fails, inspect the JSON for precise missing lines and
methods, add or adjust unit tests, then rerun the same targeted command. If
focused coverage passes but a broader run fails on unrelated source projects,
report that instead of fixing unrelated work.
