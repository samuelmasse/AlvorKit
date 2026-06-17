namespace AlvorKit.Injection;

/// <summary>
/// Tracks transient dependency resolution state and reflection caches shared across one root injector.
/// </summary>
public class InjectorPath
{
    /// <summary>
    /// Types currently in the resolution path, used to detect circular dependencies.
    /// </summary>
    public readonly HashSet<Type> Set = [];

    /// <summary>
    /// Ordered dependency resolution stack used for diagnostics.
    /// </summary>
    public readonly Stack<Type> Stack = [];

    /// <summary>
    /// Cache from dependency type to its injector scope attribute type, or <see langword="null"/> for root services.
    /// </summary>
    public readonly Dictionary<Type, Type?> InjectorAttributeTypeCache = [];

    /// <summary>
    /// Cache from dependency type to its public constructors.
    /// </summary>
    public readonly Dictionary<Type, ConstructorInfo[]> ConstructorCache = [];

    /// <summary>
    /// Cache from constructor to its reflected parameters.
    /// </summary>
    public readonly Dictionary<ConstructorInfo, ParameterInfo[]> ParameterCache = [];

    /// <summary>
    /// Pools constructor argument arrays by length to avoid repeated allocations during resolution.
    /// </summary>
    public readonly Dictionary<int, Queue<object[]>> ParameterPool = [];
}
