# ECS.Indexed

`AlvorKit.ECS.Indexed` is the engine package for observed mutation and
maintained indexes on top of `AlvorKit.ECS`. It is the engine extraction of the
indexed ECS layer proven in Craftdig (`Craftdig.World/Ent`), rebuilt in the
shape we want rather than copied: Craftdig informs the design, and migration
must stay possible, but the engine contract is the authority.

Strategy: **build this package first**, then migrate Craftdig onto it. The
analysis behind every decision and the full implementation plan are part of
this document.

The base `AlvorKit.ECS` package owns storage, handles, generated components,
and arena lifetime, and stays zero-overhead. `AlvorKit.ECS.Indexed` adds:

- typed pre-set, post-set, and pre-dispose hooks per component
- dense marker bags maintained automatically from component changes
- an indexed arena and indexed handles that drive the hook pipeline

`AlvorKit.ECS.Indexed` does not depend on injection, scopes, UI, rendering, or
persistence. Games bind its types into scopes. It is not a scheduler, a query
language, or a persistence framework; the goal is that scopes maintain named
active sets and indexes automatically when components change.

## Public API

### Handles

```csharp
public readonly record struct EntPtrIdx : IEntMut, IDisposable
{
    public static implicit operator EntMutIdx(EntPtrIdx a);
    public static implicit operator Ent(EntPtrIdx a);

    public bool IsAlive { get; }
    public EntHandle Handle { get; }

    public void Set<T, N>(in T value);   // hook pipeline, see contracts
    public bool Unset<T, N>();           // hook pipeline, see contracts
    public T? Get<T, N>();
    public bool Has<T, N>();
    public void Dispose();               // hook pipeline, see contracts
}

public readonly record struct EntMutIdx : IEntMut
{
    public static implicit operator Ent(EntMutIdx a);
    // wraps an EntPtrIdx; identical Set/Unset/Get/Has behavior, no Dispose
}
```

Both handles carry the context entity, so hooks travel with the handle from its
`Alloc` site. `EntMutIdx` is what hooks receive and what bags store; it cannot
dispose the entity. Both keep the `EntDebugView` debugger proxy.

### Hook Delegates

```csharp
public delegate void EntIdxPreHook<T>(EntMutIdx ent, in T value);
public delegate void EntIdxPostHook(EntMutIdx ent);
public delegate void EntIdxPreDisposeHook(EntMutIdx ent);
```

Pre hooks take the new value by `in` to avoid copying large component structs
per hook per call. This diverges from Craftdig (`Action<EntMutIdx, T>`) on
purpose: delegate types are the public API, and changing them later is the one
breaking change that gets more expensive with every consumer. Post and dispose
hooks have the same shape as Craftdig; method groups adapt without edits.

### Context Builder

```csharp
public class EntIdxContextBuilder
{
    public EntObj Ent { get; }

    public void AddPre<T, N>(EntIdxPreHook<T> hook) where N : IComponent;
    public void AddPost<T, N>(EntIdxPostHook hook) where N : IComponent;
    public void AddPreDispose(EntIdxPreDisposeHook hook);

    public void AddBag<N>(EntIdxBagMut<N> bag)
        where N : IComponent;                     // membership = marker
    public void AddGatedBag<N, TGate>(EntIdxGatedBagMut<N, TGate> bag)
        where N : IComponent where TGate : IComponent;  // marker && gate

    protected void Add<P, PT>(PT hook);           // internal machinery
}
```

Naming is deliberate and truthful:

- `AddBag<N>` — plain marker bag. Contains every live entity whose marker is
  true. This replaces Craftdig's `AddBagUnloaded`, whose name wrongly read as
  "unloaded-only" when it meant "loaded-or-not".
- `AddGatedBag<N, TGate>` — the general primitive: marker && gate. Any bool marker
  can gate a bag (`IsReady`, `IsActive`, ...). `TGate` is not inferrable from
  every call shape, but named bag wrapper types usually infer cleanly.

The bag identity is the marker plus its gate. `AddBag<N>` owns the plain
marker-only identity, while `AddGatedBag<N, TGate>` owns
separate gated identities. This means a scope may maintain `all monsters`,
`ready monsters`, and `visible monsters` as distinct bags over the same marker;
only an exact duplicate `(marker, gate)` registration is rejected.

The bag parameter type carries the gating semantics; the static type of a
builder reference must never decide whether a bag is plain or gated.

Hook lists are stored as `ReadOnlyMemory<delegate>` components on the
builder's `EntObj` context entity, keyed by internal marker types
(`EntIdxPre<T, N>`, `EntIdxPost<T, N>`, `EntIdxPreDispose`). This is
Craftdig's design and it is kept intentionally: it gives O(1) per
(context, T, N) hook lookup with no dictionaries, per-context isolation for
free, and reference cleanup through the existing `PageRefFields` machinery
when the context entity dies. The marker types are engine implementation
detail and are `internal`, unlike Craftdig where they are public.

### Bags

```csharp
public class EntIdxBagMut<N> where N : IComponent
{
    public ReadOnlySpan<EntMutIdx> Ents { get; }
    public int Count { get; }
    public bool Contains(EntMutIdx ent);

    internal void Add(EntMutIdx ent);
    internal void Remove(EntMutIdx ent);
}

public class EntIdxGatedBagMut<N, TGate>
    where N : IComponent
    where TGate : IComponent
{
    public ReadOnlySpan<EntMutIdx> Ents { get; }
    public int Count { get; }
    public bool Contains(EntMutIdx ent);

    internal void Add(EntMutIdx ent);
    internal void Remove(EntMutIdx ent);
}

public class EntIdxBag<N>(EntIdxBagMut<N> bag) where N : IComponent
{
    public ReadOnlySpan<EntMutIdx> Ents { get; }
    public int Count { get; }
    public bool Contains(EntMutIdx ent);
}

public class EntIdxGatedBag<N, TGate>(EntIdxGatedBagMut<N, TGate> bag)
    where N : IComponent
    where TGate : IComponent
{
    public ReadOnlySpan<EntMutIdx> Ents { get; }
    public int Count { get; }
    public bool Contains(EntMutIdx ent);
}
```

`Add`/`Remove` are `internal`: bag membership is derived state, maintained
only by the interceptors that registration installs. Craftdig already obeys
this rule by convention (verified: no gameplay call sites); the engine makes
it a compile-time rule so a bag can never disagree with its marker. The
`Mut`/read split follows AlvorKit style: the `Mut` type is what a loader
registers, the read type is what systems inject.

### Arena

```csharp
public class EntIdxArena : IDisposable
{
    public EntIdxArena(EntObj context);

    public int Allocated { get; }
    public bool IsAlive { get; }

    public virtual EntPtrIdx Alloc();
    public virtual void Dispose();
}
```

The arena holds the context `EntObj` in a strong field. This is a required
invariant, not a convenience: handles only carry the value-typed `Ent` view of
the context, which does not keep the `EntObj` alive. If nothing referenced it,
the finalizer would recycle the context entity and every hook in the scope
would silently stop firing. Craftdig gets this right today only implicitly,
through primary-constructor capture.

`EntIdxArena` implements `IDisposable` properly (Craftdig has a `virtual
Dispose()` without the interface) and exposes `IsAlive`.

### Registration Errors

```csharp
public class EntIdxRegistrationException : Exception;
```

Thrown at registration time (load time) for: a marker or gate whose generated
value type is not `bool` in `AddBag`/`AddGatedBag`, a `(T, N)` pair where
`N.Component.ValueType != typeof(T)` in `AddPre`/`AddPost`, and a duplicate bag
registration for the same marker+gate identity on the same context. All checks
read the `IComponent.Component` static metadata, so they cost nothing after
loading.

Without these checks the failures are silent: a mistyped `(T, N)` pair
registers hooks that no write ever fires, and a non-bool gate makes a bag
permanently empty.

## Mutation Contracts

These are normative. Tests in `AlvorKit.ECS.Indexed.Test` pin each one.

### Set

```
Set<T, N>(in value):
    if not IsAlive: return                    // no hooks on dead handles
    run pre hooks for (T, N) with value       // old value still readable
    base Set<T, N>(value)
    run post hooks for (T, N)                 // observe current state
```

The liveness guard is a deliberate fix over Craftdig, where hooks run even
when the base write will no-op. That gap lets a `Set<Guid, Id>` on a dead
handle insert a permanently stale entry into a GUID index (the pre hook reads
the old id as `default`, skips the remove, and adds the dead handle under the
new id). Dead handles must be inert end to end.

Set does not perform change detection; hooks that need it compare against
`ent.Get<T, N>()` themselves (the dirty-tracker pattern). Equality is not free
or definable for every `T`, and most hooks early-out cheaper than the pipeline
could.

### Unset

```
Unset<T, N>():
    if not IsAlive or not Has<T, N>: return false
    run pre hooks for (T, N) with default(T)  // old value still readable
    base Unset<T, N>
    run post hooks for (T, N)                 // observe absent state
    return true
```

This replaces Craftdig's `Set(default)` + `Unset` composition, which had three
defects: unsetting an absent component momentarily *created* it (the base set
stamps the generation), the return value was `true` even when nothing was
present, and hooks fired for no-op unsets. It also cleans the observable
order: post hooks now see the honest final state (`Has == false`), where
Craftdig's post hooks saw present-with-default. Every audited consumer reads
via `Get` and behaves identically under both orders; `Has`-based hooks only
work correctly under the new one.

### Dispose

```
Dispose():
    if not IsAlive: return                    // idempotent
    run pre-dispose hooks                     // entity fully intact
    Clear()                                   // per-component unset pipeline
    base Dispose()                            // generation bump, slot return
```

The liveness guard fixes another Craftdig gap: the base `EntPtr.Dispose` is
CAS-guarded, but the hook layer was not, so a double dispose re-fired
pre-dispose hooks and re-ran `Clear` on a dead handle. Craftdig survives by
accident (its dispose tracker reads `Ploc` as null on dead handles).

Pre-dispose hooks run while every component is still readable — this is where
persistence erase and network teardown belong. `Clear` then fires the full
unset pipeline per present component, in page-field registration order, which
is effectively arbitrary; hooks must not assume cross-component invariants
during dispose. Cleanup that needs the whole entity goes in pre-dispose;
cleanup keyed to one component goes in that component's hooks.

### Clear Fires Hooks — A Hard Contract

`EntMutate.Clear()` from the base package dispatches `field.Unset(ent)`
through the `IEntMut` constraint, which lands on `EntPtrIdx.Unset` and runs
the hook pipeline for every present component. The indexed layer depends on
this for correctness, twice over:

- key indexes clean up on dispose only because unsetting `Id` fires the pre
  hook with `default`, which removes the old dictionary key
- bags clean up on dispose through the marker unset and the bag-index unset
  (see the backstop analysis below)

Any future change to `Clear` or to the `EntField.Unset` dispatch must preserve
constrained dispatch through the handle. A test locks this in.

### Arena Dispose

Disposing an `EntIdxArena` ends the owning scope: bulk page release,
generation bumps, no per-entity hooks. It is also the performance escape hatch
for mass teardown — per-entity dispose costs one unset pipeline per component,
arena dispose costs none.

Consequence to state plainly: arena dispose invalidates the scope's indexed
views instead of maintaining them. Bags and game-side indexes still hold
now-dead handles; they are not reset, and `Count`/`Ents` are no longer meaningful
after the owning arena is disposed. This is harmless only under the intended
lifecycle: bag and index instances die with the same scope. Consumers that
outlive the arena must check `IsAlive`, the way Craftdig's
`WorldEntPersister.Write` already does. If a game needs delete semantics
(persistence erase, index removal), it disposes the individual `EntPtrIdx`
handles before tearing the scope down.

### Hook Rules

- **Order.** Hooks run in registration order. Loaders therefore control
  ordering: register trackers before or after indexes deliberately.
- **Reentrancy is supported.** Hooks may set other components; the nested
  write runs its own full pipeline. Craftdig relies on this (the dirty tracker
  sets `IsDirty` inside a pre hook; bag removal nests an index write inside
  the unset pipeline). A pre hook that sets its own `(T, N)` recurses without
  bound — that is a bug in the hook, not something the engine detects.
- **Hooks must not throw.** There is no rollback: a pre-hook throw skips the
  write and all post hooks; a post-hook throw leaves earlier post hooks
  applied. A throw leaves indexes and storage inconsistent by design.
- **Single-threaded mutation.** The base ECS tolerates some concurrency; the
  indexed layer does not. All mutation through indexed handles and all
  registration for one context happen on one thread.
- **Registration is load-time.** Hooks registered after entities were
  allocated and mutated do not see the past; there is no retroactive scan.
  Register in loaders, before systems allocate.
- **Lazy-init getters fire hooks.** A `[ComponentLazyInitialize]` getter can
  issue a `Set` from a read path on an indexed handle. Avoid lazy-init
  components on hot hooked components.

### Bag Semantics

The dense bag keeps Craftdig's proven slot layout, but the back-index key is
per bag identity. The storage mechanics live in one internal
`EntIdxBagStore<TIndex>`; plain bags instantiate it with `EntIdxBagIndex<N>`,
and gated bags instantiate it with `EntIdxGatedBagIndex<N, TGate>` so different
gates over one marker do not collide:

```csharp
internal struct EntIdxBagStore<TIndex> where TIndex : IComponent
{
    private EntMutIdx[] ents = [default, default];
    private int count = 1;

    public ReadOnlySpan<EntMutIdx> Ents => new(ents, 1, count - 1);
    public int Count => count - 1;
    public bool Contains(EntMutIdx ent) => ent.Get<int, TIndex>() > 0;

    internal void Add(EntMutIdx ent)
    {
        ent.Set<int, TIndex>(count);
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);
        ents[count++] = ent;
    }

    internal void Remove(EntMutIdx ent)
    {
        if (!Contains(ent))
            return;

        int index = ent.Get<int, TIndex>();
        ref var last = ref ents[count - 1];
        ents[index] = last;
        last.Set<int, TIndex>(index);
        last = default;
        ent.Set<int, TIndex>(-1);
        count--;
    }
}
```

`EntIdxBagMut<N>` and `EntIdxGatedBagMut<N, TGate>` are thin public wrappers
over this store with different index key types.

Slot 0 is reserved so `0` (the unset default of the internal
`EntIdxBagIndex<...>` int component) means "never in this bag". Removal writes
`-1`, not `0` — the `-1` sentinel is the reentrancy brake: the backstop pre
hook below removes on `0` only, so the bag's own internal writes never
re-trigger removal.

`AddGatedBag<N, TGate>` registers three hooks:

1. post on `(bool, N)` — recompute membership when the marker changes
2. post on `(bool, TGate)` — recompute membership when the gate changes
3. pre on `(int, EntIdxGatedBagIndex<N, TGate>)` — the **index backstop**: remove
   from the bag when the index component is unset (pre hook receives
   `default` = 0 while the old index is still readable)

`AddBag<N>` registers 1 and the corresponding `EntIdxBagIndex<N>` backstop.
The interceptors are `internal`.

The backstop is load-bearing, not defensive. `Clear` unsets components in
arbitrary order. If the bag index is unset before the marker, the later
marker hook sees `Contains == false` (the index is already gone) and never
removes — without the backstop the bag would keep a dead handle in a slot that
`Contains` can no longer find, with `count` permanently wrong. With the
backstop, both orders converge; tests exercise both.

The internal index writes intentionally run the normal hook pipeline. The cost
is two empty hook-span fetches plus one no-op backstop invocation per
add/remove — noise. Bypassing the pipeline for internal writes is not worth an
`InternalsVisibleTo` into the base package, and suppressing the unset-time
backstop would reintroduce the ordering bug.

One bag per marker+gate identity per context, enforced: two `AddGatedBag<N, TGate>`
registrations would share `EntIdxGatedBagIndex<N, TGate>` and duplicate the same
derived state. The duplicate registration throws `EntIdxRegistrationException`
(detected by the backstop hook already existing for that bag index component on
the context). Different gates over the same marker use different index
components and are valid. Two contexts may use the same marker and gate freely —
entities belong to one context, so their index ints never collide.

### Iteration Semantics

`Ents` is a span over live storage whose **length is captured at the property
call**. Three concrete behaviors follow when membership changes while a
captured span is being walked:

1. Removing an entity swap-fills its slot from the tail and writes `default`
   into the tail slot — a captured span still covers that tail slot, so the
   walk encounters `default` handles (`IsAlive == false`, all reads default).
2. The entity swapped backward into the removed slot may already have been
   passed by the cursor — it is skipped this pass.
3. Adding can grow the array — the captured span still points at the old
   array and sees none of the changes.

The contract is therefore: do not mutate a bag's own membership (its marker,
its gate, or entity dispose) while iterating its span. Stage the work:

```csharp
private readonly List<EntMutIdx> scratch = [];

public void Stream()
{
    foreach (var ent in scratchedBag.Ents)
        scratch.Add(ent);

    foreach (var ent in scratch)
        ent.IsScratched = false;

    scratch.Clear();
}
```

Mutating *other* components during iteration is fine and is the normal system
shape (Craftdig's rigid tick mutates `Position`/`Velocity` while walking the
rigid bag).

## Package Setup

```xml
<ItemGroup>
    <ProjectReference Include="$(AlvorKitRoot)src\AlvorKit.ECS\AlvorKit.ECS.csproj" />
    <ProjectReference Include="$(AlvorKitRoot)src\AlvorKit.ECS.Indexed\AlvorKit.ECS.Indexed.csproj" />
    <ProjectReference Include="$(AlvorKitRoot)src\AlvorKit.ECS.Generator\AlvorKit.ECS.Generator.csproj"
        OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>

<ItemGroup>
    <Using Include="AlvorKit.ECS" />
    <Using Include="AlvorKit.ECS.Indexed" />
    <Using Include="AlvorKit.ECS.Generator" />
</ItemGroup>
```

## Usage

### Component Shape

Ordinary generated components. Bool markers define bag membership:

```csharp
namespace MyGame.Run;

[Components]
public interface IRunComponents
{
    [ComponentToString] RunEntityKind Kind { get; set; }
    [ComponentToString] bool IsReady { get; set; }
    [ComponentToString] bool IsProjectile { get; set; }
    bool IsEnemy { get; set; }
    Vec2 Position { get; set; }
    Vec2 Velocity { get; set; }
    float Ttl { get; set; }
}
```

Generated marker types (`RunComponents.IsProjectile`) are the generic keys.
They implement `IComponent`, which is what registration validates against.

### Scope Setup

A scope owns one context builder, one indexed arena, and the bags its systems
need:

```csharp
[Run]
public sealed class RunEntIdxContextBuilder :
    EntIdxContextBuilder;

[Run]
public sealed class RunEntArena(RunEntIdxContextBuilder context) :
    EntIdxArena(context.Ent);

[Run]
public sealed class RunProjectileBagMut :
    EntIdxGatedBagMut<RunComponents.IsProjectile, RunComponents.IsReady>;

[Run]
public sealed class RunProjectileBag(RunProjectileBagMut bag) :
    EntIdxGatedBag<RunComponents.IsProjectile, RunComponents.IsReady>(bag);
```

Scopes use `EntIdxContextBuilder` with plain `AddBag` or gated `AddGatedBag`.
Scope hierarchies reuse loader code by subclassing builders
(`DimensionEntIdxContextBuilder : WorldEntIdxContextBuilder`); each scope
instance has its own context entity, so hooks never leak between scopes.

### Loader Registration

```csharp
[RunLoader]
public sealed class RunLoader(
    RunEntIdxContextBuilder context,
    RunProjectileBagMut projectileBag,
    RunSeerBagMut seerBag,
    RunSpatialIndex spatialIndex,
    RunEntIndex entIndex,
    RunDisposeTracker disposeTracker)
{
    public void Run()
    {
        context.AddGatedBag(projectileBag);
        context.AddBag(seerBag);
        context.AddPost<Vec2, RunComponents.Position>(spatialIndex.Intercept);
        context.AddPre<Guid, RunComponents.Id>(entIndex.Intercept);
        context.AddPreDispose(disposeTracker.InterceptDispose);
    }
}
```

`AddPre` when the hook needs the old value from the entity (dirty checks, key
index updates). `AddPost` when the hook recomputes state from current
components (bags, spatial indexes). `AddPreDispose` when the hook needs the
whole entity before teardown.

### Allocation And Mutation

```csharp
[Run]
public sealed class RunProjectileSpawner(RunEntArena arena)
{
    public EntPtrIdx Spawn(Vec2 position, Vec2 velocity)
    {
        return arena.Alloc().Mutate()
            .Kind(RunEntityKind.Projectile)
            .Position(position)
            .Velocity(velocity)
            .Ttl(1.5f)
            .IsProjectile(true)
            .IsReady(true);
    }
}
```

Set data components before flipping membership markers: bag hooks fire
immediately, so an entity is visible to bag consumers the instant its marker
and gate are both true. Flipping the gate last means the bag never exposes a
half-initialized entity. (Craftdig follows this convention with its own gate:
`PlayerCommonState.Load` and the chunk receivers set data first, `IsLoaded`
last.)

Indexed scopes must mutate through `EntPtrIdx`/`EntMutIdx`. The bypass surface
is structurally narrow — `EntMut`'s constructor is internal, `Ent` is
read-only, and `EntPtrIdx` never exposes its inner `EntPtr` — so the realistic
hole is allocating from a raw `EntArena` inside an indexed scope. That is a
documentation rule for now and an analyzer candidate later.

### Custom Key Index

```csharp
[World]
public sealed class WorldEntIndex
{
    private readonly Dictionary<Guid, EntMutIdx> dict = [];

    public EntMutIdx this[Guid id] => dict[id];

    public void Intercept(EntMutIdx ent, in Guid value)
    {
        if (ent.Id == value)
            return;

        if (ent.Id != default)
            dict.Remove(ent.Id);

        if (value != default)
            dict.Add(value, ent);
    }
}
```

Pre-set matters here: the old `Id` is still readable, so the stale key can be
removed. Dispose cleanup is automatic through the Clear contract — unsetting
`Id` fires this hook with `default`.

### Dirty Tracking

Trackers are closed generic classes, one per saved component, registered by
game-side discovery. This is Craftdig's real shape (a generic method group
`Intercept<T, N>` cannot be passed to `AddPre<T, N>` — `N` does not appear in
the delegate signature, so inference fails):

```csharp
[World]
public sealed class WorldComponentTracker<T, N>(WorldEntDirty dirty)
    where T : IEquatable<T>
    where N : IComponent
{
    public void AddTo(EntIdxContextBuilder context) =>
        context.AddPre<T, N>(Intercept);

    private void Intercept(EntMutIdx ent, in T value)
    {
        if (value.Equals(ent.Get<T, N>()))
            return;

        dirty.Mark(ent);
    }
}
```

Craftdig pairs this with an array-component variant (arrays cannot satisfy
`IEquatable<T>`), a per-component bit index, and an `IsLoading` suppression
flag so loading does not mark dirty. All of that stays game policy; the engine
supplies only the typed hooks. Note the pattern: suppression flags on the
entity, never raw-handle bypass.

### Spatial Index

Post-set, because membership derives from current position plus current
markers:

```csharp
[Dimension]
public sealed class DimensionChunkRigids
{
    private readonly Dictionary<Vec2i, HashSet<EntMutIdx>> dict = [];

    public void Intercept(EntMutIdx ent)
    {
        Vec2i? cloc = ent.IsRigid ? ent.Position.ToLoc().XY.ToCloc() : null;
        var prevCloc = ent.RigidCloc;

        if (prevCloc == cloc)
            return;

        if (prevCloc is Vec2i old)
        {
            var set = dict[old];
            set.Remove(ent);
            if (set.Count == 0)
                dict.Remove(old);
            ent.RigidCloc = null;
        }

        if (cloc is Vec2i next)
        {
            if (!dict.TryGetValue(next, out var set))
                dict.Add(next, set = []);
            set.Add(ent);
            ent.RigidCloc = cloc;
        }
    }
}

context.AddPost<Vec3d, DimensionComponents.Position>(chunkRigids.Intercept);
context.AddPost<bool, DimensionComponents.IsRigid>(chunkRigids.Intercept);
```

The early-out on unchanged `cloc` is the pattern that keeps hooks on hot
components cheap. Writing `RigidCloc` inside the hook is supported reentrancy.

### Dispose Hooks

```csharp
[World]
public sealed class WorldEntDisposeTracker(WorldEntPersister persister)
{
    public void InterceptDispose(EntMutIdx ent)
    {
        if (ent.Ploc != null)
            persister.Erase(ent);
    }
}

context.AddPreDispose(disposeTracker.InterceptDispose);
```

## Analysis

This section records the audit of Craftdig's implementation and consumers that
produced the design above. File references are to the Craftdig repo
(`Craftdig/src/...`) and to `src/AlvorKit.ECS`.

### Craftdig Consumer Inventory

The indexed layer is 12 files in `Craftdig.World/Ent`. Consumers, verified by
sweep:

- **Six bags**: `WorldDimensionBag` (IsDimensionScope, loaded),
  `DimensionPlayerBag` (IsPlayer, loaded), `DimensionRigidBag` (IsRigid,
  loaded), `DimensionChunkBag` (IsChunk, loaded, chunk context),
  `DimensionSeerBag` (IsSeer, unloaded-inclusive), `WorldScratchedBag`
  (IsScratched, unloaded-inclusive).
- **Three root context builders**: `WorldEntIdxContextBuilder`,
  `DimensionEntIdxContextBuilder : WorldEntIdxContextBuilder`, and
  `DimensionChunkEntIdxContextBuilder` — the last one is easy to miss and also
  depends on the hardcoded `IsLoaded` gate.
- **Three arenas** (world, dimension, chunk), each 1:1 with a builder,
  disposed by unloader classes at scope teardown.
- **Hook consumers**: GUID index (`WorldEntIndex`, pre on Id), dispose
  trackers (persister erase), spatial index (`DimensionChunkRigids`, post on
  Position/IsRigid), player sync (post on Position/IsPlayer, dimension→world),
  dirty trackers (`WorldComponentTracker<T,N>` + array variant + server
  scratch variants, pre on every `[Saved]` component discovered by
  reflection).
- **Discipline holds**: no raw base-ECS mutations of world entities anywhere;
  bag `Add`/`Remove` called only from interceptors; the staged-iteration
  pattern (`WorldEntStreamer`) used where membership mutates.

Every consumer pattern is expressible against the engine API with mechanical
edits only. That is the migration-possible bar this design was checked
against.

### Why The Index Backstop Is Load-Bearing

The subtlest part of Craftdig's design is the third hook `AddBag` registers
(`InterceptNoIndex`, pre on the bag index component), and it is essential, not
belt-and-suspenders. Entity dispose runs `Clear`, which unsets present
components in page-field registration order — effectively arbitrary. Two
orders exist:

- marker before index: the marker post hook removes from the bag; the later
  index unset hits the backstop, which finds `Contains == false` and no-ops.
- index before marker: the backstop fires with the old index still readable
  and removes from the bag; the later marker post hook finds membership
  already consistent.

Without the backstop, the second order leaves the bag permanently corrupted:
the marker hook computes `Contains == false` (the index is already gone),
skips `Remove`, and the dense array keeps a dead handle with `count` too high.
Any reimplementation that drops or bypasses this hook reintroduces the bug.
Both orders are pinned by tests.

### Latent Craftdig Issues This Design Fixes

Found during the audit; all are latent (not currently hit) because Craftdig's
call sites happen to avoid them:

1. **Dead-handle mutation runs hooks** (`EntPtrIdx.Set` has no liveness
   guard). Failure case: `Set<Guid, Id>` on a dead handle plants a stale entry
   in the GUID index. Fixed by the Set/Unset guards.
2. **Double dispose re-fires pre-dispose hooks** (hook layer had no guard over
   the CAS-guarded base). Fixed by the Dispose guard.
3. **Unset on an absent component** momentarily created it, fired hooks
   spuriously, and returned `true` incorrectly. Fixed by the Unset contract.
4. **Duplicate bag registration corrupts silently** (shared
   `EntIdxBagIndex<N>` in Craftdig). Fixed by indexing and guarding per
   marker+gate identity.
5. **Non-bool markers/gates and mismatched `(T, N)` pairs fail silently**
   (hooks that never fire, bags that stay empty). Fixed by `IComponent`
   metadata validation.
6. **`AddBag` hardcodes `WorldComponents.IsLoaded`** across all three
   builders. Fixed by the gate type parameter.

### Performance

Measured shape of the code paths (verify with the benchmark plan below):

- Base `Set` ([EntMut.cs](../src/AlvorKit.ECS/Ent/EntMut.cs)): liveness check,
  page fetch, generation stamp, write — a few ns, inlined.
- Indexed `Set` with no hooks: adds two context `Get`s (each roughly a base
  `Get`) returning empty `ReadOnlyMemory` — expect ~2–3× base `Set`. This is
  the floor every indexed mutation pays and the reason base handles stay
  hook-free.
- Indexed `Set` with hooks: plus one delegate dispatch per hook plus the hook
  body. `in T` keeps large components uncopied.
- Marker toggle with a bag transition: interceptor reads (2–3 component
  `Get`s) plus `Add`/`Remove`, whose internal index `Set` runs a near-empty
  pipeline — tens of ns.
- Gate toggle (for example, Craftdig's `IsLoaded`): O(gated bags in the context) interceptor runs per
  entity. Craftdig's per-context maximum today is 3; chunk streaming
  multiplies this per entity load/unload. Linear, small, and the scaling axis
  to watch.
- Hot components multiply hooks: Craftdig's `Position` carries three (dirty
  pre, spatial post, player-sync post) — roughly 40–80 ns per write all-in.
  The mitigation is early-out on no-change inside the hook, which all
  Craftdig hooks already do.
- Entity dispose: pre-dispose hooks plus one unset pipeline per present
  component. Mass teardown belongs to arena dispose, which bypasses all of it.
- Bag iteration: dense span of 16-byte handles; per-entity component reads
  land on per-arena page-clustered storage. This is the payoff that replaces
  arena scans and repeated `IsAlive`/marker checks.
- Registration: copy-on-append arrays, load-time only, negligible.

Hot paths (`Set`, toggles, iteration) must be allocation-free, per the src
rules; bag growth is geometric and the only steady-state allocation is `null`.

### Safety Review Summary

Beyond the fixed issues above, the contracts encode these audit findings:

- Hook exceptions have no rollback (contract: hooks do not throw).
- Reentrancy is a used feature and is bounded by the `-1` sentinel in the bag
  path; self-`(T, N)` recursion is unbounded (contract: user error).
- Span iteration has three concrete hazard behaviors (contract: staging).
- Arena dispose invalidates bags and game indexes rather than maintaining them
  (contract: scope-lifetime indexed views, `IsAlive` checks in anything that
  outlives the arena).
- Multiple arenas per context are fine for hooks, but not for shared indexed
  views if one arena can bulk-dispose independently. Bagged scopes should follow
  the 1:1 arena/context lifetime convention Craftdig uses.

### Alternatives Considered

- **Direct Craftdig port.** Fastest, proven, but carries the six latent issues
  and the `IsLoaded` coupling. Used as the reference implementation, not the
  shape.
- **Per-context static hook arrays** (context id into `Hooks<T, N>` statics)
  instead of the context-entity trick: saves one generation check per lookup
  and removes the finalizer hazard, but then context cleanup needs "which
  `(T, N)` storages did this context touch" tracking — a reimplementation of
  `EntReg.PageFields`. The context-entity design already *is* that machinery;
  keep it and document the strong-reference invariant instead.
- **Deferred index maintenance** (command buffer, end-of-frame flush): batches
  hook work and makes iteration trivially safe, but breaks same-frame
  visibility that Craftdig gameplay relies on (spawn then read the bag the
  same frame). New latency bug class, gameplay-visible. Rejected; revisit only
  if parallel system execution ever becomes real.
- **Generator-emitted bag maintenance in setters**: removes delegate dispatch
  for bags but still needs runtime hooks for key/dirty/spatial indexes, so it
  ships two mechanisms. Deferred as an optimization tier.
- **Query/scheduler ECS** (world, query DSL, command buffers, read/write
  tracking): does not match the scope/service style, still needs bags to
  avoid scans, large migration for no demonstrated need. Rejected.

External baseline audits of mature non-ECS games that hand-roll the same
machinery — with the failure modes this package's contracts close — are
recorded in [ECS.Indexed.BrogueCE.md](ECS.Indexed.BrogueCE.md) (the
fat-struct architecture at small scale),
[ECS.Indexed.NetHack.md](ECS.Indexed.NetHack.md) (four decades of growth
converging on indexes, membership fields, ids, component blocks, and a
runtime auditor), and [ECS.Indexed.Angband.md](ECS.Indexed.Angband.md)
(index-addressed slot storage — an arena without generations or hooks, and
what their absence costs).

Two further studies are the inverse: not hand-rolled non-ECS games but real
production ECS engines, read for which *usage* idioms their consumers
converge on and which engine mechanisms exist only to compensate for the
generational handles and hooked writes this package already has.
[ECS.Indexed.SpaceStation14.md](ECS.Indexed.SpaceStation14.md) covers
RobustToolbox at large scale (~1,900 components, ~1,100 systems), whose
non-generational ids and manual dirty marking validate the package by
*contrast*. [ECS.Indexed.Veloren.md](ECS.Indexed.Veloren.md) covers the
`specs` ECS, whose generational identity and storage-native change events
validate it by *convergence* — and whose access-based over-flagging and
system-scheduler DAG sharpen exactly where the package draws its lines.

A third kind is the reference ECS *libraries*, read for mechanism and API
design rather than usage. [ECS.Indexed.EnTT.md](ECS.Indexed.EnTT.md) is the
closest analog to this package of anything surveyed — its sparse-set storage
signals are the hook pipeline, its groups are the bags, its `entt::entity` is
the generational handle — and it isolates the three points the package
diverges on: pre+post `Set` hooks, supported reentrancy, per-context scopes.
[ECS.Indexed.flecs.md](ECS.Indexed.flecs.md) is its archetype-storage
counterpart and the sharpest test of the package's central bet: its hook
layer converges *harder* (it has the pre-set hook EnTT lacked, `on_replace`,
arrived at independently), while its table storage is the concrete worked
example of the archetype path the main document rejected — per-component
relocation machinery, deferred commands, and table-level change detection,
each a cost the package's dense slots avoid.

## Differences From Craftdig

Summary of every deliberate divergence, for the migration:

| Area | Craftdig | Engine |
|---|---|---|
| Loaded gating | Hardcoded `WorldComponents.IsLoaded` | `EntIdxGatedBagMut<N, TGate>` + `AddGatedBag<N, TGate>` |
| Bag names | `AddBag` (gated), `AddBagUnloaded` | `AddGatedBag` (gated), `AddBag` (plain) |
| Pre-hook delegate | `Action<EntMutIdx, T>` | `EntIdxPreHook<T>` with `in T` |
| Liveness | Hooks run on dead handles | Guards in Set/Unset/Dispose |
| Unset | `Set(default)` + `Unset`; wrong return on absent | Has-guarded pipeline; post observes absent |
| Double dispose | Re-fires hooks | Idempotent |
| Hook markers, bag index, interceptors | Public | Internal |
| Bag `Add`/`Remove` | Public (unused externally) | Internal |
| Bag `Count` | Missing | Included |
| Registration validation | None (silent failures) | `EntIdxRegistrationException` |
| Duplicate bag per marker+gate identity | Silent corruption | Throws |
| Arena | `virtual Dispose()`, no interface | `IDisposable`, `IsAlive` |

Everything else — the context-entity hook storage, the handle shapes, the
dense bag layout with slot 0 and the `-1` sentinel, the backstop hook, the
Clear-fires-hooks dispose path, immediate (non-deferred) maintenance — is kept
from Craftdig on purpose.

## Implementation Plan

### Project Layout

```
src/AlvorKit.ECS.Indexed/
    AlvorKit.ECS.Indexed.csproj      (ref AlvorKit.ECS;
                                      InternalsVisibleTo AlvorKit.ECS.Indexed.Test)
    EntIdxArena.cs
    EntIdxContextBuilder.cs          (hook, plain bag, and gated bag registration)
    EntIdxHooks.cs                   (3 public delegates + internal marker keys
                                      EntIdxPre/EntIdxPost/EntIdxPreDispose)
    EntIdxRegistrationException.cs
    Ent/EntPtrIdx.cs
    Ent/EntMutIdx.cs
    Bag/EntIdxBagMut.cs             (public plain/gated mutable wrappers)
    Bag/EntIdxBag.cs
    Bag/EntIdxBagIndex.cs            (internal, : IComponent, int value)
    Bag/EntIdxBagStore.cs            (internal dense slot storage)
    Bag/EntIdxBagInterceptor.cs      (internal, gated + ungated arities)

tests/AlvorKit.ECS.Indexed.Test/
    AlvorKit.ECS.Indexed.Test.csproj (refs ECS, ECS.Indexed, generator analyzer)
    EntIdxTestComponents.cs          ([Components] fixture: IsReady, IsThing,
                                      IsOther, Id Guid, Value int, Name string?)
    ... test files per plan below

demos/AlvorKit.ECS.Indexed.Demo.Bench/
    mirrors AlvorKit.ECS.Demo.Bench (options/result/json harness)
```

Repo rules that apply: net10.0, nullable, source files at or below 250 lines,
XML docs on every public and internal member, allocation-free hot paths, at
most one subdirectory level, MSTest via the tests `Directory.Build.props`,
`.Test` suffix. Wire the new projects into the solution the same way existing
`src`/`tests`/`demos` projects are listed.

### Stage 1 — Hook Core

Files: csproj, `EntIdxHooks.cs`, `EntIdxRegistrationException.cs`,
`EntIdxContextBuilder.cs` (base only: `Ent`, `AddPre`, `AddPost`,
`AddPreDispose`, protected `Add<P, PT>`, `(T, N)` validation),
`Ent/EntPtrIdx.cs`, `Ent/EntMutIdx.cs`, `EntIdxArena.cs`.

Implementation notes:

- `EntPtrIdx` stores `EntPtr ent` + `Ent context`; pipelines exactly as the
  contracts section, `AggressiveInlining` on the small members, hook loops
  over `ReadOnlyMemory<...>.Span`.
- `Add<P, PT>` copies the current array, appends, and re-sets the component on
  the context entity — load-time O(n²) growth is fine.
- Validation reads `N.Component` once per registration; throw with a message
  naming both types and the expected value type.

Tests (`EntIdxHookTest`, `EntIdxUnsetTest`, `EntIdxDisposeTest`,
`EntIdxContextIsolationTest`, `EntIdxRegistrationTest`):

- pre receives new value by `in` while `Get` still returns old; post runs
  after the write; multiple hooks run in registration order
- unset: absent → `false`, zero hook invocations, and no momentary presence;
  present → pre sees `default` with old readable, post observes `Has == false`,
  returns `true`
- dead handle: `Set`/`Unset` invoke no hooks and mutate nothing
- dispose: pre-dispose order, per-component unset hooks fire during clear,
  slot returned, second dispose is a complete no-op
- two builders/arenas: hooks never cross contexts
- validation: mismatched `(T, N)` throws; non-bool bag markers or gates throw

### Stage 2 — Bags

Files: `Bag/EntIdxBagMut.cs`, `Bag/EntIdxBag.cs`, `Bag/EntIdxBagIndex.cs`,
`Bag/EntIdxBagInterceptor.cs`; extend `EntIdxContextBuilder.cs` with
`AddBag<N>`, `AddGatedBag<N, TGate>`, and the per marker+gate duplicate guard.

Tests (`EntIdxBagTest`, `EntIdxBagDisposeOrderTest`, `EntIdxBagIterationTest`,
`EntIdxKeyIndexTest`):

- membership matrix for gated bags: marker×gate transitions in both orders,
  plus unset paths; ungated bags ignore the gate
- `Count`/`Contains`/`Ents` stay consistent across adds, swap-removes, and
  growth past the initial capacity
- dispose removes from the bag under **both** clear orders (force page-field
  registration order by first-touching the index component vs the marker on
  entities from fresh arenas) — this pins the backstop
- entity dispose mid-bag leaves the swapped survivor indexed correctly
- duplicate registration for one marker+gate identity on one context throws;
  same marker with different gates works; same marker+gate on two contexts
  works
- non-bool marker or gate throws
- arena dispose leaves dead handles visible in the bag span (documented
  behavior, pinned so a change is deliberate)
- iteration characterization: the three hazard behaviors, and the staging
  pattern as the correct usage
- key-index end-to-end: set/reassign/unset/dispose keep the dictionary exact
  (this is the consumer-visible proof of the Clear contract)

### Stage 3 — Benchmarks And Polish

`demos/AlvorKit.ECS.Indexed.Demo.Bench`, mirroring the `EcsBenchDemo` harness
(warmups, runs, best/mean ns/op, alloc B/op, optional JSON). Cases:

- `indexed-set-no-hooks` vs the base bench's `component-set-existing`
- `indexed-set-pre-post` (1 pre + 1 post, early-out hook bodies)
- `marker-toggle-bag` (add/remove transition each op)
- `gate-toggle-bags-4` (one gate flip driving 4 bags)
- `bag-iterate-1k` vs `arena-scan-1k` (the headline win)
- `indexed-dispose-8-components`
- `indexed-alloc-init-dispose`

Acceptance: zero alloc B/op on set, toggle, and iterate rows; paste the result
table into this document. Then run the repo lint script and focused coverage
(`--source-project AlvorKit.ECS.Indexed`).

### Definition Of Done

- All Stage 1–2 tests green; focused coverage and scoped lint clean.
- Benchmark table recorded here; no steady-state allocations on hot rows.
- Every contract in this document has at least one pinning test.
- Source files at or below 250 lines with complete XML docs.

## Craftdig Migration (Later, Separate Effort)

Craftdig stays untouched until the package ships. The engine guards ship to
Craftdig as part of this migration rather than as separate Craftdig patches.

1. Reference `AlvorKit.ECS.Indexed` and add the using where Craftdig
   centralizes ECS wiring (its `src/Directory.Build.props` already carries the
   `AlvorKit.ECS` using).
2. Delete the 12 files in `Craftdig.World/Ent`.
3. Rebase builders: `WorldEntIdxContextBuilder : EntIdxContextBuilder` and
   `DimensionChunkEntIdxContextBuilder : EntIdxContextBuilder`
   (`DimensionEntIdxContextBuilder` inherits the world builder and follows).
4. Rename bag registrations — 6 call sites: `AddBag` → `AddGatedBag` in
   `WorldLoader` (dimension bag) and `DimensionLoader` (player, rigid, chunk);
   those loaded bag wrapper types become `EntIdxGatedBagMut<N, WorldComponents.IsLoaded>`.
   `AddBagUnloaded` → `AddBag` in `DimensionLoader` (seer) and
   `WorldServerLoader` (scratched).
5. Add `in` to pre-hook handler signatures — about 6 methods:
   `WorldEntIndex.Intercept`, `WorldComponentTracker<T, N>` and the array
   variant, and the server scratch trackers. Post and dispose handlers adapt
   via method-group conversion with no edits.
6. Audit the behavior deltas: any direct `Unset<T, N>` call sites (return
   value now accurate; no hooks when absent), any reliance on hooks firing for
   dead handles (none expected), and the now-throwing registration errors.
7. Run Craftdig's tests plus a world load/unload smoke (chunk streaming
   exercises gate toggles, dispose, and persistence hooks together).

Craftdig-specific policy stays in Craftdig: `IsLoaded` itself,
saved-component discovery, dirty bit layout, persistence, GUID index, player
sync, chunk and spatial indexes.

## Feature Decisions

MVP (this package, stages above):

- indexed arena and handles with guarded pipelines
- pre/post/pre-dispose hooks with `in T` delegates
- plain and gated bags with the backstop hook and per marker+gate duplicate guard
- registration validation via `IComponent` metadata
- `Count`/`Contains` on bags
- contract-pinning tests and benchmarks

Useful later, on evidence:

- `CopyTo(Span<EntMutIdx>)` and indexer helpers on bags
- analyzer warning for raw `EntArena`/`EntPtr` use in indexed scopes
- generator sugar for bag pairs and saved-component metadata
- an all-entities bag / reindex step if registration-after-allocation ever
  becomes a real need
- debug-mode bag/marker consistency audit — the backstop makes desync
  structurally impossible through indexed handles, but raw-arena bypass (the
  documented hole) would surface exactly like the `monsterAtLoc` tripwire in
  [ECS.Indexed.BrogueCE.md](ECS.Indexed.BrogueCE.md); NetHack ships an
  every-turn `sanity_check()` subsystem for this failure class
  ([ECS.Indexed.NetHack.md](ECS.Indexed.NetHack.md)); cheap to add behind a
  debug flag if that ever bites

Rejected:

- multi-component query DSL, scheduler, command buffers, parallel systems
- built-in persistence, dirty-tracking, GUID, or spatial policy
- arena-mediated bag reset on bulk dispose: `EntIdxArena` owns allocation and
  liveness, not derived indexes. Bulk dispose is the fast invalidation path;
  callers that need maintained deletion use per-entity dispose.
- deferred index maintenance
