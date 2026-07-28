namespace AlvorKit.Injection;

/// <summary>Mutable graph-owned state for one injector scope lifetime.</summary>
internal sealed class InjectorScopeGraphNode(
    InjectorScopeId id,
    InjectorScopeId? parentId,
    InjectorScope scope,
    string? label,
    long revision)
{
    internal readonly InjectorScopeId Id = id;
    internal readonly InjectorScopeId? ParentId = parentId;
    internal readonly Type ScopeType = scope.GetType();
    internal readonly Type? AttributeType = FindAttributeType(scope.GetType());
    internal readonly long CreatedRevision = revision;
    internal InjectorScope? Scope = scope;
    internal string? Label = label;
    internal InjectorScopeLifecycle Lifecycle = InjectorScopeLifecycle.Active;
    internal long ChangedRevision = revision;

    internal InjectorScopeGraphNodeSnapshot Snapshot() =>
        new(
            Id,
            ParentId,
            ScopeType.FullName ?? ScopeType.Name,
            AttributeType?.FullName,
            Label,
            Lifecycle,
            CreatedRevision,
            ChangedRevision);

    private static Type? FindAttributeType(Type scopeType)
    {
        var baseType = scopeType.BaseType;
        if (baseType is null
            || !baseType.IsGenericType
            || baseType.GetGenericTypeDefinition() != typeof(InjectorScope<>))
        {
            return null;
        }

        return baseType.GetGenericArguments()[0];
    }
}
