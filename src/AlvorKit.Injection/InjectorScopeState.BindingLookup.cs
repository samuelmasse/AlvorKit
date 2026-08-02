namespace AlvorKit.Injection;

/// <summary>
/// Service binding lookup helpers for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    /// <summary>
    /// Finds the nearest binding for <paramref name="serviceType"/> in this scope chain.
    /// </summary>
    internal InjectorServiceBinding? FindBinding(Type serviceType)
    {
        var state = this;

        while (state != null)
        {
            if (state.bindings.TryGetValue(serviceType, out var binding))
                return binding;

            state = state.Parent;
        }

        return null;
    }

    /// <summary>
    /// Selects the scope that should resolve a constructor parameter of <paramref name="parameterType"/>.
    /// </summary>
    internal InjectorScopeState FindParameterScope(Type parameterType, InjectorPath path)
    {
        var binding = FindBinding(parameterType);
        if (binding != null)
            return binding.Owner;

        var parameterAttributeType = GetInjectorAttributeType(parameterType, path);
        var state = this;

        while (state.Parent != null)
        {
            if (state.AttributeType == parameterAttributeType)
                break;

            state = state.Parent;
        }

        return state;
    }

    /// <summary>
    /// Finds marked interfaces and base classes that should be bound automatically for an implementation.
    /// </summary>
    private IEnumerable<Type> GetAutomaticServiceTypes(Type implementationType, InjectorPath path)
    {
        var implementationAttributeType = GetInjectorAttributeType(implementationType, path);
        var seen = new HashSet<Type>();

        foreach (var interfaceType in implementationType.GetInterfaces())
        {
            if (IsAutomaticServiceType(interfaceType, implementationAttributeType, path) && seen.Add(interfaceType))
                yield return interfaceType;
        }

        var baseType = implementationType.BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            if (IsAutomaticServiceType(baseType, implementationAttributeType, path) && seen.Add(baseType))
                yield return baseType;

            baseType = baseType.BaseType;
        }
    }

    /// <summary>
    /// Returns whether <paramref name="serviceType"/> is a marked service surface compatible with an implementation.
    /// </summary>
    private bool IsAutomaticServiceType(Type serviceType, Type? implementationAttributeType, InjectorPath path)
    {
        var serviceAttributeType = GetInjectorAttributeType(serviceType, path);
        return serviceAttributeType != null && serviceAttributeType == implementationAttributeType;
    }
}
