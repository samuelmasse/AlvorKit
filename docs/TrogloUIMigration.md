# TrogloUI Migration Investigation

This note captures the initial investigation for moving
`C:\Users\Samuel\Documents\Repos\Craftdig\src\TrogloUI` into AlvorKit.

The intended first pass is a copy-paste migration: copy the projects over, then
change only the dependency bits required for the code to build against AlvorKit.
Avoid redesigns, broad cleanup, behavior changes, and namespace churn unless
they are explicitly requested later.

## Name

Recommended AlvorKit project and package names:

- `AlvorKit.UI`
- `AlvorKit.UI.Generator`

The source types should keep their existing vocabulary: `RootUi`, `RootUiMouse`,
`RootUiDraw`, `RootUiScript`, `UiProp`, `UiCallback`, `UiText`, `UiSyntax`, and
the related layout enums.

AlvorKit already has documentation that describes feature packages adding root
services such as `RootUi`, `RootUiMouse`, `RootUiDraw`, and `RootUiScript`.
That makes `AlvorKit.UI` the natural package name while preserving the root
service names as-is.

For the first copy-paste pass, keep the source namespace as `TrogloUI` if the
"nothing else should change" rule is strict. Renaming the namespace to
`AlvorKit.UI` is a public API and call-site churn change, not a dependency-port
change. Treat that as a separate explicit rename step.

## Craftdig Usage

Craftdig includes two UI projects:

- `src/TrogloUI/TrogloUI.csproj`
- `src/TrogloUI.Generator/TrogloUI.Generator.csproj`

The main direct consumer is `src/Craftdig.Menus.Common`, which references both
the runtime project and the generator project as an analyzer. Other menu and app
projects add global usings for `TrogloUI` and `TrogloUI.UiSyntax`, usually
through the menu project stack.

Runtime setup happens in `Craftdig.Menus.AppInitializeState`: it adds
`RootUiScript` to `RootScripts`, then creates root UI nodes under `RootUi`.
Gameplay/menu states then keep `EntMut` handles for menu roots, overlays,
inventory menus, modal backdrops, and menu stacks.

Craftdig menu code depends on:

- `RootUi`, `RootUiScript`, `RootUiMouse`, `RootUiFocus`, and `RootUiScale`
- `UiSyntax` helpers such as `Node`, `NodeS`, `NodeC`, `NodesRemove`,
  `NodeStackPop`, and `NodeStackTryPeek`
- generated fluent mutator methods such as `.AlignmentV(...)`,
  `.TextColorV(...)`, `.OnClickF(...)`, `.StackRootV(...)`, and
  `.MenuOriginF(...)`
- `UiProp<T>`, `UiCallback<T>`, `UiValue<T>`, and `UiText`
- layout enums such as `Alignment`, `InnerLayout`, `InnerSizing`, and
  `SizeWeightType`
- `Snap.Round`, mostly for text and scroll positioning

Craftdig also defines app-specific component interfaces such as
`IAppUiComponents` and `IAppUiTextboxComponents`. Those interfaces use
`UiProp`, `UiCallback`, and `UiText`, so the TrogloUI generator is part of the
public consumption pattern, not just an internal implementation detail.

## Current TrogloUI Shape

The copied footprint is small: about 36 files, 66 KB, and 1.6k C# lines across
the runtime project and generator project.

The runtime project owns:

- UI tree allocation and cleanup through `RootUi`
- per-frame traversal, sizing, positioning, focus, mouse, update, clipping, and
  drawing systems
- node-array storage used by `UiSyntax`
- UI component interfaces consumed by the ECS generator
- value wrappers for dynamic values, callbacks, and text

The generator project is a Roslyn analyzer. It scans interfaces marked with the
ECS components attribute, finds `UiProp<T>`, `UiCallback<T>`, `UiValue<T>`, and
`UiText` properties, then emits fluent `EntMutator<T>` extension methods.

## Dependency Deltas

The first pass should be a dependency port. Expected mapping:

- `HadvarECS` -> `AlvorKit.ECS`
- `HadvarECS.Generator` -> `AlvorKit.ECS.Generator`
- `AlvorEngine` -> `AlvorKit.Engine`
- `AlvorEngine.Loop` -> `AlvorKit.Engine.Loop`
- `Glw2D` -> `AlvorKit.Graphics2D`
- `Glw2D.Fonts` -> `AlvorKit.Graphics2D.Fonts`
- `GlwLayer` -> `AlvorKit.OpenGL.Layer`, if still directly needed
- `OpenTK.Mathematics.Vector2` -> `AlvorKit.Maths.Vec2`
- `OpenTK.Mathematics.Vector4` -> `AlvorKit.Maths.Vec4`
- `OpenTK.Mathematics.Box2` -> `AlvorKit.Maths.Box2` or
  `AlvorKit.Graphics2D.SpriteBatchClip`, depending on call site
- `OpenTK.Windowing.Common.Input.Keys` -> `AlvorKit.Windowing.Keys`

Likely mechanical source changes:

- `Vector2` to `Vec2`
- `Vector4` to `Vec4`
- `Vector2.Zero` to `Vec2.Zero`
- `Vector4.One` to `Vec4.One`
- `Vector2.ComponentMax(a, b)` to `Vec2.Max(a, b)`
- `Vector2.ComponentMin(a, b)` to `Vec2.Min(a, b)`
- `.Xy` and `.Zw` swizzles to `.XY` and `.ZW`
- `MathHelper.NextPowerOfTwo(...)` to `BitOperations.RoundUpToPowerOf2(...)`

The AlvorKit ECS generator attribute metadata name is
`AlvorKit.ECS.Generator.ComponentsAttribute`, so the TrogloUI generator must
look for that instead of `HadvarECS.Generator.ComponentsAttribute`.

## Cursor Shape Status

Craftdig-era TrogloUI has per-node cursor shape support:

- `UiProp<MouseCursor?> CursorFV`
- `RootUiMouse.Draw()` sets `mouse.Cursor`

AlvorKit now exposes cursor shape through `AlvorKit.Windowing.CursorShape`.
The windowing layer owns the platform-neutral API, and the GLFW host maps those
values to standard GLFW cursors. The agent host also carries the cursor shape in
its window state.

When moving the UI code, update the cursor dependency to the AlvorKit type:

- `UiProp<CursorShape?> CursorFV`
- `RootUiMouse.Draw()` should set `RootMouse.CursorShape` or the equivalent
  windowing property

Craftdig does not appear to set `CursorFV` anywhere, so cursor shape is unlikely
to affect immediate behavior. Keeping the API wired through windowing preserves
the migrated UI surface without introducing a temporary no-op into
`AlvorKit.UI`.

## Suggested First Pass

1. Copy `src/TrogloUI` to `src/AlvorKit.UI`.
2. Copy `src/TrogloUI.Generator` to `src/AlvorKit.UI.Generator`.
3. Rename only the project files and project names needed for AlvorKit solution
   integration.
4. Keep source namespaces and type names unchanged unless namespace rename is
   explicitly requested.
5. Update project references and global usings to AlvorKit dependencies.
6. Update the generator's ECS attribute metadata name.
7. Replace the cursor shape dependency with `AlvorKit.Windowing.CursorShape`.
8. Apply only source edits required by renamed dependency types and APIs.
9. Build the copied UI project and generator project as the targeted check.

Do not stage, commit, run broad lint, run broad tests, or perform final
verification gates unless the work is explicitly moved into Commit Mode.
