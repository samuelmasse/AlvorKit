# Menu Authoring

This document describes the preferred shape for AlvorKit UI menu code. It is
based on the Craftdig menu style and should be used when creating or
significantly changing classes that build UI with `AlvorKit.UI`.

## Menu Class Shape

A menu class is a UI composition object: it receives dependencies and builds a
node subtree. Keep the class body intentionally tiny.

- A class whose name ends in `Menu` must expose exactly one public method:
  `Create`.
- Do not add fields, properties, events, static constants, helper methods, or
  private members to a menu class.
- Constructor injection is allowed. Prefer primary constructors for injected
  collaborators such as style, app/session command surfaces, text formatting,
  input roots, or child menus.
- `Create` may accept explicit props, like `Create(EntMut root,
  AppInventoryView view)`. Do not pass the owner, parent state object, or the
  main application state itself as a prop.
- Put all helper logic inside `Create` as local variables, local constants,
  anonymous callbacks, or local functions.

Child menus follow the same shape. Non-menu collaborators extracted from menus,
such as geometry, tooltip text, or app command/state objects, may have normal
members, but they should not be named `Menu` unless they follow the menu rules.

## UI Tree Structure

Write menu code so the source layout mirrors the visible UI tree.

- Use braces after parent `Node(..., out var child)` calls to show ownership of
  child nodes.
- Keep sibling groups in screen order.
- Prefer `Mutate(childMenu.Create)` for child menus when no props are needed.
  Prefer `Mutate(node => childMenu.Create(node, props))` when props make the
  child menu reusable and explicit.
- Build visual elements as UI nodes. If a visualization needs pixel geometry,
  express it through node offsets, sizes, colors, and tooltip/selectable
  metadata rather than a separate manual drawing path.

Example:

```csharp
[App]
public class AppExampleMenu(
    AppStyle style,
    AppSession session,
    AppChildMenu childMenu)
{
    public void Create(EntMut root, AppExampleView view)
    {
        const float headerHeight = 40f;

        root.Mutate(style.PanelList);

        Node(root, out var header)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeV((0, headerHeight));
        {
            Node(header)
                .Mutate(style.Heading)
                .TextF(() => session.Title);
        }

        Node(root)
            .Mutate(node => childMenu.Create(node, view.Child));
    }
}
```

## Callbacks And Local Helpers

Prefer the smallest readable scope.

- Use anonymous methods or lambdas for callbacks that are used once, especially
  short `OnUpdateF`, `OnPressF`, `IsDisabledF`, `TextF`, `OffsetF`, and `SizeF`
  logic.
- Use a named local function when the block is reused, the name explains a UI
  concept, or the callback is large enough that a name makes the tree easier to
  scan.
- Declare local constants and local variables close to the node or helper that
  uses them.
- Avoid hoisting values to the top of `Create` unless they are shared by most
  of the menu.
- Capture local state inside `Create` when the menu needs ephemeral UI state,
  such as a previous input flag or the last rendered revision.

Prefer:

```csharp
root.Mutate()
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
- If a measure is reused across several unrelated menus or is part of a shared
  component recipe, it can move into style.
- If a number only describes one menu's geometry, animation, breakpoint, or
  label fit rule, keep it local.

## Splitting Menus

Split a menu when the `Create` method stops reading like one coherent UI
subtree.

Good splits are usually visible UI regions:

- header, toolbar, panel, timeline, modal, list, row, detail, overlay

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
