namespace AlvorKit.Injection;

/// <summary>
/// Root dependency injection scope that owns shared dependency instances and child scopes.
/// </summary>
public class Injector : InjectorScope
{
    /// <summary>
    /// Creates a root injector and registers the injector itself as a resolvable dependency.
    /// </summary>
    public Injector()
    {
        State = new(new(), null, null);
        State.Add(this);
    }
}
