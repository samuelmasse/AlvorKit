# Menu Authoring

This document describes the preferred shape for AlvorKit UI menu code. It is
based on the Craftdig menu style and should be used when creating or
significantly changing classes that build UI with `AlvorKit.UI`.

Menu code should read like the screen it builds. Optimize the declaration for
readable layout, clear ownership, and cohesive regions rather than short
methods or small files.

## Menu Class Shape

A menu class is a UI composition object: it receives dependencies and builds a
node subtree. Keep its layout together in `Create`.

- A class whose name ends in `Menu` must expose exactly one public method:
  `Create`.
- Do not add fields, properties, events, static constants, helper methods, or
  private members to a menu class.
- Constructor injection is allowed. Prefer primary constructors for injected
  collaborators such as style, app/session command surfaces, text formatting,
  input roots, or child menus.
- Name an injected style collaborator `s`, regardless of the concrete style
  type. Prefer `AppStyle s` and `.Mutate(s.EmphasisLabel)` over longer names
  such as `style`.
- `Create` may accept explicit props, like `Create(EntMut root,
  AppInventoryView view)`. Do not pass the owner, parent state object, or the
  main application state itself as a prop.
- Put all helper logic inside `Create` as local variables, local constants,
  anonymous callbacks, or local functions.
- Return `void` unless a caller needs a result. `Create` may return the created
  node or a handle when a caller uses it to manage the subtree, including
  retaining or removing recursive child branches. Make that purpose apparent
  at the consuming call sites; not every caller must consume the result.

Declarative menu `Create` methods are explicitly exempt from the fifty-line
method ceiling in [GameCodeDesign.md](AgentRules/GameCodeDesign.md). Keep
related controls together even when the layout method is longer. Do not
compress fluent chains or manufacture menu classes to satisfy that ceiling.
Procedural callbacks and local functions remain focused and subject to the
ordinary method limit. Source-file and line-width limits still apply.

Child menus follow the same shape. Non-menu collaborators extracted from menus,
such as geometry, tooltip text, or app command/state objects, may have normal
members, but they should not be named `Menu` unless they follow the menu rules.

## UI Tree Structure

Write menu code so the source layout mirrors the visible UI tree.

- Put `Node(...)` on its own line, followed by one indented fluent call per
  line. Apply the same one-call-per-line shape to configuration chains on
  existing nodes. Do this even when the whole chain fits within the preferred
  C# line width.
- Treat the `root` parameter as a mount parent owned by the caller. Do not
  mutate it for size, placement, color, layout, input, or styling.
- Create a top-level child node for the menu's own surface, such as
  `Node(root, out var panel)`, then mutate that child and put the menu's
  children under it. This lets the caller size and place the menu anywhere.
- If all visible children need to share one layout or surface, create that
  child container explicitly. Do not use the incoming `root` as that container.
- Use braces after parent `Node(..., out var child)` calls and indent their
  contents to show ownership of child nodes.
- Keep sibling groups in screen order, reading from top to bottom. Separate
  sibling controls and groups with blank lines.
- Give nodes descriptive names that identify their visible role.
- Make node creation visible in the menu tree. Do not hide simple controls or
  repeated leaves behind local functions like `Button(parent, ...)`; declare
  `Node(parent)` at the call site and use `.Mutate(...)` to apply style,
  behavior, or reusable component recipes.
- Prefer putting `.Mutate(...)` calls first in a node chain when that mutate
  establishes the node's general shape, surface, or component recipe. Follow it
  with local sizing, placement, text, callbacks, or one-off overrides. Put
  `.Mutate(...)` later only when it intentionally depends on those local values
  or is applying a final override.
- Keep small groups of nodes inline. Extract a subtree when its independent
  behavior, reuse, or recursion justifies a separate menu; a visible region
  alone does not require a class.
- Prefer `childMenu.Create(parent)` for child menus when the child should
  contribute its own top-level node to the parent layout.
- Create a caller-owned slot node only when the caller needs to size, align, or
  otherwise place the child menu's mounting area, then call
  `childMenu.Create(slot)`.
- Build visual elements as UI nodes. If a visualization needs pixel geometry,
  express it through node offsets, sizes, colors, and tooltip/selectable
  metadata rather than a separate manual drawing path.

Example:

```csharp
[App]
public class AppExampleMenu(
    AppStyle s,
    AppSession session,
    AppChildMenu childMenu)
{
    public void Create(EntMut root, AppExampleView view)
    {
        const float headerHeight = 40f;

        Node(root, out var panel)
            .Mutate(s.PanelList);
        {
            Node(panel, out var header)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeV((0, headerHeight));
            {
                Node(header)
                    .Mutate(s.Heading)
                    .TextF(() => session.Title);
            }

            childMenu.Create(panel, view.Child);
        }
    }
}
```

## Callbacks And Local Helpers

Prefer the smallest readable scope.

- Use anonymous methods or lambdas for callbacks that are used once, especially
  short `OnUpdateF`, `OnPressF`, `IsDisabledF`, `TextF`, `OffsetF`, and `SizeF`
  logic. Place simple behavior beside the control it affects.
- Use a named local function when the block is reused, the name explains a UI
  concept, or the callback is large enough that a name makes the tree easier to
  scan.
- Declare local constants and local variables close to the node or helper that
  uses them.
- Avoid hoisting values to the top of `Create` unless they are shared by most
  of the menu.
- Capture local state inside `Create` when the menu needs ephemeral UI state,
  such as a previous input flag or the last rendered revision.
- Give local state and longer behavior clear names so readers can connect
  controls to their effects without tracing unrelated code.

Prefer:

```csharp
button.Mutate()
    .OnUpdateF(() =>
    {
        if (!keyboard.IsKeyDown(Keys.LeftShift))
            return;

        if (keyboard.IsKeyPressedRepeated(Keys.Equal))
            uiScale.ScaleUp();
    });
```

over a one-use local function whose name only repeats the callback registration.

## State And Dependencies

Menus are not application state. Keep the boundary crisp.

- Application or domain state should live outside `Menus`, usually in an app
  state/session/command object.
- Menus may depend on a narrow app/session command surface and call methods on
  it.
- App/domain code should not depend on menu classes or the UI package unless it
  is root-level orchestration that exists specifically to mount the UI.
- A root load/orchestration state may create the app scope and install app
  state, menus, UI roots, and shortcuts.
- Avoid passing "something above" into a menu. Prefer explicit props or an
  injected command/session collaborator.

## Styling And Measures

Use style objects for visual language, not for every number.

- `AppStyle` or an equivalent style class should own reusable colors, fonts,
  spacing tokens, and component recipes such as panel, button, label, swatch,
  tooltip, or modal styling.
- Local layout decisions belong in the menu that owns the layout. Give local
  numbers names, but keep them near the node or helper that uses them.
- Keep one-off sizing, placement, and text visible in the layout. Extract style
  recipes for repeated patterns; avoid one-use helpers that hide a few
  straightforward node declarations.
- If a measure is reused across several unrelated menus or is part of a shared
  component recipe, it can move into style.
- If a number only describes one menu's geometry, animation, breakpoint, or
  label fit rule, keep it local.

## UI Surfaces And Scaling

Most apps should keep using the original full-window API. Inject `RootUi`,
mount ordinary nodes on it, and add `RootUiScript` to `RootScripts`. That tree
is the default surface; no surface setup is required.

Use `RootUiSurfaces` only when one window genuinely needs UI regions with
different physical scales or viewport origins. A surface owns an independent
`RootUi`, converts mouse input into its local coordinates, clips drawing to its
physical viewport, and activates the matching `RootUiScale` and
`RootUiContext` while its callbacks run.

```csharp
game = surfaces.Create(
    () => new Box2(gameViewport.Min, gameViewport.Max),
    scale: 1f,
    order: 10f);

tools = surfaces.Create(
    () => userScale.Value,
    order: 20f);

Node(game.Root, out var gameMount);
gameMenu.Create(gameMount);

Node(tools.Root, out var toolsMount);
toolsMenu.Create(toolsMount);
```

The viewport and scale functions are resolved each frame. Changing their
values relayouts the existing trees; do not rebuild a menu merely because its
surface scale changed. Higher-order surfaces draw later and receive the first
chance to handle input, while transparent space falls through to lower
surfaces. Dispose app-owned surfaces when their owning state unloads.

## Splitting Menus

Keep related screen layout together. Split a menu when substantial independent
behavior, actual reuse, or recursion makes the boundary useful. A recursive
tree branch, a reusable picker, or a separate interactive dialog can justify a
child menu.

Do not automatically extract each header, footer, toolbar, or label group.
Those regions often read better together in one screen declaration. Do not
split solely to shorten a layout method or keep a file artificially small.

Non-visual concerns should become collaborators rather than child menus:

- geometry calculations
- tooltip text
- domain view/projection objects
- command/session state
- reusable styling

After a split, the parent menu should mostly describe layout and child menu
placement. The child menu should still have one public `Create` method and no
other members.

## Naming And Scope

Follow the local app's scope and prefix.

- In app-specific demos or games, prefer the app prefix, such as `AppMenu`,
  `AppHeaderMenu`, `AppStyle`, and `AppSession`.
- Use the app scope attribute for app services and menus, such as `[App]`.
- Use root-scope services only for root orchestration and engine-level roots.
- Do not mix a feature prefix, like `RangeAllocator`, into menu class names
  once the project has an app prefix.
