# ECS.Indexed Case Study: Angband

Angband (this repo: the 4.2 line, post-4.2.6 development, C, ~190k lines in
`src/`, lineage back to Moria, 1983) audited as a third external baseline
for [ECS.Indexed.md](ECS.Indexed.md). [BrogueCE](ECS.Indexed.BrogueCE.md)
shows fat structs with scans; [NetHack](ECS.Indexed.NetHack.md) shows
pointer identity patched with indexes and an auditor. Angband is the most
architecturally advanced of the three: it stores monsters in a dense
by-value slot array addressed by integer index — an arena in all but name —
and it demonstrates precisely which problems remain when you have index
handles but no generations, and maintained derived state but no hooks.
[ECS.Indexed.SpaceStation14.md](ECS.Indexed.SpaceStation14.md) and
[ECS.Indexed.Veloren.md](ECS.Indexed.Veloren.md) are the inverse studies —
real production ECS engines (RobustToolbox, `specs`) read for usage idioms
rather than hand-rolled machinery — and [ECS.Indexed.EnTT.md](ECS.Indexed.EnTT.md)
with [ECS.Indexed.flecs.md](ECS.Indexed.flecs.md) are the reference ECS
libraries (sparse-set and archetype), read for mechanism design. File
references are to the Angband repo (`angband/src/...`).

## Halfway To An Arena

The level (`struct chunk`, `cave.h:181`) owns its entities the way an arena
does: `monsters` is a dense array of monster structs indexed by `midx`,
with `mon_max` as the high-water mark and `mon_cnt` the live count; slot 0
is reserved as "no monster". Objects are pointer-linked piles but are
*also* registered in a per-chunk index table (`objects[]`, addressed by
`obj->oidx`). Grid squares store entity references as integers
(`square.mon` is an `int16_t`, `cave.h:165`), the current actor is an index
(`mon_current`), and cross-references in game data are indexes too — an
object held by a monster stores `held_m_idx`, a mimicked object stores
`mimicking_m_idx`. `struct monster` (`monster.h:385`) carries its own index
(`midx`), a shared-catalog `race` pointer (with `original_race` preserved
across polymorph — the shape the main document recommends), a dense
`m_timed[MON_TMD_MAX]` status array, and bitflag markers (`mflag`).

Allocation is arena-shaped as well (`mon_pop`, `mon-make.c:646`): bump
`mon_max` while there is room, otherwise scan for a tombstoned slot — a
dead monster is its zeroed slot, recognized by `race == NULL`, and the
codebase skips them with literal "Paranoia" comments (64 of those across
`src/`, alongside ~1100 asserts). What is missing from the arena is exactly
two things: generations on the index, and hooks on mutation. The lessons
below are what their absence costs.

## Lessons

### Swap-Remove Where Identity Is The Index

When the array fills, `compact_monsters` (`mon-make.c:482`) deletes
far-away monsters and swap-fills holes: "move last monster into open hole"
and decrement `mon_max` — structurally identical to the dense bag's
swap-remove. But because the index *is* the identity, the move must patch
every reference by hand (`monster_index_move`, `mon-make.c:418`): the grid
square, the monster's own `midx`, its group's index entry (a fatal
`quit("Bad monster group info!")` if that fails), `held_m_idx` on every
carried object, `mimicking_m_idx`, the target tracker, and the health-bar
tracker — the last two compared *by pointer*, because Angband references
monsters by index in some places and by address in others, and both
representations dangle. The engine's bag performs the same swap-remove, but
what moves is a 16-byte handle; entity identity never changes, so the only
write is the internal bag-index component — `monster_index_move` collapsed
to one hooked `Set`.

### Capacity Is A Game Rule Without Growth

The slot array is fixed (`z_info->level_monster_max`), so capacity pressure
is handled *inside the fiction*: `compact_monsters` prints "Compacting
monsters..." and deletes real, alive, gameplay-relevant monsters that
happen to be distant; a full array makes `mon_pop` print "Too many
monsters!" and return 0, with a comment reading "Try not to crash"
(`mon-make.c:678`). Storage policy leaking into game rules is the endpoint
of arenas without growth; the engine arena grows and reserves instead.

### The Dirty-Flag System: Deferred Maintenance, Registered At Every Site

Angband's derived state — bonuses, torch radius, field of view, monster
visibility — is maintained by the classic *band flag system:
mutation sites OR bits into `player->upkeep->update`
(`PU_BONUS`, `PU_UPDATE_VIEW`, ..., `player-calcs.h:36`), and
`update_stuff` (`player-calcs.c:2565`) later dispatches each set bit to its
recompute function, with implication rules managed by hand (`PU_DISTANCE`
absorbs `PU_MONSTERS`, `player-calcs.c:2616`). This is deferred derived-
state maintenance working as designed — one recompute batches any number of
mutations — and its failure class is structural: the "registration" happens
at every mutation site, not once. `delete_monster_idx` remembers to set
`PU_UPDATE_VIEW | PU_MONSTERS` only when the dead monster emitted light
(`mon-make.c:333`); every such site must know which downstream state its
write dirties. Post-set hooks invert exactly this: the index registers once
for the components it derives from, and no mutation site knows it exists.
The main document rejected deferral for *index* maintenance (same-frame
visibility); Angband shows the other reason — per-site flag discipline is
the same forgettable convention as manual index writes.

### The Known-World Mirror: A Derived Structure The Size Of The World

Angband 4.2's flagship feature is the biggest hand-maintained derived
structure in any of these studies: `player->cave` is a complete second
chunk holding what the player *believes*, and every real object can have a
shadow (`obj->known`, `object.h:429`). The two worlds are joined by the
index table: a known object occupies the *same* `oidx` slot as its real
counterpart, and `object_lists_check_integrity` (`cave.c:503`) asserts the
pairing — same table size, matching slots, every located object present in
its square's pile. The sync surface is wide (`square_know_pile`,
`square_sense_pile`, `player_know_object`, `forget_remembered_objects`),
the deletion dance for a hallucination-imagined shadow touches four data
structures (`cave-square.c:1124`), and the integrity checker runs *inline
in production sync paths* (`cave-square.c:1171`) — not just in wizard mode
— because this machinery was the source of a generation of object-list
bugs. Two readings for the engine: this is what a maintained index looks
like scaled to "the whole world", exactly the workload hooks exist to
carry; and the mirror's *deliberate staleness is the feature* — the shadow
persists as the player's memory after the real object is gone, the same
semantics as the engine's stale-handles-after-arena-dispose contract:
staleness is coherent when it is checked deliberately.

### One Component Family Grew A Hooked Setter

Monster timed effects are never written directly: all mutation goes through
`mon_inc_timed`/`mon_dec_timed`/`mon_clear_timed` with side-effect flags
(`MON_TMD_FLG_NOTIFY`, ...) that fire messages and visibility updates — a
hand-rolled hooked setter for exactly one component family, evolved because
raw writes kept missing the side effects. But it is convention, not
structure: nothing stops `mon->m_timed[i] = v`, and no other field family
got the treatment. The hook pipeline is this pattern generalized and made
unbypassable.

### The Signed-Index Singleton

The player is not in the monster array; grid squares encode it as index
−1 (`player_place`, `player-util.c:1582`), so `square.mon` is a three-way
domain: 0 none, positive monster, negative player. Cleverer than NetHack's
dual `you`/`youmonst` and cheaper than BrogueCE's list-plus-special-case,
but every reader still branches (`square(c, grid)->mon > 0` guards
throughout), and the player remains invisible to anything that walks the
monster array. Same conclusion as the other studies, third encoding:
singletons in normal storage, marked, cost less than any special case.

### Observers Exist — Pointed At The UI

Angband has a real observer registry: typed events, multiple handlers,
`event_add_handler`/`event_signal` (`game-event.h:210`), used to decouple
frontends from the core (`EVENT_MONSTERLIST`, `EVENT_MAP`, ...). The
architecture demonstrably knows the pattern; it just never turned it
inward. State invariants inside the core — grids, index tables, the known
mirror, dirty flags — remain convention-maintained while the UI gets
hooks. The indexed ECS is that same observer discipline aimed at the state
layer itself.

## The Trajectory

Three games, one gradient: BrogueCE scans fat structs and hand-writes
presence bits; NetHack adds real indexes, membership fields, ids, and an
auditor to keep them honest; Angband reaches index-addressed slot storage,
tombstone recycling, swap-remove compaction, API-mediated writes for one
hot family, a batched dirty-flag system, and an observer registry — every
load-bearing piece of an indexed ECS, present but disconnected: indexes
without generations, deferral without registration, hooks without state,
integrity by assert instead of by construction. The engine package is these
parts connected: the index becomes a generational handle, the per-site
flags become per-component hooks, the inline auditor becomes a write-time
backstop, and the conventions become contracts.

## Mechanism Map

| Angband | ECS.Indexed |
|---|---|
| `chunk->monsters[]` dense slot array, slot 0 reserved | arena slot storage, reserved sentinel |
| `midx` self-index, `held_m_idx` cross-references | generational handles; identity survives moves |
| `race == NULL` tombstone + `mon_pop` slot recycling | generation bump on dispose, free-slot reuse |
| `compact_monsters` + `monster_index_move` fixups | bag swap-remove writing one back-index component |
| "Too many monsters!" capacity policy | growing arena; no gameplay leakage |
| `PU_*` dirty bits set at every mutation site | post-set hooks registered once per index |
| `player->cave` known mirror joined by `oidx` | maintained indexes via hooks; deliberate stale reads |
| `object_lists_check_integrity` inline in sync paths | write-time backstop; audit stays a debug option |
| `mon_*_timed` API with notify flags | hook pipeline on every component, unbypassable |
| player as grid index −1 | singleton in normal storage with a marker |
| `event_add_handler` for UI events | the same observer pattern, aimed at state |
