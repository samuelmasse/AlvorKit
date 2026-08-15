namespace AlvorKit;

/// <summary>
/// Creates and owns an explicit lifetime graph above ordinary injector scopes.
/// Scope creation and termination must flow through this object to appear in its authoritative graph.
/// </summary>
public sealed class InjectorScopeGraph : IInjectorInstanceObserver
{
    private readonly Lock gate = new();
    private readonly Dictionary<InjectorScope, InjectorScopeGraphNode> activeByScope =
        new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<InjectorScopeId, InjectorScopeGraphNode> nodes = [];
    private readonly ConditionalWeakTable<object, InjectorScopeGraphInstanceOwner> instanceOwners = [];
    private long nextId;
    private long revision;

    /// <summary>Creates a graph whose root is an existing injector or injector scope.</summary>
    public InjectorScopeGraph(InjectorScope root, string? label = null)
    {
        var node = AddNode(null, root, label);
        RootId = node.Id;
        root.Observe(this);
    }

    /// <summary>
    /// Raised synchronously after a node becomes <see cref="InjectorScopeLifecycle.Ending"/>
    /// and before caller teardown begins.
    /// </summary>
    public event Action<InjectorScopeEnding>? ScopeEnding;

    /// <summary>Gets the root node identifier.</summary>
    public InjectorScopeId RootId { get; }

    /// <summary>Gets the latest graph revision.</summary>
    public long Revision
    {
        get
        {
            lock (gate)
                return revision;
        }
    }

    /// <summary>Creates and tracks a child injector scope owned by <paramref name="parent"/>.</summary>
    public T Scope<T>(InjectorScope parent, string? label = null) where T : InjectorScope
    {
        lock (gate)
        {
            var parentNode = RequireActive(parent);
            var child = parent.Scope<T>();
            AddNode(parentNode.Id, child, label);
            return child;
        }
    }

    /// <summary>Runs an operation in a temporary tracked child scope and always ends that scope afterward.</summary>
    public void Run<T>(InjectorScope parent, Action<T> action, string? label = null) where T : InjectorScope
    {
        var scope = Scope<T>(parent, label);
        try
        {
            action(scope);
        }
        finally
        {
            End(scope);
        }
    }

    /// <summary>Changes the diagnostic label for an active tracked scope.</summary>
    public void Label(InjectorScope scope, string? label)
    {
        lock (gate)
        {
            var node = RequireActive(scope);
            node.Label = label;
            node.ChangedRevision = ++revision;
        }
    }

    /// <summary>
    /// Marks a scope as ending, runs its explicit teardown, and releases the graph's reference.
    /// Active tracked children must be ended first.
    /// </summary>
    public void End<T>(T scope, Action<T>? teardown = null) where T : InjectorScope
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

    /// <summary>Captures graph metadata without resolving or constructing injector services.</summary>
    public InjectorScopeGraphSnapshot Snapshot(bool includeEnded = false)
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
