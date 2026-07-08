namespace AlvorKit.ECS.Indexed;

public class EntIdxBagMut<N> where N : IComponent
{
    private EntIdxBagStore<EntIdxBagIndex<N>> store = new();

        public ReadOnlySpan<EntMutIdx> Ents => store.Ents;

        public int Count => store.Count;

        public bool Contains(EntMutIdx ent) => store.Contains(ent);

        internal void Add(EntMutIdx ent) => store.Add(ent);

        internal void Remove(EntMutIdx ent) => store.Remove(ent);
}

public class EntIdxGatedBagMut<N, TGate>
    where N : IComponent
    where TGate : IComponent
{
    private EntIdxBagStore<EntIdxGatedBagIndex<N, TGate>> store = new();

        public ReadOnlySpan<EntMutIdx> Ents => store.Ents;

        public int Count => store.Count;

        public bool Contains(EntMutIdx ent) => store.Contains(ent);

        internal void Add(EntMutIdx ent) => store.Add(ent);

        internal void Remove(EntMutIdx ent) => store.Remove(ent);
}
