namespace AlvorKit.ECS.Indexed;

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

    public override string ToString() => ent.ToString();
}
