# Agent Verification

This document collects lint, timing, and coverage commands plus their report
artifacts. The root `AGENTS.md` controls when these checks are required.

## When To Read This

Read this document when the user asks for Commit Mode, final verification,
linting, coverage, broad test gates, unit-test timing, or CI parity checks.

In Working Mode, do not run lint, coverage, broad test gates, or final
verification solely because work is ending. Targeted builds, tests, visual
checks, or generated-output checks are allowed when useful for the specific
change or question.

## Linting

Do not run lint by default in Working Mode. Run the repository linter when the
user asks for Commit Mode or linting, and prefer scoped linting over repo-wide
linting. Pass only files, directories, or globs in the requested Commit Mode
scope:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint -- --include "path/or/glob"
```

Repeat `--include` for multiple paths. Use formatter write mode with the same
scoped includes when you need to fix supported formatting issues in touched
files:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint -- --fix --include "path/or/glob"
```

Do not use an unfiltered `git diff --name-only` in a dirty shared worktree
because it may include other agents' changes. Run the repo-wide linter only for
broad cross-repo changes, CI parity checks, or when explicitly requested. If
scoped lint passes but repo-wide lint fails on unrelated files, report that
instead of fixing unrelated work.

## Unit Test Timing

Do not run broad unit-test timing gates by default in Working Mode. Targeted
builds or tests are allowed when useful. When the user asks for Commit Mode or a
broad unit-test gate, run direct unit test commands through the timing guard so
per-test TRX durations are checked against the repository budget:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestTiming -- --max-duration-ms 1000 -- AlvorKit.slnx --no-build --no-restore
```

Pass the normal `dotnet test` target and options after the timing guard options.
The guard adds a TRX logger, writes `slowest-tests.md` and `slowest-tests.csv`
under `out/test-timing/runs/<run-id>/`, prints `WARNING AVKTESTTIMING` lines for
tests slower than the budget, and exits nonzero when tests fail or any test
exceeds the one-second budget. Use `--warn-only` only for temporary measurement
while finding existing offenders. For a Commit Mode handoff, do not leave
slow-test warnings unresolved unless the user accepts that risk.

The coverage tool already runs tests and enforces the same timing budget from
its own TRX output, so do not run a separate timing-guard pass after coverage
unless the user asks for an independent non-instrumented timing check.

## Code Coverage

Do not run coverage by default in Working Mode. Use the coverage tool when the
user asks for Commit Mode or a coverage signal for C# source or unit-test
changes. The tool writes agent-readable and human-readable artifacts under an
isolated `out/coverage/runs/<run-id>/` directory by default. The console output
prints the exact paths for the current run.

The repository coverage gate is 95% line, 85% branch, and 95% method coverage,
and Commit Mode work should still aim for meaningful 100% coverage in touched
source modules. Use the 85% branch gate as a practical floor to avoid low-value
test setups for defensive branches, compiler-shaped branch artifacts, platform
probes, and integration boundaries. Treat missing lines in touched files as test
candidates unless the code is a CLI entry point, external process wrapper,
native-host probe, report generator integration, or other integration boundary
that is better marked with `ExcludeFromCodeCoverage`.

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

For focused Commit Mode work, gate coverage on the source project or projects
you changed:

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

Run the full coverage gate only when explicitly requested, for CI parity checks,
or as part of user-requested broad Commit Mode work:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent
```

To generate reports without enforcing the coverage percentage, use
`--threshold 0`:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --source-project AlvorKit.Script.NativeBuild --threshold 0
```

## Coverage Optimization Notes

Prefer `--agent` for automated checks. It keeps the same pass/fail gate but
skips ReportGenerator, Cobertura, and LCOV so the tool writes only the JSON data
needed for the run-scoped `coverage-summary.json` and compact Markdown summary.

The tool runs multiple selected test projects concurrently by default after a
prebuild step. Use `--max-parallel 1` only when diagnosing order-sensitive build
or test issues on a busy machine.

Omit `--agent` only when the user explicitly asks for the browser HTML report,
raw Cobertura XML, raw LCOV artifacts, or when debugging the coverage reporting
pipeline itself. When you do generate a full report, use the `HTML report:` path
printed by the coverage tool.

Use `--run-id <name>` when you want a stable run directory, or
`--output-root out/coverage/agents/<agent-id>` to place isolated runs under an
agent-specific parent directory.

## Coverage Artifacts

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
- `out/coverage/runs/<run-id>/projects/<test-project>/coverage.cobertura.xml`
  and `coverage.info`: raw Cobertura XML and LCOV reports.

## Coverage Workflow

Do not parse the HTML, Cobertura, LCOV, or test logs unless debugging the
coverage tool itself. Start with the `Agent report:` path printed by the current
coverage command.

Check `passed`. Confirm `sourceProjectFilters` contains only the source projects
you meant to gate. If it is empty during focused work, rerun with
`--source-project` before chasing unrelated failures.

If `passed` is `false`, inspect `unmeasuredModules` first; those are source
projects that did not produce coverage at all.

Inspect `files`, which is sorted with the most missing coverage first. Each
entry has `path`, `missingLines`, `missingBranches`, `missingMethods`,
`missingLineNumbers`, `missingBranchLineNumbers`, and `missingMethodNames`.

For focused work, filter `files` to the source paths touched in the change. Read
only those source files and nearby tests; do not scan the raw coverage reports
by hand.

Use the `Human report:` path printed by the current coverage command for a quick
human-readable ranked list when deciding which file to tackle next.

During Commit Mode work, after adding tests, rerun the same targeted coverage
command until the touched files have no missing lines, branches, or methods.

If the coverage gate fails, inspect the JSON for precise missing lines and
methods, add or adjust unit tests, then rerun the same targeted command. If
focused coverage passes but a broader run fails on unrelated source projects,
report that instead of fixing unrelated work.
