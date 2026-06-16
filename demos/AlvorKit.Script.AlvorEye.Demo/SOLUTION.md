# AlvorEye Demo Solution

This is the exact approach I used to beat the `AlvorEye demo game` window with AlvorEye.

The important lesson: four green progress lights are not enough by themselves. The player also has to move into the pale exit tile. My earlier shorter final movement stopped just left of the exit and dumped `Won:false`; the final working run held `D` longer before exiting.

## Commands

Build the demo:

```powershell
dotnet build demos\AlvorKit.Script.AlvorEye.Demo\AlvorKit.Script.AlvorEye.Demo.csproj
```

Run AlvorEye with a scenario:

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorEye -- run --scenario out\alvoreye-demo-results-smoke\scenario.json
```

Check the dumped result:

```powershell
Get-Content out\alvoreye-demo-results-smoke\result.json
```

The successful dumped result included:

```json
{
  "Won": true,
  "HasKey": true,
  "ButtonPressed": true,
  "SliderComplete": true,
  "CodeComplete": true,
  "AllLocksComplete": true,
  "ProgressLightsGreen": 4
}
```

The visual proof frame was:

```text
out\alvoreye-demo-results-smoke\run\frames\frame-004-win.png
```

It showed all four progress lights green, the player inside the pale exit tile, and the bright green win rectangle in the play field.

## Scenario

This was the working scenario. Coordinates are window-relative as AlvorEye sees the full captured window, including the title bar.

```json
{
  "run": {
    "executable": "dotnet",
    "args": [
      "run",
      "--project",
      "demos\\AlvorKit.Script.AlvorEye.Demo\\AlvorKit.Script.AlvorEye.Demo.csproj"
    ],
    "environment": {
      "ALVOREYE_DEMO_RESULT_PATH": "out\\alvoreye-demo-results-smoke\\result.json"
    }
  },
  "window": {
    "title": "AlvorEye demo game",
    "exact": true,
    "timeoutSeconds": 30,
    "width": 960,
    "height": 720
  },
  "output": {
    "runId": "alvoreye-demo-results-smoke",
    "directory": "out\\alvoreye-demo-results-smoke\\run"
  },
  "timeline": [
    { "action": "wait", "seconds": 1 },
    { "action": "capture", "name": "start" },

    { "action": "keyDown", "key": "D" },
    { "action": "wait", "milliseconds": 300 },
    { "action": "keyUp", "key": "D" },
    { "action": "keyDown", "key": "W" },
    { "action": "wait", "milliseconds": 1800 },
    { "action": "keyUp", "key": "W" },
    { "action": "capture", "name": "key" },

    { "action": "mouseClick", "x": 750, "y": 180, "button": "left" },
    {
      "action": "mouseDrag",
      "x": 690,
      "y": 330,
      "toX": 855,
      "toY": 330,
      "button": "left",
      "milliseconds": 500
    },
    { "action": "text", "text": "EYE" },

    { "action": "handoff", "name": "handoff" },
    { "action": "resume" },

    { "action": "keyDown", "key": "D" },
    { "action": "wait", "milliseconds": 2200 },
    { "action": "keyUp", "key": "D" },
    { "action": "capture", "name": "win" },
    { "action": "analyzeBasic" },

    { "action": "keyDown", "key": "Escape" },
    { "action": "wait", "milliseconds": 250 },
    { "action": "keyUp", "key": "Escape" },
    { "action": "wait", "milliseconds": 500 }
  ]
}
```

## Why These Inputs Work

1. Wait and capture the starting state.
2. Hold `D` briefly, then hold `W` long enough to move the blue player square onto the green key tile.
3. Capture the key state and verify the first progress light turns green.
4. Click the yellow button at roughly `(750, 180)`.
5. Drag the cyan slider across the right panel from roughly `(690, 330)` to `(855, 330)`.
6. Send text input `EYE`, which fills the three orange code blocks and completes the fourth progress light.
7. Use `handoff` and `resume` to prove the target can be frozen and continued while the cyan scan bar is moving.
8. Hold `D` for `2200ms` to move the player into the pale exit tile.
9. Capture `win` and verify the bright green win rectangle is visible.
10. Hold `Escape` for `250ms` so the game exits normally and dumps `ALVOREYE_DEMO_RESULT`.

The final confidence check is both visual and structured:

- Visual: `frame-004-win.png` shows all four progress lights and the win rectangle.
- Structured: `result.json` says `Won:true`, `AllLocksComplete:true`, and `ProgressLightsGreen:4`.
