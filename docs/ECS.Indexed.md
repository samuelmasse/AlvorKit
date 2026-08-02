# ECS.Indexed

`AlvorKit.ECS.Indexed` is the engine package for observed mutation and
maintained indexes on top of `AlvorKit.ECS`. It provides the engine-owned
contracts for hooks, maintained bags, indexed handles, and scoped arena
lifetime.

Start with the game-facing [`ECS.md`](ECS.md) guide for component declaration,
arena ownership, scoped composition, registration, iteration, and teardown.
This document is the detailed Indexed API and mutation-contract reference.

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
per hook per call. The dedicated delegate types are public API, so their
signatures are explicit and stable. Post and dispose hooks accept ordinary
method groups without adapters.

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
  true, regardless of any separate loaded or ready state.
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
intentional: it gives O(1) per `(context, T, N)` hook lookup with no
dictionaries, per-context isolation for free, and reference cleanup through
the existing `PageRefFields` machinery when the context entity dies. The
marker types are internal engine implementation details.

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
only by the interceptors that registration installs. The engine enforces this
at compile time so a bag cannot be mutated independently of its marker. The
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
would silently stop firing. The arena therefore owns that strong reference as
part of its lifetime contract.

`EntIdxArena` implements `IDisposable` and exposes `IsAlive`.

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

The liveness guard prevents hooks from running when the base write will no-op.
Without it, a `Set<Guid, Id>` on a dead handle could insert a permanently stale
entry into a GUID index: the pre hook would read the old id as `default`, skip
the remove, and add the dead handle under the new id. Dead handles are inert
end to end.

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

Unset is a direct operation rather than `Set(default)` followed by a raw unset.
Composing those operations would momentarily create an absent component, return
the wrong result for a no-op unset, and fire hooks unnecessarily. Post hooks
observe the honest final state with `Has == false`.

### Dispose

```
Dispose():
    if not IsAlive: return                    // idempotent
    run pre-dispose hooks                     // entity fully intact
    Clear()                                   // per-component unset pipeline
    base Dispose()                            // generation bump, slot return
```

The liveness guard makes Indexed disposal idempotent. Without it, a double
dispose could re-fire pre-dispose hooks and re-run `Clear` even though the base
`EntPtr.Dispose` had already rejected the dead handle.

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
outlive the arena must check `IsAlive`. If a game needs delete semantics such
as persistence erasure or index removal, it disposes the individual
`EntPtrIdx` handles before tearing the scope down.

### Hook Rules

- **Order.** Hooks run in registration order. Loaders therefore control
  ordering: register trackers before or after indexes deliberately.
- **Reentrancy is supported.** Hooks may set other components; the nested
  write runs its own full pipeline. Dirty trackers may set a dirty marker, and
  bag removal nests an index write inside the unset pipeline. A pre hook that
  sets its own `(T, N)` recurses without bound — that is a bug in the hook,
  not something the engine detects.
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

The dense bag uses a slot layout whose back-index key is per bag identity. The
storage mechanics live in one internal
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
shape, such as mutating `Position` and `Velocity` while walking a rigid-body
bag.

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
    Guid Id { get; set; }
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
        context.Add…1100 tokens truncated…eTracker(WorldEntPersister persister)
{
    public void InterceptDispose(EntMutIdx ent)
    {
        if (ent.Ploc != null)
            persister.Erase(ent);
    }
}

context.AddPreDispose(disposeTracker.InterceptDispose);
```
