# Projects And Dependencies Policy

## Scope

Read this policy before creating or reorganizing projects, changing project or
package references, adding a launchable project, changing game scopes, or
working with ECS, GL ownership, maths types, or menus.

## Hard Stops

**NATIVE-BUILD-001:** Creating a project does not authorize native package
builds or dependency installation. Those operations require an explicit user
request and permission for that run.

## VS Code Launch Configurations

Whenever an agent creates a project that can be launched directly, the same
change must add a checked-in VS Code launch configuration for it under
`.vscode/launch.json`. This requirement is unconditional and applies to
executables, demos, games, tools, and runnable fixtures in both Working Mode and
Commit Mode. Add any corresponding `.vscode/tasks.json` build task referenced
by `preLaunchTask`, and include the working directory, arguments, and
environment required for the launch configuration to exercise the project's
supported launch contract.

## Project Split Model

Before creating or reorganizing game projects, package boundaries, frontend,
backend, server, protocol, or menu packages, read
[ProjectSplitModel.md](../ProjectSplitModel.md). Keep dependency
direction consistent with the pure/frontend/menu/backend/server split.

## Game Scope Organization

Before creating or reorganizing game dependency-injection scopes,
root/game/world/level/player services, loader scopes, or state transitions, read
[GameScopeOrganization.md](../GameScopeOrganization.md). Keep scope
prefixes, attributes, service names, and constructor dependency ordering
consistent with that guide.

## Game Ents And ECS

AlvorKit game templates and game repositories must use AlvorKit ECS for game
Ents. Use `Ent` in every context. The word `Entity` is banned; use `Ents` for
the plural. This applies to prose, code identifiers, type and member names,
parameters and locals, filenames, directories, labels, and compound names.

Before creating or significantly changing generated component declarations,
Ent handles or arenas, Indexed contexts, hooks, bags, indexes, or Ent lifetime,
read [ECS.md](../ECS.md). Keep game behavior in injected systems and
services, keep Ent state in components, and follow the guide's ownership,
registration, mutation, iteration, and teardown contracts.

## GL Object Ownership

Before creating, deleting, or wiring the lifetime of any GL object (buffers,
textures, vertex arrays, programs), read [GlOwnership.md](../GlOwnership.md).
`GlLayer` is hierarchical: objects belong to the scope node that created them,
and a scope-lifetime object must not get its own `IDisposable` or `Delete*`
teardown — disposing the scope's node reclaims everything it owns in one sweep.

## Maths Types

Before adding a maths helper, choosing vector or matrix shapes for an API, or
searching for maths type sources, read [Maths.md](../Maths.md). The
concrete `Vec`, `Mat`, `Quat`, box, and geometry structs are generated into
the `AlvorKit.Maths.Primitives` package by `scripts/AlvorKit.Script.MathsGen`
and have no committed source under `src/`; the doc lists every public type,
the naming scheme, and the usage rules, and
[MathsReference.md](../MathsReference.md) documents each family's
members.

Using the AlvorKit maths types and `ScalarMath` is mandatory, not stylistic.
Represent positions, sizes, offsets, directions, extents, ranges, rotations,
and transforms with the published maths types, and call the published vector,
matrix, and `ScalarMath` functions instead of re-deriving them. Do not model
a maths value as a scalar tuple such as `(int, int, int)`, parallel `x`/`y`/
`z` parameters or fields, or a local vector, box, or range type, and do not
hand-roll clamp, lerp, saturate, min/max, distance, power-of-two, or
bit-count logic that the maths surface already provides. A project that
needs a maths value but lacks the reference gains the `AlvorKit.Maths`
reference; a missing reference is never a reason to invent a local shape.

## UI Menu Authoring

Before creating or significantly changing an AlvorKit UI menu, read
[MenuAuthoring.md](../MenuAuthoring.md). Menu classes should follow that
single-`Create`-method shape unless a more specific `AGENTS.md` explicitly
allows a different local pattern.

## Dependency Direction

Before adding a project reference, preserve the role boundaries in
`ProjectSplitModel.md`. Pure packages must not reference UI, GL, frontend,
menu, audio, or windowing packages. Frontend packages may depend on
`AlvorKit.Engine`, but should not depend on `AlvorKit.Engine.Loop`; loop
ownership belongs in an executable, menu, or another composition package.
