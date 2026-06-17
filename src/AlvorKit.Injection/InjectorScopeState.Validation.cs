namespace AlvorKit.Injection;

/// <summary>
/// Validation helpers for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    /// <summary>
    /// Pushes <paramref name="type"/> into the current resolution path and rejects cycles.
    /// </summary>
    private void ValidateCircularDependency(Type type, InjectorPath path)
    {
        path.Stack.Push(type);

        if (!path.Set.Add(type))
            throw new InjectorException(path, $"Circular dependency detected while resolving '{type.FullName}'.");
    }

    /// <summary>
    /// Rejects duplicate registrations for the same concrete dependency type.
    /// </summary>
    private void ValidateDoesNotAlreadyExist(object instance, Type type, InjectorPath path)
    {
        if (instances.TryGetValue(type, out var exist))
        {
            if (exist == instance)
                throw new InjectorException(path, $"Cannot add instance '{instance}' of type '{type.FullName}' as " +
                    $"the same instance is already present in the scope.");
            else throw new InjectorException(path, $"Cannot add instance '{instance}' of type '{type.FullName}' as " +
                    $"a different instance '{exist}' of the same type is already present in the scope.");
        }
    }

    /// <summary>
    /// Rejects a dependency type excluded by include patterns on this scope chain.
    /// </summary>
    private void ValidateIncluded(Type type, InjectorPath path)
    {
        if (!IsIncluded(type))
            throw new InjectorException(path, $"Type '{type.FullName}' cannot be injected " +
                $"because it is not included by any of the inclusion patterns.");
    }

    /// <summary>
    /// Rejects dependency types whose injector attribute does not match this scope.
    /// </summary>
    private void ValidateInjectorAttributeType(Type type, InjectorPath path)
    {
        var attributeType = GetInjectorAttributeType(type, path);

        if (attributeType == null && AttributeType != null)
            throw new InjectorException(path,
                $"Type '{type.FullName}' cannot be injected into scope '{AttributeType.FullName}' " +
                $"because it does not define the required attribute '{AttributeType.FullName}'.");

        if (attributeType != null && AttributeType == null)
            throw new InjectorException(path,
                $"Type '{type.FullName}' defines an injector attribute '{attributeType.FullName}', " +
                "but the current scope does not define any attribute, making injection invalid.");

        if (attributeType != AttributeType)
            throw new InjectorException(path,
                $"Type '{type.FullName}' defines attribute '{attributeType!.FullName}', " +
                $"but the current scope requires '{AttributeType!.FullName}'.\n" +
                "Ensure the instance is created within the correct scope.");
    }
}
