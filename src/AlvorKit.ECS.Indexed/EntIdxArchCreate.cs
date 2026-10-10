namespace AlvorKit;

/// <summary>Builds an Indexed allocation's unobserved archetypal shape before publishing sparse membership.</summary>
public readonly struct EntIdxArchCreate<A>
{
    private readonly EntIdxArena arena;

    internal EntIdxArchCreate(EntIdxArena arena) => this.arena = arena;

    /// <summary>Adds the first archetypal field without allocating an Ent or closing registration.</summary>
    public EntIdxArchCreate<A, EntArchInit<T, N, A>> With<T, N>(in T value) => new(arena, new(value));
}

/// <summary>Retains a reusable typed final shape; Create preserves Indexed context and arena lifetime rules.</summary>
public readonly struct EntIdxArchCreate<A, TInit> where TInit : struct, IEntArchInit<A>
{
    private readonly EntIdxArena arena;
    private readonly TInit init;

    internal EntIdxArchCreate(EntIdxArena arena, TInit init)
    {
        this.arena = arena;
        this.init = init;
    }

    /// <summary>Adds an archetypal field; each field may occur only once in a final shape.</summary>
    public EntIdxArchCreate<A, EntArchInit<T, N, A, TInit>> With<T, N>(in T value) => new(arena, new(init, value));

    /// <summary>Allocates once into the final shape. Sparse state remains absent until written through the returned handle.</summary>
    public EntPtrIdx Create()
    {
        if (arena == null)
            throw new EntArenaDisposedException();

        return arena.CreateArchetypal<A, TInit>(init);
    }
}
