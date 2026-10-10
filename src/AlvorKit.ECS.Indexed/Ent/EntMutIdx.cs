namespace AlvorKit;

/// <summary>Borrowed mutable Indexed handle; its arena owns allocation and disposal.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntMutIdx : IEntMut
{
    private readonly EntPtrIdx ent;

    public EntHandle Handle => ent.Handle;
    public bool IsAlive => ent.IsAlive;

    internal EntMutIdx(EntPtrIdx ent) => this.ent = ent;

    public static implicit operator Ent(EntMutIdx a) => a.ent;

    public T? Get<T, N>() => ent.Get<T, N>();

    public bool Has<T, N>() => ent.Has<T, N>();

    public void Set<T, N>(in T value) => ent.Set<T, N>(value);

    public bool Unset<T, N>() => ent.Unset<T, N>();

    /// <summary>Reads archetypal storage directly without boxing this borrowed handle.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetArchetypal<T, N, A>() => ent.GetArchetypal<T, N, A>();

    /// <summary>Tests archetypal membership without boxing or Indexed dispatch.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasArchetypal<T, N, A>() => ent.HasArchetypal<T, N, A>();

    /// <summary>Writes unobserved archetypal storage while preserving sparse Indexed state.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetArchetypal<T, N, A>(in T value) => ent.SetArchetypal<T, N, A>(in value);

    /// <summary>Removes an unobserved archetypal field without boxing or sparse callbacks.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool UnsetArchetypal<T, N, A>() => ent.UnsetArchetypal<T, N, A>();

    /// <summary>Runs Clear notifications and index removal without ordinary write reactions.</summary>
    public void Clear() => ent.Clear();

    /// <summary>Publishes a private backindex during already-validated bag maintenance.</summary>
    internal void SetBagIndex<TIndex>(int index) => ent.SetBagIndex<TIndex>(index);

    /// <summary>Clears a private backindex after occupied membership has been validated.</summary>
    internal void UnsetBagIndex<TIndex>() => ent.UnsetBagIndex<TIndex>();

    public override string ToString() => ent.ToString();
}
