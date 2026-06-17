namespace AlvorKit.Injection;

/// <summary>
/// Validation and reflection helpers for <see cref="InjectorScope"/>.
/// </summary>
public abstract partial class InjectorScope
{
    /// <summary>
    /// Finds the parameterless constructor required to create <paramref name="type"/> as a child scope.
    /// </summary>
    private ConstructorInfo GetConstructor(InjectorRoot root, Type type)
    {
        if (root.ScopeConstructorsCache.TryGetValue(type, out var val))
            return val;

        var constructors = type.GetConstructors();

        if (constructors.Length == 0)
            throw new InjectorScopeException(this,
                $"Scope '{type.FullName}' cannot be created because it has no public constructors.\n" +
                "Ensure the scope class has a parameterless constructor.");

        var constructor = constructors.FirstOrDefault(x => x.GetParameters().Length == 0)
            ?? throw new InjectorScopeException(this, $"Scope '{type.FullName}' must have a parameterless constructor.");

        root.ScopeConstructorsCache.Add(type, constructor);
        return constructor;
    }

    /// <summary>
    /// Gets the injector attribute type declared by a generic <see cref="InjectorScope{T}"/> base class.
    /// </summary>
    private Type GetAttributeType(InjectorRoot root, Type type)
    {
        if (root.ScopeAttributeTypeCache.TryGetValue(type, out var val))
            return val;

        if (type.BaseType == null || !type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition() != typeof(InjectorScope<>))
            throw new InjectorScopeException(this, $"Scope '{type.FullName}' must inherit from {nameof(InjectorScope)}<T>.");

        var attributeType = type.BaseType.GetGenericArguments()[0];
        root.ScopeAttributeTypeCache.Add(type, attributeType);
        return attributeType;
    }

    /// <summary>
    /// Rejects scope nesting that would repeat an attribute type in the same parent chain.
    /// </summary>
    private void ValidateAttributeType(Type type, Type attributeType)
    {
        var parent = State;
        while (parent != null)
        {
            if (parent.AttributeType == attributeType)
                throw new InjectorScopeException(this, $"Scope '{type.FullName}' with injector attribute " +
                    $"type '{attributeType.FullName}' cannot be created as another scope with the same " +
                    $"injector attribute type already exists higher in the scope stack.");
            parent = parent.Parent;
        }
    }

    /// <summary>
    /// Ensures this scope was created by an injector or parent scope before use.
    /// </summary>
    private void ValidateInitialized([NotNull] InjectorScopeState? state)
    {
        if (state == null)
            throw new InjectorScopeException(this, $"Scope '{GetType().FullName}' cannot be used as it is uninitialized. " +
                $"The scope must be created by calling the '{nameof(Scope)}' method of '{nameof(Injector)}' or of an existing scope");
    }
}
