# AlvorEye Agent Guide

AlvorEye is the repository's agent-facing visual automation tool. It lets agents
launch or attach to desktop windows, move them into view, capture full-window
frames, send keyboard and mouse input, freeze the target for handoff, resume it,
and write artifacts that can be inspected after the run.

Use it when a task asks an agent to inspect, verify, play, or debug a visual
desktop target. Typical examples are OpenGL demos, game prototypes, animation
checks, visual UI regressions, and workflows where screenshots are the evidence
that something actually happened.

If the target game is wired through `AgentGlfwWindowHost`, prefer
AlvorSense instead. Read `docs/AlvorSense.md` and use
`scripts/AlvorKit.Script.AlvorSense` for exact-time engine-native input,
hidden rendering, and screenshots without driving the real desktop.

Do not use AlvorEye as a replacement for normal unit tests, static analysis, or
non-visual command-line verification. It is best when the important facts live
on screen.

## Tool Location

Project:

```text
scripts/AlvorKit.Script.AlvorEye/AlvorKit.Script.AlvorEye.csproj
```

Run help:

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorEye -- help
```

Current primary platform:

- Windows-first implementation.
- Uses Win32 window discovery, placement, full-window capture, `SendInput`, and
  thread suspension for handoff.
- macOS and Linux are intended future adapter targets, but v1 behavior is
  Windows-oriented.

## When Agents Should Use It

Use AlvorEye when the visual target is not wired for AlvorSense and:

- A user asks whether a desktop game or demo visually works.
- A rendering change needs screenshot evidence.
- A demo needs keyboard or mouse input before the useful state is visible.
- A scene changes over time and must be captured after waits or inputs.
- An agent needs to pause a running game, inspect the frame, think, and continue.
- A result should be supported by artifacts under `out/alvoreye`.

Prefer ordinary build/test/lint commands when the output is not visual.

## Mental Model

AlvorEye has two main modes:

- `run --scenario <file>` executes a complete JSON scenario and then stops the
  target process it launched.
- `session --scenario <file>` launches or attaches once, then reads one JSONL
  command or command batch at a time from stdin.

Additional commands:

- `handoff --session <id>` freezes a known session target and captures it.
- `resume --session <id>` resumes a known session target.
- `help` prints the supported CLI syntax.

Every run should create artifacts:

- `manifest.json` with actions and frame paths.
- `logs/stdout.log` and `logs/stderr.log` for the target process.
- `frames/*.png` captures.
- Optional `*.analysis.json` sidecars from `analyzeBasic`.

## Basic Scenario Shape

A scenario is JSON with these sections:

```json
{
  "run": {
    "executable": "dotnet",
    "args": ["run", "--project", "demos\\Some.Demo\\Some.Demo.csproj"],
    "workingDirectory": null,
    "environment": {
      "OPTIONAL_RESULT_PATH": "out\\some-run\\result.json"
    }
  },
  "window": {
    "title": "Window title",
    "exact": true,
    "timeoutSeconds": 30,
    "width": 960,
    "height": 720
  },
  "output": {
    "runId": "case-name",
    "directory": "out\\alvoreye\\case-name"
  },
  "timeline": [
    { "action": "wait", "seconds": 1 },
    { "action": "capture", "name": "start" }
  ]
}
```

The `run` section may be omitted when attaching to an already-running window,
but most automated checks should launch the target so logs and cleanup are
predictable.

When target arguments contain repo-relative paths, be deliberate about
`workingDirectory`. For one-off agent scenarios, prefer omitting it when running
AlvorEye from the repo root, or set it to an absolute repository path. A relative
`workingDirectory` can make otherwise valid target paths resolve somewhere
unexpected. If window discovery times out, inspect the target `stderr.log`
before adjusting the window title.

The `output.directory` should normally be under `out/`, because those artifacts
are temporary evidence, not source files.

## Human Input Coordination

AlvorEye sends keyboard and mouse input to the active desktop. While an AlvorEye
scenario is driving a target, the human operator should keep hands off the
keyboard and mouse unless the agent has paused or said input control is over.

Before starting any scenario or session action batch that sends mouse or
keyboard input, tell the user clearly:

```text
Hands off keyboard and mouse now: I am about to drive the target for about N seconds.
I will tell you when it is safe to resume.
```

When the input burst, scenario, or handoff-safe pause is complete, say:

```text
Safe to use keyboard and mouse again.
```

For long exploratory sessions, prefer short input bursts with captures between
them. This gives the human natural windows to use the machine and makes it
easier to recover if focus or timing is wrong.

## Timeline Actions

Supported action kinds:

- `wait`: wait for `seconds` or `milliseconds`.
- `capture`: capture the full target window to `frames/`.
- `key`: press and release a key.
- `keyDown`: hold a key down.
- `keyUp`: release a held key.
- `text`: send text input through Unicode keyboard events.
- `mouseMove`: move the cursor to a window-relative point.
- `mouseClick`: click at a window-relative point.
- `mouseDrag`: press, wait, move, and release.
- `handoff`: capture and freeze the target process.
- `resume`: unfreeze the target process.
- `analyzeBasic`: write simple image analysis for the last captured frame.

Example held movement:

```json
[
  { "action": "keyDown", "key": "D" },
  { "action": "wait", "milliseconds": 700 },
  { "action": "keyUp", "key": "D" },
  { "action": "capture", "name": "after-move" }
]
```

Use held keys for games. A single `key` action may be too quick for a game loop
that polls input once per frame.

Before sending keyboard input, make sure the target window is focused. A harmless
click in the content area, followed by a short wait and capture, is often enough.
If held keys produce no visible change, suspect focus before assuming the timing
or controls are wrong.

Treat movement timing as something to measure visually. A useful calibration is
to hold one direction for a known duration, capture the result, then use the
visible displacement to choose shorter follow-up holds. For multi-step routes,
capture after each segment until the input timing is reliable.

Example mouse click:

```json
{ "action": "mouseClick", "x": 750, "y": 180, "button": "left" }
```

Coordinates are relative to the captured window rectangle, including the title
bar and border on Windows. If the game or UI reports client coordinates, expect
some vertical offset. The safest workflow is to capture, inspect, click a
generous target, then capture again.

Mouse drags are also empirical. Start near the center of the visible handle or
thumb, drag past the intended endpoint when the UI permits it, and capture after
the drag so the handle position and any related state change are visible.

Example text input:

```json
{ "action": "text", "text": "EYE" }
```

Use `text` when the target has a character-input path. Use `key` or held
`keyDown`/`keyUp` when the target polls physical keys.

## Handoff and Resume

`handoff` is for agent thinking time. It captures the live frame, freezes the
target process, optionally captures again after freeze, and returns control.

Example:

```json
[
  { "action": "capture", "name": "before-handoff" },
  { "action": "handoff", "name": "handoff" },
  { "action": "resume" }
]
```

Use handoff when:

- A game is moving and the agent needs a stable frame.
- A puzzle or UI state needs visual analysis before choosing the next inputs.
- You want proof that freeze/resume works for a target.

Avoid leaving targets frozen for longer than needed. Some games or frameworks
may not like process suspension. If a target has its own pause control, a future
scenario strategy may prefer that, but v1 defaults to process suspension.

## Session JSONL Mode

Use `session --scenario <file>` when an agent wants to observe, decide, and then
send more instructions without relaunching the target.

Each stdin line is either one action:

```json
{"action":"capture","name":"look"}
```

or a batch:

```json
{"actions":[{"action":"keyDown","key":"D"},{"action":"wait","milliseconds":500},{"action":"keyUp","key":"D"}]}
```

AlvorEye emits structured result lines with status, frozen state, queue count,
and last frame path. While frozen, active input actions are queued until
`resume`, but capture and analysis actions can still run.

## Visual Verification Patterns

A good AlvorEye run should not only perform inputs. It should also produce
evidence.

Useful pattern:

1. Wait for the target to initialize.
2. Capture `start`.
3. If keyboard input will be used, focus the target with a harmless click.
4. Apply one small input group.
5. Capture after that group.
6. Compare frames or inspect the capture.
7. Continue only when the visible state matches the plan.
8. Capture the final success state.
9. If the target can dump a final result, exit it normally and compare that
   structured result to the visual evidence.

For unfamiliar interactive targets, split the work into observation,
calibration, and final proof runs. Observation runs map the visible regions,
calibration runs measure input effects, and the final run records the shortest
clear evidence trail for the successful path.

When reporting a visual success, mention both:

- the frame path that shows the state, and
- any structured result or log line that confirms the state.

## Basic Image Analysis

`analyzeBasic` operates on the last captured frame and writes a JSON sidecar.

It can report:

- `nonBlank`
- `changedPixels`
- `colorHits`
- bounds of changed pixels

Example:

```json
[
  { "action": "capture", "name": "win" },
  { "action": "analyzeBasic", "color": "#2ee85c" }
]
```

Treat image analysis as a quick sanity check, not a substitute for inspecting
the actual frame when a human or agent judgment matters.

## Demo Game

The AlvorEye practice target lives here:

```text
demos/AlvorKit.Script.AlvorEye.Demo/
```

Files:

- `AGENT_GOAL.md`: the only instructions a solving agent should read first.
- `SOLUTION.md`: a documented solved run with exact commands and scenario JSON.

The demo intentionally requires:

- visual capture,
- held keyboard movement,
- mouse click,
- mouse drag,
- text input,
- handoff and resume,
- final visual verification,
- final result dump after normal exit.

The result dump prints to stdout as:

```text
ALVOREYE_DEMO_RESULT { ...json... }
```

When `ALVOREYE_DEMO_RESULT_PATH` is set, the same JSON is written to that path.
Agents should not rely on the dump before visually solving the game. In the
demo, four green progress lights mean the locks are done, but the player still
must enter the pale exit tile. The correct final confidence check is both the
bright green win rectangle in the final frame and `Won:true` in the result JSON.

## Practical Gotchas

- Full-window coordinates include the title bar and borders on Windows.
- Game loops often miss very short key presses; prefer `keyDown`, `wait`,
  `keyUp`.
- If the window title bar appears inactive or held keys do nothing, click a
  harmless point in the target content and capture again before changing the
  route.
- Mouse clicks can be too quick for polling-only games; target apps should use
  event callbacks for click-like actions when possible.
- Use screenshots as the decision source while solving. Treat stdout logs and
  result files as launch diagnostics or post-solve audits unless the task says
  otherwise.
- Always wait a second or two before the first capture if the target is launching
  a graphics context.
- Do not close a target before capturing the final success state.
- In `run` mode, AlvorEye stops an owned target after the timeline. If the target
  needs to write a result on normal exit, include an input sequence that exits it
  normally before the timeline ends.
- Keep smoke scenarios and captures under `out/`.
- If a run fails visually, inspect both `manifest.json` and frame PNGs before
  changing code.

## Minimal Smoke Checklist

For a visual demo or game:

```powershell
dotnet build demos\Some.Demo\Some.Demo.csproj
dotnet run --project scripts\AlvorKit.Script.AlvorEye -- run --scenario out\some-smoke\scenario.json
```

Then inspect:

```powershell
Get-Content out\some-smoke\run\manifest.json
Get-Content out\some-smoke\run\logs\stdout.log
Get-ChildItem out\some-smoke\run\frames
```

Open the final frame and confirm the screen says what the task requires.

## Extending AlvorEye

When changing AlvorEye itself, follow the script project rules under
`scripts/AGENTS.md`:

- keep parsing and planning testable,
- add focused tests under `tests/AlvorKit.Script.AlvorEye.Test`,
- run the relevant `dotnet test`,
- run scoped lint,
- use coverage for touched script source,
- keep native/windowing behavior behind the platform adapter boundary.

When changing only a visual demo that uses AlvorEye, follow the demo rules under
`demos/AGENTS.md`: build the demo and run scoped lint; coverage is not required
for demo-only code.
