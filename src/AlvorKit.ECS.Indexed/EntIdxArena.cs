namespace AlvorKit;

/// <summary>Owns Indexed Ents and borrows a context that must remain alive until this arena is disposed.</summary>
public class EntIdxArena(Ent context) : IDisposable
{
    private readonly EntArena arena = new();

    public int Allocated => arena.Allocated;

    public bool IsAlive => arena.IsAlive;

    public virtual EntPtrIdx Alloc() => new(arena.Alloc(), context);

    public virtual void Dispose() => arena.Dispose();
}
