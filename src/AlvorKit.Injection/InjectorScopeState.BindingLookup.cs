namespace AlvorKit.Injection;

/// <summary>
/// Service binding lookup helpers for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    /// <summary>
    /// Finds the nearest binding or hosted registration for <paramref name="serviceType"/>.
    /// </summary>
    private void FindRegistration(
        Type serviceType,
        out InjectorServiceBinding? binding,
        out InjectorScopeState? host)
    {
        var state = this;

        while (state != null)
        {
            if (state.bindings.TryGetValue(serviceType, out binding))
            {
                host = null;
                return;
            }
            if (state.hostedTypes.Contains(serviceType))
            {
                binding = null;
                host = state;
                return;
            }

            state = state.Parent;
        }

        binding = null;
        host = null;
    }

    /// <summary>
    /// Selects the scope that should resolve a constructor parameter of <paramref name="parameterType"/>.
    /// </summary>
    internal InjectorScopeState FindParameterScope(Type parameterType, InjectorPath path)
    {
        var parameterAttributeType = GetInjectorAttributeType(parameterType, path);
        if (parameterAttributeType is null && path.HostScopes.Count > 0)
            return path.HostScopes.Peek();

        FindRegistration(parameterType, out var binding, out var host);
        if (host is not null)
            return host;
        if (binding != null)
            return binding.Owner;

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
