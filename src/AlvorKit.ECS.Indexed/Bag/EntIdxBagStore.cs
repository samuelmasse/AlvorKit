namespace AlvorKit.ECS.Indexed;

internal struct EntIdxBagStore<TIndex> where TIndex : IComponent
{
    private EntMutIdx[] ents = [default, default];
    private int count = 1;

    public EntIdxBagStore()
    {
    }

    internal readonly ReadOnlySpan<EntMutIdx> Ents => new(ents, 1, count - 1);

    internal readonly int Count => count - 1;

    internal readonly bool Contains(EntMutIdx ent) => ent.Get<int, TIndex>() > 0;

    internal void Add(EntMutIdx ent)
    {
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);

        ent.Set<int, TIndex>(count);
        ents[count] = ent;
        count++;
    }

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
