namespace AlvorKit.Injection;

/// <summary>
/// Shared root state for an injector tree.
/// </summary>
public class InjectorRoot
{
    /// <summary>
    /// Transient resolution path and reflection caches owned by this root.
    /// </summary>
    public readonly InjectorPath Path = new();

    /// <summary>
    /// Cache from scope type to the parameterless constructor used to create it.
    /// </summary>
    public readonly Dictionary<Type, ConstructorInfo> ScopeConstructorsCache = [];

    /// <summary>
    /// Cache from scope type to the injector attribute type that gates services in that scope.
    /// </summary>
    public readonly Dictionary<Type, Type> ScopeAttributeTypeCache = [];
}
