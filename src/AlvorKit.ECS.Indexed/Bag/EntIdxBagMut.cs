namespace AlvorKit.ECS.Indexed;

/// <summary>Maintains a dense indexed set of entities whose marker component is true.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
public class EntIdxBagMut<N> where N : IComponent
{
    private EntMutIdx[] ents = [default, default];
    private int count = 1;

    /// <summary>Gets the live dense entity span. The length is captured when the property is read.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => new(ents, 1, count - 1);

    /// <summary>Gets the number of entities currently in the bag.</summary>
    public int Count => count - 1;

    /// <summary>Returns whether the entity currently has a positive slot in this bag.</summary>
    /// <param name="ent">The entity to check.</param>
    public bool Contains(EntMutIdx ent) => ent.Get<int, EntIdxBagIndex<N>>() > 0;

    /// <summary>Adds an entity to the dense bag and records its slot on the entity.</summary>
    /// <param name="ent">The entity to add.</param>
    internal void Add(EntMutIdx ent)
    {
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);

        ent.Set<int, EntIdxBagIndex<N>>(count);
        ents[count] = ent;
        count++;
    }

    /// <summary>Removes an entity from the dense bag by swap-filling from the tail.</summary>
    /// <param name="ent">The entity to remove.</param>
    internal void Remove(EntMutIdx ent)
    {
        int index = ent.Get<int, EntIdxBagIndex<N>>();
        if (index <= 0)
            return;

        count--;
        var last = ents[count];
        ents[index] = last;
        if (index != count)
            last.Set<int, EntIdxBagIndex<N>>(index);

        ents[count] = default;
        ent.Set<int, EntIdxBagIndex<N>>(-1);
    }
}

/// <summary>Maintains a dense indexed set of entities whose marker and gate components are true.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
/// <typeparam name="TGate">The bool gate component type.</typeparam>
public class EntIdxBagMut<N, TGate>
    where N : IComponent
    where TGate : IComponent
{
    private EntMutIdx[] ents = [default, default];
    private int count = 1;

    /// <summary>Gets the live dense entity span. The length is captured when the property is read.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => new(ents, 1, count - 1);

    /// <summary>Gets the number of entities currently in the bag.</summary>
    public int Count => count - 1;

    /// <summary>Returns whether the entity currently has a positive slot in this bag.</summary>
    /// <param name="ent">The entity to check.</param>
    public bool Contains(EntMutIdx ent) => ent.Get<int, EntIdxBagIndex<N, TGate>>() > 0;

    /// <summary>Adds an entity to the dense bag and records its slot on the entity.</summary>
    /// <param name="ent">The entity to add.</param>
    internal void Add(EntMutIdx ent)
    {
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);

        ent.Set<int, EntIdxBagIndex<N, TGate>>(count);
        ents[count] = ent;
        count++;
    }

    /// <summary>Removes an entity from the dense bag by swap-filling from the tail.</summary>
    /// <param name="ent">The entity to remove.</param>
    internal void Remove(EntMutIdx ent)
    {
        int index = ent.Get<int, EntIdxBagIndex<N, TGate>>();
        if (index <= 0)
            return;

        count--;
        var last = ents[count];
        ents[index] = last;
        if (index != count)
            last.Set<int, EntIdxBagIndex<N, TGate>>(index);

        ents[count] = default;
        ent.Set<int, EntIdxBagIndex<N, TGate>>(-1);
    }
}

