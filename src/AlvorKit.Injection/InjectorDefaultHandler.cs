namespace AlvorKit.Injection;

/// <summary>
/// Default reflection-based construction handler used when no custom handler accepts a dependency type.
/// </summary>
public class InjectorDefaultHandler : InjectorHandler
{
    /// <summary>
    /// Shared stateless default handler instance.
    /// </summary>
    public static readonly InjectorDefaultHandler Instance = new();

    /// <summary>
    /// Instantiates <paramref name="type"/> by resolving each constructor parameter from the nearest valid scope.
    /// </summary>
    public override object Instantiate(Type type, InjectorScopeState scope, InjectorPath path)
    {
        var constructor = Constructor(type, path);

        if (!path.ParameterCache.TryGetValue(constructor, out var parameters))
        {
            parameters = constructor.GetParameters();
            path.ParameterCache.Add(constructor, parameters);
        }

        if (!path.ParameterPool.TryGetValue(parameters.Length, out var pool))
        {
            pool = [];
            path.ParameterPool.Add(parameters.Length, pool);
        }

        var parameterValues = pool.Count > 0 ? pool.Dequeue() : new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var newScope = scope.FindParameterScope(paramType, path);
            parameterValues[i] = newScope.Get(paramType, path);
        }

        object result;
        try
        {
            result = constructor.Invoke(parameterValues);
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            if (e.InnerException is InjectorException)
                throw e.InnerException;
            else throw new InjectorException(path, $"Constructor for type {type.FullName} threw an exception.", e);
        }
        finally
        {
            Array.Clear(parameterValues);
            pool.Enqueue(parameterValues);
        }

        return result;
    }
}
