namespace AlvorKit.ECS.Indexed;

internal sealed class EntIdxBagInterceptor<N>(EntIdxBagMut<N> bag) where N : IComponent
{
        internal void Update(EntMutIdx ent)
    {
        bool shouldContain = ent.Get<bool, N>();
        bool contains = bag.Contains(ent);

        if (shouldContain && !contains)
            bag.Add(ent);
        else if (!shouldContain && contains)
            bag.Remove(ent);
    }

        internal void RemoveWhenIndexUnsets(EntMutIdx ent, in int value)
    {
        if (value == 0)
            bag.Remove(ent);
    }
}

internal sealed class EntIdxGatedBagInterceptor<N, TGate>(EntIdxGatedBagMut<N, TGate> bag)
    where N : IComponent
    where TGate : IComponent
{
        internal void Update(EntMutIdx ent)
    {
        bool shouldContain = ent.Get<bool, N>() && ent.Get<bool, TGate>();
        bool contains = bag.Contains(ent);

        if (shouldContain && !contains)
            bag.Add(ent);
        else if (!shouldContain && contains)
            bag.Remove(ent);
    }

        internal void RemoveWhenIndexUnsets(EntMutIdx ent, in int value)
    {
        if (value == 0)
            bag.Remove(ent);
    }
}
