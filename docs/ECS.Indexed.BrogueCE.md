# ECS.Indexed Case Study: BrogueCE

BrogueCE (Brogue Community Edition, C, ~42k lines) audited as an external
baseline for [ECS.Indexed.md](ECS.Indexed.md): a mature non-ECS roguelike
that solves every problem the indexed ECS formalizes, by hand. Unlike the
Craftdig analysis in the main document, this study did not shape the design;
it validates the contracts against an independent codebase's failure modes.
[ECS.Indexed.NetHack.md](ECS.Indexed.NetHack.md) (the same architecture at
large scale) and [ECS.Indexed.Angband.md](ECS.Indexed.Angband.md)
(index-based storage without generations or hooks) are the companion
studies; [ECS.Indexed.SpaceStation14.md](ECS.Indexed.SpaceStation14.md) and
[ECS.Indexed.Veloren.md](ECS.Indexed.Veloren.md) are the inverse — real
production ECS engines read for usage idioms — and
[ECS.Indexed.EnTT.md](ECS.Indexed.EnTT.md) and
[ECS.Indexed.flecs.md](ECS.Indexed.flecs.md) are the reference ECS
libraries (sparse-set and archetype), read for mechanism design. File
references are to the BrogueCE repo (`BrogueCE/src/brogue/...`).

## Not An ECS

BrogueCE has no entities, components, or systems — zero ECS vocabulary in
the source. It is the classic pre-ECS architecture:

- **Fat structs.** `creature` is a ~40-field monolith (`Rogue.h:2260`) —
  every creature carries waypoint checklists, corpse-absorption state, and
  the vampire's `carriedMonster` pointer whether it uses them or not. `item`
  is the union of every item kind's fields (weapon damage, wand charges, key
  locations). `status[NUMBER_OF_STATUS_EFFECTS]` — 27 shorts on every
  creature, membership = nonzero — is the fat-struct answer to sparse
  components: every entity pays every slot, always.
- **Flyweight catalogs for variation.** `monsterCatalog[id]` is copied by
  value into `creature.info` at spawn (`Monsters.c:65`) and then mutated in
  place; `tileCatalog`, `mutationCatalog`, `hordeCatalog`, and item tables
  drive everything else.
- **Bitflag markers.** `bookkeepingFlags` (`MB_*`), `info.flags` (`MONST_*`),
  `info.abilityFlags` (`MA_*`), cell flags, item flags — the same role bool
  marker components play, packed into unsigned longs.
- **Intrusive linked lists and scans.** Creatures live in per-level singly
  linked lists; items chain through an embedded `nextItem`. There are no
  materialized sets: every query iterates a list and tests flags.

What makes it worth recording is that it independently evolved an ad-hoc
version of nearly every mechanism in the indexed ECS, each with its cost
visible in the source.

## Lessons

### Manual Index Maintenance Is The Disease The Hook Pipeline Cures

`HAS_MONSTER`/`HAS_DORMANT_MONSTER` cell flags form a boolean spatial index
maintained by hand — 36 write sites across 7 files, on every
spawn/kill/move/teleport path. The codebase documents its own hazard:

> `Combat.c:1704` — "This must be done at the same time as removing the
> HAS_MONSTER flag, or game state might end up inconsistent."

and `monsterAtLoc` (`Monsters.c:2056`) carries a `brogueAssert(0)` "should
be unreachable" tripwire for the desync case. The flag is also only a
presence bit, not an index: a positive lookup still scans the whole monster
list. This is exactly the invariant class the post-set hook turns from
by-convention into by-construction (the spatial index in the main document's
Usage section), with a real reverse index at the end instead of a scan gate.

### Tombstones Are Deferred Membership Without Staging

`killCreature` guards itself with `MB_IS_DYING | MB_HAS_DIED`
(`Combat.c:1646`, "let's avoid overkill") — the idempotence the Dispose
contract specifies. Death does not unlink: it sets `MB_HAS_DIED`, the
iterator silently skips tombstoned creatures (`Monsters.c:921`), and a
reaper (`removeDeadMonsters`, `RogueMain.c:951`) unlinks at a safe point in
the turn loop. Two costs follow:

- The reaper cannot use the iterator — it skips exactly the nodes the reaper
  needs, so it walks raw list nodes (the comment at `RogueMain.c:952` says
  so). Baking a policy filter into the only iterator forces bypasses.
- The bit conflates "dead" with "hidden from iteration": moving a dead ally
  to `purgatory` for resurrection must first *unset* `MB_HAS_DIED` "since
  the purgatory list should be iterable" (`RogueMain.c:967`).

This validates two engine decisions: liveness is a storage-level generation
fact, not a game flag; and membership is per-bag, not a global bit. Brogue
needs the tombstone dance because mid-iteration removal would corrupt the
walk — the staged-scratch pattern is the same deferral moved into the
consumer, without contaminating iteration or the liveness bit.

### Reference Cleanup Without Generations Is A Sweep Per Death

`creature*` is stored freely (`leader`, `carriedMonster`,
`rogue.lastTarget`, `rogue.yendorWarden`), and nothing detects staleness, so
every kill hunts references eagerly: `killCreature` nulls the two globals it
knows about (`Combat.c:1655`), and `demoteMonsterFromLeadership`
(`Monsters.c:4095`) walks every monster and dormant list on every level of
the dungeon to null `follower->leader` — O(all creatures in the game) per
death, correct only if every pointer-holding site is remembered at every
kill site. This is the strongest external argument for generational handles:
a dead handle reading defaults makes a forgotten reference benign instead of
a use-after-free, and a pre-hook key index answers the reverse lookup
(leader → followers) without a sweep.

### Scopes Appear Anyway

Each `levels[i]` owns `monsters` and `dormantMonsters` lists; globals of the
same names are re-pointed to the current level's lists on stairs
(`RogueMain.c:688`), and `purgatory` is world-scoped. That is a world/level
scope hierarchy expressed as a mutable current-scope pointer. Cross-scope
work shows the seam: `demoteMonsterFromLeadership` addresses lists as
`level == 0 ? monsters : &levels[level-1].monsters`, an off-by-one mapping
every cross-level call site re-derives.

### Special-Casing A Singleton Taxes Every Loop Forever

The player is a global `creature` in no list, so every "all creatures" site
iterates as `!handledPlayer ? &player : nextCreature(&it)` (four sites in
`Monsters.c` alone). Singletons belong in the same storage and bags as
everything else; an `IsPlayer` marker bag with one member costs nothing.

### Event-Sourced Persistence Is The Other Pole From Dirty Tracking

Brogue saves are compressed input logs replayed deterministically from the
seed (`Recordings.c`); saving and replay are the same feature, and there is
no state serializer and no dirty tracking at all. The trade: every RNG call
must be replay-identical, load time scales with game length, and logic
changes invalidate old saves (`EXIT_STATUS_FAILURE_RECORDING_OOS`). The
dirty-tracking hooks in the indexed ECS exist because AlvorKit games sit at
the opposite pole — incremental state serialization. Both poles are
coherent; mixing them is not.

### Catalog-Copy Vs Component Divergence

Terrain composes by flyweight — four layer enums per cell, `terrainFlags(p)`
ORs the catalog rows at query time (`Globals.c:581`), zero per-cell storage
beyond the enums. Creatures do the opposite: the whole catalog row is copied
into `creature.info` and mutated in place (mutations scale stats, negation
strips ability flags), so "what diverged from the archetype" is unanswerable
and negation recovery needs lossy side bookkeeping (`wasNegated`,
`totalPowerCount`). Games on the indexed ECS should keep archetype data in
shared tables and put divergence in components, where set/unset is
observable and the delta is the data.

## The Scale Caveat

It cuts both ways: at Brogue's size — tens of creatures per level,
turn-based, single-threaded — scans-plus-flags is a perfectly good
architecture, and its costs show up not as performance but as invariants
maintained by comment ("must be done at the same time"), assertion
tripwires, and whole-dungeon sweeps. Those are the costs the indexed ECS
converts into contracts.

## Mechanism Map

| BrogueCE | ECS.Indexed |
|---|---|
| `HAS_MONSTER` cell flags, 36 hand-written sites | post-set hooks maintaining a spatial index |
| `MB_HAS_DIED` tombstone + iterator skip + reaper | generation liveness + staged-scratch iteration |
| kill-time pointer sweeps (`demoteMonsterFromLeadership`) | generational handles + pre-hook key indexes |
| per-level lists + re-pointed globals | scope hierarchy with per-scope arenas and bags |
| `monsters`/`dormantMonsters`/`purgatory` lists | marker bags |
| `bookkeepingFlags`/`MONST_*`/`MA_*` bitflags | bool marker components |
| `status[27]` dense array on every creature | sparse per-component storage |
| `killCreature` overkill guard | idempotent Dispose contract |
| input-replay saves | dirty-tracking persistence hooks (opposite pole) |
