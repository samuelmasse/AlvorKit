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

- [ ] 0.1 Convert `BlendMetrics` from a 44-argument positional record to an
      init-property record with defaults; replace the positional construction
      in `BlendStyle` with named initializers. No visual change.
- [ ] 0.2 Add a `Scrim` token to `BlendPalette` (modal tint).
- [ ] 0.3 Add optional `Keyboard` to `BlendStyleOptions`; Blend buttons run
      their click on focused-Enter when it is provided. Migrate AzureTentacle's
      `ToolbarActionButton` Enter shim onto it. Optional in the same slice:
      disabled fill/border/cursor branches in `ButtonFill`/`ButtonBorder`.

## Phase A — Promote Existing App-Side Recipes Into BlendStyle

Each step adds the recipe to `BlendStyle` and dedupes the current owner in the
same slice.

- [ ] A.1 Label family from AzureTentacle `AppStyle`: `Label`, `MutedLabel`,
      `EmphasisLabel`, `CellLabel`, `MutedCellLabel`, `EmphasisCellLabel`.
- [ ] A.2 List containers: `PanelFillList`, `PanelFitList`, `HorizontalRow`
      from AzureTentacle; self-sizing `VerticalList`, `HorizontalList`, and
      weighted `HorizontalFill` shapes from the Visualizer's current style.
- [ ] A.3 `SelectableListRow` plus `ListRowFill`/`ListRowTextColor` from
      AzureTentacle (also the future scenario-picker option row).
- [ ] A.4 Dock scaffolding from `EditorShellMenu` local functions: `DockPanel`,
      `TabStrip`, `Splitter`; dedupe the editor-shell demo onto them.

## Phase B — New Blend Components

- [ ] B.1 Modal recipes: `ModalLayer` (uses `Scrim`), `ModalPanel` (centered,
      strong border), `ModalContent` (padded vertical list).
- [ ] B.2 Tooltip system: move the `TooltipFV` UI component interface from
      `demos/AlvorKit.Ranges.Demo.Visualizer/Menus/Styling/IAppUiComponents.cs`
      into Blend (the UI generator already runs on Blend), add the tooltip
      surface recipe, and port `AppTooltipMenu` (mouse-follow, clamped to root)
      as `BlendTooltipMenu`.
- [ ] B.3 `Swatch` (legend color chip) and a metric label/value row helper.

## Phase C — Visualizer Rebuild, Region By Region

- [ ] C.1 Wiring: add the `AlvorKit.UI.Blend` project reference and `<Using>`;
      rewrite `AppStyle : BlendStyle` (Inter + `BlendControlChrome`), keeping
      temporary compat members so old menus still compile; add `AppLayout` for
      dock widths and app geometry.
- [ ] C.2 Shell frame: `AppMenu` becomes MenuBar + Toolbar + workspace docks +
      StatusBar; `AppHeaderMenu` is deleted and its content redistributed
      (scenario item to MenuBar, buttons to Toolbar, readouts to StatusBar).
- [ ] C.3 Metrics dock: `AppMetricsMenu` on `PanelTitle` + metric rows.
- [ ] C.4 Memory viewport: chip header with mode + swatch legend + aggregate
      chips; existing strip menus slot into the body unchanged.
- [ ] C.5 Timeline bottom dock: `TabStrip` with real switching for the five
      overlay modes; timeline strip in the tab body.
- [ ] C.6 Scenario picker: rebuild on `ModalLayer`/`ModalPanel`/`ModalContent`
      with `SelectableListRow` two-line options.
- [ ] C.7 Tooltips: consume the Blend tooltip; delete `IAppUiComponents` and
      the app tooltip menu.
- [ ] C.8 Cleanup: delete compat members and dead chrome colors from
      `AppStyle`; what remains is only the allocator data palette
      (`AllocationColor`, `CommandColor`, overlay ramps, block anatomy).

## Phase D — Verify And Document

- [ ] D.1 Run the demo and compare against the design cards; fix seams.
      Prefer AlvorSense (`docs/AlvorSense.md`) for driving and screenshotting.
- [ ] D.2 Update `docs/MenuAuthoring.md` / Blend docs if new guidance emerged
      (e.g. when to use dock scaffolding vs raw lists).

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
