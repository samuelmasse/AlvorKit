namespace AlvorKit.ECS.Indexed;

/// <summary>Maintains a dense indexed set of entities whose marker component is true.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
public class EntIdxBagMut<N> where N : IComponent
{
    private EntIdxBagStore<EntIdxBagIndex<N>> store = new();

    /// <summary>Gets the live dense entity span. The length is captured when the property is read.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => store.Ents;

    /// <summary>Gets the number of entities currently in the bag.</summary>
    public int Count => store.Count;

    /// <summary>Returns whether the entity currently has a positive slot in this bag.</summary>
    /// <param name="ent">The entity to check.</param>
    public bool Contains(EntMutIdx ent) => store.Contains(ent);

    /// <summary>Adds an entity to the dense bag and records its slot on the entity.</summary>
    /// <param name="ent">The entity to add.</param>
    internal void Add(EntMutIdx ent) => store.Add(ent);

    /// <summary>Removes an entity from the dense bag by swap-filling from the tail.</summary>
    /// <param name="ent">The entity to remove.</param>
    internal void Remove(EntMutIdx ent) => store.Remove(ent);
}

/// <summary>Maintains a dense indexed set of entities whose marker and gate components are true.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
/// <typeparam name="TGate">The bool gate component type.</typeparam>
public class EntIdxGatedBagMut<N, TGate>
    where N : IComponent
    where TGate : IComponent
{
    private EntIdxBagStore<EntIdxGatedBagIndex<N, TGate>> store = new();

    /// <summary>Gets the live dense entity span. The length is captured when the property is read.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => store.Ents;

    /// <summary>Gets the number of entities currently in the bag.</summary>
    public int Count => store.Count;

    /// <summary>Returns whether the entity currently has a positive slot in this bag.</summary>
    /// <param name="ent">The entity to check.</param>
    public bool Contains(EntMutIdx ent) => store.Contains(ent);

    /// <summary>Adds an entity to the dense bag and records its slot on the entity.</summary>
    /// <param name="ent">The entity to add.</param>
    internal void Add(EntMutIdx ent) => store.Add(ent);

    /// <summary>Removes an entity from the dense bag by swap-filling from the tail.</summary>
    /// <param name="ent">The entity to remove.</param>
    internal void Remove(EntMutIdx ent) => store.Remove(ent);
}
