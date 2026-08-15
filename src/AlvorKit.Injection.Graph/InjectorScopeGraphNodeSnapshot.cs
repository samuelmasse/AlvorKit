namespace AlvorKit;

/// <summary>Immutable diagnostic metadata for one tracked injector scope.</summary>
public sealed record InjectorScopeGraphNodeSnapshot(
    InjectorScopeId Id,
    InjectorScopeId? ParentId,
    string ScopeType,
    string? AttributeType,
    string? Label,
    InjectorScopeLifecycle Lifecycle,
    long CreatedRevision,
    long ChangedRevision);
