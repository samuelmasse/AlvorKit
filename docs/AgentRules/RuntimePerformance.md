# Runtime Performance Policy

## Scope

Read this policy before changing render/update loops, polling, simulation,
resources, native boundaries, validation, allocation behavior, disposal,
deletion, unload, or teardown.

## Invariants And Resolution

Runtime deletion, disposal, unload, and scope teardown are allocation-sensitive.
Genuinely cold final process-shutdown orchestration may allocate only when the
cost is intentional and does not define a reusable runtime API. This resolves
the former contradictory teardown statements in favor of the stronger runtime
contract.

### Hot-Path Data Layout

- Never cache, precompute, or retain a value that is obtainable through a
  simple independent mathematical formula from already-held IDs, indices,
  coordinates, and constants. Compute it where needed. In particular, do not
  add full-volume companion arrays for row-major addressing, chunk addressing,
  bit positions, masks, or similar arithmetic derivations. If the formula is
  awkward at the use site, improve the representation or move the calculation
  to the appropriate cold boundary instead of materializing a lookup cache.
- Do not retain a small fixed lookup array or list when a closed mapping from
  an index, enum, coordinate delta, or similarly compact key can be expressed
  directly. Use a switch expression or a simple formula, and expose a named
  count plus an indexed operation when callers must iterate the mapping.
  Derive reverse, opposite, and related mappings from the same named
  representation instead of adding a second lookup or linear search. Retained
  tables are appropriate only when the values are authored or configured data,
  can change at runtime, or measured performance justifies the storage.
- For a dense grid whose hot loop repeatedly visits a fixed neighborhood,
  strongly prefer a blocked sentinel border around the retained data. Convert
  public coordinates or unpadded addresses to padded row-major indices in cold
  API, loading, and synchronization paths. The hot loop should queue indices,
  apply fixed row and layer offsets, and reconstruct through indexed state
  without converting back to coordinates.
- Sentinel padding removes per-neighbor world-boundary branches because every
  fixed offset lands on valid retained storage and border cells reject entry
  through the ordinary data check. Preserve the unpadded external contract and
  account for the padded capacity in every index-addressed companion array.
- Padding guarantees semantic index safety; it does not by itself prove that
  the runtime removed managed-array range checks. Inspect generated code or
  measure before adding lower-level access, and use unsafe access only for a
  demonstrated remaining bottleneck with layout invariants that make it safe.

## Runtime Allocation Discipline

Avoid managed allocations in runtime, render-loop, resource lifetime,
validation, bind/unbind, delete/dispose cleanup, polling, and other hot-path
code unless the allocation is explicitly intended and documented. This includes
arrays, `List<T>`, LINQ, iterator blocks, closures, params arrays, boxing,
string formatting, and defensive copies. Treat teardown and delete paths as
allocation-sensitive unless the user explicitly says otherwise.

When a native API passes a pointer and count for handles, ids, state values, or
other blittable data, do not copy it into a managed array just to validate,
track, delete, or forward it. Prefer `Span<T>`/`ReadOnlySpan<T>` over native
memory, `stackalloc`, caller-owned buffers, or a no-allocation scan. If a stable
snapshot is truly required, document why the allocation is acceptable and keep
it outside hot paths when possible.

When fixing an allocation-sensitive bug, solve the stated contract directly. Do
not introduce helper abstractions, diagnostic string construction, broader
validation policy, or extra state while fixing a narrow span, pointer, upload,
bind, delete, or lifetime contract. For byte-count contracts, prefer
`MemoryMarshal.AsBytes`, validate the byte count, and forward the resulting span
without allocation. For low-level runtime changes, scan touched code for
allocation constructs when practical; in a Working Mode handoff, list this scan
if it was skipped.

## Runtime Source Guidance

- Treat low-level runtime and game-loop code as if it must remain viable at
  roughly 5,000 FPS.
- Prefer structs, readonly structs, spans, ref-friendly APIs, `stackalloc`,
  pooled or caller-owned buffers, and explicit ownership where they reduce
  allocation pressure without obscuring code.
- Allocation is normally acceptable during startup, asset loading,
  configuration, diagnostics, and explicit cold loading. It is not acceptable
  merely because a path is named teardown or cleanup.
- Do not over-optimize cold code; reserve low-level techniques for real
  frame-time, allocation, ownership, or native-boundary pressure.
