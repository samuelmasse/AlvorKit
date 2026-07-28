namespace AlvorKit.LivePatch;

/// <summary>Immutable exact-instance or injector-ownership selector.</summary>
public sealed class LivePatchSelector
{
    private LivePatchSelector(
        LivePatchSelectorKind kind,
        InjectorScopeId scopeId,
        object? instance)
    {
        Kind = kind;
        ScopeId = scopeId;
        Instance = instance;
    }

    /// <summary>Gets the selector category.</summary>
    public LivePatchSelectorKind Kind { get; }

    /// <summary>Gets the selected scope when <see cref="Kind"/> is scope-based.</summary>
    public InjectorScopeId ScopeId { get; }

    /// <summary>Gets the selected reference when <see cref="Kind"/> is <see cref="LivePatchSelectorKind.ExactInstance"/>.</summary>
    public object? Instance { get; }

    /// <summary>Selects one exact injected reference instance.</summary>
    public static LivePatchSelector ExactInstance(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (instance.GetType().IsValueType)
            throw new NotSupportedException("Exact-instance live patches require a reference receiver.");
        return new(LivePatchSelectorKind.ExactInstance, default, instance);
    }

    /// <summary>Selects receivers owned by one exact active scope.</summary>
    public static LivePatchSelector ExactScope(InjectorScopeId scopeId) =>
        new(LivePatchSelectorKind.ExactScope, scopeId, null);

    /// <summary>Selects receivers owned by one active scope and its active descendants.</summary>
    public static LivePatchSelector Descendants(InjectorScopeId scopeId) =>
        new(LivePatchSelectorKind.ScopeAndDescendants, scopeId, null);

    /// <summary>Selects all eligible receivers or a static method.</summary>
    public static LivePatchSelector All() =>
        new(LivePatchSelectorKind.All, default, null);

    internal bool Matches(object? receiver, InjectorScopeGraph graph)
    {
        if (Kind == LivePatchSelectorKind.All)
            return true;
        if (receiver is null)
            return false;
        if (Kind == LivePatchSelectorKind.ExactInstance)
            return ReferenceEquals(Instance, receiver);
        if (!graph.TryGetOwner(receiver, out var owner))
            return false;
        return Kind == LivePatchSelectorKind.ExactScope
            ? owner == ScopeId
            : graph.IsDescendantOrSelf(owner, ScopeId);
    }

    internal bool EndsWith(InjectorScopeEnding ending, InjectorScopeGraph graph)
    {
        if (Kind == LivePatchSelectorKind.All)
            return false;
        if (Kind is LivePatchSelectorKind.ExactScope or LivePatchSelectorKind.ScopeAndDescendants)
            return ScopeId == ending.Id;
        return Instance is not null &&
            graph.TryGetOwner(Instance, out var owner) &&
            owner == ending.Id;
    }

    internal bool Overlaps(LivePatchSelector other, InjectorScopeGraph graph)
    {
        if (Kind == LivePatchSelectorKind.All || other.Kind == LivePatchSelectorKind.All)
            return true;
        if (Kind == LivePatchSelectorKind.ExactInstance ||
            other.Kind == LivePatchSelectorKind.ExactInstance)
        {
            if (Kind == other.Kind)
                return ReferenceEquals(Instance, other.Instance);

            var instance = Kind == LivePatchSelectorKind.ExactInstance
                ? this
                : other;
            var scoped = ReferenceEquals(instance, this) ? other : this;
            if (instance.Instance is null ||
                !graph.TryGetOwner(instance.Instance, out var owner))
            {
                return false;
            }

            return scoped.Kind == LivePatchSelectorKind.ExactScope
                ? owner == scoped.ScopeId
                : graph.IsDescendantOrSelf(owner, scoped.ScopeId);
        }
        if (Kind == LivePatchSelectorKind.ExactScope &&
            other.Kind == LivePatchSelectorKind.ExactScope)
        {
            return ScopeId == other.ScopeId;
        }
        if (Kind == LivePatchSelectorKind.ScopeAndDescendants &&
            other.Kind == LivePatchSelectorKind.ScopeAndDescendants)
        {
            return graph.IsDescendantOrSelf(ScopeId, other.ScopeId) ||
                graph.IsDescendantOrSelf(other.ScopeId, ScopeId);
        }

        var exact = Kind == LivePatchSelectorKind.ExactScope ? this : other;
        var descendants = ReferenceEquals(exact, this) ? other : this;
        return graph.IsDescendantOrSelf(exact.ScopeId, descendants.ScopeId);
    }

    /// <inheritdoc />
    public override string ToString() => Kind switch
    {
        LivePatchSelectorKind.ExactInstance =>
            $"instance:{RuntimeHelpers.GetHashCode(Instance!):X8}",
        LivePatchSelectorKind.ExactScope => $"scope:{ScopeId}",
        LivePatchSelectorKind.ScopeAndDescendants => $"descendants:{ScopeId}",
        _ => "all"
    };
}
