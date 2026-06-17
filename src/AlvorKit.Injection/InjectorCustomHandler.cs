namespace AlvorKit.Injection;

/// <summary>
/// Base class for user-defined construction handlers that can override default reflection-based activation.
/// </summary>
public abstract class InjectorCustomHandler : InjectorHandler
{
    /// <summary>
    /// Returns whether this handler should instantiate dependencies of <paramref name="type"/>.
    /// </summary>
    public abstract bool Handles(Type type);
}
