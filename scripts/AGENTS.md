# Scripts Instructions

## Scope

These instructions apply to C# code under `scripts/` and the matching tests under
`tests/AlvorKit.Script.*.Test/`.

This file is a delta over the root instructions. Use the root for shared
Working/Commit Mode, visual automation, verification, C# defaults, and test-size
policy unless this file is more specific.

## Command-Line Tools

- Build script command-line surfaces with `System.CommandLine`.
- Let `System.CommandLine` own generated help, version output, command routing,
  parse errors, option aliases, arity, and subcommand structure.
- Do not add hand-written usage strings, `HelpText` constants, `ShowHelp`
  sentinels, or custom `help` command paths for ordinary script CLIs.
- Keep command trees close to the entry point or parser type, with options and
  arguments represented as `Option<T>` and `Argument<T>`.
- Read values from `ParseResult` and pass a small immutable request/options
  object to execution code.
- Keep manual argument scanning narrowly bounded to real boundary cases, such as
  separating a script's own options from arbitrary forwarded child-process
  arguments. Prefer a named, tested splitter for that case.
- Tests may call parse-only helpers, but app entry points should invoke the
  `RootCommand` so generated `--help` output and parser errors match user
  behavior.

## Script Shape

- Separate parsing, planning, filesystem/process IO, and emission logic so each
  part can be tested without shelling out or touching real external state.
- Use fakes, temporary workspaces, and test doubles instead of invoking real
  external tools whenever possible.
- Keep script project folders shallow: use at most one level of subdirectories
  beneath each script project unless an existing project already has a stronger
  local convention.

## Commit Mode Overrides

- Keep each edited script C# file at or below 150 lines. This target does not
  apply to matching test files, which follow the root test-size policy.
- If a touched script file is already over 150 lines, split out cohesive helpers
  or data shapes before adding more code. If generated shape, platform glue, or
  tightly coupled declarations make that unreasonable, call out the small
  exception in the handoff.
- Do not use partial classes solely to satisfy the script file-size target.
  Prefer cohesive top-level helper types with domain-specific names; if no clean
  extraction exists, keep the file whole instead of spreading one logical type
  across partial files.
- Add concise XML documentation comments for every type, constructor, method,
  field, and property introduced or changed; explain contracts, invariants, edge
  cases, or side effects.
- Add or update focused tests in the matching
  `tests/AlvorKit.Script.*.Test/` project for behavior changes and refactors
  that move meaningful logic.
- Cover happy paths, edge cases, invalid input, and failure behavior when the
  script code has explicit error handling.
- In final review, re-read changed files and check the 150-line script target,
  meaningful XML docs, no file-size-only partials, and focused test coverage.
