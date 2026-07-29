namespace AlvorKit.Injection;

/// <summary>
/// Hosted dependency graph helpers for <see cref="InjectorScope"/>.
/// </summary>
public abstract partial class InjectorScope
{
    /// <summary>
    /// Hosts an unscoped concrete service and its unscoped dependency graph in this scope.
    /// </summary>
    /// <typeparam name="T">Unscoped concrete service type owned by this scope.</typeparam>
    public void Host<T>()
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Host(typeof(T));
        }
    }

    /// <summary>
    /// Hosts an unscoped concrete service and its unscoped dependency graph in this scope.
    /// </summary>
    /// <param name="type">Unscoped concrete service type owned by this scope.</param>
    public void Host(Type type)
    {
        ValidateInitialized(State);
        lock (State.Root)
        {
            State.Host(type);
        }
    }
}
