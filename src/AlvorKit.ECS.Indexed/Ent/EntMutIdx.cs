namespace AlvorKit.ECS.Indexed;

/// <summary>Represents a mutable indexed entity handle that cannot dispose the entity.</summary>
[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntMutIdx : IEntMut
{
    private readonly EntPtrIdx ent;

    /// <summary>Converts this mutable handle to a read-only base ECS handle.</summary>
    public static implicit operator Ent(EntMutIdx a) => a.ent;

    internal EntMutIdx(EntPtrIdx ent) => this.ent = ent;

    /// <summary>Gets the stable handle for this entity generation.</summary>
    public EntHandle Handle => ent.Handle;

    /// <summary>Returns whether this handle still points at a live entity generation.</summary>
    public bool IsAlive => ent.IsAlive;

    /// <summary>Gets the component value for the requested value and marker types, or the default value when absent.</summary>
    public T? Get<T, N>() => ent.Get<T, N>();

    /// <summary>Returns whether this entity currently has the requested component.</summary>
    public bool Has<T, N>() => ent.Has<T, N>();

    /// <summary>Sets a component through the indexed hook pipeline.</summary>
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);

    /// <summary>Unsets a component through the indexed hook pipeline.</summary>
    public bool Unset<T, N>() => ent.Unset<T, N>();

    /// <summary>Formats this entity and selected components for diagnostics.</summary>
    public override string ToString() => ent.ToString();
}

