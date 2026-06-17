namespace AlvorKit.ECS;

/// <summary>Provides builder-style mutation helpers for a mutable entity handle.</summary>
/// <param name="Ent">The mutable entity handle being configured.</param>
public readonly record struct EntMutator<T>(T Ent) where T : IEntMut
{
    /// <summary>Returns the underlying entity handle.</summary>
    public static implicit operator T(EntMutator<T> x) => x.Ent;

    /// <summary>Wraps an entity handle in a mutator.</summary>
    public static implicit operator EntMutator<T>(T x) => new(x);

    /// <summary>Runs an arbitrary mutation action and returns this mutator.</summary>
    public EntMutator<T> Mutate(Action<T> action)
    {
        action.Invoke(Ent);
        return this;
    }

    /// <summary>Sets a component value and returns this mutator.</summary>
    public EntMutator<T> Set<CT, N>(in CT value)
    {
        Ent.Set<CT, N>(value);
        return this;
    }
}
