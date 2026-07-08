namespace AlvorKit.ECS.Indexed;

/// <summary>Stores the dense entity slots for an indexed bag keyed by its index component.</summary>
/// <typeparam name="TIndex">The integer component key that stores each entity's bag slot.</typeparam>
internal struct EntIdxBagStore<TIndex> where TIndex : IComponent
{
    private EntMutIdx[] ents = [default, default];
    private int count = 1;

    /// <summary>Creates an empty dense bag store with slot zero reserved.</summary>
    public EntIdxBagStore()
    {
    }

    /// <summary>Gets the live dense entity span. The length is captured when the property is read.</summary>
    internal readonly ReadOnlySpan<EntMutIdx> Ents => new(ents, 1, count - 1);

    /// <summary>Gets the number of entities currently in the bag.</summary>
    internal readonly int Count => count - 1;

    /// <summary>Returns whether the entity currently has a positive slot in this bag.</summary>
    /// <param name="ent">The entity to check.</param>
    internal readonly bool Contains(EntMutIdx ent) => ent.Get<int, TIndex>() > 0;

    /// <summary>Adds an entity to the dense bag and records its slot on the entity.</summary>
    /// <param name="ent">The entity to add.</param>
    internal void Add(EntMutIdx ent)
    {
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);

        ent.Set<int, TIndex>(count);
        ents[count] = ent;
        count++;
    }

    /// <summary>Removes an entity from the dense bag by swap-filling from the tail.</summary>
    /// <param name="ent">The entity to remove.</param>
    internal void Remove(EntMutIdx ent)
    {
        int index = ent.Get<int, TIndex>();
        if (index <= 0)
            return;

        count--;
        var last = ents[count];
        ents[index] = last;
        if (index != count)
            last.Set<int, TIndex>(index);

        ents[count] = default;
        ent.Set<int, TIndex>(-1);
    }
}
