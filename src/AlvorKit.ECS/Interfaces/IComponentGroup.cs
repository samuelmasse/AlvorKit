namespace AlvorKit.ECS;

/// <summary>Identifies a generated group of component marker types.</summary>
public interface IComponentGroup
{
    /// <summary>Gets the source interface type that declared the component group.</summary>
    abstract static Type SourceInterfaceType { get; }
}
