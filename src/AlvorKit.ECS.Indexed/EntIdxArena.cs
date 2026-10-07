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
