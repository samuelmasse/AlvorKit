# ECS.Indexed Case Study: Veloren

Veloren (Rust, open-world voxel RPG, built on the `specs` ECS —
`specs 0.20`, amethyst fork, pinned in `Cargo.toml`) audited as a fifth case
study for [ECS.Indexed.md](ECS.Indexed.md), and the second of the "real ECS
in production" kind after [Space Station 14](ECS.Indexed.SpaceStation14.md).
The pairing is the point: both are large shipping ECS games, but `specs`
makes three core choices RobustToolbox does not — **generational entity
identity**, **storage-native change events**, and **per-component storage-type
selection** — and each lands closer to this package's design than SS14 did.
Where SS14's divergences (non-generational ids, manual dirty marking)
validated the package by contrast, Veloren's *alignments* validate it by
convergence, and its two remaining divergences (access-based over-flagging,
a system scheduler DAG) sharpen exactly where the package draws its lines.
[ECS.Indexed.EnTT.md](ECS.Indexed.EnTT.md) and
[ECS.Indexed.flecs.md](ECS.Indexed.flecs.md) are the companion
reference-library studies, whose signals/hooks and groups are the package's
hooks and bags. File references are to the Veloren repo
(`veloren/common/...`, `veloren/server/...`).

## The Architecture In Brief

`specs`' `Entity` is a **generational index** — a slot number plus a
generation counter, with the allocator recycling dead slots and bumping the
generation so a stale `Entity` fails to resolve rather than aliasing a
newcomer. This is the package's `EntPtrIdx`/`EntMutIdx` liveness model
almost exactly, arrived at by a mainstream ECS. Components are Rust structs
implementing `Component` with an associated `Storage` type; systems are
`impl System` with typed `SystemData`, iterated by `Join` over component
storages; a `DispatcherBuilder` schedules systems with explicit
inter-system dependencies. Network identity is a *separate* `Uid`
(`NonZeroU64`, monotonically allocated, `common/src/uid.rs:12`) mapped to
`Entity` through `IdMaps` (`uid.rs:47`) — the SS14 `NetEntity` split, but
here the local identity is already generational, so the second
representation exists **only** for cross-machine stability, not to paper
over local reuse.

## Lessons

### Generational Identity Is What A Modern ECS Reaches For

`specs` ships the package's exact stale-handle answer: kill an entity, the
slot is reusable but the generation advances, and any handle from before
resolves to nothing. SS14 chose the other branch — never reuse ids, pay
sparse dictionary storage forever — and the package sided with `specs`:
dense slot storage plus a generation stamp buys stale-safety *and*
array-indexed access. Veloren is the existence proof that this is the
ergonomic default at scale, not an exotic optimization. And the `Uid`/
`IdMaps` layer makes the finer point the main document's Custom Key Index
section already implies: stable external identity is a *derived index over
generational handles*, not a replacement for them. Veloren keeps the
generational `Entity` as the truth and maintains `Uid → Entity` beside it —
which is precisely the key-index-hook pattern (`AddPre<Guid, Id>`),
hand-rolled as a resource.

### Storage-Native Change Events: Dirty Tracking With Zero Mark Calls

This is the headline. Of 85 component `impl`s, **41 use `FlaggedStorage`
or `DerefFlaggedStorage`** — nearly half — making change-tracked storage the
dominant kind. The consumer is a single generic
`UpdateTracker<C>` (`common/net/src/sync/track.rs:10`): it registers a
`ReaderId` on the storage's event channel (`track.rs:22`) and, each tick,
drains `ComponentEvent::Inserted/Modified/Removed` into three `BitSet`s
(`track.rs:37`), which the sync system joins against `Uid` to emit network
deltas. **There is no manual mark anywhere** — mutation sites write through
`write_storage::<C>()` and the event is emitted by the storage itself.

Put beside SS14's ~1,100 hand-written `Dirty()` calls, this is the
strongest external validation of the package's central bet: dirty tracking
belongs *inside the write pipeline*, not at the call site. Veloren pushes it
one level deeper than the package even proposes — into the storage type
rather than a registered hook — and gets the same payoff: forgetting to
mark is structurally impossible. The package's pre-hook dirty tracker
(Usage section) is this mechanism expressed as a hook instead of a storage
wrapper, which is the right choice when the same write must also feed bags
and key indexes that a storage wrapper cannot see.

### Access-Based Flagging Over-Syncs — Which Vindicates The Hook's Change Detection

`specs` `FlaggedStorage` emits `Modified` on mutable *access*, not on actual
value change: any `get_mut` marks the entity modified even if nothing is
written (see `common/net/src/sync/packet.rs:36`, a routine `get_mut` write
path). So `UpdateTracker` collects an **over-approximation** — a system that
grabs a mutable reference and early-returns still generates a network
update. This is the well-known cost of tracking at the storage layer: it is
automatic and unforgettable, but coarse.

The main document's Set contract reads like a direct response: *"Set does
not perform change detection; hooks that need it compare against
`ent.Get<T, N>()` themselves (the dirty-tracker pattern)."* That is the
refinement Veloren's storage cannot express. A hooked `Set` with a
comparing pre-hook (the `WorldComponentTracker` pattern) marks dirty **only
on real change** — the precision Veloren gives up for the convenience of
storage-level automation. Lesson for our usage: automatic-on-access is
seductive and wrong for expensive downstream work; keep change detection in
the hook body, early-out on equality, and the same write can feed a
delta-sync consumer without over-sending.

### Membership By Presence, Not By Marker

Veloren uses **zero** `NullStorage` (empty marker) components. Where SS14
materializes 37 `Active*Component` markers, Veloren expresses active sets by
**joining over component presence directly** — `(entities, uids, &*groups,
alignments.maybe()).join()` (`common/src/comp/group.rs`) — with `.maybe()`
for optional members and the change-event `BitSet`s themselves acting as
transient membership sets in the sync join (`track.rs`). Having the
component *is* the membership; the join is the query.

Two production ECS games, two different answers, and the package sits
between them deliberately. `specs`' join-over-presence is clean but pays a
storage walk per query with no materialized hot set; SS14's markers
materialize membership but as a second component you must remember to add
and remove. The package's bag is the synthesis: membership is derived
automatically from a marker (or marker && gate) by hooks, *and* it is
materialized into a dense span for O(count) iteration — the join's
directness with the marker's explicitness, neither hand-maintained. Veloren
shows the idiom is viable without materialization; the package's bet is that
the hot sets (the ones a system sweeps every tick) are worth materializing,
and the backstop makes them free to keep correct.

### The Scheduler DAG Is For Parallel Systems — And It Stays Out Of Index Consistency

Veloren leans hard on the piece the main document *rejected as the package's
architecture*: a dispatcher with explicit dependencies. `add_local_systems`
(`common/systems/src/lib.rs`) wires a real DAG by system name —
`controller` after `mount`, `stats` after `buff`, and `phys` fanning in
from four predecessors (`interpolation`, `controller`, `mount`, `stats`,
`lib.rs:33`) with six systems fanning out after it (`projectile`,
`shockwave`, `beam`, ...). `specs` runs independent systems in parallel;
the DAG exists to serialize the data hazards.

This does not contradict the package's rejection — it scopes it. The
scheduler solves **coarse-grained parallel system execution**, a concern the
package deliberately does not own. Crucially, Veloren's *index-shaped*
consistency — the change events feeding sync — is **not** scheduled through
the DAG; it is immediate, emitted by the storage at write time and drained
by one tracker. Veloren thus splits exactly where the package does: system
parallelism is a scheduler's job (out of scope, deferred to the game's own
dispatch), while derived-state maintenance is immediate and write-driven (in
scope, the hook pipeline). The main document's "deferred index maintenance —
rejected; revisit only if parallel system execution ever becomes real" is
answered here: parallel systems *are* real in Veloren, and its index
maintenance still stayed immediate.

### One Component Set, Two Derived Consumers, Two Strategies

The same components feed two independent derived representations by two
different mechanisms. Network sync is **incremental**: FlaggedStorage change
events → `UpdateTracker` BitSets → per-component deltas. Character
persistence is **snapshot**: `server/src/persistence/character/conversions.rs`
converts live components to database rows wholesale (`convert_body_to_database_json`
at `:226`, `convert_inventory_from_database_items`, and siblings), with no
change events involved. Delta where staleness is cheap to recompute per
tick; snapshot where the consumer is transactional and periodic.

This is the package's multi-hook story in the wild: one `Set` pipeline,
several consumers, each choosing its own maintenance discipline — a bag
recomputes membership, a key index updates a dictionary, a dirty tracker
marks for delta sync, a snapshot pass ignores the hooks entirely and reads
current state. The pipeline is neutral about which consumers attach; Veloren
attaching a change-driven consumer and a snapshot consumer to the same
components is the concrete demonstration that the two coexist without
fighting.

### Storage Type Is A Density Decision, Made Per Component

`specs` forces every component to name a storage: `VecStorage` (dense array,
fast for common components — 9+ uses), `DenseVecStorage` (indirection table,
for sparse components — 13+ uses), wrapped in `FlaggedStorage` when change
tracking is wanted. Veloren picks per component by how many entities carry
it. This is the sparse-vs-dense tradeoff the package embodies structurally:
data that (almost) every entity has lives in component storage; membership
that a *subset* has, and that a system sweeps, is a bag (a materialized
dense set). `specs` surfaces the choice as a type annotation the author must
reason about; the package folds the common case (hot subset iteration) into
a named primitive so the decision is "is this a swept set? → bag" rather
than "which of four storage backends?". Same physics, less per-component
deliberation.

## What Transfers And What Does Not

Convergence to lean on: generational handles are the mainstream ECS choice,
not a bespoke one — the package is on solid ground. Storage/pipeline-native
change tracking beats call-site marking decisively; keep dirty tracking in
the hook, never expose a manual mark. External stable identity (`Uid`) is a
derived key index over generational handles, exactly the key-index-hook
shape. And immediate write-driven index maintenance coexists with real
parallel systems — the two concerns separate cleanly, so the package is
right not to absorb the scheduler.

Refinements Veloren teaches: put change *detection* in the hook body
(compare + early-out), because storage-level access-flagging over-syncs;
and materialize the hot swept sets as bags rather than re-joining presence
every tick, taking the explicitness of markers without the hand-maintenance.

Does not transfer: the dispatcher DAG and parallel `Join` (system scheduling
is the game's concern, out of charter); network packet/delta encoding and
DB conversion (consumer policy). And the storage-wrapper approach to change
events, while elegant, is deliberately *not* copied — a wrapper sees only
its own component, whereas a hook sees the whole entity and can drive bags,
key indexes, and dirty marks from one write. The package trades `specs`'
zero-config storage flag for the hook's reach.

## Mechanism Map

| Veloren (`specs`) | ECS.Indexed |
|---|---|
| `Entity` = generational index (slot + generation), recycled | `EntPtrIdx`/`EntMutIdx` generational liveness |
| `Uid` (`NonZeroU64`) + `IdMaps` `Uid → Entity` | key index over handles (`AddPre<Guid, Id>`) |
| `FlaggedStorage` change events → `UpdateTracker` BitSets | dirty tracking inside the hooked write pipeline |
| `Modified`-on-`get_mut` (over-approximate) | hook does its own change detection + early-out |
| membership by `Join`/`.maybe()` over presence | marker-derived, hook-maintained, materialized bag |
| `DispatcherBuilder` system-dependency DAG | out of scope — game schedules systems; hooks stay immediate |
| change-sync (delta) + DB persistence (snapshot), same comps | one `Set` pipeline, many consumers, each its own strategy |
| `VecStorage` / `DenseVecStorage` per-component density choice | dense component storage vs bags, as a named primitive |
| storage wrapper sees one component | hook sees the whole entity, drives all indexes from one write |
