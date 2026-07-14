namespace AlvorKit.ECS;

public interface IEntMut : IEnt
{
    bool IsAlive { get; }

    void Set<T, N>(in T value);

    bool Unset<T, N>();

    /// <summary>Overwrites or structurally adds an archetypal component.</summary>
    void SetArchetypal<T, N, A>(in T value)
    {
        var handle = Handle;
        new EntMut(handle.Index, handle.Generation).SetArchetypal<T, N, A>(value);
    }

    /// <summary>Structurally removes an archetypal component and returns whether it was present.</summary>
    bool UnsetArchetypal<T, N, A>()
    {
        var handle = Handle;
        return new EntMut(handle.Index, handle.Generation).UnsetArchetypal<T, N, A>();
    }
}
