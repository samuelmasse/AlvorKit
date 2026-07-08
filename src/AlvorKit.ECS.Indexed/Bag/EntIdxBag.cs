namespace AlvorKit.ECS.Indexed;

public class EntIdxBag<N>(EntIdxBagMut<N> bag) where N : IComponent
{
        public ReadOnlySpan<EntMutIdx> Ents => bag.Ents;

        public int Count => bag.Count;

        public bool Contains(EntMutIdx ent) => bag.Contains(ent);
}

public class EntIdxGatedBag<N, TGate>(EntIdxGatedBagMut<N, TGate> bag)
    where N : IComponent
    where TGate : IComponent
{
        public ReadOnlySpan<EntMutIdx> Ents => bag.Ents;

        public int Count => bag.Count;

        public bool Contains(EntMutIdx ent) => bag.Contains(ent);
}
