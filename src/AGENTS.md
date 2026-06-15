# Source Instructions

## Scope

These instructions apply to C# code under `src/` and the matching tests under
`tests/AlvorKit.*.Test/`.

## Working Standard

- Review the requested source code and nearby collaborators before editing.
- Treat refactoring as part of normal source work, but keep it focused on the
  touched project and directly related tests unless a broader refactor is
  explicitly requested.
- Preserve existing behavior unless the task asks for a behavior change.
- Keep changes cohesive, reviewable, and grounded in the existing architecture.

## Game-Code Performance

- This is low-level video game/runtime code. Design hot-path code as if it must
  stay viable at roughly 5,000 FPS.
- Avoid GC churn in game-loop paths, render/update paths, tight polling loops,
  and per-frame helper methods.
- Prefer structs, readonly structs, spans, ref-friendly APIs, stack allocation,
  pooled or caller-owned buffers, and explicit ownership where they reduce
  allocations without making the code obscure.
- Keep hot-path APIs allocation-free unless a task explicitly accepts the cost.
  Watch for hidden allocations from LINQ, closures, iterator blocks, boxing,
  params arrays, string formatting, async state machines, and defensive copies.
- It is acceptable to allocate during startup, asset loading, configuration,
  diagnostics, error paths, teardown, and explicit load/unload operations.
- Do not over-optimize cold code. Favor clarity outside the hot path and reserve
  low-level techniques for places where frame-time or allocation pressure matters.

## C# Style

- Prefer functional style where it improves clarity: pure helpers, immutable
  values, small transformations, explicit inputs and outputs, and minimal shared
  mutable state.
- Separate state tracking, validation, platform/API calls, and resource lifetime
  logic so each part can be tested directly.
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
- A `.cs` file may live directly at the root of its source project when that is
  the clearest home. Subdirectories are optional organization, not a requirement.
- Organize related `.cs` files into clear subdirectories when a project grows
  large enough that grouping improves navigation.
- Keep project folders shallow: use at most one level of subdirectories beneath
  each source project, and avoid nesting those groups further.

## File Size

- Keep each edited C# file at or below 150 lines.
- When a touched file is already over 150 lines, split out cohesive helpers or
  data shapes before adding more code.
- If a file cannot reasonably be kept under 150 lines because of API shape,
  generated compatibility, platform glue, or tightly coupled declarations, call
  that out clearly and keep the exception as small as possible.

## Documentation

- Add concise XML documentation comments for every type, constructor, method,
  field, and property introduced or changed.
- Documentation should explain purpose, contracts, invariants, ownership,
  performance expectations, edge cases, or side effects; do not add comments that
  merely restate the implementation.
- Keep implementation comments rare. Use them only to explain non-obvious
  runtime constraints, native/API compatibility, resource lifetime rules, or
  algorithmic choices.

## Tests

- Add or update unit tests for every behavior change and every refactor that
  moves meaningful logic.
- Prefer focused tests in the matching project under `tests/AlvorKit.*.Test/`.
- Cover happy paths, edge cases, invalid input, conflict detection, and failure
  behavior when the code has explicit error handling.
- For hot-path work, add allocation or performance-sensitive tests when practical
  and avoid test designs that require real graphics hardware unless the task is
  explicitly integration-focused.
- Before finishing, run the most relevant `dotnet test` command. If the change
  crosses project boundaries, run the broader solution tests or explain why that
  was not possible.

## Final Review

- Re-read the changed files before finishing.
- Check that edited files respect the 150-line target, the repo-wide
  170-character code line limit, XML docs are meaningful, constructors are not
  redundant, hot paths avoid allocation churn, and tests cover the changed
  behavior.
- Report the exact tests run and any remaining risk.
