namespace AlvorKit.Injection;

/// <summary>Immutable point-in-time view of a tracked injector scope graph.</summary>
public sealed record InjectorScopeGraphSnapshot(
    long Revision,
    InjectorScopeId RootId,
    InjectorScopeGraphNodeSnapshot[] Nodes);
