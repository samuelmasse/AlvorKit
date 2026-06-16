# AlvorEye Demo Game

Use AlvorEye to observe and beat the window titled `AlvorEye demo game`.

The game is intentionally visual. Do not read game state from stdout. Capture frames, compare what changed, and use the visible colored regions to decide the next input.

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
- Hold `Escape` briefly to close the game.

Visual verification:

- The four small progress lights at the top of the right panel turn green as locks are solved.
- The moving cyan scan bar near the top of the play field is useful for testing capture timing and handoff freeze/resume.
- Winning fills the center of the play field with a bright green rectangle.
- Do not close the game until a capture shows all four progress lights green and the bright green win rectangle.

Result dump:

- When the game exits normally, stdout prints one line starting with `ALVOREYE_DEMO_RESULT ` followed by JSON.
- The result is an audit after the visual solve, not a substitute for screenshots.
- Hold `Escape` briefly or close the window after visually confirming the win state.
- If `ALVOREYE_DEMO_RESULT_PATH` is set, the same JSON is also written to that file.

Suggested AlvorEye workflow:

- Start with `run` or `session` and capture after a one-to-two second wait.
- Use held key actions with short waits for movement rather than single taps.
- Capture after each lock and verify the corresponding progress light changed.
- Use `handoff` while the scan bar is moving, inspect the frozen frame, then `resume` and continue.
- If only two lights are green, keep playing; the remaining locks are usually the slider and typed code.
