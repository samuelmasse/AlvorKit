# AlvorSense Demo Solution

This solves the demo through `AgentGlfwWindowHost`. The demo is run with
`ALVORKIT_WINDOWING_AGENT=1`, so AlvorSense controls exact update counts, text
input, mouse input, rendering, and screenshots without creating a visible
target window.

## Commands

Build the demo:

```powershell
dotnet build demos\AlvorKit.Script.AlvorSense.Demo\AlvorKit.Script.AlvorSense.Demo.csproj
```

Run the harness solve:

```powershell
$env:ALVORKIT_WINDOWING_AGENT = "1"
$env:ALVORSENSE_DEMO_RESULT_PATH = "out\alvorsense-windowing-harness\result.json"

@'
render
screenshot out\alvorsense-windowing-harness\00-start.png
key D down
updates 15 0.016
key D up
key W down
updates 109 0.016
key W up
render
screenshot out\alvorsense-windowing-harness\01-key.png
move 750 160
mouse Left down
update 0.016
mouse Left up
render
screenshot out\alvorsense-windowing-harness\02-button.png
move 690 306
mouse Left down
update 0.016
move 855 306
update 0.016
mouse Left up
render
screenshot out\alvorsense-windowing-harness\03-slider.png
text KIT
update 0.016
render
screenshot out\alvorsense-windowing-harness\04-code.png
key D down
updates 110 0.016
key D up
render
screenshot out\alvorsense-windowing-harness\05-win.png
state
quit
'@ | dotnet run --project demos\AlvorKit.Script.AlvorSense.Demo
```

## Result

The successful structured result is:

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

The final visual proof is `out\alvorsense-windowing-harness\05-win.png`: it shows
all four progress lights green, the slider handle at the right edge, the blue
player square inside the pale exit tile, and the bright green win rectangle in
the play field.
