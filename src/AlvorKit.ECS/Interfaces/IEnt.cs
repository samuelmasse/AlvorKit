namespace AlvorKit.ECS;

/// <summary>Represents an entity handle that can query component values by component marker type.</summary>
public interface IEnt
{
    /// <summary>Gets the stable handle for this entity generation.</summary>
    EntHandle Handle { get; }

    /// <summary>Gets the component value for the requested value and marker types, or the default value when absent.</summary>
    T? Get<T, N>();

    /// <summary>Returns whether the entity currently has the requested component.</summary>
    bool Has<T, N>();
}
