namespace AlvorKit.Injection;

/// <summary>
/// Service binding registration and resolution helpers for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    /// <summary>
    /// Binds all compatible scoped service surfaces implemented by <paramref name="implementationType"/>.
    /// </summary>
    public void Bind(Type implementationType, InjectorPath? path = null)
    {
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(implementationType, path);
            ValidateImplementationType(implementationType, path);
            var serviceTypes = GetAutomaticServiceTypes(implementationType, path).ToArray();

            foreach (var serviceType in serviceTypes)
                ValidateImplementationBinding(serviceType, implementationType, path);

            foreach (var serviceType in serviceTypes)
                BindServiceType(serviceType, implementationType, path);
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(implementationType);
        }
    }

    /// <summary>
    /// Binds <paramref name="serviceType"/> to <paramref name="implementationType"/>.
    /// </summary>
    public void Bind(Type serviceType, Type implementationType, InjectorPath? path = null)
    {
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(serviceType, path);
            ValidateImplementationBinding(serviceType, implementationType, path);
            BindServiceType(serviceType, implementationType, path);
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(serviceType);
        }
    }

    /// <summary>
    /// Binds all compatible scoped service surfaces implemented by <paramref name="instance"/>.
    /// </summary>
    public void Bind(object instance, InjectorPath? path = null)
    {
        var implementationType = instance.GetType();
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(implementationType, path);
            ValidateImplementationType(implementationType, path);
            ValidateConcreteAliasCompatible(implementationType, instance, path);
            var serviceTypes = GetAutomaticServiceTypes(implementationType, path).ToArray();

            foreach (var serviceType in serviceTypes)
                ValidateInstanceBinding(serviceType, instance, path);

            CacheAlias(implementationType, instance, path);

            foreach (var serviceType in serviceTypes)
                BindServiceInstance(serviceType, instance, path);
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(implementationType);
        }
    }

    /// <summary>
    /// Adds <paramref name="instance"/> under an explicit <paramref name="serviceType"/> alias.
    /// </summary>
    public void Add(Type serviceType, object instance, InjectorPath? path = null)
    {
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(serviceType, path);
            ValidateInstanceBinding(serviceType, instance, path);
            BindServiceInstance(serviceType, instance, path);
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(serviceType);
        }
    }

    /// <summary>
    /// Registers an implementation binding after validation has already succeeded.
    /// </summary>
    private void BindServiceType(Type serviceType, Type implementationType, InjectorPath path)
    {
        ValidateServiceKeyAvailable(serviceType, path);
        bindings[serviceType] = new InjectorImplementationBinding(serviceType, this, implementationType);
    }

    /// <summary>
    /// Registers an instance binding and aliases its concrete type.
    /// </summary>
    private void BindServiceInstance(Type serviceType, object instance, InjectorPath path)
    {
        ValidateInstanceBinding(serviceType, instance, path);
        CacheAlias(serviceType, instance, path);
        CacheAlias(instance.GetType(), instance, path);
        bindings[serviceType] = new InjectorInstanceBinding(serviceType, this, instance);
    }

    /// <summary>
    /// Caches <paramref name="instance"/> under <paramref name="serviceType"/> when no conflicting value exists.
    /// </summary>
    private void CacheAlias(Type serviceType, object instance, InjectorPath path)
    {
        if (instances.TryGetValue(serviceType, out var existing))
        {
            if (ReferenceEquals(existing, instance))
                return;

            throw new InjectorException(path, $"Cannot register instance '{instance}' as type '{serviceType.FullName}' " +
                $"because a different instance '{existing}' is already present in the scope.");
        }

        instances[serviceType] = instance;
    }
}
