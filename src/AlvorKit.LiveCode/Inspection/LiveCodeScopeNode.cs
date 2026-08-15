namespace AlvorKit;

/// <summary>Wire-safe metadata for one tracked injector scope.</summary>
public sealed record LiveCodeScopeNode(
    long Id,
    long? ParentId,
    string ScopeType,
    string? AttributeType,
    string? Label,
    string Lifecycle,
    long CreatedRevision,
    long ChangedRevision);
