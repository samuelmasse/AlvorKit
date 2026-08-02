# Source Instructions

## Scope

These instructions apply only to descendants of `src/`. Matching tests are
governed by `tests/AGENTS.md`; read both scoped files when a task changes both
trees.

This file is a delta over the root instructions. Before changing C#, read
[`../docs/AgentRules/CSharp.md`](../docs/AgentRules/CSharp.md). Before changing
runtime, game-loop, resource, native-boundary, allocation, disposal, or teardown
code, also read
[`../docs/AgentRules/RuntimePerformance.md`](../docs/AgentRules/RuntimePerformance.md).

## Runtime And Game Code

- Treat low-level runtime and game-loop code as if it must remain viable at
  roughly 5,000 FPS.
- Keep game-loop, render/update, tight polling, per-frame helper, resource
  lifetime, validation, bind/unbind, delete/dispose, scope teardown, and other
  hot-path code allocation-free unless the task explicitly accepts the cost.
- Watch for hidden allocations from arrays, `List<T>`, LINQ, closures, iterator
  blocks, boxing, `params` arrays, string formatting, async state machines, and
  defensive copies.
- Prefer structs, readonly structs, spans, ref-friendly APIs, `stackalloc`,
  pooled or caller-owned buffers, and explicit ownership where they reduce
  allocation pressure without obscuring the code.
- Allocation is acceptable during startup, asset loading, configuration,
  diagnostics, and explicit cold loading. Genuinely cold final process shutdown
  may allocate when intentional; reusable unload and teardown APIs remain
  allocation-sensitive.
- For GL object lifetime, follow [../docs/GlOwnership.md](../docs/GlOwnership.md):
  objects belong to the `GlLayer` node that created them, and scope-lifetime
  objects get no `IDisposable` or `Delete*` teardown of their own — disposing
  the scope's node reclaims everything it owns.
- Do not over-optimize cold code; reserve low-level techniques for real
  frame-time, allocation, ownership, or native-boundary pressure.

## Source Shape

- Separate state tracking, validation, platform/API calls, and resource lifetime
  logic so each responsibility can be tested directly.
- In game and runtime source, do not use auto-properties for object-owned
  runtime state. Auto accessors are banned in hand-authored classes and
  non-record structs; records are the sole hand-authored exception. Use
  explicit private fields so owned state is visible in one place.
- Immutable handles and dependencies should be private readonly fields. Expose
  state through the narrowest useful property when the API needs it; keep any
  backing field private even when the property returns it by `ref` or
  `ref readonly`.
- Follow the shared C# policy's member-category and accessibility order in every
  class and struct.
- Keep source project folders shallow: use at most one level of subdirectories
  beneath each source project unless an existing project already has a stronger
  local convention.

## Commit Mode Overrides

- Keep each edited source C# file at or below 250 lines. This target does not
  apply to matching test files, which follow the root test-size policy.
- If a touched source file is already over 250 lines, split out cohesive helpers
  or data shapes before adding more code. If no clean split exists because of
  API shape, platform glue, or tightly coupled declarations, call out the small
  exception in the handoff.
- Add concise XML documentation comments for every type, constructor, method,
  field, and property introduced or changed; explain contracts, invariants,
  ownership, performance expectations, edge cases, or side effects.
- Add or update focused tests in the matching `tests/AlvorKit.*.Test/` project
  for behavior changes and refactors that move meaningful logic.
- Cover happy paths, edge cases, invalid input, conflict detection, and failure
  behavior when the changed code has explicit error handling.
- For hot-path work, add allocation or performance-sensitive tests when
  practical.
- Avoid test designs that require real graphics hardware unless the task is
  explicitly integration-focused.
- In final review, re-read changed files and check the 250-line source target,
  meaningful XML docs, hot-path allocation discipline, and focused test coverage.
