namespace AlvorKit;

/// <summary>Owns Indexed registrations. Registration closes at first allocation; dispose after every borrowed arena.</summary>
public class EntIdxContext : IDisposable
{
    /// <summary>Owns lifecycle registrations and bag identity markers.</summary>
    private readonly EntPtr hooks = new();
    /// <summary>Generation-qualified registry slot owned by this context.</summary>
    private readonly EntIdxContextKey identity;
    /// <summary>Intrusive list of typed plans that must be released at disposal.</summary>
    private EntIdxPlan? plans;
    /// <summary>Registration closes permanently after the first Ent allocation.</summary>
    private bool frozen;
    /// <summary>Number of live arenas borrowing this context.</summary>
    private int arenas;

    /// <summary>Identifies the owned registry slot without retaining a managed handle reference.</summary>
    internal EntIdxContextKey Identity => identity;
    /// <summary>Current lifecycle callback arrays, frozen before Ent allocation.</summary>
    internal EntIdxLifecycleCallbacks Lifecycle => hooks.Get<EntIdxLifecycleCallbacks, EntIdxLifecycle>();

    /// <summary>Reports whether this context still owns its registrations.</summary>
    public bool IsAlive => hooks.IsAlive;

    /// <summary>Allocates a registry identity for independently owned registrations.</summary>
    public EntIdxContext() => identity = EntIdxContexts.Add(this);

    /// <summary>Observes every Set and present Unset, including equal values and same-reference publications.</summary>
    public void OnWrite<T, N>(EntWriteHandler reaction) where N : IComponent
    {
        ValidateRegistration<T, N>();
        GetPlan<T, N>().AddWrite(reaction, false);
    }

    /// <summary>Observes a value or presence change using EqualityComparer&lt;T&gt;.Default.</summary>
    public void OnChange<T, N>(EntChangeHandler<T> reaction) where N : IComponent
    {
        ValidateRegistration<T, N>();
        GetPlan<T, N>().AddChange(reaction, false);
    }

    /// <summary>Registers one index's removal from an intact Ent during Clear and individual Dispose.</summary>
    public EntIdxIndex AddIndex(EntLifecycleHandler remove)
    {
        EnsureRegistrationOpen();
        AddRemoval(remove);
        return new(this);
    }

    /// <summary>Observes explicit Clear, including an empty Ent; Dispose does not invoke this notification.</summary>
    public void OnClearing(EntLifecycleHandler notification)
    {
        EnsureRegistrationOpen();
        var callbacks = hooks.Get<EntIdxLifecycleCallbacks, EntIdxLifecycle>();
        hooks.Set<EntIdxLifecycleCallbacks, EntIdxLifecycle>(
            callbacks with { Clearing = Append(callbacks.Clearing.Span, notification) });
    }

    /// <summary>Observes individual disposal before index removal; bulk arena disposal does not notify.</summary>
    public void OnDisposing(EntLifecycleHandler notification)
    {
        EnsureRegistrationOpen();
        var callbacks = hooks.Get<EntIdxLifecycleCallbacks, EntIdxLifecycle>();
        hooks.Set<EntIdxLifecycleCallbacks, EntIdxLifecycle>(
            callbacks with { Disposing = Append(callbacks.Disposing.Span, notification) });
    }

    /// <summary>Maintains a publicly read-only bag for one sparse boolean marker.</summary>
    public void AddBag<N>(EntIdxBag<N> bag) where N : IComponent
    {
        ValidateRegistration<bool, N>();
        ValidateBag<EntIdxBagIndex<N>>();
        bag.Bind(Identity);
        hooks.Set<int, EntIdxBagIndex<N>>(1);
        AddRemoval(bag.Remove);
        GetPlan<bool, N>().AddChange(bag.Update, true);
    }

    /// <summary>Maintains a bag while both sparse boolean components are true.</summary>
    public void AddGatedBag<N, TGate>(EntIdxGatedBag<N, TGate> bag) where N : IComponent where TGate : IComponent
    {
        ValidateRegistration<bool, N>();
        ValidateRegistration<bool, TGate>();
        ValidateBag<EntIdxGatedBagIndex<N, TGate>>();
        bag.Bind(Identity);
        hooks.Set<int, EntIdxGatedBagIndex<N, TGate>>(1);
        AddRemoval(bag.Remove);
        GetPlan<bool, N>().AddChange(bag.UpdateMarker, true);

        if (typeof(N) != typeof(TGate))
            GetPlan<bool, TGate>().AddChange(bag.UpdateGate, true);
    }

    /// <summary>Releases all callbacks and captures. Live arenas and active callbacks must end first.</summary>
    public void Dispose()
    {
        if (!IsAlive)
            return;

        EntIdxDispatch.EnsureIdle(Identity);

        if (arenas != 0)
            throw new InvalidOperationException("Dispose every Indexed arena before its context.");

        while (plans != null)
        {
            plans.Release(identity.Index);
            plans = plans.Next;
        }

        hooks.Dispose();
        EntIdxContexts.Remove(identity);
    }

    /// <summary>Finds or creates one typed plan during registration.</summary>
    internal EntIdxWritePlan<T, N> GetPlan<T, N>()
    {
        var plan = EntIdxPlans<T, N>.Get(identity.Index);

        if (plan == null)
        {
            plan = new(plans);
            EntIdxPlans<T, N>.Set(identity.Index, plan);
            plans = plan;
        }

        return plan;
    }

    /// <summary>Rejects mismatched values and archetypal components before any registration is installed.</summary>
    internal void ValidateRegistration<T, N>() where N : IComponent
    {
        EnsureRegistrationOpen();
        var component = N.Component;

        if (component.IsArchetypal)
            throw new EntIdxRegistrationException($"Archetypal component {typeof(N).FullName} cannot register Indexed observers.");

        if (component.ValueType != typeof(T))
            throw new EntIdxRegistrationException(
                $"Component {typeof(N).FullName} stores {component.ValueType.FullName}, not {typeof(T).FullName}.");
    }

    /// <summary>Creates an arena and records its borrow of this live context.</summary>
    internal EntArena CreateArena()
    {
        EnsureAlive();
        EntIdxDispatch.EnsureNotIndexing();
        var arena = new EntArena();
        arenas++;
        return arena;
    }

    /// <summary>Ends one arena borrow after its Ents have been invalidated.</summary>
    internal void ReleaseArena() => arenas--;

    /// <summary>Closes registration and rejects allocations from index maintenance.</summary>
    internal void Freeze()
    {
        EnsureAlive();
        EntIdxDispatch.EnsureNotIndexing();
        frozen = true;
    }

    /// <summary>Rejects use after registration ownership has been released.</summary>
    internal void EnsureAlive() => ObjectDisposedException.ThrowIf(!IsAlive, this);

    /// <summary>Requires a live context that has never allocated an Ent.</summary>
    private void EnsureRegistrationOpen()
    {
        EnsureAlive();

        if (frozen)
            throw new EntIdxRegistrationException("Register all Indexed observers before the first Ent allocation.");
    }

    /// <summary>Rejects a duplicate built-in bag identity on this context.</summary>
    private void ValidateBag<TIndex>()
    {
        if (hooks.Has<int, TIndex>())
            throw new EntIdxRegistrationException($"Bag identity {typeof(TIndex).FullName} is already registered on this context.");
    }

    /// <summary>Built-in bags have fixed validated inputs and need no temporary public index registration builder.</summary>
    private void AddRemoval(EntLifecycleHandler remove)
    {
        var callbacks = hooks.Get<EntIdxLifecycleCallbacks, EntIdxLifecycle>();
        hooks.Set<EntIdxLifecycleCallbacks, EntIdxLifecycle>(
            callbacks with { Removing = Append(callbacks.Removing.Span, remove) });
    }

    /// <summary>Copies a cold registration list once when appending a lifecycle callback.</summary>
    private static T[] Append<T>(ReadOnlySpan<T> items, T item)
    {
        var next = new T[items.Length + 1];
        items.CopyTo(next);
        next[^1] = item;
        return next;
    }
}
