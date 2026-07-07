# ECS.Indexed Case Study: Space Station 14

Space Station 14 (C#, content for the RobustToolbox engine, engine vendored
in-repo) audited as a fourth case study for [ECS.Indexed.md](ECS.Indexed.md)
— and the first of a different kind. The trio
([BrogueCE](ECS.Indexed.BrogueCE.md), [NetHack](ECS.Indexed.NetHack.md),
[Angband](ECS.Indexed.Angband.md)) shows non-ECS games hand-rolling the
machinery; SS14 runs a real ECS in production at brutal scale — roughly
1,900 registered components and 1,100 entity systems in content, 5,200
event subscriptions, servers running hours-long rounds with thousands of
live entities. The main document rejected the query/scheduler ECS as this
package's architecture; SS14 is evidence about *usage*: which idioms an
ECS's consumers converge on when the codebase is huge, and which engine
mechanisms exist only to compensate for something our design already has.
[ECS.Indexed.Veloren.md](ECS.Indexed.Veloren.md) is the companion
production-ECS study (`specs`, Rust), which converges on the package where
SS14 diverges — generational identity, storage-native change tracking; and
[ECS.Indexed.EnTT.md](ECS.Indexed.EnTT.md) and
[ECS.Indexed.flecs.md](ECS.Indexed.flecs.md) are the reference-library
studies (sparse-set and archetype).
File references are to the SS14 repo (`space-station-14/Content.*/...`,
engine under `space-station-14/RobustToolbox/...`).

## The Architecture In Brief

`EntityUid` is a bare `readonly int Id` — **not generational**
(`Robust.Shared/GameObjects/EntityUid.cs:34`). Ids are allocated
monotonically and never reused within a session
(`EntityManager.cs:1115`, `NextEntityUid++`), so a stale reference resolves
to "no such entity" rather than aliasing a newcomer; the doc comment still
warns that fabricated ids are "effectively undefined behavior". Component
storage is a per-type `Dictionary<EntityUid, IComponent>` in a
`CompIdx`-indexed array (`EntityManager.Components.cs:39`). Components are
data classes initialized from YAML prototypes (the catalog/divergence split
the main document recommends, at scale); logic lives in systems; systems
communicate through **directed events keyed by (component, event) pairs** —
`SubscribeLocalEvent<TComp, TEvent>` (`EntityEventBus.Directed.cs:27`),
with `ref` event structs to avoid copies and optional `before`/`after`
ordering by system type.

## Lessons

### Marker Components Are The Active-Set Idiom At Scale

Content defines 37 `Active*Component`s — empty marker classes whose whole
job is set membership. The canonical shape
(`Content.Shared/DoAfter/`): `ActiveDoAfterComponent` is an empty class
("Added to entities that are currently performing any doafters");
entering the set is `EnsureComp<ActiveDoAfterComponent>(uid)`
(idempotent add), leaving is `RemCompDeferred<ActiveDoAfterComponent>(uid)`
(`SharedDoAfterSystem.cs:155`), and the hot update loop iterates
`EntityQueryEnumerator<ActiveDoAfterComponent, DoAfterComponent>`
(`SharedDoAfterSystem.Update.cs:31`) — join the marker against the data
component, touch nothing else. This is the bag pattern as the *dominant
production idiom*, independently converged on 37 times: the per-frame set
must be gated by a marker or the loop pays for every entity that ever had
the component. Two usage notes transfer directly: markers stay empty (data
lives beside them, never in them), and set-enter is idempotent
(`Set<bool, N>(true)` already is). SS14's version walks a dictionary of
marker holders; the bag is the same idea materialized into a dense span.

### Membership Never Changes Mid-Iteration — By Institution

SS14 does not stage iteration; it defers the mutation instead, and the
engine ships the discipline as API: `QueueDel` (194 content uses) queues
entity deletion into a `Queue`+`HashSet` drained at tick boundaries
(`EntityManager.cs:72`, `ProcessQueueudDeletions` at
`EntityManager.cs:300`), and `RemCompDeferred` (149 uses) is the same for
single components — the DoAfter exemplar above *leaves its own active set*
with the deferred form precisely because it is inside the query loop over
that set. Same hazard the main document's iteration contract covers, the
mirrored resolution: engine-blessed deferral instead of consumer-side
staging. The transferable lesson is not which side defers but that there
is exactly **one blessed pattern, provided and named** — a thousand systems
did not each invent their own. Our staged-scratch pattern should stay the
single documented shape, stated wherever bags are taught.

### Directed (Component, Event) Keying Scales; Registration Order Does Not

5,200 subscriptions across 1,100 systems, all keyed `(TComp, TEvent)` and
dispatched off a component index (`EntityEventBus.Directed.cs:253`) — the
same keying as the package's `(T, N)` hooks, and strong evidence for it:
at scale, nobody subscribes to "position changed" in the abstract, they
subscribe to "position changed *on entities that have my component*".
`ref`-struct events parallel the `in T` pre-hook delegates. But implicit
ordering did not survive: with independent systems subscribing to the same
pair, SS14 needed explicit `before`/`after` arrays and a DAG sort
(`EntityEventBus.Ordering.cs`). The package's "hooks run in registration
order, loaders order deliberately" is the right contract for one loader
per scope — and SS14 marks where it stops scaling: the moment hook
consumers are written by people who cannot see each other's registration
code, ordering metadata becomes the API. Watch for that threshold; do not
add the DAG before it.

### Manual Dirty Marking Is The Footgun; Generate It Away

SS14 components replicate to clients only when explicitly marked: content
contains ~1,100 manual `Dirty(uid, comp)` calls, each a site where a
mutation happened and a human remembered to say so — and forgetting is a
classic SS14 bug class (state visibly stale until something else dirties
the component). The engine's response was to remove the human:
`[AutoNetworkedField]` with `[AutoGenerateComponentState(fieldDeltas:
true)]` source-generates the marking (exemplar:
`Robust.Shared/Audio/Components/AudioComponent.cs:19`). This is the
strongest available validation of putting dirty tracking *inside the write
pipeline* (the pre-hook dirty tracker in the main document's Usage
section): mutate-then-remember-to-mark APIs do not survive contact with a
large contributor base. Never expose one; the hooked `Set` is the marking.

### Identity Without Generations: The Monotonic-Id Tax

Robust's stale-handle answer — never reuse ids — is safe but shapes the
whole storage layer: ids are sparse forever, so components live in
per-type dictionaries, so every `Comp<T>(uid)`/`TryComp` is a hash lookup.
The mitigations then pile up as API surface: cached `EntityQuery<T>`
handles, 404 uses of `EntityQueryEnumerator<>` to amortize lookups in
loops, the `Entity<T>` struct bundling uid+component so resolved access
travels together, and ~200 explicit `Deleted`/`TerminatingOrDeleted`
checks in content. The package's dense slot storage with generation-
stamped handles buys the same stale-safety with array-indexed access — and
`EntPtrIdx` carrying its context is the same ergonomic move as
`Entity<T>`: resolve once, pass the bundle. That idiom is convergent;
design call sites around it.

### Write Ownership Wants An Analyzer

Components can restrict who mutates them: `[Access(typeof(SharedAudioSystem))]`
at class level and per-field permission overrides
(`Robust.Shared/Analyzers/AccessAttribute.cs:53`), enforced by a Roslyn
analyzer. This is the production-scale version of two things the main
document does structurally: bag `Add`/`Remove` made `internal` (ownership
of derived state), and the "analyzer warning for raw arena use in indexed
scopes" deferred-feature bullet. SS14's experience says the analyzer
approach works and is worth its cost exactly when consumer count makes
convention unreviewable — the same threshold as the ordering lesson.

### The Spatial Index Is Hooks All The Way Down — Including The Hot Path

The engine's broadphase/lookup structure is maintained entirely by
subscriptions — `BroadphaseComponent` add/init/terminating, grid and map
creation, physics body changes (`EntityLookupSystem.cs:122`) — and
position changes raise a `readonly struct MoveEvent` synchronously from
the transform setters themselves
(`TransformComponent.cs:174`, `:541`). That is immediate, hook-driven
index maintenance on the hottest component in the engine, in a game that
ships — the existence proof for the main document's same-frame-visibility
choice over deferred index maintenance. The discipline that makes it
survivable: events are `ref` structs (no allocation), and directed
`MoveEvent` subscribers in content number just ten — hot-component hooks
are treated as a scarce resource. That matches the package's guidance
(early-out hook bodies, few hooks on hot components) and hardens it into a
review rule: a new hook on a hot component is a design decision, not a
convenience.

### Handler Failure Is Contained At The Loop, Not The Pipeline

The DoAfter update loop wraps per-entity work in a `try/catch` compiled
under `EXCEPTION_TOLERANCE` (`SharedDoAfterSystem.Update.cs`) — a
long-running server eventually wants one entity's bug to not end the
round. Note where the containment sits: around the *system's* per-entity
work, never inside the event dispatch or the state write. That is the
right reading of the main document's "hooks must not throw, no rollback"
contract — the pipeline stays bare, and any tolerance policy belongs to
the consumer loop that can define a sane resume point.

## What Transfers And What Does Not

Transfers into our usage: marker-gated active sets as the default system
shape; one blessed membership-mutation-during-iteration pattern; `(T, N)`
hook keying with `in` payloads; dirty marking generated inside the write
path, never manual; handle bundles (`EntPtrIdx` as `Entity<T>`) as the
call-site currency; analyzer-enforced write ownership once conventions
outgrow review; hot-component hook budgets.

Does not transfer: broadcast events and event relays, network replication
and prediction, YAML prototype tooling — out of package scope by charter.
And the core architectural divergence stands: SS14 pays dictionary lookups
for non-generational stable identity and recovers the cost with query
caching; the package's generational handles over dense pages get both
properties structurally. Nothing in SS14's experience argues for adopting
the world/query/scheduler shape the main document rejected — its own
consumers spend their effort narrowing queries back down to marker-gated
sets, which is where bags already start.

## Mechanism Map

| Space Station 14 | ECS.Indexed |
|---|---|
| `Active*Component` empty markers ×37 | bool marker components + bags (materialized) |
| `EnsureComp` / `RemCompDeferred` set transitions | idempotent `Set<bool, N>`; staged iteration |
| `QueueDel` + tick-boundary drain | staged-scratch pattern (consumer-side deferral) |
| `SubscribeLocalEvent<TComp, TEvent>` + `ref` events | `(T, N)` hooks with `in T` payloads |
| `before`/`after` subscription ordering DAG | registration order per loader (DAG only past that scale) |
| manual `Dirty()` ×~1,100 → `[AutoNetworkedField]` | dirty tracking inside the hooked write pipeline |
| monotonic never-reused `EntityUid` + dictionary storage | generational handles + dense page storage |
| `Entity<T>` uid+component bundle | `EntPtrIdx` handle carrying its context |
| `[Access]` analyzer-enforced write ownership | `internal` bag mutation; analyzer-candidate bullet |
| broadphase maintained by subscriptions; sync `MoveEvent` | immediate hook-maintained spatial index |
| `EXCEPTION_TOLERANCE` catch in system loops | hooks never throw; tolerance belongs to consumers |
