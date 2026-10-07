namespace AlvorKit;

/// <summary>A dense, publicly read-only bag maintained by one context's sparse boolean marker.</summary>
public class EntIdxBag<N> where N : IComponent
{
    /// <summary>Owns dense membership and private backindexes for this bag.</summary>
    private EntIdxBagStore<EntIdxBagIndex<N>> store = new();

    /// <summary>Borrowed membership span, invalidated by membership changes.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => store.Ents;
    /// <summary>Current number of maintained members.</summary>
    public int Count => store.Count;

    /// <summary>Validates the private backindex and Ent identity.</summary>
    public bool Contains(EntMutIdx ent) => store.Contains(ent);

    /// <summary>Claims this bag for exactly one context registration.</summary>
    internal void Bind(EntIdxContextKey context) => store.Bind(context);

    /// <summary>Applies a changed marker value after the component commit.</summary>
    internal void Update(EntMutIdx ent, in EntChange<bool> change) => store.Update(ent, change.After);

    /// <summary>Detaches membership during clear or individual disposal.</summary>
    internal void Remove(EntMutIdx ent) => store.Remove(ent);
}
