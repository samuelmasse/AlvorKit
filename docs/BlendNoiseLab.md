# Blend Noise Lab

Build `demos/AlvorKit.UI.Blend.Demo.NoiseLab` — a FastNoise2 texture designer
in the Blend editor shell — and grow `AlvorKit.UI.Blend` the missing form
controls along the way: number fields, sliders, dropdowns, text fields, and
checkboxes. The parameter dock is generated from FastNoise2's runtime metadata
(variables, hybrids, node lookups), so the controls are exercised generically
rather than hand-wired.

This is a multi-session plan. Work top to bottom, one checkbox per slice; every
step should build on its own. Tick boxes as steps land.

## Design Reference

- Design cards (self-contained HTML, open in a browser):
  [docs/design/noise-lab/](design/noise-lab/)
  - `noise-lab-shell.html` — target main screen, 1440x900
  - `noise-lab-dropdown.html` — Source dropdown open + Histogram tab state
  - `noise-lab-widgets.html` — form-controls sheet, every state + metrics
  - `noise-lab-palette.html` — app-local ramps and viewport colors
- Published preview (all four cards, scaled):
  <https://claude.ai/code/artifact/18157b3b-f356-4874-b4d9-f72fc087521f>

Chrome values on the cards are exact Blend tokens; seeds, timings, and preset
names are illustrative.

## Decisions Already Made

- Fields keep the label *inside* the control at `FieldHeight` 24: muted 11px
  label left, 12px value right. No separate label column.
- Slider fill stays chrome-neutral: `Selection` idle, `ActiveSurface` fill +
  `Accent` border only while dragging. Accent marks active/open states only.
- The dropdown popup reuses the tooltip surface language (`Raised` +
  `StrongBorder` + root-layer float) and clamps to the root like
  `BlendTooltipMenu`.
- The number-field edit mode *is* the text field: one shared edit engine
  (`BlendTextEdit`), numeric mode re-parses on commit and reverts on garbage.
- New metrics: `DropdownOptionHeight` 22, `CheckboxSize` 14, `CaretWidth` 1,
  `FieldArrowPadding` 3, `DragPixelsPerStep` 8. Everything else reuses
  `FieldHeight`, `FieldTextPadding`, `MutedFontSize`.
- Ramps, probe, and histogram colors are app-local (Noise Lab semantics), same
  split as the Visualizer's allocator palette.
- Demo project name: `AlvorKit.UI.Blend.Demo.NoiseLab` (scenario suffix after
  `.Demo`, matching demo naming rules).

## Phase 0 — Engine Prerequisite: Mouse Dispatch

Diagnosed root cause (differs from the earlier "RootUiMouse is broken" guess):
`RootUiScript` ran all UI input dispatch in the render-phase `Frame` event,
but input state is tick-scoped to the update phase — `WindowLoop` ticks
mouse/keyboard/text right after the `Update` event. Wheel offsets and key
edges were therefore never visible to the UI, and button presses were visible
only if they happened to span a render. Real-user clicks (held across many
frames) mostly worked; AlvorSense gestures (`press, update, release, update`
with no render in between) never did — which is why dispatch looked dead
engine-wide during the Visualizer migration. `RootUiMouse` itself was correct.

- [x] 0.1 Stood up `tests/AlvorKit.UI.Test` with a harness that mirrors the
      RootLoop wiring over `FakeWindowHost` + `GlNoop`-backed sprite roots;
      the wheel test reproduced the dead dispatch (and an agent-gesture test
      reproduces the press/click loss without renders).
- [x] 0.2 Moved `RootUiScript` work from `Frame` (render phase) to
      `Update` (logical phase). Capture semantics verified: the pressed node
      keeps `IsPressed` while the button is held even if the cursor leaves it
      (drag fields rely on this). Tab focus edges also live again.
- [x] 0.3 Covered hover, press, click, focus-on-press, release-off-node,
      held-capture, renderless agent-gesture click, and scroll in
      `RootUiMouseDispatchTest` (8 tests). Double-click is covered later with
      the number-field edit tests.

## Phase A — Blend Form Controls

Each step lands the recipe plus focused tests; the demo consumes them in
Phase B+.

All of Phase A landed in `BlendFields` (builders with data accessors) plus the
`BlendFieldChrome` internal helper and the `BlendDropdownMenu` collaborator,
rather than parameterless `BlendStyle` recipes — form controls bind to values,
so they take options records at the call site. `BlendStyle` gained public
`Rule` overloads, a public `ActivateOnEnter`, and a `TextFont` accessor.

- [x] A.1 `BlendFields.Checkbox`: 14px box + label row, whole row is the hit
      target, Enter toggles when focused; `CheckboxSize`/`CheckGlyphFontSize`
      metrics (check glyph at 12px — 10px glyphs rasterize unreliably).
- [x] A.2 `BlendTextEdit`: single-line editable text state (insert,
      backspace/delete, home/end, arrows, shift-select, Ctrl+A, selection
      replace, Enter/Tab commit, Esc cancel) fed by keyboard runes and key
      repeat; caret blink with typing wake. Covered by 12 unit tests in
      `tests/AlvorKit.UI.Blend.Test`.
- [x] A.3 `BlendFields.TextField`: field surface + `BlendTextEdit` +
      caret/selection nodes, placeholder, edit-on-focus-edge (Esc does not
      instantly re-enter), commit on blur.
- [x] A.4 `BlendFields.NumberField`/`IntField`: label-inside field, hover
      arrows with click stepping, drag-scrub with deadzone (Ctrl snap, Shift
      fine, Esc cancels a drag), click-without-drag or focused-Enter starts
      an inline edit (select-all); numeric commit re-parses and reverts on
      garbage. Deviation from the widgets card: edit-mode text is
      left-aligned with the label hidden (Blender behavior) instead of
      right-aligned under the label.
- [x] A.5 `BlendFields.SliderField`: bounded field with inset fill bar;
      relative drag maps horizontal motion across the field width to the
      range; fill `Selection` → `ActiveSurface` + `Accent` border while
      scrubbing.
- [x] A.6 `BlendFields.DropdownField` + `BlendDropdownMenu`: closed field
      with value/swatch/caret glyph; shared root-layer popup at anchor width
      with 22px rows, selected accent bar + "current" tag, hover-or-arrow-key
      highlight, Enter picks, Esc/press-away closes, flips above near the
      root edge. Field interaction behavior is verified via AlvorSense in
      Phase F (fields need fonts + GL, so no unit harness yet).

## Phase B — Demo Scaffold And Shell

- [x] B.1 Project scaffold: csproj (UI.Blend + FastNoise2 backend + engine
      refs), top-level `Program.cs`, `AppScope`/`AppAttribute`,
      `RootLoadState`, `AppStyle : BlendStyle` (Inter + chrome + keyboard,
      `ChipFontSize` 12 — the 11px period glyph rasterizes to nothing).
- [x] B.2 Shell frame per the shell card: MenuBar (brand + File/Node/View/Help
      + live metadata counts), Toolbar (fractal dropdown, seed int field +
      R randomize, Auto toggle, Regenerate), params dock + viewport
      workspace, StatusBar. Deferred with Phase E: Export button, ui-scale
      controls, bottom dock.

## Phase C — Noise Pipeline

- [x] C.1 `AppNoiseField`: fractal-over-source graph from probed metadata
      catalogs, parameters discovered per node (variables/hybrids with
      defaults, ranges, enum names, descriptions), reusable buffers,
      `Texture2D` upload, min/max + generation-time stats. Values cache
      app-side (FastNoise2 has no getters); rejected writes don't stick.
- [x] C.2 Viewport menu: header chips (slice, z, min/max, gen ms), drag pans
      1:1, wheel zooms around center, Shift-wheel steps z, auto regenerate
      on dirty (~2.5 ms Simplex, ~18 ms cellular at the default window). The
      sample grid is derived from the visible viewport area — one sample per
      UI unit — so window resizes regenerate more or fewer samples at the
      same feature scale instead of scaling the image, and ui scale is the
      only display zoom. Buffers and the texture are recreated on resize
      (cold path).
- [x] C.3 Value probe tooltip: sample value + pixel + world coordinates.

## Phase D — Metadata-Driven Parameter Dock

- [x] D.1 `AppNoiseParameter` rows typed from metadata
      (float/int/enum/hybrid + ranges + tooltips from descriptions).
- [x] D.2 Dock sections per node, rebuilt on `UiRevision`: float→NumberField
      (bounded metadata range→SliderField), int→IntField, enum→DropdownField
      over metadata enum names, hybrid→NumberField (hybrids expose no range),
      Source lookup→DropdownField over the probed source catalog; edits mark
      dirty. Verified live: swapping Simplex→CellularDistance regrew the dock
      with Distance Function/Return Type enums, jitter params, Minkowski P.
- [x] D.3 Post section: Normalize/Invert checkboxes + ramp dropdown
      (grayscale/terrain/magma/viridis/two-tone with midpoint swatches).
      Resolution/format dropdowns + tiled preview deferred to Phase E with
      export, so the dock shows no dead controls.

## Phase E — Presets, Histogram, Export

- [ ] E.1 Bottom dock tabs (Presets / Histogram / Export Log) on the Blend
      tab strip.
- [ ] E.2 Presets: JSON snapshots of node graph + seed under
      `res/noise-lab/presets`; list rows + save-as `TextField` + Save/Delete.
- [ ] E.3 Histogram tab: value distribution bars + mean marker from the last
      generation.
- [ ] E.4 Export: PNG (8-bit gray) to `out/noise-lab`, status/log line per
      export. ps1tex export can wait for AlvorEye integration.

## Phase F — Verify And Document

- [x] F.1 AlvorSense pass (screenshots in `out/alvorsense-sessions/
      noiselab*/`): shell vs card, Source dropdown open + pick with dock
      rebuild, Feature Scale drag-scrub mid-hold (accent border, exact step
      math, live regen), Minkowski P inline edit (select-all replace, Enter
      commit), enum dropdown pick, Invert + Magma ramp, value probe, pan +
      zoom, R randomize with exact int seed display. Not yet exercised in
      app: `TextField` (lands with presets in E.2; engine-level covered by
      `BlendTextEditTest`), arrow click-stepping, `SliderField` fill (no
      bounded float in the FBm/cellular metadata — needs a node with ranged
      floats or a preset-driven case).
- [ ] F.2 Update `docs/MenuAuthoring.md` / Blend docs with form-control
      guidance (when to use NumberField vs SliderField, popup collaborator
      pattern), plus the deferred D.2 item from the Visualizer migration.

## Deviations From The Plan

- Engine root cause was `RootUiScript` running input work in the render-phase
  `Frame` event, not a `RootUiMouse` bug; fixed by moving the work to
  `Update` (see Phase 0).
- Form controls are `BlendFields` builder methods with options records, not
  parameterless `BlendStyle` recipes: they bind to values.
- Field carets and step arrows are node-built triangles (`BlendFieldChrome`
  `DownCaret`/`SideArrow`) — ▾/◂/▸ glyphs rasterize as tofu in Inter.
- `IntField` carries native int accessors beside the float core; float-only
  bridging rounded a ~1.6e9 seed to the float grid on display and commit.
- Number-field edits happen in place: the value stays right-aligned exactly
  where it displays, with the label and step arrows staying put (the caret
  and selection anchor from the right edge). Clicking an arrow during an
  edit commits the edit, then steps.
- Step arrows are always visible on drag fields, not hover-revealed as on
  the widgets card — hover pop-in read as layout jitter.
- Hybrids render as `NumberField` (metadata exposes no hybrid range), so Gain
  is not the slider shown on the widgets card.
- Ramp dropdown swatches are midpoint colors, not gradients — dropdown items
  carry a single swatch color.
- Row recipes must apply `Mutate(style.Board)` before size setters; the
  dropdown popup initially rendered one full-panel row because Board's
  `SizeRelative(1,1)` overrode the row height (MenuAuthoring order rule).
- Click-away blur is Blend policy, not engine behavior: the `Root` recipe
  (and the demo's viewport) are silent-focusable, so pressing empty chrome
  silently steals focus for one tick — field edits commit, popups close.
  The engine keeps focus untouched for plain non-focusable presses so other
  UI consumers (Craftdig's TrogloUI menus, game-world textboxes) keep their
  focus-retention semantics after migrating. Both behaviors are locked by
  focus tests in `RootUiMouseDispatchTest`.
- FastNoise2 returns int variable min/max through the float metadata getters
  as reinterpreted bits (denormals like 3E-45); `AppNoiseField` converts them
  back with `BitConverter.SingleToInt32Bits`. Octaves is really [2, 16] — the
  mangled range briefly clamped it to [0, 0], which read as "stuck at zero".
  `Apply` also only caches values the node accepts now.
- Sprite-batch texture draws sample v as `1 - y/H`
  (`SpriteBatchWriter.Geometry`), so a top-down sample buffer renders
  vertically mirrored under `SpriteBatchFlip.None` — which also made
  vertical panning feel inverted and silently mirrored the probe's rows.
  The viewport texture node sets `TextureFlip.Vertical` so screen ==
  sample buffer == future PNG export; the pan math and probe stay
  unmirrored. (The old FastNoise2 demo compensated with a `+Y` pan sign
  instead and lived with the mirrored display.)

## Stays App-Local

- Ramp definitions and `RampColor(t)` mapping, probe/crosshair colors,
  histogram bar color, viewport backing black, tile seam guide.
- FastNoise2 node construction, sampling buffers, and stats.
- Preset serialization format and file locations.
- `AppLayout` numbers (dock widths, viewport insets).

## Open Questions

- Drag-value APIs: does `RootUiMouse` expose enough (position + pressed node)
  for drag widgets, or do Blend controls need a small drag-state helper? Decide
  in A.4 once 0.2 lands.
- Enum variables: FastNoise2 metadata exposes enum member names per variable;
  confirm the binding surface while building D.1 (fall back to int fields if
  member names are not reachable).
- Digit jitter in proportional Inter for scrubbing values — same open question
  as the Visualizer; revisit after C.2 with real numbers on screen.
