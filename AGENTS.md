# Repository Instructions

## Scope

These instructions apply repo-wide. More specific `AGENTS.md` files under
`src/`, `scripts/`, and `demos/` add area-specific rules.

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
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library|all> --output-root out\bindgen-review\<case>\before
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library|all> --output-root out\bindgen-review\<case>\after
git diff --no-index -- out\bindgen-review\<case>\before out\bindgen-review\<case>\after
```

Review the generated source and project-file diff carefully. Use focused
fixtures under `out/bindgen-review/` when a full binding output is too large,
and summarize the meaningful generated-code changes before handing off.

## C# File Placement

A `.cs` file may live directly at the root of its project when that is the
clearest home. Do not create a subdirectory solely because the file is C#.

## Linting

Run the repository linter before handing off changes that touch code, workflow
configuration, JSON, or Markdown:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint
```

Use formatter write mode when you need to fix supported formatting issues in
touched files:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint -- --fix
```

## Code Coverage

Use the coverage tool whenever a change touches C# source or unit tests. The
tool writes agent-readable and human-readable artifacts under `out/coverage/`.

Quick commands:

- Agent full strict gate:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent`
- Agent focused strict gate:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --test-project AlvorKit.Script.NativeBuild.Test`
- Agent report-only targeted run:
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --test-project AlvorKit.Script.NativeBuild.Test --threshold 0`
- Full human/browser report: omit `--agent`.
- Open the browser report after a full report run:
  `Invoke-Item .\out\coverage\html\index.html`

Run the full strict gate before finishing broad or cross-project work:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent
```

For focused work, run only the matching test project:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --test-project AlvorKit.Script.NativeBuild.Test
```

The `--test-project` value may be a test project name, project file name, or
repository-relative path. Repeat `--test-project` to run more than one focused
test project. In targeted mode, the gate only expects source modules reachable
through the selected test projects' `ProjectReference` graph.

To generate reports without enforcing the coverage percentage, use
`--threshold 0`:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --test-project AlvorKit.Script.NativeBuild.Test --threshold 0
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
  `totals`, `modules`, `unmeasuredModules`, missing line details, and test logs.
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
- Check `passed`. If it is `false`, inspect `unmeasuredModules` first; those are
  source projects that did not produce coverage at all.
- Inspect `files`, which is sorted with the most missing coverage first. Each
  entry has `path`, `missingLines`, `missingBranches`, `missingMethods`,
  `missingLineNumbers`, `missingBranchLineNumbers`, and `missingMethodNames`.
- For focused work, filter `files` to the source paths touched in the change.
  Read only those source files and nearby tests; do not scan the raw coverage
  reports by hand.
- Use `out/coverage/coverage-summary.md` for a quick human-readable ranked list
  when deciding which file to tackle next.
- After adding tests, rerun the same targeted coverage command until the touched
  files have no missing lines, branches, or methods, then run the broader gate
  when feasible.

If the strict gate fails, inspect the JSON for precise missing lines and
methods, add or adjust unit tests, then rerun the same targeted command. Before
handing off, run the full strict gate when feasible, or state exactly why it was
not possible and point to the generated report.
