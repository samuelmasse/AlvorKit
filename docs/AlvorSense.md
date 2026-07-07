# AlvorSense Agent Guide

AlvorSense is the repository's engine-native visual harness for AlvorKit games.
It runs a game through `AgentGlfwWindowHost` from `AlvorKit.Windowing.Agent`, keeps the process alive between
agent turns, and lets the agent drive exact update counts, exact delta times,
keyboard input, mouse input, text input, rendering, screenshots, and state
inspection without moving the user's real mouse or opening a visible game
window.

Use AlvorSense before AlvorEye when the target game is wired for it. A target is
wired for AlvorSense when its normal startup path creates an `AgentGlfwWindowHost`
and the process can be run with `ALVORKIT_WINDOWING_AGENT=1`. Use AlvorEye for
visual desktop targets that are not AlvorSense-aware, such as arbitrary native
windows, external applications, or demos that still require real desktop input.

## Tool Location

Project:

```text
scripts/AlvorKit.Script.AlvorSense/AlvorKit.Script.AlvorSense.csproj
```

Run commands:

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- start --id <id> --project <project.csproj>
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id <id>
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- list
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- status --id <id>
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- stop --id <id>
```

Session artifacts live under:

```text
out/alvorsense-sessions/<id>/
```

Each session directory contains `session.json`, `ready.json`, `stdout.log`,
`stderr.log`, `requests/`, `responses/`, and any screenshots or result files you
choose to place there.

## Mental Model

AlvorSense has two layers:

- `AlvorKit.Windowing.Agent` is the automation-aware window host package.
- `scripts/AlvorKit.Script.AlvorSense` is the persistent agent-facing CLI that
  keeps a game process alive between turns.

The game owns time. When the agent sends `updates 50 0.001`, the game receives
exactly fifty update calls with exactly `0.001` seconds each. Nothing advances
while the agent is reading output, inspecting screenshots, or deciding what to
do next.

Rendering is also explicit. Use `render` when a frame should be drawn, and
`screenshot <path.png>` when the agent needs visual evidence. The hidden render
surface is backed by the GLFW window handed to `AgentGlfwWindowHost`, so the
game can still use its normal OpenGL layer setup.

## Wiring A Game

Games should keep their normal creation path explicit. To make a game
AlvorSense-aware, create the GLFW API object/window yourself, then hand both to
`AgentGlfwWindowHost`:

```csharp
var glfw = new GlfwBackend();
if (!glfw.Init())
    throw new InvalidOperationException("Failed to initialize GLFW.");

glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
glfw.WindowHint(GlfwWindowHint.Visible, false);
var window = glfw.CreateWindow(900, 640, "Demo game", default, default);
if (window == default)
    throw new InvalidOperationException("Failed to create the GLFW window.");

glfw.MakeContextCurrent(window);
var gl = new GlLayer(new GlBackend(glfw.GetProcAddress));
using var host = new AgentGlfwWindowHost(glfw, window, gl);
using var loop = new WindowLoop(host);
var screen = new WindowScreen(loop);

screen.IsVisible = true;
loop.Update += Update;
loop.Render += Render;
loop.Run();
```

When `ALVORKIT_WINDOWING_AGENT` is absent, this runs as a normal GLFW window.
When the variable is present, the same game runs in agent mode and reads command
lines from standard input. Most consumers should keep GLFW creation shared and
switch only the host wrapper, not add demo-specific agent code.

## Start A Session

Start launches the target project, sets `ALVORKIT_WINDOWING_AGENT=1` and
`ALVORKIT_AUDIO_SILENT=1` for the target process, waits until the game prints
its generated command usage, and then writes JSON containing the session id,
session directory, ready flag, and host process id.

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- start --id demo `
    --project demos\AlvorKit.Script.AlvorEye.Demo\AlvorKit.Script.AlvorEye.Demo.csproj `
    --env ALVOREYE_DEMO_RESULT_PATH=out\alvorsense-sessions\demo\result.json
```

Prefer passing `--id` even for quick exploration. A stable id makes follow-up
`send`, `stop`, screenshot paths, result paths, and chat handoffs obvious. If
you omit `--id`, run `list` and use the newest returned id for later commands.

Useful options:

- `--id <id>` chooses a stable session id.
- `--project <project.csproj>` selects the target game project.
- `--workdir <dir>` sets the target working directory. The default is the
  current directory.
- `--env NAME=VALUE` passes an extra environment variable to the target. Repeat
  it for multiple variables.
- `--timeout <seconds>` controls startup, send, or stop waits. The default is
  30 seconds.

AlvorSense silences AlvorKit miniaudio output by default so agent-driven game
runs do not play through the user's speakers. To intentionally hear a target
during an audio debugging run, pass `--env ALVORKIT_AUDIO_SILENT=0` on `start`.

For high-DPI visual matching, pass
`--env ALVORKIT_WINDOWING_AGENT_MONITOR_SCALE=2` or another positive scale.
This changes the simulated monitor scale reported by `RootScreen.MonitorScale`
while preserving the screenshot's physical client size.

## Send Commands

`send` writes a batch of command lines to the running game and then appends
`state` automatically unless the batch exits the target. The response is JSON.
Commands can come from standard input, `--command <line>`, `--file <path>`, the
legacy `--commands <path>` file option, or a single trailing command line after
the options.
The important fields are:

- `ok`: whether the expected state line or exit was observed.
- `commandCount`: number of command lines accepted from the request.
- `stateLine`: latest captured state line for the request, when available.
- `outputLines`: target stdout lines produced by this batch.
- `processExited`: whether the game has exited.
- `exitCode`: target exit code when available.
- `error`: failure text when the batch timed out or the host failed.

Example:

```powershell
@'
render
screenshot out\alvorsense-sessions\demo\00-start.png
'@ | dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id demo
```

If you want to send one command from PowerShell, pipe it too:

```powershell
'render' | dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id demo
```

Or pass the one-liner directly:

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id demo render
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id demo --command "update 0.016"
```

AlvorSense ignores blank command lines and lines starting with `#`, so command
files can include short notes.

Malformed target commands do not terminate the target game loop. The target
writes an `ALVORSENSE_COMMAND_ERROR ...` line, and the foreground JSON response
sets `ok` to `false` with the same message in `error`.

## List And Status

Use `list` when you need to recover a generated id or see recent sessions:

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- list
```

Use `status` when you know the id and want an agent-readable session summary:

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- status --id demo
```

Both commands read only persisted session files. They do not send input to the
target game.

## Command Reference

Common commands:

- `help` or `--help`: print generated command usage.
- `update <dt>`: run one update with exact delta seconds.
- `update <dt> <mouseDx> <mouseDy>`: pan the mouse, then run one update.
- `updates <count> <dt>`: run many exact updates.
- `updates <count> <dt> <mouseDx> <mouseDy>`: pan the mouse every update.
- `step <dt>`: run one update and one render.
- `render`: draw one frame.
- `render <dt>`: render with an explicit render delta.
- `screenshot <path.png>`: render and save a PNG.
- `state`: print simulated time, update count, render count, and mouse position.
- `input-state`: print focus, mouse position, held keys, held mouse buttons, and pending text.
- `quit` or `exit`: exit the agent command loop.

Input commands:

- `key <name> down`: press a key.
- `key <name> repeat`: inject a key repeat.
- `key <name> up`: release a key.
- `mouse <button> down`: press a mouse button.
- `mouse <button> up`: release a mouse button.
- `move <x> <y>`: set absolute mouse position in client coordinates.
- `pan <dx> <dy>`: move the mouse relative to its current position.
- `wheel <dx> <dy>`: inject mouse wheel movement.
- `text <value>`: inject text input.
- `clipboard <value>`: set clipboard text exposed by the window host.

Use AlvorKit-owned enum names for keys and mouse buttons, such as `D`, `W`,
`Space`, `Left`, and `Right`. Key and mouse button names are case-insensitive,
so `key D down` and `key d down` both press `Keys.D`.

## Stop A Session

Stop sends `quit`, waits for the target to exit, and returns any final output.
Use it when the session is finished so the process releases files and writes any
normal shutdown result.

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- stop --id demo
```

If the game writes a structured result on normal exit, set that result path with
`--env` at session start and inspect it after `stop`.

## Observe, Think, Act

AlvorSense is interactive. Prefer one long-lived game session over repeatedly
restarting the target. Start once, observe, send a batch, inspect the result,
and continue from the same session until the task is solved or the game reaches
an unrecoverable state.

Keep the chat user oriented while using the harness:

- Share important screenshots in chat with Markdown image links so the user can
  see what the agent sees.
- Briefly explain the meaningful input/update batches being sent, such as
  "holding D for 15 updates, then W for 109 updates" or "clicking the yellow
  button and dragging the slider right."
- Do not paste every command line when it would be noisy; summarize key actions,
  exact update counts, screenshot names, and the observed result.
- Mention when a run is still the same live session and when a restart is truly
  necessary.

Recommended loop:

1. `render` and `screenshot` the initial frame.
2. Share the screenshot in chat and describe the visible state.
3. Send a focused input or update batch.
4. Capture again.
5. Share the new screenshot when it changes the decision or proves progress.
6. Continue from the observed state in the same session.
7. Stop only after a final screenshot proves success.

Example movement batch:

```powershell
@'
key D down
input-state
updates 15 0.016
key D up
render
screenshot out\alvorsense-sessions\demo\01-moved.png
'@ | dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id demo
```

Example mouse batch:

```powershell
@'
move 750 160
mouse Left down
update 0.016
mouse Left up
update 0.016
render
screenshot out\alvorsense-sessions\demo\02-clicked.png
'@ | dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id demo
```

The `update` command always needs a delta time. For one-frame clicks, drags, and
button releases, use a small explicit delta such as `update 0.016`; do not send
bare `update`.

Example drag batch:

```powershell
@'
move 690 307
mouse Left down
updates 20 0.016 10 0
mouse Left up
update 0.016
render
screenshot out\alvorsense-sessions\demo\03-dragged.png
'@ | dotnet run --project scripts\AlvorKit.Script.AlvorSense -- send --id demo
```

## AlvorSense Versus AlvorEye

Prefer AlvorSense when:

- The target is an AlvorKit game using `AgentGlfwWindowHost`.
- You need exact update counts and exact delta times.
- You want screenshots without moving the real cursor.
- You want the game paused naturally while the agent thinks.
- You want deterministic input and time owned by the windowing layer.

Use AlvorEye when:

- The target is not wired with `AgentGlfwWindowHost`.
- The task is about a real desktop window, external app, or OS-level workflow.
- You need to verify real window placement, focus, or desktop input behavior.
- The game cannot render through the hidden GLFW context owned by the agent host.

Do not use AlvorSense as a replacement for unit tests, lint, coverage, or
non-visual command-line verification. It is for visual and interactive behavior
where seeing the frame matters.

## Practical Gotchas

- A session is not useful until `start` reports readiness. If `send` times out,
  read the session `stderr.log` first.
- `send --id <id> render` is accepted as a one-line command. For multi-line
  batches, prefer stdin, repeated `--command`, or `--file`.
- If a screenshot command succeeds but the file is missing, first confirm the
  response `ok` value and command output, then prefer an explicit path under
  `out/`.
- If a command is malformed, the response should contain
  `ALVORSENSE_COMMAND_ERROR` in `outputLines`, set `ok` to `false`, and keep the
  target process alive.
- Put screenshots and result files under `out/alvorsense-sessions/<id>/` or
  another ignored `out/` directory.
- Use `updates` for held movement and `update` for one-frame mouse clicks.
- If held movement seems wrong, send `input-state` after the `key down` command
  to verify focus and the currently held key before changing route timing.
- Capture after each meaningful input group; stdout state is useful, but visual
  proof should come from screenshots.
- If a target exits unexpectedly, inspect the JSON response, `stdout.log`, and
  `stderr.log`.
- Always `stop` sessions you start. Check for leftover processes if a smoke run
  is interrupted.

## Demo Practice Target

The AlvorEye demo has been wired through `AgentGlfwWindowHost`, so it can be
solved with AlvorSense:

```text
demos/AlvorKit.Script.AlvorEye.Demo/
```

Read `AGENT_GOAL.md` as the puzzle instructions and use `SOLUTION.md` only as a
reference after solving or when validating the harness itself.
