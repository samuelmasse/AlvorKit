# Scripts Instructions

## Scope

These instructions apply to C# code under `scripts/` and the matching tests under
`tests/AlvorKit.Script.*.Test/`.

## Working Standard

- Review the requested script code and its nearby collaborators before editing.
- Treat refactoring as part of normal script work, but keep it focused on the
  touched script project and directly related tests unless a broader refactor is
  explicitly requested.
- Preserve existing behavior unless the task asks for a behavior change.
- Keep changes cohesive, boring, and easy to review; avoid sweeping rewrites that
  do not pay for themselves in clarity, testability, or correctness.

## CLI Tools

- Build script command-line surfaces with `System.CommandLine`.
- Let `System.CommandLine` own generated help, version output, command routing,
  parse errors, option aliases, arity, and subcommand structure. Do not add
  hand-written usage strings, `HelpText` constants, `ShowHelp` sentinels, or
  custom `help` command paths for ordinary script CLIs.
- Keep command trees close to the entry point or parser type, with options and
  arguments represented as `Option<T>` and `Argument<T>`. Read values from
  `ParseResult` and pass a small immutable request/options object to execution
  code.
- Keep any manual argument scanning narrowly bounded to real boundary cases,
  such as separating a script's own options from arbitrary forwarded child
  process arguments. Prefer a named, tested splitter for that case.
- Tests may call parse-only helpers, but app entry points should invoke the
  `RootCommand` so generated `--help` output and parser errors behave the same
  way users see them.

## Visual Automation Scripts

- Prefer `scripts/AlvorKit.Script.AlvorSense` over AlvorEye for games wired with
  `AgentGlfwWindowHost` from `AlvorKit.Windowing.Agent`. Read
  `docs/AlvorSense.md` before using or changing that workflow.
- When using AlvorSense, share important screenshots in chat, summarize the key
  command batches and visual observations, and continue one live session
  whenever practical instead of repeatedly restarting the target.
- Use `scripts/AlvorKit.Script.AlvorEye` for desktop targets that are not wired
  for AlvorSense. Read `docs/AlvorEye.md` before using or changing AlvorEye.

## C# Style

- Prefer functional style where it improves clarity: pure helpers, immutable
  values, small transformations, explicit inputs and outputs, and minimal shared
  mutable state.
- Separate parsing, planning, filesystem/process IO, and emission logic so each
  part can be tested without shelling out or touching real external state.
- Prefer primary constructors for classes and records when they express the
  dependency or value shape cleanly.
- Do not create boilerplate constructors when a primary constructor or generated
  record constructor is sufficient.
- Prefer expression-bodied members (`=>`) for simple one-expression methods,
  properties, and operators when it improves readability. Use block bodies when
  the member has multiple statements, meaningful control flow, comments, or
  side effects that benefit from being visually emphasized.
- Prefer file-scoped namespaces, nullable-aware code, collection expressions, and
  the style already enforced by `.editorconfig`.
- Avoid new production dependencies unless the task clearly needs them and the
  tradeoff is explained.
- A `.cs` file may live directly at the root of its script project when that is
  the clearest home. Subdirectories are optional organization, not a requirement.
- Organize related `.cs` files into clear subdirectories when a script project
  grows large enough that grouping improves navigation.
- Keep script project folders shallow: use at most one level of subdirectories
  beneath each script project, and avoid nesting those groups further.

## File Size

- Keep each edited C# file at or below 150 lines.
- This 150-line target applies to script source files, not tests. Matching test
  files under `tests/AlvorKit.Script.*.Test/` may be up to 750 lines when the
  scenarios are cohesive.
- When a touched file is already over 150 lines, split out cohesive helpers or
  data shapes before adding more code.
- Do not use partial classes solely to satisfy the script source file-size
  target. If a file grows past the target, first extract cohesive top-level
  helper types with domain-specific names. If no clean extraction exists, keep
  the file whole, call out the exception, and avoid spreading one logical type
  across partial files.
- If a file cannot reasonably be kept under 150 lines because of generated shape,
  platform-specific glue, or tightly coupled declarations, call that out clearly
  and keep the exception as small as possible.

## Documentation

- Add concise XML documentation comments for every type, constructor, method,
  field, and property introduced or changed.
- Documentation should explain purpose, contracts, invariants, edge cases, or
  side effects; do not add comments that merely restate the implementation.
- Keep implementation comments rare. Use them only to explain non-obvious
  external constraints, generated-code compatibility, native interop details, or
  algorithmic choices.

## Tests

- Add or update unit tests for every behavior change and every refactor that
  moves meaningful logic.
- Prefer focused tests in the matching project under `tests/AlvorKit.Script.*.Test/`.
- Cover happy paths, edge cases, invalid input, and failure behavior when the
  script code has explicit error handling.
- Use fakes, temporary workspaces, and test doubles instead of invoking real
  external tools whenever possible.
- Before finishing, run the most relevant `dotnet test` command. If the change
  crosses script-project boundaries, run the broader solution tests or explain
  why that was not possible.

## Final Review

- Re-read the changed files before finishing.
- Check that edited script source files respect the 150-line target, edited test
  files stay within the 750-line test limit, the repo-wide 170-character code
  line limit is respected, XML docs are meaningful, constructors are not
  redundant, and tests cover the changed behavior.
- Report the exact tests run and any remaining risk.
