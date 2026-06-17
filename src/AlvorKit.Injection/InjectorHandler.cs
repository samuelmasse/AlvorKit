namespace AlvorKit.Injection;

/// <summary>
/// Base class for dependency construction handlers.
/// </summary>
public abstract class InjectorHandler
{
    /// <summary>
    /// Instantiates <paramref name="type"/> using <paramref name="state"/> and the shared resolution <paramref name="path"/>.
    /// </summary>
    public abstract object Instantiate(Type type, InjectorScopeState state, InjectorPath path);

    /// <summary>
    /// Returns the single public constructor supported for dependency injection.
    /// </summary>
    protected ConstructorInfo Constructor(Type type, InjectorPath path)
    {
        if (!path.ConstructorCache.ContainsKey(type))
            path.ConstructorCache.Add(type, type.GetConstructors());

        var constructors = path.ConstructorCache[type];

        if (constructors.Length == 0)
            throw new InjectorException(path, $"Type '{type.FullName}' has no accessible constructors.");

        if (constructors.Length > 1)
        {
            throw new InjectorException(path,
                $"Type '{type.FullName}' has multiple constructors. " +
                "Only one constructor is supported for dependency injection.");
        }

        return constructors[0];
    }
}
