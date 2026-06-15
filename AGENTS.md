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
output before and after the change whenever feasible. Use an ignored directory
under `out/` so the snapshots are disposable:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library> --output-root out\bindgen-review\<case>\before
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library> --output-root out\bindgen-review\<case>\after
git diff --no-index -- out\bindgen-review\<case>\before out\bindgen-review\<case>\after
```

Regenerate only the binding library whose generator inputs, configuration, or
source project changed. Use `all` only when the change intentionally affects
every generated binding project, and say why in the handoff. Review the
generated source and project-file diff carefully. Use focused fixtures under
`out/bindgen-review/` when a full binding output is too large, and summarize the
meaningful generated-code changes before handing off.

Do not wire bindgen into normal restore or build targets. Local binding mode is
explicit: create `AlvorKit.Local.props`, run bindgen for the changed library,
then build. If `UseLocalBindings=true` fails because `out/bindgen` is missing,
keep that failure and tell the user to run bindgen rather than making builds
generate code.

## C# File Placement

A `.cs` file may live directly at the root of its project when that is the
clearest home. Do not create a subdirectory solely because the file is C#.

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
tool writes agent-readable and human-readable artifacts under `out/coverage/`.

Quick commands:

- Agent focused strict gate for a touched source project:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild`
- Agent focused strict gate with an explicit test project:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild --test-project AlvorKit.Script.NativeBuild.Test`
- Agent report-only targeted run:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild --threshold 0`
- Agent full strict gate:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent`
- Full human/browser report: omit `--agent`.
- Open the browser report after a full report run:
  `Invoke-Item .\out\coverage\html\index.html`

For focused work, gate coverage on the source project or projects you changed:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild
```

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

Run the full strict gate only before finishing broad or cross-project work,
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
  data needed for `coverage-summary.json` and the compact Markdown summary.
- The tool runs multiple selected test projects concurrently by default after a
  prebuild step. Use `--max-parallel 1` only when diagnosing order-sensitive
  build or test issues on a busy machine.
- Omit `--agent` only when you need the browser HTML report, raw Cobertura XML,
  or raw LCOV artifacts.

Generated artifacts:

- `out/coverage/coverage-summary.json`: agent-oriented summary with `passed`,
  `testProjectFilters`, `sourceProjectFilters`, `totals`, `modules`,
  `unmeasuredModules`, missing line details, and test logs.
- `out/coverage/coverage-summary.md`: compact human-readable summary.
- `out/coverage/projects/<test-project>/coverage.json`: raw Coverlet JSON for
  each executed test project.
- `out/coverage/projects/<test-project>/dotnet-test.log`: captured test output.
- `out/coverage/html/index.html`: browser-readable ReportGenerator report when
  `--agent` is omitted.
- `out/coverage/reportgenerator.log`: captured `dotnet tool restore` and
  ReportGenerator output when `--agent` is omitted.
- `out/coverage/projects/<test-project>/coverage.cobertura.xml` and
  `coverage.info`: raw Cobertura XML and LCOV reports when `--agent` is omitted.

Agent workflow:

- Do not parse the HTML, Cobertura, LCOV, or test logs unless debugging the
  coverage tool itself. Start with `out/coverage/coverage-summary.json`.
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
- Use `out/coverage/coverage-summary.md` for a quick human-readable ranked list
  when deciding which file to tackle next.
- After adding tests, rerun the same targeted coverage command until the touched
  files have no missing lines, branches, or methods.

If the strict gate fails, inspect the JSON for precise missing lines and
methods, add or adjust unit tests, then rerun the same targeted command. If
focused coverage passes but a broader run fails on unrelated source projects,
report that instead of fixing unrelated work.
