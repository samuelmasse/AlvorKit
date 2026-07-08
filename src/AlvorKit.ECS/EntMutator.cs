namespace AlvorKit.ECS;

public readonly record struct EntMutator<T>(T Ent) where T : IEntMut
{
    public static implicit operator T(EntMutator<T> x) => x.Ent;

    public static implicit operator EntMutator<T>(T x) => new(x);

    public EntMutator<T> Mutate(Action<T> action)
    {
        action.Invoke(Ent);
        return this;
    }

    public EntMutator<T> Set<CT, N>(in CT value)
    {
        Ent.Set<CT, N>(value);
        return this;
    }
}
