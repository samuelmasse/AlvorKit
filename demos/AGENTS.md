# Demo Instructions

## Scope

These instructions apply to C# code under `demos/`. Demos are an override
layer, not `src/` code: optimize them as teaching surfaces and runnable
narrative walkthroughs, while still respecting game-runtime hot paths.

## Working Standard

- Review the requested demo and nearby collaborators before editing.
- In Working Mode, use demo builds, AlvorSense, or other proof tools only when
  useful for the specific change.
- Keep refactors focused on the touched demo project unless the task asks for a
  broader change.
- Preserve existing behavior unless a behavior change is requested.
- Resolve checked-in demo assets under `res/` through
  `AlvorKit.Script.Workspace`; `demos/Directory.Build.props` provides the shared
  reference and global using. Prefer `ProjectRoot.ResDirectory(...)` or
  `ProjectRoot.FindFromCurrentProcess(...)` over process-working-directory paths
  or local repository-root walkers.

## Naming

- Use `{ExactPackageName}.Demo[.{ScenarioOrVariant}]`; verify exact package
  names mechanically rather than guessing from dotted names.
- Preserve real dotted package names, such as
  `AlvorKit.Graphics2D.Fonts.Demo`. Put scenarios after `.Demo`, such as
  `AlvorKit.Graphics2D.Demo.Lines`.
- Keep `AlvorKit.Demo`. Use `AlvorKit.Engine.Demo.*` for engine scenarios, not
  `AlvorKit.Engine.Loop.Demo.*`.
- When renaming, update the folder, `.csproj`, solution entry, namespaces,
  titles, resource paths, and docs together.

## Visual Proof

- Use AlvorSense when visual proof is useful in Working Mode or required in
  Commit Mode and the demo creates its window with `AgentGlfwWindowHost` from
  `AlvorKit.Windowing.Agent`. Read `docs/AlvorSense.md` first.
- When using AlvorSense, share important screenshots in chat, summarize the
  input/update batches, and continue one live session when practical.
- Use AlvorEye only when the demo is not wired for AlvorSense or the task needs
  real desktop window behavior. Read `docs/AlvorEye.md` first.

## Demo Shape

- Structure each demo around a clear main path that reads like a step-by-step
  guide, usually in `Program.cs` or the first type reached from it.
- Prefer top-level statements in `Program.cs`. Do not introduce a static
  `Program` entry point unless interop or tooling requires it.
- Let the walkthrough show intent through clear names, ordered sections, and
  purposeful comments about sequencing, native/API relationships, lifetime,
  layout, or performance constraints.
- Keep incidental setup, platform glue, resource wrappers, repetitive
  initialization, and disposal plumbing out of the main narrative when it
  distracts from the lesson.
- Prefer explicit readable steps over clever compression. Extract only
  incidental code, not the concept being demonstrated.
- Treat demos as happy-path walkthroughs. Avoid defensive scaffolding and
  `try`/`catch` blocks unless the demo is about failure behavior or a native
  lifetime requires cleanup.
- Prefer direct cleanup when lifetime is local and obvious, such as destroying a
  window and terminating a backend at the end of `Program.cs`. Extract lifetime
  management only for a real reusable concept or a clearer demo.

## Runtime Style

- Separate initialization, resource lifetime, input, update, render, and cleanup
  so the game loop stays legible.
- Demo code is still game/runtime code. Keep render/update/input polling,
  per-frame helpers, tight benchmark loops, and other hot paths allocation-free
  unless the task explicitly accepts the cost.
- Watch for hidden allocations from LINQ, closures, iterator blocks, boxing,
  params arrays, string formatting, async state machines, and defensive copies.
- Prefer structs, readonly structs, spans, ref-friendly APIs, stack allocation,
  pooled or caller-owned buffers, and explicit ownership where they reduce hot
  path allocations without hiding the lesson.
- Allocate freely when clarity wins in startup, asset loading, configuration,
  diagnostics, error paths, teardown, and explicit load/unload operations.

## C# Style

- Prefer functional style where it improves clarity: pure helpers, immutable
  values, small transformations, explicit inputs and outputs, and minimal shared
  mutable state.
- Prefer primary constructors when they express the value or dependency shape;
  do not add boilerplate constructors or mirrored private state only to satisfy
  a source-code style rule.
- Prefer expression-bodied members for simple one-expression members when they
  read well. Use block bodies for meaningful control flow, comments, or side
  effects.
- Keep demo projects compact, but do not force `src/` one-type-per-file
  pressure onto demos.
- Keep demo-specific state, native handles, and lifetime sequencing on the demo
  instance. Reserve `static` for constants, pure value helpers, and genuinely
  stateless shared utilities.
- A `.cs` file may live directly at the root of its demo project when that is
  clearest. Use at most one level of subdirectories when extra structure helps.
- Multiple related types may share one `.cs` file when they support one demo
  concept and keeping them together improves readability.
- When a large cohesive demo is easier to follow as one demo class, partial
  source files are allowed; use that exception to preserve narrative, not to
  mimic production organization.

## File Size

- In Working Mode, treat 750 lines per demo source file as cleanup guidance, not
  a blocker.
- In Commit Mode, keep edited demo C# files at or below 750 lines when
  practical.
- Larger demo files are acceptable when they preserve a readable walkthrough and
  prevent needless file/type sprawl; call out the exception if it cannot
  reasonably be reduced.

## Documentation

- In Commit Mode, add concise XML documentation for public and private demo
  types and members introduced or changed, including a quick purpose summary for
  private helpers.
- For local functions under top-level statements, use a short leading comment
  with the same quick purpose summary.
- Prefer one useful `<summary>` over mechanical `<param>` comments. Add other
  XML tags only for important contracts.
- Documentation and walkthrough comments should explain purpose, ownership,
  sequencing, performance expectations, native/API compatibility, edge cases, or
  side effects; avoid restating implementation.

## Tests And Commit Mode

- Do not create tests or run coverage for demo-only code; demo projects have no
  unit tests by design.
- If logic becomes important enough for unit coverage, move it into `src/` or
  another non-demo project and test it there.
- In Commit Mode, build the touched demo project or explain why that was not
  possible, then report the exact command and any remaining risk.
- In Commit Mode, re-read changed files and check the 750-line target,
  repo-wide 170-character code line limit, meaningful docs, constructor clarity,
  and hot-path allocation discipline before handoff.
