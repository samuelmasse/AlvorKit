namespace AlvorKit;

/// <summary>Owns Indexed Ents and borrows a context that must remain alive until this arena is disposed.</summary>
public class EntIdxArena(EntIdxContext context) : IDisposable
{
    /// <summary>Owns Ent allocations while the supplied context remains borrowed.</summary>
    private readonly EntArena arena = context.CreateArena();

    /// <summary>Number of live Ent allocations owned by this arena.</summary>
    public int Allocated => arena.Allocated;

    /// <summary>Whether arena ownership remains valid.</summary>
    public bool IsAlive => arena.IsAlive;

    /// <summary>Freezes registrations and allocates an Ent using this context.</summary>
    public virtual EntPtrIdx Alloc()
    {
        if (!arena.IsAlive)
            throw new EntArenaDisposedException();

        context.Freeze();
        return new(arena.Alloc(), context.Identity);
    }

    /// <summary>Begins final-shape allocation; Create freezes registration and returns an owning Indexed handle.</summary>
    public EntIdxArchCreate<A> AllocArchetypal<A>()
    {
        if (!arena.IsAlive)
            throw new EntArenaDisposedException();

        return new(this);
    }

    /// <summary>
    /// Queries unobserved archetypal columns within this arena. Use Indexed handles for every sparse mutation;
    /// the query's base row handles must not bypass sparse hooks or Indexed lifecycle operations.
    /// </summary>
    public EntArchQuery<A> QueryArchetypal<A>() => arena.QueryArchetypal<A>();

    /// <summary>Commits a typed final shape with the same context and allocation guards as ordinary allocation.</summary>
    internal EntPtrIdx CreateArchetypal<A, TInit>(TInit init) where TInit : struct, IEntArchInit<A>
    {
        if (!arena.IsAlive)
            throw new EntArenaDisposedException();

        context.Freeze();
        var ent = new EntArchCreate<A, TInit>(arena, init).Create();
        return new(ent, context.Identity);
    }

    /// <summary>Invalidates all owned Ents without per-Ent callbacks, then releases the context borrow.</summary>
    public virtual void Dispose()
    {
        if (!arena.IsAlive)
            return;

        EntIdxDispatch.EnsureIdle(context.Identity);
        arena.Dispose();
        context.ReleaseArena();
    }
}
