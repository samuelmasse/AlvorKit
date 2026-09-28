namespace AlvorKit;

/// <summary>
/// Creates and owns an explicit lifetime graph above ordinary injector scopes.
/// Scope creation and termination must flow through this object to appear in its authoritative graph.
/// </summary>
public class InjectorScopeGraph : IInjectorInstanceObserver
{
    private readonly Lock gate = new();
    private readonly Dictionary<InjectorScope, InjectorScopeGraphNode> activeByScope =
        new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<InjectorScopeId, InjectorScopeGraphNode> nodes = [];
    private readonly ConditionalWeakTable<object, InjectorScopeGraphInstanceOwner> instanceOwners = [];
    private readonly InjectorScopeId rootId;
    private long nextId;
    private long revision;

    /// <summary>Gets the root node identifier.</summary>
    public InjectorScopeId RootId => rootId;

    /// <summary>Gets the latest graph revision.</summary>
    public long Revision
    {
        get
        {
            lock (gate)
                return revision;
        }
    }

    /// <summary>Creates a graph with an unlabeled existing root scope and observes its owned instances.</summary>
    public InjectorScopeGraph(InjectorScope root)
    {
        var node = AddNode(null, root, null);
        rootId = node.Id;
        root.Observe(this);
    }

    /// <summary>Creates a graph with a labeled existing root scope and observes its owned instances.</summary>
    public InjectorScopeGraph(InjectorScope root, string label)
    {
        var node = AddNode(null, root, label);
        rootId = node.Id;
        root.Observe(this);
    }

    /// <summary>
    /// Raised synchronously after a node becomes <see cref="InjectorScopeLifecycle.Ending"/>
    /// and before caller teardown begins.
    /// </summary>
    public event Action<InjectorScopeEnding>? ScopeEnding;

    /// <summary>Creates and tracks an unlabeled child scope owned by <paramref name="parent"/>.</summary>
    public T Scope<T>(InjectorScope parent) where T : InjectorScope => CreateScope<T>(parent, null);

    /// <summary>Creates and tracks a labeled child scope owned by <paramref name="parent"/>.</summary>
    public T Scope<T>(InjectorScope parent, string label) where T : InjectorScope => CreateScope<T>(parent, label);

    /// <summary>Runs work in an unlabeled temporary child scope and ends it even if the action throws.</summary>
    public void Run<T>(InjectorScope parent, Action<T> action) where T : InjectorScope => RunScope(Scope<T>(parent), action);

    /// <summary>Runs work in a labeled temporary child scope and ends it even if the action throws.</summary>
    public void Run<T>(InjectorScope parent, string label, Action<T> action) where T : InjectorScope =>
        RunScope(Scope<T>(parent, label), action);

    /// <summary>Changes the diagnostic label for an active tracked scope.</summary>
    public void Label(InjectorScope scope, string label) => ChangeLabel(scope, label);

    /// <summary>Removes the diagnostic label from an active tracked scope.</summary>
    public void ClearLabel(InjectorScope scope) => ChangeLabel(scope, null);

    /// <summary>Announces scope ending and releases its reference after the caller has performed cleanup.</summary>
    public void End(InjectorScope scope) => EndScope<InjectorScope>(scope, null);

    /// <summary>
    /// Marks a scope as ending, runs its explicit teardown, and releases the graph's reference.
    /// Active tracked children must be ended first.
    /// </summary>
    public void End<T>(T scope, Action<T> teardown) where T : InjectorScope => EndScope(scope, teardown);

    /// <summary>Resolves an active scope by graph identifier without creating anything.</summary>
    public bool TryGetActiveScope(InjectorScopeId id, [NotNullWhen(true)] out InjectorScope? scope)
    {
        lock (gate)
        {
            if (nodes.TryGetValue(id, out var node)
                && node.Lifecycle == InjectorScopeLifecycle.Active
                && node.Scope is not null)
            {
                scope = node.Scope;
                return true;
            }

            scope = null;
            return false;
        }
    }

    /// <summary>Gets the stable graph identifier for an active tracked scope.</summary>
    public InjectorScopeId GetId(InjectorScope scope)
    {
        lock (gate)
            return RequireActive(scope).Id;
    }

    /// <summary>Finds the exact graph node that owns an injected reference instance.</summary>
    public bool TryGetOwner(object instance, out InjectorScopeId ownerId)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (instanceOwners.TryGetValue(instance, out var owner))
        {
            lock (gate)
            {
                if (nodes.TryGetValue(owner.Id, out var node) &&
                    node.Lifecycle != InjectorScopeLifecycle.Ended)
                {
                    ownerId = owner.Id;
                    return true;
                }
            }
        }

        ownerId = default;
        return false;
    }

    /// <summary>Tests active graph ancestry without resolving any injector service.</summary>
    public bool IsDescendantOrSelf(InjectorScopeId candidate, InjectorScopeId ancestor)
    {
        lock (gate)
        {
            if (!nodes.TryGetValue(candidate, out var node) ||
                node.Lifecycle != InjectorScopeLifecycle.Active)
            {
                return false;
            }

            while (true)
            {
                if (node.Id == ancestor)
                    return true;
                if (node.ParentId is not { } parent ||
                    !nodes.TryGetValue(parent, out node))
                {
                    return false;
                }
            }
        }
    }

    /// <inheritdoc />
    public void OnInstanceOwned(InjectorScope owner, object instance)
    {
        if (instance.GetType().IsValueType)
            return;

        InjectorScopeId ownerId;
        lock (gate)
        {
            if (!activeByScope.TryGetValue(owner, out var node))
                return;
            ownerId = node.Id;
        }

        instanceOwners.Remove(instance);
        instanceOwners.Add(instance, new(ownerId));
    }

    /// <summary>Captures metadata for scopes that have not ended without resolving services.</summary>
    public InjectorScopeGraphSnapshot Snapshot() => Snapshot(false);

    /// <summary>Captures metadata, optionally including ended scopes, without resolving services.</summary>
    public InjectorScopeGraphSnapshot Snapshot(bool includeEnded)
    {
        lock (gate)
        {
            var snapshots = nodes.Values
                .Where(x => includeEnded || x.Lifecycle != InjectorScopeLifecycle.Ended)
                .OrderBy(x => x.Id.Value)
                .Select(x => x.Snapshot())
                .ToArray();
            return new(revision, RootId, snapshots);
        }
    }

    private T CreateScope<T>(InjectorScope parent, string? label) where T : InjectorScope
    {
        lock (gate)
        {
            var parentNode = RequireActive(parent);
            var child = parent.Scope<T>();
            AddNode(parentNode.Id, child, label);
            return child;
        }
    }

    private void RunScope<T>(T scope, Action<T> action) where T : InjectorScope
    {
        try
        {
            action(scope);
        }
        finally
        {
            End(scope);
        }
    }

    private void ChangeLabel(InjectorScope scope, string? label)
    {
        lock (gate)
        {
            var node = RequireActive(scope);
            node.Label = label;
            node.ChangedRevision = ++revision;
        }
    }

    private void EndScope<T>(T scope, Action<T>? teardown) where T : InjectorScope
    {
        InjectorScopeGraphNode node;

        lock (gate)
        {
            node = RequireActive(scope);
            RequireNoActiveChildren(node);
            node.Lifecycle = InjectorScopeLifecycle.Ending;
            node.ChangedRevision = ++revision;
        }

        try
        {
            ScopeEnding?.Invoke(new(node.Id, node.ParentId, scope));
            teardown?.Invoke(scope);
        }
        finally
        {
            lock (gate)
            {
                node.Lifecycle = InjectorScopeLifecycle.Ended;
                node.Scope = null;
                node.ChangedRevision = ++revision;
                activeByScope.Remove(scope);
            }
        }
    }

    private InjectorScopeGraphNode AddNode(InjectorScopeId? parentId, InjectorScope scope, string? label)
    {
        var id = new InjectorScopeId(++nextId);
        var node = new InjectorScopeGraphNode(id, parentId, scope, label, ++revision);
        nodes.Add(id, node);
        activeByScope.Add(scope, node);
        return node;
    }

    private InjectorScopeGraphNode RequireActive(InjectorScope scope)
    {
        if (!activeByScope.TryGetValue(scope, out var node)
            || node.Lifecycle != InjectorScopeLifecycle.Active)
        {
            throw new InjectorScopeGraphException(
                $"Injector scope '{scope.GetType().FullName}' is not active in this scope graph.");
        }

        return node;
    }

    private void RequireNoActiveChildren(InjectorScopeGraphNode parent)
    {
        foreach (var child in nodes.Values)
        {
            if (child.ParentId == parent.Id
                && child.Lifecycle != InjectorScopeLifecycle.Ended)
            {
                throw new InjectorScopeGraphException(
                    $"Cannot end '{parent.Id}' while child '{child.Id}' is {child.Lifecycle}.");
            }
        }
    }
}
