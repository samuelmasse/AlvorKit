namespace AlvorKit.Injection;

/// <summary>
/// Fluent helpers for ordered injector scope setup.
/// </summary>
public static class InjectorScopeExtensions
{
    /// <summary>
    /// Runs <paramref name="action"/> against <paramref name="scope"/> and returns the same scope for further setup.
    /// </summary>
    /// <typeparam name="T">Concrete scope type flowing through the setup chain.</typeparam>
    /// <param name="scope">Scope being configured.</param>
    /// <param name="action">Setup action to run with the concrete scope.</param>
    /// <returns>The same <paramref name="scope"/> instance.</returns>
    public static T Run<T>(this T scope, Action<T> action) where T : InjectorScope
    {
        action(scope);
        return scope;
    }

    /// <summary>
    /// Adds <paramref name="instance"/> to <paramref name="scope"/> and returns the same scope for further setup.
    /// </summary>
    /// <typeparam name="T">Concrete scope type flowing through the setup chain.</typeparam>
    /// <param name="scope">Scope receiving the instance.</param>
    /// <param name="instance">Existing dependency instance to cache in the scope.</param>
    /// <returns>The same <paramref name="scope"/> instance.</returns>
    public static T With<T>(this T scope, object instance) where T : InjectorScope
    {
        scope.Add(instance);
        return scope;
    }

    /// <summary>
    /// Creates an instance from <paramref name="create"/>, adds it to <paramref name="scope"/>, and returns the same scope for further setup.
    /// </summary>
    /// <typeparam name="T">Concrete scope type flowing through the setup chain.</typeparam>
    /// <param name="scope">Scope receiving the created instance.</param>
    /// <param name="create">Factory that can resolve dependencies from the concrete scope before creating the instance.</param>
    /// <returns>The same <paramref name="scope"/> instance.</returns>
    public static T With<T>(this T scope, Func<T, object> create) where T : InjectorScope
    {
        scope.Add(create(scope));
        return scope;
    }
}
