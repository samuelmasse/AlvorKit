# Tests Instructions

## Scope

These instructions apply to all test projects under `tests/`.

## Shared Test Code

- Name runnable test projects with the `.Test` suffix so `Directory.Build.props`
  gives them MSTest, coverage, and the shared helper reference.
- Add a concise XML `<summary>` doc comment to each runnable test method.
- Use `AlvorKit.Testing` for cross-project test fixtures such as temporary
  workspaces, disposable filesystem setup, and small repository/project writers.
- When the same helper shape appears in more than one test project, move it to
  `tests/AlvorKit.Testing` instead of copying it again.
- Keep project-specific harnesses local when they depend on internals from one
  source project or make sense only for that project.
- Favor explicit, readable fixture builders over clever abstractions. Shared
  helpers should remove repeated setup, not hide the assertion being tested.

## Test File Size

- In Working Mode, treat the 750-line test file limit as cleanup guidance, not a
  blocker for making Working Mode changes work.
- In Commit Mode, test files have their own file-size limit: up to 750 lines for
  a cohesive file. This is intentionally different from the normal source or
  script code limits.
- During Commit Mode work, do not split test files just to keep them tiny.
  Keeping related scenarios together is preferred when the file remains at or
  below 750 lines.
- During Commit Mode work, split a test file when the scenarios stop sharing
  context or it would exceed 750 lines, not because an agent prefers small files.

## Test Quality

- These rules are always on when writing tests, in both Working Mode and
  Commit Mode.
- Do not add coverage-only smoke bundles that merely call many APIs. Each
  runnable test should assert observable state, generated output, returned data,
  or a specific exception. If a coverage sweep is unavoidable, keep it small and
  state the behavioral invariant it protects in the test summary.
- When adding repository config fixtures, prefer shared helpers from
  `AlvorKit.Testing` over copied YAML/JSON writer methods. If a helper shape is
  needed in two projects, move it before adding the second copy.
- When changing a supported config format, keep tests for both the primary
  format and any explicitly supported transitional fallback format.
- Tests for generated artifact names or run IDs should prefer user-meaningful
  identifiers, such as native library names, over generic filenames like
  `bindgen`.

## Visual Test Harnesses

- Prefer direct unit tests for deterministic behavior. When a visual game smoke
  check is needed and the target is wired with `AgentGlfwWindowHost` from `AlvorKit.Windowing.Agent`, prefer
  AlvorSense over AlvorEye and read `docs/AlvorSense.md`.
- When using AlvorSense for a visual smoke check, share important screenshots in
  chat, summarize the key batches and observed changes, and keep the same live
  session running whenever practical.
- Use AlvorEye only for targets that cannot run under AlvorSense or when the
  test intentionally covers real desktop window/input behavior. Read
  `docs/AlvorEye.md` before using it.

## Verification

- The checks in this section are Commit Mode gates. In Working Mode, run
  focused tests, coverage, or lint only when they are useful while iterating.
- In Commit Mode, for C# test changes, run focused coverage for the affected
  source project with `--source-project`, adding `--test-project` only when you
  need to choose the test project explicitly, then run scoped lint before
  handoff.
- In Commit Mode, for generated binding test changes, run focused coverage with
  `--binding` using the native library name, such as
  `dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --agent --binding xxhash --threshold 0`.
  This checks both the generated API project and its `.Backend` project; inspect
  the reported missing coverage before handoff.
- In Commit Mode, if a shared helper changes behavior used by many projects,
  prefer the full strict coverage gate before handoff.
