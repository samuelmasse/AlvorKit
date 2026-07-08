namespace AlvorKit.ECS.Indexed;

/// <summary>Allocates indexed ECS entities for one hook context.</summary>
/// <param name="context">The context entity that stores hook lists.</param>
public class EntIdxArena(EntObj context) : IDisposable
{
    private readonly EntArena arena = new();

    /// <summary>Gets the number of live entities allocated from this arena.</summary>
    public int Allocated => arena.Allocated;

    /// <summary>Returns whether the arena is still alive.</summary>
    public bool IsAlive => arena.IsAlive;

    /// <summary>Allocates a disposable indexed entity that carries this arena's hook context.</summary>
    public virtual EntPtrIdx Alloc() => new(arena.Alloc(), (Ent)context);

    /// <summary>Bulk-invalidates all entities in the arena without running per-entity indexed hooks.</summary>
    public virtual void Dispose() => arena.Dispose();
}
