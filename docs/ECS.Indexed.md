# ECS.Indexed

`AlvorKit.ECS.Indexed` adds synchronous sparse write reactions, maintained
indexes, and explicit Ent teardown to `AlvorKit.ECS`. Start with
[ECS.md](ECS.md) for generated components, scope composition, and ownership.

## Registration

```csharp
using var context = new EntIdxContext();

// Reactions may update other components or other Ents.
context.OnChange<int, RunComponents.Health>(TrackHealth);

// Mutable arrays publish even when the same reference is assigned again.
context.OnWrite<int[], RunComponents.Tiles>(MarkTilesDirty);

// Index callbacks update external index state, before reactions.
context.AddIndex(ids.Remove)
    .OnChange<Guid, RunComponents.Id>(ids.Update);

// One removal callback can cover several inputs of the same index.
context.AddIndex(spatial.Remove)
    .OnChange<Vec3, RunComponents.Position>(spatial.UpdatePosition)
    .OnChange<bool, RunComponents.IsRigid>(spatial.UpdateRigid);

// Lifecycle callbacks read intact data before index removal.
context.OnClearing(CaptureReset);
context.OnDisposing(CaptureDeletion);

context.AddBag(scratched);
context.AddGatedBag(projectiles);

using var arena = new EntIdxArena(context);
var ent = arena.Alloc();
```

An `EntIdxContext` owns its callbacks and captured references. An
`EntIdxArena` borrows the context and owns its allocations. Dispose all
arenas before their context. Context disposal rejects live arenas and active
callbacks, clears captures, and is repeatable. Registration and new arena
construction after context disposal throw `ObjectDisposedException`.

`EntPtrIdx` and `EntMutIdx` remain unmanaged handles. They carry a borrowed
context identity, keeping native storage and generic `unmanaged` consumers usable.

Only components with an `OnChange` subscriber compare values and capture a
snapshot. Notification-only components commit and notify directly. Unobserved
writes and suppressed changes do not push dispatch frames, but still validate
active operations. Handles remain 16-byte unmanaged values. See the
[Indexed benchmarks](../bench/README.md) for timing and allocation boundaries.

Arena construction leaves registration open. The first allocation from any
arena using the context closes registration permanently. Late registration
throws `EntIdxRegistrationException`; there is no retroactive indexing.
Finish every loader's registrations before loading or spawning Ents. A loader
must not poll for newly registered observers during runtime.

Registrations validate the component's exact value type and require sparse
storage. Bags require boolean markers and gates. Each index may watch a
component once. Callbacks within each phase run in registration order.

The Indexed layer is single-threaded. One thread owns a context's registration,
mutation, reads, and teardown. Callbacks are synchronous.

## Write And Change Reactions

```csharp
// Every Set and present Unset, including equal values and reused references.
public delegate void EntWriteHandler(EntMutIdx ent);

// Only changes in value or presence.
// EntChange<T> is a readonly ref struct, valid during synchronous delivery.
// Before/After return ref readonly T?; WasPresent/IsPresent return bool.
public delegate void EntChangeHandler<T>(EntMutIdx ent, in EntChange<T> change);
```

`context.OnWrite<T, N>(reaction)` publishes every ordinary `Set` and every
removal of a present component. Read the committed value and presence through
the supplied handle. This subscription needs no before/after snapshot and
performs no equality comparison unless the same component also has a change
subscriber. Use it for mutable arrays, buffers, and explicit publications:

```csharp
context.OnWrite<int[], RunComponents.Tiles>(ent => dirty.MarkTiles(ent));
```

`context.OnChange<T, N>(reaction)` observes changes in presence or changes
according to `EqualityComparer<T>.Default`. Comparison happens once per write,
shared by all change subscribers. Equality implementations must be pure: they
must not mutate ECS state or invoke callbacks. Their own allocation behavior
remains part of the component's cost; implement efficient equality for hot
value types.

```csharp
context.OnChange<int, RunComponents.Health>(TrackHealth);

private void TrackHealth(EntMutIdx ent, in EntChange<int> change)
{
    dirty.MarkHealth(ent);
}
```

Both subscription kinds follow the same sequence:

1. Validate mutation against active callbacks.
2. Resolve component storage once; compare and capture only if needed.
3. Commit the write, even if the new value compares equal.
4. Deliver the applicable index callbacks.
5. Deliver the applicable ordinary reactions.

All affected indexes finish before the first reaction, regardless of relative
registration order. Within each phase, write and change subscribers preserve
their combined registration order. On an equal write, only write subscribers
run. Index callbacks see committed data; they should maintain their own state
without depending on a later index in that phase.

Setting a default or null value makes the component present. Absence is distinct
from a present default or null, so adding or removing either is a change.
`Unset` on an absent component is silent. Dead handles are inert. Equal writes
still replace storage, including distinct objects that compare equal.

`EntChange<T>` is a scoped view passed by `in`: it captures the old value once
and borrows the committed value through a read-only reference. Mutation guards
keep that component and its allocation stable throughout synchronous delivery.
Copy `Before` and `After` explicitly if values must be retained beyond delivery.
The values are shallow; arrays and mutable objects are not cloned. `OnChange`
cannot detect earlier in-place mutation of the same reference; use `OnWrite`
to publish those mutations. A callback must not mutate objects reachable
through the view.

Reactions may synchronously write other components or other Ents, including in
another context. Nested writes complete both phases before returning. Revisiting
an active `(Ent, T, N)` pair throws before commit, even if the attempted write is
equal or the attempted Unset is absent. Clear and Dispose of an Ent with active
delivery also throw. Perform those lifetime operations after delivery returns.

## Maintained Indexes

`context.AddIndex(remove)` returns an `EntIdxIndex`. Chain
`OnChange<T, N>(update)` for value-based inputs, or `OnWrite<T, N>(update)` for explicit publications. The removal callback
runs once per explicit Clear or individual Dispose, regardless of the number
of watched components or their presence.

```csharp
public class RunIds
{
    private readonly Dictionary<Guid, EntMutIdx> ids = [];

    public void Update(EntMutIdx ent, in EntChange<Guid> change)
    {
        if (change.Before == change.After)
            return;

        if (change.Before != Guid.Empty)
            ids.Remove(change.Before);

        if (change.After != Guid.Empty)
            ids.Add(change.After, ent);
    }

    public void Remove(EntMutIdx ent)
    {
        if (ent.Id != Guid.Empty)
            ids.Remove(ent.Id);
    }
}
```

Index callbacks maintain external collections. They cannot write, clear,
dispose, or allocate Indexed Ents, including through captured handles or another
context. Scope teardown is also prohibited during index maintenance. Engine
bag maintenance alone writes its private back-index components directly.

Register every component on which membership depends. Dependencies through
references to other Ents require an explicit game-owned relationship; the
engine does not discover them automatically. A copied handle or a different
callback parameter does not bypass the mutation guards.

## Clear And Individual Dispose

`EntPtrIdx` and `EntMutIdx` both support `Clear()`. Only the owning
`EntPtrIdx` supports `Dispose()`. Generic `IEntMut` Clear dispatches through
the handle's lifecycle implementation.

Both operations:

1. Enter teardown and reject Indexed mutation of the target.
2. Notify `OnClearing` or `OnDisposing` while all components remain readable.
3. Remove the Ent from every registered index, including bags.
4. Clear component storage directly or release the underlying allocation.

They never synthesize per-component write reactions. Dirty trackers,
replication trackers, and derived-component reactions therefore cannot recreate
components while storage is being removed. Cleanup does not depend on sparse
page-field creation order.

`Clear` preserves the allocation and invokes only `OnClearing`.
Every explicit Clear of a live Ent notifies, even if it is already empty.
`Dispose` invokes only `OnDisposing`, then invalidates the allocation.
Repeated disposal and Clear of a dead handle are inert.

Lifecycle callbacks may operate on other Ents. They must treat the target's
data as read-only, including mutable objects reachable through components.
Lazy initialization is a write: use a borrowed `Ent` read handle when reading
a lazy component must not initialize it.

Persisted-record erasure and network deletion belong in lifecycle callbacks.
They must capture or use the existing component values without resetting
components on the target themselves. Register the cleanup for both lifecycle
operations when explicit Clear also needs those external effects.

## Bags

```csharp
public class EntIdxBag<N> where N : IComponent
{
    public ReadOnlySpan<EntMutIdx> Ents { get; }
    public int Count { get; }
    public bool Contains(EntMutIdx ent);
}

public class EntIdxGatedBag<N, TGate> where N : IComponent where TGate : IComponent
{
    public ReadOnlySpan<EntMutIdx> Ents { get; }
    public int Count { get; }
    public bool Contains(EntMutIdx ent);
}
```

A plain bag contains an Ent when its marker is true. A gated bag requires both
marker and gate to be true. The bag is one publicly read-only object shared by
registration and systems; it needs no mutable wrapper or paired injected service.

One bag identity may be registered once per context. A plain marker and its
different marker/gate pairs are separate identities. A bag instance belongs
to exactly one registration. Other contexts can use separate bag instances
with the same marker and gate.

Membership checks validate the occupied slot and stored handle, so foreign,
default, removed, and disposed handles are not members. Removal swap-fills
the slot from the tail and repairs the survivor's back-index. Explicit index
removal during teardown eliminates component-unset backstops and sentinels.

`Ents` is a span over live storage with a captured length. Do not change that
bag's marker, gate, or Ent lifetime while iterating it. Stage membership-changing
work in a reusable buffer. Mutating unrelated components is allowed unless
their reactions also change that bag's membership.

## Bulk Scope Teardown

`EntIdxArena.Dispose()` releases its pages without per-Ent callbacks.
Registered bags and external indexes become invalid views and may contain
dead handles. Their `Count` and `Ents` must not be used afterward.

Keep the arena, context, and derived views in one scope lifetime. Context
sharing across arenas is supported, but bulk-invalidating one arena also
invalidates any shared views containing its Ents. Stop using those views.
Use individual Dispose when external cleanup or continued index use is required.

Disposing an arena or context during its active callbacks is rejected. This
prevents invalidating handles or callback storage in the middle of delivery.

## Failure, Storage, And Cost

Callbacks must not throw. There is no rollback: storage commits before callback
delivery, so a failure can leave derived state incomplete. Dispatch guards unwind
on exceptions to avoid retaining stack pointers; this does not make failed
callback delivery recoverable.

Registration builds ordered callback chains once and records whether each
component needs equality and change snapshots. Typed plan arrays use compact
context slots, avoiding a sparse ECS page per observed component. Each context
owns a linked chain of its plans and clears every table entry before releasing
its slot. Context ownership makes repeated context-liveness checks unnecessary
while a borrowed arena still has live Ents. Lifecycle callbacks and bag registration
identities use the context's private ECS storage.

The write path resolves one component slot for presence, optional old-value
capture, and commit. Dispatch frames live on the stack; their fields are
initialized directly, and `finally` restores the active frame on exceptions.
No per-write managed frame, event, or snapshot object is created. Notification
and change paths retain the same index and reentrancy guards.

Built-in bags consume the changed boolean value directly. Equal marker writes
skip membership maintenance while still reaching ordinary write subscribers.
Transitions into membership add directly; removal validates the occupied slot
before repairing its survivor. Private back-index writes reuse those established
lifetime and membership guarantees.
Component page creation, bag capacity growth, equality implementations, and
consumer-owned callbacks and collections retain their normal allocation costs.

Archetypal reads, writes, queries, rows, and spans deliberately remain
unobserved. Sparse and archetypal components may coexist on one Ent, and
lifecycle callbacks can read both. Raw storage mutation and mutation through
retained refs or mutable component objects bypass Indexed interception and are
outside its guards. Use the supported Indexed sparse operations for observed
state and follow the archetypal ownership rules in [ECS.Archetypal.md](ECS.Archetypal.md).
