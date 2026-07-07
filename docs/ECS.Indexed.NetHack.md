# ECS.Indexed Case Study: NetHack

NetHack (this repo: the NetHack 5.0 development branch, C, ~260k lines in
`src/` alone, lineage back to Hack, 1984) audited as a second external
baseline for [ECS.Indexed.md](ECS.Indexed.md). Where
[BrogueCE](ECS.Indexed.BrogueCE.md) shows the fat-struct architecture at
small scale, NetHack shows what four decades of growth do to it: the
codebase converged, one retrofit at a time, on real indexes, explicit
membership fields, stable ids, optional component blocks — and a runtime
auditor to hold the conventions together.
[ECS.Indexed.Angband.md](ECS.Indexed.Angband.md) completes the hand-rolled
set with index-based storage that stops just short of generational handles,
and [ECS.Indexed.SpaceStation14.md](ECS.Indexed.SpaceStation14.md) with
[ECS.Indexed.Veloren.md](ECS.Indexed.Veloren.md) are the inverse studies —
real production ECS engines read for usage idioms — and
[ECS.Indexed.EnTT.md](ECS.Indexed.EnTT.md) and
[ECS.Indexed.flecs.md](ECS.Indexed.flecs.md) are the reference ECS
libraries (sparse-set and archetype), read for mechanism design. File
references are to the NetHack repo (`NetHack/src/...`,
`NetHack/include/...`).

## Not An ECS — But Converging On One

The core is classic: `struct monst` (`monst.h:97`) and `struct obj`
(`obj.h:35`) are fat structs on intrusive chains (`nmon`, `nobj`, plus a
union of context back-pointers — `nexthere`/`ocontainer`/`ocarry`). But
three choices diverge from BrogueCE and point toward components:

- **Archetype by pointer, not copy.** `struct permonst *data` points into
  the shared `mons[]` catalog; per-instance divergence lives in `monst`
  fields, and shapeshifters keep their original index in `cham`
  (`monst.h:102`). This is the shape the main document recommends — shared
  tables, divergence on the entity — and NetHack demonstrates both its
  benefit (polymorph = swap the pointer) and its residual cost (`replmon`,
  below).
- **`mextra`/`oextra`: sparse components, arrived at backwards.** Role data
  outgrew the struct, so it moved to an optional heap block
  (`mextra.h:205`): `edog` (pet), `eshk` (shopkeeper), `epri` (priest),
  `egd` (vault guard), `emin`, `ebones`, `mgivenname`, each with a
  `has_edog(mon)`-style test (`mextra.h:227`) — `Has<T, N>` in all but
  name. What is missing is the other half: storage, hooks, and queries.
  "All monsters with `edog`" is still a scan of `fmon` with a test per
  node.
- **Membership as data.** Every object carries `where` — "where the object
  *thinks* it is" (`obj.h:74`): `OBJ_FREE`/`FLOOR`/`CONTAINED`/`INVENT`/
  `MINVENT`/`MIGRATING`/`BURIED`/`ONBILL`, plus two liveness states grown
  late (`OBJ_LUAFREE` — freed but still referenced by Lua — and
  `OBJ_DELETED`, pending the reaper). Monsters gained the same thing as
  `mstate` (`MON_FLOOR`/`OFFMAP`/`DETACH`/`MIGRATING`/`LIMBO`...,
  `monst.h:59`), whose declaration comment still says "debugging info" while
  the sanity checker treats it as normative. A comment at `monst.h:219`
  shows the direction of travel: the vault-guard-parked-at-(0,0) positional
  hack is slated for replacement by an explicit `MON_PARKED` bit. This is
  the hand-rolled bag back-index (`EntIdxBagIndex<N>`) — and "thinks it is"
  concedes the failure mode it exists to catch.

## Lessons

### A Real Index Arrives With The Full Maintenance Burden

Unlike BrogueCE's presence bit, NetHack keeps true O(1) reverse indexes:
per-cell pointer grids `level.monsters[COLNO][ROWNO]` and
`level.objects[COLNO][ROWNO]` (`rm.h:474`), read through `m_at(x, y)`
(`rm.h:516`). They are maintained by hand at every place/remove/move/
polymorph/worm-segment site. The codebase knows what that costs:
`EXTRA_SANITY_CHECKS` wraps even `place_worm_seg` in an "over mon" tripwire
(`rm.h:519`), and the index shape leaks into the rules — the `#if 0` block
at `rm.h:504` documents that one slot per cell "wouldn't allow buried
monster and surface monster at same location". An index the game maintains
by convention constrains design *and* needs auditing; an index maintained
by hooks is a consumer, not a constraint.

### The Sanity Checker Is The Price Of By-Convention Invariants

`sanity_check()` (`wizcmds.c:1461`) runs **every turn** when the wizard-mode
option or the fuzzer is active (`allmain.c:193`) and audits everything:
hero, objects, timers, monsters, light sources, ball-and-chain, traps,
engravings, level terrain. `mon_sanity_check` (`mon.c:258`) is fully
bidirectional — every `fmon` monster must be at its claimed grid cell, and
every grid pointer must be in `fmon` with matching coordinates — with the
singleton exemptions hand-coded (the steed is in `fmon` but *not* on the
map; the parked vault guard is at (0,0)). `obj_sanity_check`
(`mkobj.c:2949`) cross-checks each chain against the `where` field and worn
masks against equipment slots. This is the main document's "debug-mode
bag/marker consistency audit" bullet, shipped as a production subsystem —
proof both that manual index maintenance desyncs in practice and that an
auditor is the mitigation of last resort when invariants are not
structural. The backstop hook makes the equivalent desync impossible to
write, which is why the engine's auditor stays a someday-bullet instead of
a subsystem.

### Tombstones At Scale: 272 Filter Sites

`DEADMONSTER(mon)` is `mhp < 1` (`monst.h:215`); death marks, a deferred
reaper unlinks — `dmonsfree` (`mon.c:2492`) at end of turn
(`allmain.c:188`, `cmd.c:1032`), before save, before bones. The filter is
decentralized: 272 `DEADMONSTER` occurrences across 49 files in `src/`,
every loop remembering to skip. BrogueCE sits at the other end of the same
dial — filter baked into its one iterator, reaper forced to bypass it.
Neither can say "alive" as a storage fact, so one repeats the check
everywhere and the other contorts to un-check it. The reaper also keeps a
pending-dead counter and cross-checks it on every sweep
(`mon.c:2512`, "dmonsfree: %d removed doesn't match %d pending") —
bookkeeping about the bookkeeping. Objects later grew the identical
machinery: `OBJ_DELETED` plus `dobjsfree` (`mkobj.c:2831`), which panics on
a non-deleted entry in the deleted chain.

### Persistence Forces Ids Into A Pointer World

Runtime identity is the pointer, but `m_id`/`o_id` monotonic ids exist
anyway, because pointers cannot cross a save file or a level file. The two
representations coexist and translate at O(n): leashes bind by id
(`obj->leashmon`, resolved by `find_mid` scanning `fmon` — `apply.c:939`,
`light.c:376`), shop bills by `bo_id` via `find_oid` (`shk.c:3243`). Timers
and light sources attach to entities by pointer, save as ids, and are
relinked on restore (`relink_timers`, `relink_light_sources`,
`restore.c:741`) with panics when the graph doesn't close
("relink_light_sources: no id mapping", `light.c:544`). Scope changes pay
the same bill: levels are serialized to disk on exit and reloaded on entry
(`savelev`/`getlev` in `goto_level`, `do.c:1655`/`do.c:1716`), and the
comment at `do.c:1638` names "dangling timers and light sources" as why
even discarded levels go through `savelev`'s cleanup pass. A generational
handle collapses the dual representation: the id *is* the reference, no
find scans, no relink phase.

### `replmon`: What Pointer Identity Costs On Relocation

When a monster must move to a different allocation (`replmon`,
`mon.c:2520`), every reference site is enumerated by hand: inventory
`ocarry` back-pointers, the polearm context, the map grid, worm segments,
its light source (deleted and recreated, with the hedge "here we rely on
fact that `mtmp' hasn't actually been deleted"), the `fmon` chain,
`u.ustuck`, `u.usteed`, and shop bills (`replshk`). Miss one, dangle one.
This is BrogueCE's death-sweep lesson at ten times the reference surface,
and the same conclusion: identity must survive relocation, which is what
stable handles over slot storage provide — consumers hold the handle, only
the arena knows the address.

### The Half-Unified Singleton

The hero is both `struct you u` and a global `struct monst youmonst`
(`decl.h:1086`). The wrapper lets monster-facing code accept the hero — an
improvement over BrogueCE's fully separate player — but the unification is
partial: `m_poisongas_ok` still branches `is_you ? u.ux : mtmp->mx`
(`mon.c:342`), and the sanity checker carries permanent exemptions for the
steed and the parked guard. Partial unification halves the tax and keeps
paying it forever; a singleton that lives in normal storage with an
`IsPlayer` marker pays it once, at zero marginal cost per loop.

## The Trajectory

Read as a time series, NetHack is the scan-and-flags architecture patched
at every point the main document's contracts address, in the order the
pain arrived: real reverse indexes when scans got slow (`level.monsters`),
explicit membership fields when chains desynced (`where`, `mstate`),
tombstone reapers when mid-iteration death corrupted walks (`DEADMONSTER`,
`OBJ_DELETED`), stable ids when persistence met pointers (`m_id`,
relinking), optional component blocks when the fat struct burst (`mextra`),
and a per-turn auditor when none of the above could be trusted
(`sanity_check`). Every mechanism is real and battle-tested; every one is
maintained by hand and cross-checked at audit time rather than guaranteed
at write time. The indexed ECS is the same destination with the invariants
moved into the mutation pipeline.

## Mechanism Map

| NetHack | ECS.Indexed |
|---|---|
| `level.monsters[][]` grids, hand-maintained | post-set hooks maintaining a spatial index |
| `obj->where`, `mon->mstate` membership fields | bag back-index (`EntIdxBagIndex<N>`), internal |
| `sanity_check()` every turn under option/fuzzer | write-time backstops; debug audit stays a someday-bullet |
| `DEADMONSTER` at 272 sites + `dmonsfree` reaper | generation liveness + guarded pipelines |
| `m_id`/`o_id`, `find_mid`/`find_oid`, restore relink | the handle is the id; no translation layer |
| `replmon` manual reference enumeration | stable handles over slot storage |
| `mextra`/`oextra` with `has_*` tests | sparse components + `Has<T, N>` + bags for the query half |
| `permonst *data` shared catalog | shared tables + divergence in components (recommended shape) |
| `youmonst` wrapper + `is_you` branches | singletons in normal storage and bags |
| `savelev`/`getlev` level swap + relink phases | scope arenas + persistence hooks at teardown |
