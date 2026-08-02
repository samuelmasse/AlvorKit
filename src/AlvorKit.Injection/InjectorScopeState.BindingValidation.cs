namespace AlvorKit.Injection;

/// <summary>
/// Service binding validation helpers for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    /// <summary>
    /// Rejects implementation types that cannot be constructed in this scope.
    /// </summary>
    private void ValidateImplementationType(Type implementationType, InjectorPath path)
    {
        if (implementationType.ContainsGenericParameters)
            throw new InjectorException(path, $"Type '{implementationType.FullName}' cannot be bound because it is an open generic type.");

        if (implementationType.IsInterface || implementationType.IsAbstract)
            throw new InjectorException(path, $"Type '{implementationType.FullName}' cannot be bound because it is not concrete.");

        ValidateIncluded(implementationType, path);
        ValidateInjectorAttributeType(implementationType, path);
    }

    /// <summary>
    /// Rejects invalid service-to-implementation bindings.
    /// </summary>
    private void ValidateImplementationBinding(Type serviceType, Type implementationType, InjectorPath path)
    {
        ValidateServiceType(serviceType, path);
        ValidateImplementationType(implementationType, path);

        if (serviceType == implementationType)
            throw new InjectorException(path, $"Type '{serviceType.FullName}' cannot be bound to itself.");

        if (!serviceType.IsAssignableFrom(implementationType))
            throw new InjectorException(path, $"Type '{implementationType.FullName}' cannot be bound to " +
                $"'{serviceType.FullName}' because it is not assignable to the service type.");

        ValidateServiceAndProviderAttributes(serviceType, implementationType, path);
        ValidateServiceKeyAvailable(serviceType, path);
    }

    /// <summary>
    /// Rejects invalid service-to-instance bindings.
    /// </summary>
    private void ValidateInstanceBinding(Type serviceType, object instance, InjectorPath path)
    {
        var implementationType = instance.GetType();

        ValidateServiceType(serviceType, path);
        ValidateImplementationType(implementationType, path);

        if (!serviceType.IsAssignableFrom(implementationType))
            throw new InjectorException(path, $"Instance type '{implementationType.FullName}' cannot be registered as " +
                $"'{serviceType.FullName}' because it is not assignable to the service type.");

        ValidateServiceAndProviderAttributes(serviceType, implementationType, path);
        ValidateServiceKeyAvailable(serviceType, path);
        ValidateConcreteAliasCompatible(implementationType, instance, path);
    }

    /// <summary>
    /// Rejects invalid service alias types.
    /// </summary>
    private void ValidateServiceType(Type serviceType, InjectorPath path)
    {
        if (serviceType.ContainsGenericParameters)
            throw new InjectorException(path, $"Type '{serviceType.FullName}' cannot be bound because it is an open generic type.");

        ValidateIncluded(serviceType, path);
    }

    /// <summary>
    /// Rejects duplicate bindings or aliases in this scope.
    /// </summary>
    private void ValidateServiceKeyAvailable(Type serviceType, InjectorPath path)
    {
        if (bindings.ContainsKey(serviceType) || instances.ContainsKey(serviceType))
            throw new InjectorException(path, $"Type '{serviceType.FullName}' is already registered in this scope.");
    }

    /// <summary>
    /// Rejects concrete aliases that would point at a different existing instance.
    /// </summary>
    private void ValidateConcreteAliasCompatible(Type implementationType, object? instance, InjectorPath path)
    {
        if (!instances.TryGetValue(implementationType, out var existing))
            return;

        if (instance != null && ReferenceEquals(existing, instance))
            return;

        throw new InjectorException(path, $"Type '{implementationType.FullName}' is already registered in this scope.");
    }

    /// <summary>
    /// Rejects service and provider types with conflicting injector attributes.
    /// </summary>
    private void ValidateServiceAndProviderAttributes(Type serviceType, Type providerType, InjectorPath path)
    {
        var serviceAttributeType = GetInjectorAttributeType(serviceType, path);
        var providerAttributeType = GetInjectorAttributeType(providerType, path);

        if (serviceAttributeType != null && serviceAttributeType != providerAttributeType)
            throw new InjectorException(path, $"Type '{serviceType.FullName}' defines attribute " +
                $"'{serviceAttributeType.FullName}', but provider type '{providerType.FullName}' defines " +
                $"'{providerAttributeType?.FullName ?? "none"}'.");
    }

    /// <summary>
    /// Rejects handler-created instances that do not satisfy the requested service type or current scope.
    /// </summary>
    private void ValidateCreatedInstanceType(Type requestedType, object instance, InjectorHandler handler, InjectorPath path)
    {
        var instanceType = instance.GetType();

        if (!requestedType.IsAssignableFrom(instanceType))
            throw new InjectorException(path, $"Handler '{handler}' for type '{requestedType.FullName}' returned an " +
                $"object of mismatched type '{instanceType.FullName}'.");

        ValidateServiceAndProviderAttributes(requestedType, instanceType, path);
        ValidateInjectorAttributeType(instanceType, path);
    }
}
