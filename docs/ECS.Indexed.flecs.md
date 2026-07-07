# ECS.Indexed Case Study: flecs

flecs (C/C++ ECS, `v4.1.6`, the other reference ECS alongside
[EnTT](ECS.Indexed.EnTT.md)) audited as a seventh case study for
[ECS.Indexed.md](ECS.Indexed.md). It is the sharpest test in the survey of
the package's central architectural bet, because flecs makes the one choice
the main document explicitly rejected — **archetype/table storage**, where an
entity's components live in a table keyed by its exact component set and
adding or removing a component physically *moves* the entity to a different
table. The result is a study with two faces. flecs's **hook layer converges
on the package even harder than EnTT did** — it has the pre-set hook EnTT
lacked, arrived at independently — while its **storage layer demonstrates,
concretely and quantifiably, the costs the package's rejection of archetypes
avoids**: per-component relocation machinery, deferred commands, and
table-granular change detection. File references are to the flecs repo
(`flecs/src/...`, `flecs/include/flecs.h`, manuals under `flecs/docs/`).

## The Architecture In Brief

`ecs_entity_t` is a 64-bit **generational identifier** — low 32 bits index
(`ECS_ENTITY_MASK`), high bits a 16-bit generation (`ECS_GENERATION`,
`ECS_GENERATION_INC`, `api_defines.h:372`), recycled with the generation
bumped so stale ids fail validation (the package's liveness model, again).
Components live in **archetype tables**: all entities with the exact same set
of components share a table of packed columns, and a structural change runs
`flecs_table_move` (`src/storage/table.c:2011`, called from
`src/entity.c:220`) to copy the entity's columns to the destination table.
Reactivity is **two-tiered**: per-component *hooks* (`ecs_type_hooks_t`,
`flecs.h:963`) and general *observers* (`OnAdd`/`OnSet`/`OnRemove` events).
On top sits the machinery the package deliberately does not have —
**relationships** (entity pairs, `ChildOf`, `IsA`), a **query engine** with
caching and a DSL, **pipelines/systems** with phases, and a **deferred
command queue** that batches structural changes during iteration.

## Lessons

### flecs Independently Added The Pre-Set Hook EnTT Lacked

The EnTT study's sharpest finding was that EnTT's single post-only `on_update`
signal cannot read the old value, which is why the package splits `Set` into
pre and post. flecs closes that exact gap with a dedicated hook. Its
`ecs_type_hooks_t` (`flecs.h:963`) carries `on_add`, `on_set`, `on_remove` —
and **`on_replace`**, documented as "invoked with the existing and new value
before the value is assigned. Invoked after on_add and before on_set"
(`flecs.h:1013`). That is the package's pre-set hook precisely: old value
readable, new value in hand, before the write — the signature the key index
needs to remove the stale entry.

The convergence goes one level deeper, into the bypass problem. The package
warns that indexed scopes "must mutate through `EntPtrIdx`/`EntMutIdx`" and
keeps the raw handle internal so a write cannot skip the pre-hook. flecs hits
the identical hazard and resolves it the same way: registering `on_replace`
"prevents using operations that return a mutable pointer to the component,
like `get_mut()`, `ensure()`, and `emplace()`" (`flecs.h:1015`). A mutable
pointer is a pre-hook bypass; flecs makes the pre-hook and the raw pointer
mutually exclusive, the package makes the raw handle unreachable. Two
independent libraries, the same pre-set hook motivated by the same key-index
need, closing the same bypass. This is the strongest external validation the
pre+post split has.

(flecs adds one hook the package rejects on purpose: `on_validate`, which
returns false to cancel `on_set` (`flecs.h:1021`) — a veto. The package's
contract is "hooks must not throw, no rollback"; a cancelling hook is a
rollback path the package declines, keeping the pipeline bare.)

### Hooks Versus Observers: flecs Names What The Package's Hooks Are

flecs runs two reactive tiers and its manual draws the line explicitly
(`docs/ObserversManual.md:114`): hooks "are part of the interface of a
component… the counterpart to OOP methods in ECS," one per component per
event, "priority treatment: always invoked before observers -or in the case
of a remove operation- after observers," efficient, and permitted to mutate
the component. Observers are "a mechanism that enable *other* parts of the
application to respond," many per event, matching multi-term queries, and
"should never mutate a component."

This taxonomy clarifies the package precisely: **the package's hooks are
flecs hooks, not flecs observers.** Component-local, privileged, synchronous,
mutation-capable, registered at load time — flecs's own rules echo the
package's contracts almost verbatim: "once a component is in use, hooks
cannot be changed" is the package's "registration is load-time," and "hooks
can mutate a component" is the package's supported reentrancy. What the
package *omits* is the observer tier — dynamic multi-term query subscriptions
— and that omission is exactly the "not a query language, not a scheduler"
boundary the main document draws. flecs having both, and articulating why
they are "almost opposites," confirms the package built the right one and
consciously left the other to the game. (One minor divergence: flecs allows a
single hook per event; the package allows a registration-ordered list per
`(T, N)`, because loaders compose trackers and indexes on the same
component.)

### The Archetype Move Tax Is The Cost The Package's Slots Avoid

Half of `ecs_type_hooks_t` is value-relocation machinery — `ctor`, `dtor`,
`copy`, `move`, `copy_ctor`, `move_ctor`, `ctor_move_dtor`, `move_dtor`
(`flecs.h:963`) — and it exists for one reason: archetype storage physically
moves component values between tables on every structural change
(`flecs_table_move`, `table.c:2011`). Add a component and the entity's every
other component is copied or moved to a new table; `ctor_move_dtor` and
`move_dtor` are documented as precisely the "moved to a new table" and "moved
during a remove" cases (`flecs.h:975`, `:983`).

The package's dense-slot storage moves nothing on a structural change. Adding
a marker flips a bool in the entity's existing slot and updates one bag's
dense array; the entity never relocates, its other components never move. So
the package needs **zero** of flecs's eight relocation hooks. This is the
concrete, countable form of the main document's archetype rejection: the
query/archetype ECS "still needs bags to avoid scans, large migration for no
demonstrated need." flecs quantifies the "large" — an entire per-component
move/copy protocol whose sole customer is the table layout. The package buys
cache-friendly component adjacency a different way (per-arena page-clustered
slots) without paying relocation on mutation. When evaluating any future
"should we go archetypal for iteration speed" pressure, this is the bill:
archetypes trade mutation cost for iteration adjacency, and the package
already has the adjacency.

### Deferred Commands Are The Rejected Deferral Path, Made Real

Because you cannot move an entity between tables while iterating one, flecs
defers structural changes: operations inside iteration are enqueued into a
command buffer (`ecs_defer_begin`/`ecs_defer_end`, `src/commands.c`) and
applied at sync points. The manual states the consequence plainly
(`ObserversManual.md:2158`): "when operations are deferred, because observers
are always executed when the operation is executed, invoking the observer
will also be delayed… most operations are deferred, [so] most observers will
also be invoked during sync points." And it names the reliability cost
(`:109`): "command batching impact[s] how and when events are emitted."

This is the exact design the main document rejected under "Deferred index
maintenance… breaks same-frame visibility that Craftdig gameplay relies on
(spawn then read the bag the same frame). New latency bug class,
gameplay-visible. Rejected." flecs is that path realized: correct, scalable,
and parallelism-friendly, but observer effects land at sync points rather
than at the write, and batching makes the timing non-deterministic. The
package's immediate hooks plus the staged-iteration contract get
same-frame visibility because dense-slot storage *tolerates* immediate
membership mutation — only the bag's own array shifts, and the iteration
contract covers that one hazard. flecs had to defer because archetype tables
cannot tolerate it. The two are the same fork the main document analyzed, and
flecs is the concrete evidence that the rejected branch has exactly the cost
the document predicted.

### Table-Level Change Detection Is The Coarse End Of A Spectrum

flecs offers change detection, and its granularity is set by the storage:
"Changes are tracked at the table level, for each component in the table.
While this is less granular than per entity tracking, the mechanism has
minimal overhead" (`docs/Queries.md:3353`). The caveat is the tell
(`:3379`): a query with `out`/`inout` terms always marks its tables dirty
just by iterating, so change detection is only useful with `in` terms — the
same over-marking hazard EnTT's access-flagging and Veloren's `get_mut`
over-flagging showed, here at table scale (one changed entity dirties its
whole table for the query).

Place the four ECS studies on a granularity axis and the package sits at the
precise end: flecs tracks per *table* (cheapest, coarsest), Veloren and EnTT
per *entity* via storage events (medium, but access-based so still
over-reports), and the package's dirty tracker compares value-in-hook and
marks per entity *only on real change* (most precise). Each step toward
precision costs a comparison; each step toward coarseness over-processes
downstream. The package deliberately spends the comparison, in the hook body,
because its downstream consumer (incremental serialization) is expensive per
false positive. flecs spending nothing and re-processing whole tables is the
right call for its consumer (bulk query skipping); the lesson is that change-
detection granularity should be chosen to match the consumer's per-item cost,
and the package's hook placement is what makes the precise choice available.

### Tags Are Markers — But flecs Hooks Do Not Fire On Them

A flecs tag is a zero-size component, added and removed like any component,
membership meaning has-the-tag (`docs/Quickstart.md:468`) — the marker
concept the package's bags are built on. But the hooks/observers list carries
a restriction: "hooks can only be configured for components, not tags"
(`ObserversManual.md:120`). A zero-size tag cannot carry an `on_add` hook, so
tag-driven reactions must go through the heavier observer tier.

The package has no such split. Its markers are ordinary `bool` components with
full hook support, so a bag is maintained entirely by the marker's own post
hook — the same mechanism as any data component, no second tier required. The
lesson is small but load-bearing for the bag design: keeping markers as
first-class hooked components (rather than a distinct zero-size kind) is what
lets bag maintenance ride the ordinary `Set` pipeline. flecs's tag/component
asymmetry is a reminder not to introduce a special marker kind that the hook
pipeline cannot see.

### Singletons And The Query Boundary — Two Confirmations

Two smaller findings each close a thread from earlier studies. **Singletons
are first-class** in flecs: `EcsSingleton` marks a component retrievable
without naming an entity (`docs/Quickstart.md:1205`) — the same conclusion
every prior study reached the hard way (Brogue's global player, NetHack's
`youmonst`, Angband's index −1), and the shape the package endorses
(singletons in normal storage with a marker). **Relationships and queries are
the drawn boundary**: flecs's flagship features — entity pairs, `ChildOf`/
`IsA` hierarchies, the query DSL, cached queries, pipeline phases — are
precisely the "multi-component query DSL, scheduler, command buffers" the
main document lists under Rejected. flecs is the fully realized version of
what lies past the package's boundary, and it is a materially larger, more
complex system (relationship cleanup policies, query caching invalidation,
deferred pipelines). Seeing it whole confirms the boundary is a project
boundary, not a missing feature: the package is an indexed-mutation layer, and
flecs past `on_replace` is a different, larger kind of thing.

## What Transfers And What Diverges

Corroborated, decisively: the pre-set hook is not a Craftdig quirk but a
mechanism the leading archetype ECS independently added, bypass consequence
and all; the hook-versus-observer split names the package's single tier as
the *hook* tier and validates omitting the observer tier; generational
handles over recycled ids are again the identity model; first-class
singletons are the settled answer.

Divergences, each now backed by flecs as the worked example of the road not
taken:

1. **Slots, not archetypes.** flecs's eight relocation hooks and its
   table-move on every structural change are the concrete price of archetype
   adjacency; the package's slots pay none of it and get adjacency from
   per-arena paging instead.
2. **Immediate hooks, not deferred commands.** flecs must defer because tables
   cannot be restructured mid-iteration; the package's slots can, so it keeps
   same-frame visibility and deterministic hook timing, fencing iteration with
   the staging contract instead of a command queue.
3. **Precise per-entity change detection in the hook**, not table-level — the
   most granular point on the spectrum, chosen because the package's
   serialization consumer is expensive per false positive.
4. **Markers as hooked bool components**, not a zero-size tag kind the hooks
   cannot see.

Out of scope by charter, and confirmed as a genuine project boundary rather
than a gap: relationships/pairs, the query engine and DSL, pipelines and
phase scheduling, and the deferred command infrastructure. The package
borrows flecs's hook shape (and takes heart that flecs reached for
`on_replace` too) while declining the archetype storage that forces flecs's
relocation, deferral, and coarse-tracking costs.

## Mechanism Map

| flecs (`v4.1.6`) | ECS.Indexed |
|---|---|
| `ecs_entity_t` = index + 16-bit generation, recycled | `EntPtrIdx`/`EntMutIdx` generational liveness |
| `on_add` / `on_set` / `on_remove` hooks | post-set / post-set / pre-dispose hooks |
| `on_replace` (old + new value, pre-assign) | the pre-set hook — same key-index motivation |
| `on_replace` disables `get_mut`/`ensure`/`emplace` | raw handle kept internal to close the bypass |
| `on_validate` veto (cancel on_set) | rejected — hooks must not throw, no rollback |
| hooks (privileged, 1×) vs observers (general, N×) | the package's single tier is the *hook* tier |
| archetype table move + 8 relocation hooks per type | dense slots — zero relocation on structural change |
| deferred command queue, effects at sync points | immediate hooks + staged iteration, same-frame visible |
| table-level change detection (coarse, cheap) | per-entity compare-in-hook dirty mark (precise) |
| tags = zero-size markers, but unhookable | markers = hooked `bool` components, fully in-pipeline |
| `EcsSingleton` first-class | singleton in normal storage with a marker |
| relationships, query DSL, pipelines | out of scope — the drawn project boundary |
