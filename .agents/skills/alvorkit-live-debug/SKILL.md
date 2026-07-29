---
name: alvorkit-live-debug
description: Observe, diagnose, update, and verify a running AlvorKit game with AlvorSense, LiveCode, predefined bridges, frozen inspection, and Source Update.
---

# AlvorKit Live Debug

Use this skill when work depends on the behavior or internal state of a running
AlvorKit game, or when an original C# method should be edited without restarting
the process.

## Required reading

Before acting, read:

- `docs/AgentLiveDevelopment.md`
- `docs/AlvorSense.md` before visual control
- `docs/LiveCode.md` before scoped execution or bridge work

## Choose the narrowest mechanism

- Use AlvorSense for deterministic input, updates, rendering, screenshots, and
  user-visible proof.
- Use a predefined LiveCode bridge for a stable, allowlisted operation.
- Use ordinary LiveCode for exact-scope inspection or a short-lived diagnostic
  command.
- Use frozen inspection only after the game-frame heartbeat is stale.
- Use Source Update when the intended effect is a normal edit to an existing
  method in its original `.cs` file.

Source Update changes the original method definition. Private access and
captured constructor parameters remain ordinary compiled IL. Do not create
handler classes, redeclare fields, use reflection to bypass visibility, or
simulate an original-file edit through dispatch.

## Source Update workflow

1. Start the target through AlvorSense `--editable-project`.
2. Capture the initial visible state.
3. Create a live workspace bound to the exact AlvorSense and LiveCode sessions.
4. Start the workspace Source Update coordinator.
5. Edit the real project `.cs` file with a normal diff.
6. Submit the file and unified diff with `source apply`.
7. Advance at least one safe frame through AlvorSense.
8. Require `source status` to report `applied`.
9. Capture the visible result in the same live session.

Version 1 supports one existing ordinary method-body change per generation. If
the edit changes declarations, signatures, fields, constructor captures,
attributes, async/iterator shape, unsafe code, generics, lambdas, local
functions, or other metadata shape, stop and use a restart/rebuild workflow.

## Workspace discipline

Agent-authored files belong only beneath:

```text
tmp/live/<workspace-id>/lc/
tmp/live/<workspace-id>/bridge/
tmp/live/<workspace-id>/puppet/
tmp/live/<workspace-id>/source/diffs/
```

Do not put disposable submissions in production or demo source directories.
The exception is the intentional original `.cs` edit itself.

Never record a LiveCode capability token in workspace files, chat, logs, or
documentation.

## Verification and cleanup

AlvorSense is the visible source of truth. Share important before/after
screenshots and summarize meaningful update/input batches.

Before finishing:

- wait for every queued source operation to become terminal;
- treat ambiguous transport or runtime results as restart-required;
- stop the idle coordinator;
- restart or stop the target after any applied generation;
- resolve persistent workspace interventions only after cleanup is proved;
- stop agent-started AlvorSense sessions; and
- leave user-owned sessions running unless the user asks otherwise.

Report the original source path, immutable diff path, runtime generation,
terminal evidence path, and visual proof.
