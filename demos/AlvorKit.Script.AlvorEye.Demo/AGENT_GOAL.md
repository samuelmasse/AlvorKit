# AlvorSense Demo Game

This project is historically named `AlvorKit.Script.AlvorEye.Demo`, but the
demo can be solved with either AlvorEye or AlvorSense. AlvorSense is suggested
when you want the engine-native route: use `scripts/AlvorKit.Script.AlvorSense`
to observe and beat the demo through `AgentGlfwWindowHost`.

When using AlvorSense, start the game with `ALVORKIT_WINDOWING_AGENT=1`, drive
it with interactive agent commands, and request screenshots with
`screenshot <path>`. AlvorEye remains a valid choice when practicing desktop
visual automation or when you specifically want OS-level window/input behavior.

The game is intentionally visual. Capture frames, share important screenshots
with the chat user, compare what changed, and use the visible colored regions to
decide the next input. If you use AlvorSense, keep working in the same live game
session whenever practical instead of restarting after each observation.

Goal:

1. Move the blue player square onto the green key tile in the left play field.
2. Click the large yellow button in the right panel.
3. Drag the cyan slider handle in the right panel all the way to the right.
4. Type the code `EYE`; the three orange code lights should fill in.
5. Move the blue player square into the pale exit tile near the upper-right of the play field.

Controls:

- Hold `W`, `A`, `S`, `D` or the arrow keys to move.
- Use the mouse to click the yellow button.
- Use the mouse to drag the cyan slider handle.
- Send text input for `EYE`.
- Send `quit` after visually confirming the win.

Visual verification:

- The four small progress lights at the top of the right panel turn green as locks are solved.
- The moving cyan scan bar near the top of the play field is useful for testing capture timing and handoff freeze/resume.
- Winning fills the center of the play field with a bright green rectangle.
- Do not close the game until a capture shows all four progress lights green and the bright green win rectangle.

Result dump:

- When the agent command loop exits, stdout prints one line starting with
  `ALVOREYE_DEMO_RESULT ` followed by JSON.
- The result is an audit after the visual solve, not a substitute for screenshots.
- If `ALVOREYE_DEMO_RESULT_PATH` is set, the same JSON is also written to that file.

Suggested AlvorSense workflow:

- Start with `render` and `screenshot out\...\00-start.png`.
- Use `key <name> down`, `updates <count> <delta>`, and `key <name> up` for movement.
- Use `move`, `mouse Left down`, `update`, and `mouse Left up` for mouse locks.
- Use `text EYE` for the code lock.
- Capture after each lock and verify the corresponding progress light changed.
- If only two lights are green, keep playing; the remaining locks are usually the slider and typed code.
