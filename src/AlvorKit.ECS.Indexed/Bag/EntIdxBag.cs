namespace AlvorKit.ECS.Indexed;

/// <summary>Exposes read-only access to a plain marker indexed bag.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
/// <param name="bag">The mutable bag maintained by the indexed context.</param>
public class EntIdxBag<N>(EntIdxBagMut<N> bag) where N : IComponent
{
    /// <summary>Gets the live dense entity span. The length is captured when the property is read.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => bag.Ents;

    /// <summary>Gets the number of entities currently in the bag.</summary>
    public int Count => bag.Count;

    /// <summary>Returns whether the entity currently has a positive slot in this bag.</summary>
    /// <param name="ent">The entity to check.</param>
    public bool Contains(EntMutIdx ent) => bag.Contains(ent);
}

/// <summary>Exposes read-only access to a gated marker indexed bag.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
/// <typeparam name="TGate">The bool gate component type.</typeparam>
/// <param name="bag">The mutable bag maintained by the indexed context.</param>
public class EntIdxBag<N, TGate>(EntIdxBagMut<N, TGate> bag)
    where N : IComponent
    where TGate : IComponent
{
    /// <summary>Gets the live dense entity span. The length is captured when the property is read.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => bag.Ents;

    /// <summary>Gets the number of entities currently in the bag.</summary>
    public int Count => bag.Count;

    /// <summary>Returns whether the entity currently has a positive slot in this bag.</summary>
    /// <param name="ent">The entity to check.</param>
    public bool Contains(EntMutIdx ent) => bag.Contains(ent);
}
