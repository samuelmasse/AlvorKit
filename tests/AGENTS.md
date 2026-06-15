# Tests Instructions

## Scope

These instructions apply to all test projects under `tests/`.

## Shared Test Code

- Name runnable test projects with the `.Test` suffix so `Directory.Build.props`
  gives them MSTest, coverage, and the shared helper reference.
- Use `AlvorKit.Testing` for cross-project test fixtures such as temporary
  workspaces, disposable filesystem setup, and small repository/project writers.
- When the same helper shape appears in more than one test project, move it to
  `tests/AlvorKit.Testing` instead of copying it again.
- Keep project-specific harnesses local when they depend on internals from one
  source project or make sense only for that project.
- Favor explicit, readable fixture builders over clever abstractions. Shared
  helpers should remove repeated setup, not hide the assertion being tested.

## Test File Size

- Do not split test files just to keep them tiny. A cohesive test file around
  750 lines is acceptable when it keeps related scenarios together.
- Split a test file when the scenarios stop sharing context, not because an
  agent prefers small files.

## Verification

- For C# test changes, run the focused coverage command for the affected test
  project when practical, then run the repo linter before handing off.
- If a shared helper changes behavior used by many projects, prefer the full
  strict coverage gate before finishing.
