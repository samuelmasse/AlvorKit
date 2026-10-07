namespace AlvorKit;

/// <summary>Borrowed mutable Indexed handle; its arena owns allocation and disposal.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntMutIdx : IEntMut
{
    private readonly EntPtrIdx ent;

    public static implicit operator Ent(EntMutIdx a) => a.ent;

    internal EntMutIdx(EntPtrIdx ent) => this.ent = ent;

    public EntHandle Handle => ent.Handle;

    public bool IsAlive => ent.IsAlive;

    public T? Get<T, N>() => ent.Get<T, N>();

    public bool Has<T, N>() => ent.Has<T, N>();

    public void Set<T, N>(in T value) => ent.Set<T, N>(value);

    public bool Unset<T, N>() => ent.Unset<T, N>();

    /// <summary>Runs Clear notifications and index removal without ordinary write reactions.</summary>
    public void Clear() => ent.Clear();

    /// <summary>Publishes a private backindex during already-validated bag maintenance.</summary>
    internal void SetBagIndex<TIndex>(int index) => ent.SetBagIndex<TIndex>(index);

    /// <summary>Clears a private backindex after occupied membership has been validated.</summary>
    internal void UnsetBagIndex<TIndex>() => ent.UnsetBagIndex<TIndex>();

    public override string ToString() => ent.ToString();
}
