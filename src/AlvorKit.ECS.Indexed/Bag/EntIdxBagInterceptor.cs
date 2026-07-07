namespace AlvorKit.ECS.Indexed;

/// <summary>Maintains a plain marker bag from indexed hook callbacks.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
internal sealed class EntIdxBagInterceptor<N>(EntIdxBagMut<N> bag) where N : IComponent
{
    /// <summary>Recomputes plain bag membership after the marker changes.</summary>
    /// <param name="ent">The entity whose marker changed.</param>
    internal void Update(EntMutIdx ent)
    {
        bool shouldContain = ent.Get<bool, N>();
        bool contains = bag.Contains(ent);

        if (shouldContain && !contains)
            bag.Add(ent);
        else if (!shouldContain && contains)
            bag.Remove(ent);
    }

    /// <summary>Removes the entity when the bag index component is unset before the marker.</summary>
    /// <param name="ent">The entity whose bag index is being changed.</param>
    /// <param name="value">The incoming index value.</param>
    internal void RemoveWhenIndexUnsets(EntMutIdx ent, in int value)
    {
        if (value == 0)
            bag.Remove(ent);
    }
}

/// <summary>Maintains a gated marker bag from indexed hook callbacks.</summary>
/// <typeparam name="N">The bool marker component type.</typeparam>
/// <typeparam name="TGate">The bool gate component type.</typeparam>
internal sealed class EntIdxBagInterceptor<N, TGate>(EntIdxBagMut<N, TGate> bag)
    where N : IComponent
    where TGate : IComponent
{
    /// <summary>Recomputes gated bag membership after either the marker or gate changes.</summary>
    /// <param name="ent">The entity whose marker or gate changed.</param>
    internal void Update(EntMutIdx ent)
    {
        bool shouldContain = ent.Get<bool, N>() && ent.Get<bool, TGate>();
        bool contains = bag.Contains(ent);

        if (shouldContain && !contains)
            bag.Add(ent);
        else if (!shouldContain && contains)
            bag.Remove(ent);
    }

    /// <summary>Removes the entity when the bag index component is unset before marker or gate hooks run.</summary>
    /// <param name="ent">The entity whose bag index is being changed.</param>
    /// <param name="value">The incoming index value.</param>
    internal void RemoveWhenIndexUnsets(EntMutIdx ent, in int value)
    {
        if (value == 0)
            bag.Remove(ent);
    }
}

