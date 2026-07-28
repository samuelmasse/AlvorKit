namespace AlvorKit.Injection;

/// <summary>Synchronous notification raised after a node rejects new work and before teardown runs.</summary>
public sealed record InjectorScopeEnding(
    InjectorScopeId Id,
    InjectorScopeId? ParentId,
    InjectorScope Scope);
