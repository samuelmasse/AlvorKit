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

Do not wire bindgen into normal restore or build targets. Local binding mode is
explicit: create `AlvorKit.Local.props`, run bindgen for the changed library,
then build. If `UseLocalBindings=true` fails because `out/bindgen` is missing,
keep that failure and tell the user to run bindgen rather than making builds
generate code.

Do not add `LOCAL_BINDINGS` or any other compile-time symbol to distinguish
local generated bindings from packaged bindings. `UseLocalBindings` may choose
MSBuild project references instead of package references, but C# source and
tests must compile the same way in both modes.

## C# File Placement

A `.cs` file may live directly at the root of its project when that is the
clearest home. Do not create a subdirectory solely because the file is C#.

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

The binding coverage path reads `native/<library>/conf/bindgen.json`, measures
both the generated API project and its `.Backend` project, selects matching test
projects by package or project reference, and forces `UseLocalBindings=true` so
Coverlet instruments the generated project assemblies. If this fails because
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
