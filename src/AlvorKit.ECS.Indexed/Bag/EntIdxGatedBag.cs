namespace AlvorKit;

/// <summary>A dense, publicly read-only bag maintained while both sparse boolean components are true.</summary>
public class EntIdxGatedBag<N, TGate> where N : IComponent where TGate : IComponent
{
    /// <summary>Owns dense membership and private backindexes for this gated bag.</summary>
    private EntIdxBagStore<EntIdxGatedBagIndex<N, TGate>> store = new();

    /// <summary>Borrowed membership span, invalidated by membership changes.</summary>
    public ReadOnlySpan<EntMutIdx> Ents => store.Ents;
    /// <summary>Current number of members satisfying both inputs.</summary>
    public int Count => store.Count;

    /// <summary>Validates the private backindex and Ent identity.</summary>
    public bool Contains(EntMutIdx ent) => store.Contains(ent);

    /// <summary>Claims this bag for exactly one context registration.</summary>
    internal void Bind(EntIdxContextKey context) => store.Bind(context);

    /// <summary>Combines the changed marker with the committed gate.</summary>
    internal void UpdateMarker(EntMutIdx ent, in EntChange<bool> change) =>
        store.Update(ent, change.After && ent.Get<bool, TGate>());

    /// <summary>Combines the changed gate with the committed marker.</summary>
    internal void UpdateGate(EntMutIdx ent, in EntChange<bool> change) =>
        store.Update(ent, change.After && ent.Get<bool, N>());

    /// <summary>Detaches membership during clear or individual disposal.</summary>
    internal void Remove(EntMutIdx ent) => store.Remove(ent);
}
