namespace AlvorKit.Injection;

/// <summary>
/// Stores instances, handlers, includes, and scope metadata for one injector scope.
/// </summary>
/// <param name="Root">Shared root state for the injector tree.</param>
/// <param name="Parent">Parent scope state, or <see langword="null"/> for the root scope.</param>
/// <param name="AttributeType">Required injector attribute type for services in this scope.</param>
/// <param name="Owner">Public scope object that owns instances cached by this state.</param>
public partial record InjectorScopeState(
    InjectorRoot Root,
    InjectorScopeState? Parent,
    Type? AttributeType,
    InjectorScope Owner)
{
    /// <summary>
    /// Cached service instances owned by this scope.
    /// </summary>
    private readonly Dictionary<Type, object> instances = new(16);

    /// <summary>
    /// Scope-local service aliases for interfaces and base classes.
    /// </summary>
    private readonly Dictionary<Type, InjectorServiceBinding> bindings = [];

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

        if (TryGetHostBoundary(type, path, out var boundary))
            return boundary.GetWithinHostedGraph(type, path!);

        FindRegistration(type, out var binding, out var host);
        if (host is not null)
            return host.GetHosted(type, path);
        if (binding != null)
            return GetBound(type, binding, path);

        var instance = NewUnbound(type, path);
        instances[type] = instance;
        return instance;
    }

    /// <summary>
    /// Creates a new service of <paramref name="type"/> without adding it to this scope's instance cache.
    /// </summary>
    public object New(Type type, InjectorPath? path = null)
    {
        if (TryGetHostBoundary(type, path, out var boundary))
            return boundary.NewWithinHostedGraph(type, path!);

        FindRegistration(type, out var binding, out var host);
        if (host is not null)
            return host.NewHosted(type, path);
        if (binding != null)
            return NewBound(type, binding, path);

        return NewUnbound(type, path);
    }

    /// <summary>
    /// Creates one unbound service in this scope.
    /// </summary>
    private object NewUnbound(Type type, InjectorPath? path)
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

            ValidateCreatedInstanceType(type, instance, handler, path);
            Root.NotifyInstanceOwned(Owner, instance);

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
            Root.NotifyInstanceOwned(Owner, instance);
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
