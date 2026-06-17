namespace AlvorKit.ECS;

/// <summary>Represents an entity handle that can read, set, and unset components.</summary>
public interface IEntMut : IEnt
{
    /// <summary>Returns whether this handle still points at a live entity generation.</summary>
    bool IsAlive { get; }

    /// <summary>Sets the component value for the requested value and marker types.</summary>
    void Set<T, N>(in T value);

    /// <summary>Removes the requested component and returns whether it was present.</summary>
    bool Unset<T, N>();
}
