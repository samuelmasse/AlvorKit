namespace AlvorKit.Injection;

/// <summary>Describes whether a tracked injector scope can still receive work.</summary>
public enum InjectorScopeLifecycle
{
    /// <summary>The scope is available for resolution and child-scope creation.</summary>
    Active,

    /// <summary>The scope is running its explicit teardown and rejects new graph work.</summary>
    Ending,

    /// <summary>The scope has completed teardown and its object is no longer retained by the graph.</summary>
    Ended
}
