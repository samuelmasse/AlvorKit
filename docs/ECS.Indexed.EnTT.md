# ECS.Indexed Case Study: EnTT

EnTT (C++17 header-only ECS, `v4.0.0`, the de-facto reference ECS library —
used by Minecraft, Mortal Kombat, and much of the C++ game industry) audited
as a sixth case study for [ECS.Indexed.md](ECS.Indexed.md), and a different
kind again. The trio ([BrogueCE](ECS.Indexed.BrogueCE.md),
[NetHack](ECS.Indexed.NetHack.md), [Angband](ECS.Indexed.Angband.md)) are
non-ECS games hand-rolling the machinery; [SS14](ECS.Indexed.SpaceStation14.md)
and [Veloren](ECS.Indexed.Veloren.md) are shipping games *using* an ECS. EnTT
is neither — it is a general-purpose ECS *library*, studied not for usage
idioms but for **mechanism and API design**, because it is the closest analog
to this package of anything surveyed. EnTT's per-storage
construct/update/destroy signals are the package's hook pipeline; EnTT's
groups are the package's bags; EnTT's `entt::entity` is the package's
generational handle. Where the package writes contracts, EnTT ships an API —
so comparing the two both corroborates the package's choices and pinpoints
the three places it deliberately diverges.
[ECS.Indexed.flecs.md](ECS.Indexed.flecs.md) is the companion
reference-library study — the archetype-storage counterpart, whose hook
layer converges even harder (it has the pre-set hook EnTT lacks) while its
table storage exhibits the costs the package's dense slots avoid. File
references are to the EnTT repo (`entt/src/entt/...`, manual under
`entt/docs/md/entity.md`).

## The Architecture In Brief

`entt::entity` is a **generational identifier** split into an index and a
version — for the 32-bit default, a 20-bit entity mask and a 12-bit version
(`entity.hpp:44`), extracted by `to_entity` (slot) and `to_version`
(generation) (`entity.hpp:109`). Destroyed entities are recycled through a
free list with the version bumped, so a stale handle fails `valid()` rather
than aliasing — the package's `EntPtrIdx` liveness model, in the reference
library. Components live in **sparse-set storage** (`sparse_set.hpp`: a
packed array for iteration plus a sparse index for O(1) lookup). The
`registry` owns one storage per component type and exposes three signal sinks
per type — `on_construct<T>()`, `on_update<T>()`, `on_destroy<T>()`
(`registry.hpp:967`, `:991`, `:1015`). **Views** iterate the intersection of
several storages lazily; **groups** materialize and maintain a set for a
fixed component combination. There is no scope layer: one registry is the
world, and multiple worlds means multiple registries.

## Lessons

### The Signal Pipeline Is The Package's Hooks — With One Signal Per Event

EnTT's storages are wrapped in a `sigh_mixin` (`mixin.hpp`) that publishes a
signal on every mutation: `emplace` runs the insert then publishes
construction (`mixin.hpp:339`), `patch` runs the change then publishes update
(`mixin.hpp:353`), and `pop` publishes destruction *before* the erase
(`mixin.hpp:81`). Consumers connect via the registry sinks. This is
precisely the package's `EntIdxPreHook`/`EntIdxPostHook` design — hooks keyed
by component, driven by the mutation pipeline — and EnTT being *the* reference
ECS is strong evidence the design is the mainstream answer, not a Craftdig
peculiarity.

The instructive difference is granularity. EnTT gives **one signal per event
with fixed timing**: construct and update fire *after* the write, destroy
fires *before*. There is no pre-construction signal and — critically — no
old-value signal on update. The package deliberately splits `Set` into a
`pre` and a `post` hook, and the reason is exactly the gap EnTT's single
`on_update` leaves open: a key index must read the *old* id to remove the
stale dictionary entry before the new one lands (the Custom Key Index
section). On EnTT you cannot see the old value in `on_update`; you must cache
it yourself or route through the reactive mixin. The package's pre+post pair
is the considered superset of EnTT's construct/update/destroy — same
mechanism, one more hook point, added for a use case the reference library
makes you work around.

### The Ordering Contract Matches, Down To The Wording

EnTT's manual states the timing as a normative contract (`entity.md:384`):
construction listeners fire "**after** components have been created", update
listeners "**after** components have been updated", destruction listeners
"**before** components have been destroyed". That is the package's Set
(post-hook sees new state), Unset, and Dispose (pre-dispose sees the entity
intact) ordering, independently arrived at and independently documented. The
corroboration extends to intent: EnTT says destruction listeners are
"intended to provide users with an easy way to perform cleanup and nothing
more" (`entity.md:401`) — the same role the package assigns pre-dispose
("persistence erase and network teardown belong" there). Two libraries, the
same ordering pinned as contract, is about as much external validation as an
ordering choice can get.

### EnTT Forbids The Reentrancy The Package Supports — A Real Design Fork

EnTT's manual lists hard limits on listeners (`entity.md:393`): removing the
component from within a construction or update listener is "not allowed";
assigning or removing components inside a destruction listener "should be
avoided… can lead to undefined behavior"; connecting or disconnecting other
listeners from within a listener is UB. EnTT chose to **forbid reentrant
mutation** to keep the sparse-set storage simple and fast.

The package chose the opposite and says so: reentrancy is a *supported*
feature — "hooks may set other components; the nested write runs its own full
pipeline" — because Craftdig relies on it (the dirty tracker sets a flag
inside a pre hook; bag removal nests an index write inside the unset
pipeline). The package pays for that permissiveness with the `-1` sentinel
that brakes the bag's self-retrigger and with the explicit caveat that
self-`(T, N)` recursion is unbounded user error. This is the sharpest
divergence in the whole survey: the reference library treats hook-driven
mutation as a footgun to prohibit; the package treats it as a primitive to
support and fence. Both are defensible — the lesson to carry is that the
package's stance is a *deliberate cost*, not an oversight, and the sentinel
brake plus the reentrancy caveat are load-bearing precisely because EnTT's
simpler prohibition was on the table and rejected.

### Groups Are Bags, And EnTT's Rationale Is The Package's Rationale

EnTT groups (`entity.md:1989`) materialize a maintained set for a component
combination, kept correct automatically: "all groups affect… the creation and
destruction of their components… they must _observe_ changes in the pools of
interest and arrange data _correctly_." That is the package's bag exactly —
membership derived from component changes via the construct/destroy signals,
materialized into a contiguous region for O(count) iteration. Even the design
philosophy is verbatim the package's anti-archetype stance:

> Groups fit _usage patterns_… Users can decide when to pay for groups and to
> what extent… designed to optimize only the real use cases when users find
> they need to. (`entity.md:1999`)

This is precisely why the main document rejected the "optimize-everything"
query/archetype ECS: pay for the sets you declare, not for every combination.
The reference library reached the same conclusion. Two structural details
transfer directly:

- **Owning vs non-owning.** A full-owning group reorders the actual component
  storage so members pack at the front (zero extra memory, fastest, but a
  component can be owned by only one group). A non-owning group keeps a
  separate index array — "pretty rare and should be avoided" (`entity.md:2008`).
  The package's bag is the non-owning form (its own dense `EntMutIdx[]`), and
  the owning form is what the deferred "generator-emitted bag maintenance"
  optimization tier would become.
- **Owning exclusivity == one-bag-per-marker.** EnTT forbids two owning
  groups over the same component because they would fight over storage order;
  the package forbids two bags per marker per context because they would share
  `EntIdxBagIndex<N>` and corrupt each other's dense arrays. Same collision,
  same resolution, enforced at registration in both.

### The Reactive Mixin Is Dirty-Tracking As A First-Class Storage

EnTT ships change-collection as a storage type. The `reactive_mixin`
(`mixin.hpp:386`, manual `entity.md:481`) is a storage that observes the
construct/update/destroy signals of *other* storages and accumulates the
affected entities, motivated by the canonical reactive example: "100 fighting
units… only 10 changed their positions… update only the 10." This is
Veloren's `UpdateTracker` and the package's dirty tracker, promoted to a
library primitive — and the design echoes the package twice over: the
reactive storage is built *on top of* the signals (a consumer of hooks, not a
parallel mechanism), and "unlike all other storage, these classes do not
support signals by default" (`entity.md:525`) — it is a terminal leaf, exactly
as the package's dirty tracker sits at the end of the hook chain and emits
nothing further. The package keeps dirty tracking as a game-side hook rather
than an engine storage type, but EnTT confirms the shape is right: change
observation is a consumer of the write pipeline, never a bypass of it.

### Deletion Policy Is A Choice — And Bags Force Compaction

EnTT offers two deletion policies (`entity.md:1480`). The default is
swap-and-pop compaction: fast, but it moves the last element into the hole, so
positions and pointers are unstable and a captured iterator can see the moved
element — the package's bag iteration hazard, exactly. The alternative is
in-place deletion, which leaves a **tombstone** (`entity.md:740`, a
version-based sentinel) to preserve positions and pointers. The decisive
detail: "groups are incompatible with stable storage and even refuse to
compile" (`entity.md:1508`) — because a group *must* compact to keep members
contiguous. The package's bag is the compacting kind for the same reason, and
its iteration contract documents the same three hazards EnTT's compaction
implies; the package's reserved slot 0 and `-1` removal sentinel are its
version of EnTT's `null`/`tombstone` reserved values. The transferable point:
compaction (bags/groups) and pointer stability are mutually exclusive by
construction, in both systems — a consumer that needs stable references holds
handles and reads through storage, never a bag slot.

### One Registry, No Scopes — The Package's Deliberate Extra Layer

EnTT hooks are per-registry-per-component: a signal sink belongs to one
storage in one registry, and isolating two worlds means instantiating two
registries by hand. The package adds a layer EnTT does not model — hooks are
per-*context*-per-`(T, N)`, stored on a context entity, so one process runs
many scopes (world, level, chunk) each with its own arenas, bags, and hook
sets, isolated for free and reused by subclassing builders. This is the
package's clearest divergence from the reference design, and it is
motivated: the target games are scope-structured (Craftdig's world/dimension/
chunk), and EnTT's single-registry model would force either registry-per-scope
juggling or manual scope tagging on every component. The package pays one
strong-reference invariant (the arena must keep its context entity alive) to
get hierarchical scopes that EnTT leaves to the user.

## What Transfers And What Diverges

Corroborated by the reference implementation: hooks keyed by component and
driven by the write pipeline are the mainstream ECS design; the exact
Set-post / Dispose-pre ordering is a contract two independent libraries pin
identically; maintained materialized sets ("groups"/"bags") that cost only
what you declare are the right answer over optimize-everything archetypes;
membership maintenance rides the same construct/destroy signals; compaction
and pointer stability are mutually exclusive; generational handles over
packed storage are the identity model.

Deliberate divergences, each a considered cost the package pays for a reason
EnTT's simpler choice would deny it:

1. **Pre + post `Set` hooks**, not EnTT's single post-only `on_update` — for
   the old-value read the key index needs.
2. **Supported reentrancy** fenced by the `-1` sentinel, not EnTT's outright
   prohibition — because bag removal and dirty tracking nest writes.
3. **Per-context scopes** on a context entity, not EnTT's one registry — for
   the hierarchical world/level/chunk structure of the target games.

Out of scope by charter, as with the other library-shaped study: the sparse-
set storage internals, view/group iteration codegen, snapshot serialization,
and the meta/reflection system. The package borrows EnTT's *shape* (signals +
groups + generational handles) and hardens the three points above into
contracts the library leaves as API and documentation.

## Mechanism Map

| EnTT (`v4.0.0`) | ECS.Indexed |
|---|---|
| `entt::entity` = index + version, free-list recycled | `EntPtrIdx`/`EntMutIdx` generational liveness |
| `on_construct` / `on_update` / `on_destroy` sinks | post-set / (post-set) / pre-dispose hooks |
| construct=after, update=after, destroy=before (contract) | Set post-hook, Dispose pre-hook ordering |
| single post-only `on_update` (no old value) | pre + post `Set` split — pre reads the old value |
| listeners forbidden to mutate reentrantly (UB) | reentrancy supported, fenced by the `-1` sentinel |
| groups: maintained materialized set, "pay for what you declare" | bags: marker-derived, hook-maintained dense set |
| owning-group exclusivity (one owner per component) | one bag per marker per context, enforced |
| non-owning group = separate index array | the bag's own `EntMutIdx[]` dense array |
| `reactive_mixin` observes other storages' signals | game-side dirty tracker as a hook consumer |
| swap-pop vs in-place (`tombstone`) deletion policy | bag compaction + iteration staging; slot-0/`-1` sentinels |
| one registry = one world | per-context scopes with their own arenas and hooks |
