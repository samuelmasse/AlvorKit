namespace AlvorKit.Injection;

/// <summary>
/// Stores instances, handlers, includes, and scope metadata for one injector scope.
/// </summary>
/// <param name="Root">Shared root state for the injector tree.</param>
/// <param name="Parent">Parent scope state, or <see langword="null"/> for the root scope.</param>
/// <param name="AttributeType">Required injector attribute type for services in this scope.</param>
public partial record InjectorScopeState(InjectorRoot Root, InjectorScopeState? Parent, Type? AttributeType)
{
    /// <summary>
    /// Cached service instances owned by this scope.
    /// </summary>
    private readonly Dictionary<Type, object> instances = new(16);

    /// <summary>
    /// Optional include patterns that restrict which service types this scope can resolve.
    /// </summary>
    private List<Regex>? includes;

    /// <summary>
    /// Optional custom construction handlers searched before the default handler.
    /// </summary>
    private List<InjectorCustomHandler>? handlers;

    /// <summary>
    /// Gets a cached service of <paramref name="type"/>, creating and caching it when missing.
    /// </summary>
    public object Get(Type type, InjectorPath? path = null)
    {
        if (instances.TryGetValue(type, out var exist))
            return exist;

        var instance = New(type, path);
        instances[type] = instance;
        return instance;
    }

    /// <summary>
    /// Creates a new service of <paramref name="type"/> without adding it to this scope's instance cache.
    /// </summary>
    public object New(Type type, InjectorPath? path = null)
    {
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(type, path);
            ValidateIncluded(type, path);
            ValidateInjectorAttributeType(type, path);

            var handler = FindHandler(type, path);
            object instance;

            try
            {
                instance = handler.Instantiate(type, this, path);
            }
            catch (InjectorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InjectorException(path, $"Handler '{handler}' for type '{type.FullName}' threw an exception.", e);
            }

            if (instance.GetType() != type)
                throw new InjectorException(path, $"Handler '{handler}' for type '{type.FullName}' returned an " +
                    $"object of mismatched type '{instance.GetType().FullName}'.");

            return instance;
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(type);
        }
    }

    /// <summary>
    /// Adds an existing <paramref name="instance"/> to this scope's cache after validating its type and scope.
    /// </summary>
    public void Add(object instance, InjectorPath? path = null)
    {
        var type = instance.GetType();
        path ??= Root.Path;

        try
        {
            ValidateCircularDependency(type, path);
            ValidateDoesNotAlreadyExist(instance, type, path);
            ValidateIncluded(type, path);
            ValidateInjectorAttributeType(type, path);
            instances[type] = instance;
        }
        finally
        {
            path.Stack.Pop();
            path.Set.Remove(type);
        }
    }

    /// <summary>
    /// Adds an inclusion pattern that permits matching service types in this scope.
    /// </summary>
    public void Include(Regex pattern)
    {
        includes ??= [];
        includes.Add(pattern);
    }

    /// <summary>
    /// Adds a custom construction handler searched before parent handlers and the default handler.
    /// </summary>
    public void Handler(InjectorCustomHandler handler)
    {
        handlers ??= [];
        handlers.Add(handler);
    }
}
