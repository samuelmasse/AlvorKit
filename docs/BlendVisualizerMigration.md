# Blend Visualizer Migration

Rebuild `demos/AlvorKit.Ranges.Demo.Visualizer` as a Blend editor-shell app and
grow `src/AlvorKit.UI.Blend` into the shared, opinionated UI framework along
the way. Blend apps may all share the same look; that is the point.

This is a multi-session plan. Work top to bottom, one checkbox per slice; every
step should build and run on its own. Tick boxes as steps land.

## Design Reference

- Design cards (self-contained HTML, open in a browser):
  [docs/design/blend-visualizer/](design/blend-visualizer/)
  - `visualizer-shell.html` — target main screen, 1440x900
  - `visualizer-picker.html` — scenario picker modal state
  - `blend-components.html` — component sheet, new vs existing Blend controls
  - `blend-palette.html` — Blend chrome tokens + app-local data palette
- Published preview (all four cards, scaled):
  <https://claude.ai/code/artifact/48d66007-0e5f-456c-9ecf-3b75bf1264e7>

Target layout: `MenuBar` (brand + scenario item) / `Toolbar` (playback, speed,
Labels/Padding toggles, ui scale) / workspace = left "Allocator State" metrics
dock + memory viewport (chip header with mode + legend, two memory strips,
hover tooltip) + bottom timeline dock with real overlay-mode `Tab`s /
`StatusBar` (FPS, playing, scenario i/n, step i/n, speed, ui). Values shown in
the cards are illustrative; heights and colors are exact Blend tokens.

## Decisions Already Made

- Adopt the Blend default palette and Inter wholesale. The old teal palette,
  Roboto Mono chrome, and 112px header panel are gone, not ported.
- The whole old header becomes MenuBar + Toolbar + StatusBar.
- Overlay modes (memory and timeline) become tabs/chips, not cycle buttons.
- The scenario picker stays a modal, rebuilt from new Blend modal recipes.
- Allocator data colors are app semantics, not chrome: they stay in the demo.
- Prerequisite: the in-flight uncommitted work (AzureTentacle Blend migration,
  menu-authoring refactor, RootUi system changes) lands before Phase 0 starts.

## Phase 0 — Blend Infrastructure

- [x] 0.1 Convert `BlendMetrics` from a 44-argument positional record to an
      init-property record with defaults; replace the positional construction
      in `BlendStyle` with named initializers. No visual change.
- [x] 0.2 Add a `Scrim` token to `BlendPalette` (modal tint).
- [x] 0.3 Add optional `Keyboard` to `BlendStyleOptions`; Blend buttons run
      their click on focused-Enter when it is provided (falls back to the
      press callback when no click is set). Migrated AzureTentacle's
      `ToolbarActionButton`/`PrimaryToolbarActionButton` shims onto it.
      Skipped: disabled fill/border/cursor branches (nothing uses them yet).

## Phase A — Promote Existing App-Side Recipes Into BlendStyle

Each step adds the recipe to `BlendStyle` and dedupes the current owner in the
same slice.

- [x] A.1 Label family from AzureTentacle `AppStyle`: `Label`, `MutedLabel`,
      `EmphasisLabel`, `CellLabel`, `MutedCellLabel`, `EmphasisCellLabel`.
- [x] A.2 List containers: `PanelFillList`, `PanelFitList`, `HorizontalRow`,
      plus `HeaderStrip`, `InsetPanelList`, `ListBody` from AzureTentacle;
      self-sizing `VerticalList`, `HorizontalList`, and weighted
      `HorizontalFill` shapes from the Visualizer's old style.
- [x] A.3 `SelectableListRow` from AzureTentacle (hover fill baked in;
      selected fill stays a call-site `ColorF` override).
- [x] A.4 Dock scaffolding from `EditorShellMenu` local functions: `Dock`,
      `TabStrip`, `Splitter`, `ActiveTabAccent` (+ `ActiveTabAccentOffset`
      metric); editor-shell demo deduped onto them.

## Phase B — New Blend Components

- [x] B.1 Modal recipes: `ModalLayer` (uses `Scrim`), `ModalPanel` (centered,
      strong border, vertical weighted list), `ModalContent` (padded list).
      Also added a public `StrongBorder` recipe.
- [x] B.2 Tooltip system: `IBlendUiComponents.TooltipFV` in Blend (with the
      ECS generator analyzer wired into the Blend csproj), the `Tooltip`
      surface recipe, and `BlendTooltipMenu` (mouse-follow, clamped to root).
- [x] B.3 `Swatch` and `MetricRow` recipes (+ `SwatchWidth`, `SwatchHeight`,
      `MetricRowHeight`, `ModalContentPadding`, `TooltipPadding`,
      `InsetPanelPadding` metrics).

## Phase C — Visualizer Rebuild, Region By Region

- [x] C.1 Wiring: `AlvorKit.UI.Blend` project reference and `<Using>`;
      `AppStyle : BlendStyle` (Inter + `BlendControlChrome` + `Keyboard`);
      `AppLayout` for dock widths and app geometry. No compat members were
      needed — the whole phase landed in one pass.
- [x] C.2 Shell frame: `AppMenu` is MenuBar (brand + scenario item +
      description) + `AppToolbarMenu` + workspace docks + `AppStatusMenu`;
      `AppHeaderMenu` deleted. The toolbar rebuilds its controls on a new
      `AppSession.UiRevision` so Play/Pause and toggle actives stay current.
- [x] C.3 Metrics dock: `Dock` + `PanelTitle` + `MetricRow` label/value rows.
- [x] C.4 Memory viewport: `HeaderStrip` with mode chip + `Swatch` legend +
      aggregate stats; strip menus slot into the body unchanged.
- [x] C.5 Timeline bottom dock: `TabStrip` with real switching for the five
      overlay modes (rebuilds on `UiRevision`); caption row with last call +
      step; timeline lane fills the tab body.
- [x] C.6 Scenario picker: `ModalLayer`/`ModalPanel`/`PanelTitle`/
      `ModalContent` with two-line options (reactive fill/border, accent bar).
- [x] C.7 Tooltips: `AppTooltipMenu` is now a thin `BlendTooltipMenu`
      subclass; `IAppUiComponents` deleted (Blend's `TooltipFV` serves the
      strip/timeline call sites unchanged).
- [x] C.8 Cleanup: `AppStyle` holds only the allocator data palette plus
      `PanelInsetColor`/`RuleWidth`/`FontSizeSmall` bridges for the strip
      rendering collaborators.

## Phase D — Verify And Document

- [x] D.1 Ran via AlvorSense (screenshots in `out/alvorsense-sessions/
      blendviz*/`): shell, tabs, tooltip, scrub, toggles, and status bar all
      match the design. Caveat: mouse press/click dispatch is dead engine-wide
      after the RootUi systems refactor in 6a33281 (hover, raw polling, and
      keyboard work) — verified with focus-retention tests; needs a
      `RootUiMouse` fix. All visualizer features remain reachable via
      keyboard shortcuts meanwhile.
- [ ] D.2 Update `docs/MenuAuthoring.md` / Blend docs if new guidance emerged
      (e.g. when to use dock scaffolding vs raw lists).

## Deviations From The Plan

- Blend recipes follow the one-arg `Mutate(s.Recipe)` convention that landed
  in 6a33281, with static `Active*` variants + rebuild-on-revision instead of
  `Func<bool>` reactive actives.
- The visualizer uses `OnPressF` (old app semantics) rather than `OnClickF`
  while click dispatch is broken; Blend's Enter activation falls back to the
  press callback when no click callback is set.
- Toolbar `-`/`+` use `ToolbarButton` instead of `SquareButton`: the 10px
  square-button font drops the hyphen glyph (rasterizes to nothing) while
  12px renders it.
- `AppMemoryStripLabels` is currently dead code (no callers since the strip
  texture refactor) — block labels no longer render; predates this migration.

## Stays App-Local

- The ~40 allocator data colors: `AllocationColor(slot)`,
  `CommandColor(kind)`, occupancy/density/efficiency/fragmentation/churn/
  relocation/outlier ramps, free-block/tail/retained/padding/latest/highlight,
  strip outline and active-frame colors.
- Strip and timeline rendering: geometry, texture, label, and tooltip-text
  collaborators under `Menus/`.
- `AppUiScale` + the ui-scale shortcuts and toolbar buttons.
- `AppLayout` numbers (dock widths, insets) and single-menu local constants.

## Open Questions

- Digit jitter: metric values in proportional Inter may look jumpy while
  playing. If it bothers, add an optional `MonoFont` slot to
  `BlendStyleOptions` (Roboto Mono is already embedded engine-side) or use
  tabular figures. Decide after C.3 is running.
- Dropdown/popup primitive for MenuBar items: future Blend component, not
  needed for this migration (the scenario item opens the modal directly).
