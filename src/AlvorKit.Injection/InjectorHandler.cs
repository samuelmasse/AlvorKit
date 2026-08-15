namespace AlvorKit;

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
        {
            var candidates = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(constructor => constructor.IsPublic || constructor.IsAssembly)
                .ToArray();
            path.ConstructorCache.Add(type, candidates);
        }

        var constructors = path.ConstructorCache[type];

        if (constructors.Length == 0)
            throw new InjectorException(path, $"Type '{type.FullName}' has no public or internal constructors.");

        if (constructors.Length > 1)
        {
            throw new InjectorException(path,
                $"Type '{type.FullName}' has multiple constructors. " +
                "Only one public or internal constructor is supported for dependency injection.");
        }

        return constructors[0];
    }
}
