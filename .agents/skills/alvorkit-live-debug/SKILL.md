---
name: alvorkit-live-debug
description: Observe, diagnose, intervene in, and verify a running AlvorKit game with AlvorSense, LiveCode, predefined bridges, frozen inspection, and LivePatch. Use when Codex is visually testing an AlvorKit game and needs internal scoped state, when behavior should be changed experimentally without restarting, when a LiveCode or LivePatch session must be recorded and cleaned up, or when a stalled game loop needs frozen inspection.
---

# AlvorKit Live Debug

Use AlvorSense as the user-visible source of truth and LiveCode as the scoped
debugger. Keep one target alive while observing, inspecting, intervening, and
verifying.

## Required Reading

Read `../../../docs/AgentLiveDevelopment.md` completely before acting.

Read only the additional guide needed for the chosen surface:

- `../../../docs/AlvorSense.md` before driving or extending AlvorSense.
- `../../../docs/LiveCode.md` before arbitrary LiveCode, bridges, puppet
  commands, or frozen inspection.
- `../../../docs/LivePatch.md` before installing or replacing a method patch.
- `../../../docs/AlvorEye.md` only when the target is not wired for AlvorSense.

## Workflow

1. Start or identify the exact AlvorSense-driven game and observe it through
   normal input.
2. List LiveCode sessions and bind a new workspace to the exact advertised
   session and process identity.
3. Capture the scope graph and prefer numeric scope IDs for later commands.
4. Inspect before mutating. Prefer a predefined typed bridge when it already
   expresses the operation.
5. Write arbitrary LiveCode to a new numbered file beneath the workspace's
   `lc/` directory. Write LivePatch handlers beneath `lp/`. Never overwrite an
   executed submission.
6. Pass `--workspace` to every supported AlvorSense, LiveCode, bridge, puppet,
   frozen, and LivePatch command so exact inputs, results, and source hashes are
   recorded.
7. Return to AlvorSense after any intervention, reproduce the user-visible
   behavior, and capture evidence.
8. Remove patches and other persistent effects, prove their cleanup, stop only
   sessions started by the agent, and close the workspace.

AlvorSense pauses its deterministic game loop between batches. If a LiveCode
operation waits to enter the game thread, keep the original operation running,
send one workspace-recorded `update 0 0 0` through AlvorSense, and then collect
the original result. Do not submit a duplicate while the first operation is
pending.

Use frozen inspection only when the frame heartbeat is stalled. Resume or
restart the target before attempting visible verification.

## Communication

For an interactive showcase, state the exact next command or submission, its
expected effect, persistence, and cleanup, then wait for approval before each
meaningful mutation. After execution, show the exact input and output, explain
what changed, and propose the next step.

For a normal task that already authorizes an in-scope runtime change, do not ask
for redundant approval. Still disclose persistent effects and cleanup.

Never expose or persist a LiveCode capability token.
