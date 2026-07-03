# Tests Instructions

## Scope

These instructions apply to all test projects under `tests/`.

This file is a delta over the root instructions. Use the root for shared
Working/Commit Mode, visual automation, verification, C# defaults, and test-size
policy unless this file is more specific.

## Shared Test Code

- Name runnable test projects with the `.Test` suffix so
  `Directory.Build.props` gives them MSTest, coverage, and the shared helper
  reference.
- Add a concise XML `<summary>` doc comment to each runnable test method.
- Use `AlvorKit.Testing` for cross-project fixtures such as temporary
  workspaces, disposable filesystem setup, and small repository/project writers.
- When the same helper shape appears in more than one test project, move it to
  `tests/AlvorKit.Testing` before adding the second copy.
- Keep project-specific harnesses local when they depend on one source
  project's internals or make sense only for that project.
- Favor explicit fixture builders that remove repeated setup without hiding the
  assertion being tested.

## Test Quality

- These rules apply whenever writing tests.
- Do not add coverage-only smoke bundles that merely call many APIs. Each
  runnable test should assert observable state, generated output, returned data,
  or a specific exception.
- If a coverage sweep is unavoidable, keep it small and state the behavioral
  invariant it protects in the test summary.
- When adding repository config fixtures, prefer shared helpers from
  `AlvorKit.Testing` over copied YAML/JSON writer methods.
- When changing a supported config format, keep tests for both the primary
  format and any explicitly supported transitional fallback format.
- Tests for generated artifact names, output directories, or run IDs should use
  meaningful identifiers such as native library names, not generic filenames
  like `bindgen`.

## Visual Test Harnesses

- Prefer direct unit tests for deterministic behavior.
- When a visual game smoke check is needed and the target is wired with
  `AgentGlfwWindowHost` from `AlvorKit.Windowing.Agent`, use the root
  AlvorSense guidance. Use AlvorEye only for targets that cannot run under
  AlvorSense or tests that intentionally cover real desktop window/input
  behavior.

## Commit Mode Verification

- For C# test changes, run focused coverage for the affected source project with
  `--source-project`, adding `--test-project` only when an explicit test project
  choice is useful, then run scoped lint.
- For generated binding test changes, run focused coverage with `--binding`
  using the native library name and `--threshold 0`, such as `xxhash`, and
  inspect missing generated API and `.Backend` coverage before handoff.
- If shared helper behavior changes across many projects, prefer the full strict
  coverage gate before handoff.
