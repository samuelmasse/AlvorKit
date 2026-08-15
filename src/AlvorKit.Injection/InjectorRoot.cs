namespace AlvorKit;

/// <summary>
/// Shared root state for an injector tree.
/// </summary>
public class InjectorRoot
{
    internal readonly List<IInjectorInstanceObserver> InstanceObservers = [];

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

    internal void NotifyInstanceOwned(InjectorScope owner, object instance)
    {
        foreach (var observer in InstanceObservers)
            observer.OnInstanceOwned(owner, instance);
    }
}
