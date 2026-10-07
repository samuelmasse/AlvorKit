namespace AlvorKit;

/// <summary>Dense membership with one-based private backindexes and swap removal.</summary>
internal struct EntIdxBagStore<TIndex> where TIndex : IComponent
{
    /// <summary>Dense members; slot zero is reserved for absence.</summary>
    private EntMutIdx[] ents;
    /// <summary>One past the last occupied member, including reserved slot zero.</summary>
    private int count;
    /// <summary>Owning registration identity, set once to prevent accidental sharing.</summary>
    private EntIdxContextKey owner;

    /// <summary>Creates empty dense storage with a reserved absent slot.</summary>
    public EntIdxBagStore()
    {
        ents = [default, default];
        count = 1;
    }

    /// <summary>Borrowed occupied range excluding the sentinel.</summary>
    internal readonly ReadOnlySpan<EntMutIdx> Ents => new(ents, 1, count - 1);

    /// <summary>Occupied member count excluding the sentinel.</summary>
    internal readonly int Count => count - 1;

    /// <summary>Checks the indexed slot and handle so bags sharing a component key remain independent.</summary>
    internal readonly bool Contains(EntMutIdx ent)
    {
        int index = ent.Get<int, TIndex>();
        return index > 0 && index < count && ents[index] == ent;
    }

    /// <summary>Rejects registering the same storage with another bag owner.</summary>
    internal void Bind(EntIdxContextKey context)
    {
        if (owner != default)
            throw new EntIdxRegistrationException("A bag instance belongs to exactly one context registration.");

        owner = context;
    }

    /// <summary>Applies a changed boolean membership input without a redundant membership probe.</summary>
    internal void Update(EntMutIdx ent, bool shouldContain)
    {
        // Only changed boolean inputs reach this path. A transition to membership starts outside the bag.
        if (shouldContain)
            Add(ent);
        else Remove(ent);
    }

    /// <summary>Appends a newly qualifying member and publishes its private backindex.</summary>
    private void Add(EntMutIdx ent)
    {
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);

        ent.SetBagIndex<TIndex>(count);
        ents[count] = ent;
        count++;
    }

    /// <summary>Swap-removes an occupied member and repairs the moved member backindex.</summary>
    internal void Remove(EntMutIdx ent)
    {
        int index = ent.Get<int, TIndex>();

        if (index <= 0 || index >= count || ents[index] != ent)
            return;
        count--;
        var last = ents[count];
        ents[index] = last;

        if (index != count)
            last.SetBagIndex<TIndex>(index);

        ents[count] = default;
        ent.UnsetBagIndex<TIndex>();
    }
}
