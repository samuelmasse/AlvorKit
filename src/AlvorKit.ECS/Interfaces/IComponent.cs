namespace AlvorKit.ECS;

/// <summary>Identifies a generated component marker type.</summary>
public interface IComponent
{
    /// <summary>Gets metadata describing the component value type and marker type.</summary>
    abstract static EntComponent Component { get; }
}
