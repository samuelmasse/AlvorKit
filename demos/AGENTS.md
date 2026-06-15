# Demo Instructions

## Scope

These instructions apply to C# code under `demos/`.

## Working Standard

- Review the requested demo code and nearby collaborators before editing.
- Treat refactoring as part of normal demo work, but keep it focused on the
  touched demo project unless a broader refactor is explicitly requested.
- Preserve existing behavior unless the task asks for a behavior change.
- Treat demo code as a teaching surface. A person browsing the repository should
  be able to understand what the demo proves and follow the important steps
  without hunting through incidental plumbing.
- Keep changes cohesive, reviewable, and easy to understand for someone learning
  from the demo.

## Demo Narrative

- Structure each demo around a clear main path that reads like a step-by-step
  guide. This often belongs in `Program.cs`, or in the first type reached from
  `Program.cs`.
- Prefer top-level statements in `Program.cs` for executable demos. `Program.cs`
  should use its space to show the primary walkthrough, not just forward to a
  static app or runner type.
- Do not introduce a `Program` static class entry point for demos unless the
  demo has a specific interop or tooling requirement that needs it.
- Let the main path explain the demo's intent through clear names, ordered
  sections, and purposeful comments where they help the reader understand the
  sequence.
- Comment the main walkthrough path enough to explain intent, sequencing,
  native/API relationships, and important lifetime or layout constraints. Keep
  implementation comments out of utility helpers, formatting functions, guards,
  and other incidental plumbing unless a non-obvious constraint would otherwise
  be hidden there.
- Keep setup, platform glue, resource wrappers, repetitive initialization,
  disposal plumbing, and other boilerplate out of the main narrative when it
  distracts from the concept being demonstrated.
- Push boilerplate into local functions, private methods, or small helper types
  only when doing so makes the main narrative easier to read.
- Prefer explicit, readable steps over clever compression in demo-facing code,
  especially when the code is meant to show how to use an API.
- Treat demos as happy-path walkthroughs. Avoid `try`/`catch` blocks and
  defensive error-handling scaffolding unless the demo is specifically about
  failure behavior or a native resource lifetime requires cleanup.
- Do not introduce wrapper types, disposable lifetime helpers, or abstractions
  solely to avoid `try`/`finally`, satisfy a style rule, or hide simple cleanup.
- Prefer direct, readable cleanup in the demo path when the lifetime is local and
  obvious, such as destroying a window and terminating a backend at the end of
  `Program.cs`.
- Extract lifetime management only when the helper represents a real reusable
  concept or substantially clarifies the demo, not when it wraps a one-line
  `Dispose` call.
- Do not hide the essential idea behind abstraction. Extract only the code that
  is incidental to the lesson, not the lesson itself.

## Game-Code Performance

- Demo code is still video game/runtime code. Design hot-path code as if it must
  stay viable at roughly 5,000 FPS.
- Avoid GC churn in game-loop paths, render/update paths, input polling, tight
  benchmark loops, and per-frame helper methods.
- Prefer structs, readonly structs, spans, ref-friendly APIs, stack allocation,
  pooled or caller-owned buffers, and explicit ownership where they reduce
  allocations without making the demo harder to follow.
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
- Separate initialization, resource lifetime, input, update, render, and cleanup
  logic so the game loop stays obvious.
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
- Keep demo projects intentionally compact. Prefer as few files and as few types
  as practical while preserving a clear step-by-step explanation.
- A `.cs` file may live directly at the root of its demo project when that is
  the clearest home. Subdirectories are optional organization, not a requirement.
- Multiple related types can live in the same `.cs` file when they support one
  demo concept and keeping them together improves readability.
- Create new files or one-level subdirectories only when a single file becomes
  harder to navigate than the extra project structure.
- Keep demo project folders shallow: use at most one level of subdirectories
  beneath each demo project, and avoid nesting those groups further.

## File Size

- Keep each edited C# file at or below 750 lines.
- Larger demo files are acceptable when they preserve a readable, step-by-step
  narrative and prevent needless file/type sprawl.
- When a touched file is already over 750 lines, split out cohesive helpers or
  data shapes before adding more code.
- If a file cannot reasonably be kept under 750 lines because of demo flow,
  platform glue, or tightly coupled declarations, call that out clearly and keep
  the exception as small as possible.

## Documentation

- Add concise XML documentation comments for every type, constructor, method,
  field, and property introduced or changed.
- Every demo method should have at least a quick XML `<summary>`, including
  private helpers and formatting methods. These summaries may be brief, but they
  should say what role the method plays in the demo.
- For local functions under top-level statements, use a short leading comment
  that gives the same quick purpose summary.
- For methods and constructors, write one useful `<summary>` comment instead of
  separate `<param>` comments for each parameter. Add other XML tags only when
  they explain an important contract that the summary cannot express clearly.
- Documentation should explain purpose, contracts, ownership, performance
  expectations, edge cases, or side effects; do not add useless comments or
  comments that merely restate the implementation.
- Keep implementation comments rare. Use them only to explain non-obvious
  runtime constraints, native/API compatibility, resource lifetime rules, or
  algorithmic choices.

## Tests

- Do not create tests for demos or demo-only code.
- If logic moves out of a demo and receives tests elsewhere, those test files
  use the repo-wide 750-line test limit, not the normal source or demo file-size
  limits.
- Prefer moving testable rules into small helpers instead of testing through a
  live graphics window.
- If logic becomes important enough to need unit coverage, move it into `src/`
  or another non-demo project and test it there.
- Avoid test designs that require real graphics hardware unless the task is
  explicitly integration-focused.
- Before finishing, build the touched demo project or explain why that was not
  possible.

## Final Review

- Re-read the changed files before finishing.
- Check that edited files respect the 750-line target, the repo-wide
  170-character code line limit, XML docs are meaningful, constructors are not
  redundant, and hot paths avoid allocation churn.
- Report the exact build or verification command run and any remaining risk.
