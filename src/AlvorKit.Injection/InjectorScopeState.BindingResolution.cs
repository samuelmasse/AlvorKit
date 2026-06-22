namespace AlvorKit.Injection;

/// <summary>
/// Service binding resolution helpers for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    /// <summary>
    /// Gets an instance through <paramref name="binding"/> and caches any missing aliases.
    /// </summary>
    private object GetBound(Type requestedType, InjectorServiceBinding binding, InjectorPath? path)
    {
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(requestedType, path);
            ValidateIncluded(requestedType, path);

            return binding switch
            {
                InjectorImplementationBinding implementation => GetImplementationBinding(implementation, path),
                InjectorInstanceBinding instance => GetInstanceBinding(instance, path),
                _ => throw new InjectorException(path, $"Unknown binding for type '{requestedType.FullName}'.")
            };
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(requestedType);
        }
    }

    /// <summary>
    /// Creates a fresh instance through <paramref name="binding"/> without caching the service alias.
    /// </summary>
    private object NewBound(Type requestedType, InjectorServiceBinding binding, InjectorPath? path)
    {
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(requestedType, path);
            ValidateIncluded(requestedType, path);

            if (binding is InjectorImplementationBinding implementation)
                return implementation.Owner.New(implementation.ImplementationType, path);

            throw new InjectorException(path,
                $"Type '{requestedType.FullName}' is bound to an existing instance and cannot be created with New.");
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(requestedType);
        }
    }

    /// <summary>
    /// Gets or creates the implementation instance for <paramref name="binding"/>.
    /// </summary>
    private object GetImplementationBinding(InjectorImplementationBinding binding, InjectorPath path)
    {
        var instance = binding.Owner.Get(binding.ImplementationType, path);
        binding.Owner.CacheAlias(binding.ServiceType, instance, path);
        return instance;
    }

    /// <summary>
    /// Gets the existing instance for <paramref name="binding"/>.
    /// </summary>
    private object GetInstanceBinding(InjectorInstanceBinding binding, InjectorPath path)
    {
        binding.Owner.CacheAlias(binding.ServiceType, binding.Instance, path);
        binding.Owner.CacheAlias(binding.Instance.GetType(), binding.Instance, path);
        return binding.Instance;
    }
}
