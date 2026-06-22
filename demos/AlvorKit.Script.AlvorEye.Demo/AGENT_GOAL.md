# AlvorSense Demo Game

This project is historically named `AlvorKit.Script.AlvorEye.Demo`, but the
demo can be solved with either AlvorEye or AlvorSense. Prefer AlvorSense for the
engine-native route unless the user specifically asks for OS-level desktop
automation.

Before playing, read the repository instructions carefully:

- Read `AGENTS.md` and any nested `AGENTS.md` files that apply to this work.
- Read `docs/AlvorSense.md` before starting or driving the game with
  `scripts/AlvorKit.Script.AlvorSense`.

Goal:

1. Move the blue player square onto the green key tile in the left play field.
2. Click the large yellow button in the right panel.
3. Drag the cyan slider handle in the right panel all the way to the right.
4. Type the code `EYE`; the three orange code lights should fill in.
5. Move the blue player square into the pale exit tile near the upper-right of
   the play field.

The game is intentionally visual. Capture frames, share important screenshots
with the chat user, compare what changed, and use the visible colored regions to
decide the next input. Keep working in the same live game session whenever
practical instead of restarting after each observation.

Do not close the game until a capture shows all four progress lights green and
the bright green win rectangle in the center of the play field. After visually
confirming the win, quit the game and report the `ALVOREYE_DEMO_RESULT` JSON
printed by stdout.
