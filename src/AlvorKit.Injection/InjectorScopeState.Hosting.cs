namespace AlvorKit.Injection;

/// <summary>
/// Hosted dependency graph registration and resolution for <see cref="InjectorScopeState"/>.
/// </summary>
public partial record InjectorScopeState
{
    private readonly HashSet<Type> hostedTypes = [];

    /// <summary>
    /// Registers an unscoped concrete type whose unscoped graph is owned by this scope.
    /// </summary>
    public void Host(Type type, InjectorPath? path = null)
    {
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(type, path);
            ValidateHostedType(type, path);
            hostedTypes.Add(type);
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(type);
        }
    }

    private object GetHosted(Type type, InjectorPath? path)
    {
        if (instances.TryGetValue(type, out var existing))
            return existing;

        path ??= Root.Path;
        path.HostScopes.Push(this);
        try
        {
            var instance = NewUnbound(type, path);
            instances[type] = instance;
            return instance;
        }
        finally
        {
            path.HostScopes.Pop();
        }
    }

    private object NewHosted(Type type, InjectorPath? path)
    {
        path ??= Root.Path;
        path.HostScopes.Push(this);
        try
        {
            return NewUnbound(type, path);
        }
        finally
        {
            path.HostScopes.Pop();
        }
    }

    private object GetWithinHostedGraph(Type type, InjectorPath path)
    {
        if (instances.TryGetValue(type, out var existing))
            return existing;
        if (bindings.TryGetValue(type, out var binding))
            return GetBound(type, binding, path);

        var instance = NewUnbound(type, path);
        instances[type] = instance;
        return instance;
    }

    private object NewWithinHostedGraph(Type type, InjectorPath path)
    {
        if (bindings.TryGetValue(type, out var binding))
            return NewBound(type, binding, path);

        return NewUnbound(type, path);
    }

    private bool TryGetHostBoundary(Type type, InjectorPath? path, out InjectorScopeState boundary)
    {
        boundary = null!;
        if (path is null || path.HostScopes.Count == 0)
            return false;
        if (GetInjectorAttributeType(type, path) is not null)
            return false;

        boundary = path.HostScopes.Peek();
        return true;
    }
}
