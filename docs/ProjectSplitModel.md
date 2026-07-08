# Project Split Model

This is the preferred package model for AlvorKit games. It is the clean target
because it separates game truth from presentation, persistence, networking, UI
composition, and executable startup.

The model is not about creating many projects for its own sake. Each project
answers one question:

- What is the game?
- How is it rendered?
- How is it controlled through menus?
- How is it saved or hosted?
- How does this product start?

## Core Principle

Pure packages define the game. Runtime packages adapt the game to a place where
it runs.

```text
Executable
  -> Menus
    -> Frontend
      -> Pure

Server executable
  -> Server
    -> Backend
      -> Pure
```

Dependency direction is the important invariant. Higher packages may depend on
lower packages. Lower packages must not depend on higher packages.

This must be enforced at the project-reference level. It is vitally important
that a project does not take on a dirty dependency in its `.csproj` just to make
a type convenient to reach. A pure project that references a frontend, menu,
windowing, GL, or audio project has defeated the purpose of the split even if
the offending API is only used in one small file. Fix the boundary instead:
move the file, introduce a pure contract or event, seed data from the upper
layer, or create a correctly named adapter project above both sides.

## Perfect Model

The perfect model is:

- `Game.App`: pure app/session/settings/persistence models.
- `Game.App.Frontend`: app-wide GL, audio, window-facing assets, loaded textures, fonts, icons, and other client resources.
- `Game.Run`, `Game.World`, `Game.Level`, or similar: pure gameplay state, rules, simulation, ids, stage data, scoring, and deterministic systems.
- `Game.Run.Frontend`, `Game.World.Frontend`, or similar: renderers, cameras, visual assets, GPU buffers, and client-only presentation for the pure gameplay layer.
- `Game.Menus`: concrete menu states, shared UI style, menu controls, cursor behavior, HUDs, pause screens, overlays, options, and state-transition glue.
- `Game`: executable startup for the client product.

For a small local game, this is usually enough:

```text
Game
  -> Game.Menus
    -> Game.App.Frontend
    -> Game.Run.Frontend
      -> Game.Run
    -> Game.App
```

`Game.App` and `Game.Run` stay pure. They should not reference UI,
OpenGL, MiniAudio, windowing, renderers, or menu code.

## Pure Packages

Pure packages hold domain truth:

- app/session state,
- settings and persisted score models,
- gameplay state,
- simulation rules,
- scoring,
- stage data,
- ids and definitions,
- deterministic input snapshots,
- semantic output events.

Pure packages should be easy to test headlessly. They should not know whether
the game is rendered with OpenGL, streamed over a network, controlled by a menu,
or hosted by a server.

Good dependencies for pure packages:

- maths,
- dependency injection attributes/scopes,
- pure data libraries,
- protocol or command contracts.

Avoid in pure packages:

- GL and GPU resources,
- UI nodes,
- window/input roots,
- audio engines,
- file-backed asset loading unless the package is explicitly a backend or
  persistence package,
- menu states,
- frontend renderers.

When pure code needs to affect presentation, emit semantic facts instead of
calling presentation directly. Examples:

- `RunSound.BossDie` instead of `AppAudio.Play(...)`.
- `RunInput` instead of direct `RootKeyboard` reads.
- `RunBestScore` seeded at scope creation instead of direct score-store access.

## Frontend Packages

Frontend packages adapt pure state to a local visual/audio client:

- GL layers,
- textures and texture atlases,
- sprite/font baking,
- renderers,
- cameras,
- visual effects,
- frontend asset catalogs,
- audio engine ownership,
- window-facing icon/title resources.

Frontend packages may depend on pure packages because they present them. Pure
packages must not depend on frontend packages.

Frontend packages may depend on `AlvorKit.Engine` for engine primitives such as
canvas, sprites, GL roots, input facades, and rendering helpers. They should not
depend on `AlvorKit.Engine.Loop`. Loop participation belongs in the executable,
menus, or another composition package that owns `State`, `Script`,
`RootScripts`, and state transitions. If a frontend package needs a script
adapter, keep the resource in frontend and move the adapter up to the
composition layer.

Use frontend packages when the code needs a GPU object, a window-facing object,
a renderer, a texture, or client-only presentation state.

## Menu Packages

Menus are client composition. They sit high in the graph because they naturally
glue many client-facing pieces together.

Menu packages may depend on:

- pure app state,
- frontend assets and audio,
- pure gameplay scopes,
- gameplay frontend renderers,
- UI packages,
- commands that transition between states.

Menus should own:

- shared menu style and menu controls,
- cursor and menu navigation helpers,
- main menu,
- options,
- pause,
- HUD,
- inventory or overlay UI,
- game over/results,
- state transition commands,
- UI-only input routing.

Menus should not own pure gameplay rules. If a menu button starts a run, the
menu package creates and seeds the run scope. The run simulation still lives in
the pure run package.

`Game.Menus.Common` is optional. Use it only when menu packages themselves have
split and need shared UI pieces without depending on one concrete menu package.
Good examples are `Game.Menus`, `Game.Editor.Menus`, `Game.Debug.Menus`, or a
launcher/client-shell menu package all sharing the same style, cursor behavior,
navigation helpers, and reusable controls. A singleplayer game with one menu
package usually should keep those pieces directly in `Game.Menus`.

## Backend Packages

Backend packages are optional. Add them when the game has substantial nonvisual
runtime work:

- save/load,
- persistence formats,
- background workers,
- chunk or world storage,
- terrain generation,
- authoritative nonvisual simulation,
- cache and file management.

Backend packages may depend on pure packages. They should not depend on
frontend or menu packages.

For a small local arcade game, a separate backend may not be needed. Simple
persistence can live in `Game.App` if it is small and app-wide. If persistence
becomes a real subsystem, move it to `Game.App.Backend`, `Game.World.Backend`,
or another scope-specific backend package.

## Server Packages

Server packages are optional. Add them when there is a client/server model.

Server packages own:

- authoritative tick loops,
- client connections,
- socket listeners,
- auth and allowlists,
- replication,
- server-side command handling,
- hosted world/session lifetime.

Server packages may depend on pure, backend, and protocol packages. They should
not depend on frontend or menu packages.

A dedicated server executable should reference server packages, not client menu
packages.

## Protocol Packages

Protocol packages are optional but become important for multiplayer.

Protocol packages own:

- wire commands,
- shared request/response models,
- serialization contracts,
- network ids,
- protocol constants.

They should be pure and low in the dependency graph so both client and server
can reference them without pulling each other in.

## Client/Server Model

For a larger client/server game, the graph usually expands like this:

```text
Game.Client
  -> Game.Menus
    -> Game.App.Frontend
    -> Game.Player.Frontend
    -> Game.World.Frontend
    -> Game.Client.Runtime
    -> Game.Protocol
    -> Game.App
    -> Game.World

Game.Server.Cli
  -> Game.Server
    -> Game.World.Backend
    -> Game.Protocol
    -> Game.App
    -> Game.World

Game.World.Frontend
  -> Game.World

Game.World.Backend
  -> Game.World

Game.Protocol
  -> pure dependencies only
```

Frontend and backend are siblings over the same pure world or gameplay package.
Neither should depend on the other unless there is a deliberately named bridge
package above both.

Menus remain a client-only composition layer. Server code must not reference
menus.

## No Client/Server Model

For a local-only game, keep the graph smaller:

```text
Game
  -> Game.Menus
    -> Game.App.Frontend
    -> Game.Run.Frontend
      -> Game.Run
    -> Game.App
```

Add a backend package only when there is enough persistence or background work
to justify it. Do not create server or protocol packages without a real
networked product shape.

## Placement Rules

When adding code, place it by dependency need:

- Needs no GL, UI, audio engine, windowing, or disk-specific runtime: pure package.
- Needs GL, textures, renderers, camera, or audio engine: frontend package.
- Needs UI nodes, focus, menu navigation, HUD, pause, or state glue: menu package.
- Needs save files, storage, generation, background persistence, or nonvisual runtime IO: backend package.
- Needs sockets, clients, auth, replication, or authoritative hosted ticks: server package.
- Needs shared client/server wire contracts: protocol package.
- Only starts one product: executable package.

If a lower package wants an upper package, invert the dependency. Pass data in,
seed the scope, or emit a semantic event for the upper layer to handle.

Before adding a `ProjectReference`, ask whether that reference preserves the
package's role. Do not add UI or frontend references to pure packages, frontend
or menu references to backend packages, or client/menu references to server
packages. A wrong-way project reference is not harmless plumbing; it changes
what the package is.

## Why This Is The Right Default

This model gives the codebase clear pressure lines:

- pure gameplay stays testable,
- rendering can change without rewriting simulation,
- menus can compose every client layer without leaking UI into the domain,
- servers can run without a graphics stack,
- persistence can evolve without dragging in windowing,
- multiplayer contracts stay shared and stable,
- startup remains thin and product-specific.

When the package graph follows these rules, architectural problems become easy
to spot. A pure package referencing UI, GL, MiniAudio, windowing, or menus is a
wrong-way dependency. A server package referencing frontend or menu code is a
wrong-way dependency. A menu package referencing several client layers is normal:
menus are the composition surface.
