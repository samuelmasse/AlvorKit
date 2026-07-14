namespace AlvorKit.ECS;

/// <summary>Begins an allocation that enters its final archetypal shape once.</summary>
public readonly struct EntArchCreate<A>(EntArena arena)
{
    /// <summary>Adds the first component to the final shape.</summary>
    public EntArchCreate<A, EntArchInit<T, N, A>> With<T, N>(in T value) =>
        new(arena, new(value));
}

/// <summary>Stores a typed final shape and its component values before allocation.</summary>
public readonly struct EntArchCreate<A, TInit>(EntArena arena, TInit init)
    where TInit : struct, IEntArchInit<A>
{
    /// <summary>Adds another component to the final shape.</summary>
    public EntArchCreate<A, EntArchInit<T, N, A, TInit>> With<T, N>(in T value) =>
        new(arena, new(init, value));

    /// <summary>Allocates the Ent and appends it directly to the resolved final arch.</summary>
    public EntPtr Create()
    {
        int archId = EntArchCreateState<A, TInit>.ArchId;
        EntPtr ent = arena.Alloc();
        int allocId = arena.Index;
        int row = EntArchRows<A>.Append(allocId, archId, ent);
        ent.Set<EntArchLoc, A>(new(allocId, archId, row));
        init.WriteValues(allocId, archId, row);
        return ent;
    }
}

internal static class EntArchCreateState<A, TInit>
    where TInit : struct, IEntArchInit<A>
{
    internal static readonly int ArchId = EntArchGraph<A>.ResolveSignature<TInit>();
}
