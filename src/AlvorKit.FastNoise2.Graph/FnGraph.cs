namespace AlvorKit;

/// <summary>Creates, validates, and owns a graph of native FastNoise2 nodes.</summary>
/// <param name="fn">The borrowed binding implementation. It must outlive this graph and every sampling call.</param>
/// <param name="maximumFeatureSet">The greatest FastSIMD implementation the native dispatcher may select.</param>
/// <remarks>
/// Graph construction is a cold configuration operation. The graph validates exact metadata members, graph ownership,
/// required connections, and cycles. Sampling through a complete, immutable graph does not allocate managed memory.
/// </remarks>
public class FnGraph(Fn fn, FnFeatureSet maximumFeatureSet) : IDisposable
{
    private readonly FnMetadata metadata = new(fn);
    private readonly List<FnNode> nodes = [];
    private readonly Dictionary<FnConnectionKey, FnNode> connections = [];
    private readonly HashSet<FnNode> readyNodes = [];
    private readonly HashSet<FnNode> encodedRoots = [];
    private readonly uint maximumFeatureSet = ValidateFeatureSet(maximumFeatureSet);
    private int generation;
    private bool disposed;

    /// <summary>Creates a graph that selects the fastest compiled FastSIMD implementation supported by the current CPU.</summary>
    /// <param name="fn">The borrowed binding implementation. It must outlive this graph and every sampling call.</param>
    public FnGraph(Fn fn) : this(fn, FnFeatureSet.Maximum)
    {
    }

    /// <summary>Creates and retains a node of the requested typed kind.</summary>
    /// <param name="type">The FastNoise2 1.1.1 metadata node to instantiate.</param>
    /// <returns>A non-owning value handle to the new mutable node.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a defined value.</exception>
    /// <exception cref="InvalidOperationException">The pinned metadata is absent or native construction fails.</exception>
    /// <exception cref="ObjectDisposedException">This graph has been disposed.</exception>
    /// <remarks>
    /// The native <c>fnNewFromMetadata</c> call constructs a node and this graph owns its returned reference. Metadata
    /// names are resolved with ordinal, case-sensitive comparison. Required sources must be connected before sampling.
    /// </remarks>
    public FnGraphNode Create(FnNodeType type)
    {
        var name = FnNames.Node(type);
        var metadataId = metadata.FindNode(name);
        ThrowIfDisposed();
        var node = fn.NewFromMetadata(metadataId, maximumFeatureSet);

        if (node == default)
            throw new InvalidOperationException($"FastNoise2 failed to create node '{name}'.");

        nodes.Add(node);
        RebuildReadyNodes();
        return new(this, node, generation);
    }

    /// <summary>Loads and retains a complete graph from FastNoise2's encoded Base64 node-tree format.</summary>
    /// <param name="encodedTree">A complete tree copied from the upstream Node Editor.</param>
    /// <returns>A non-owning handle to the decoded root node.</returns>
    /// <exception cref="ArgumentException"><paramref name="encodedTree"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">FastNoise2 rejects the encoded tree.</exception>
    /// <exception cref="ObjectDisposedException">This graph has been disposed.</exception>
    /// <remarks>
    /// <c>fnNewFromEncodedNodeTree</c> returns only the root reference; native node connections retain all descendants.
    /// Encoded trees are version-coupled assets. This package loads them but does not expose an encoding operation.
    /// </remarks>
    public FnGraphNode CreateEncoded(string encodedTree)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedTree);
        var node = fn.NewFromEncodedNodeTree(encodedTree, maximumFeatureSet);

        if (node == default)
            throw new InvalidOperationException("FastNoise2 rejected the encoded node tree.");

        nodes.Add(node);
        encodedRoots.Add(node);
        readyNodes.Add(node);
        return new(this, node, generation);
    }

    /// <summary>Releases all owned native node references and invalidates every previously returned handle.</summary>
    /// <exception cref="ObjectDisposedException">This graph has already been disposed.</exception>
    /// <remarks>
    /// References are released in reverse creation order through <c>fnDeleteNodeRef</c>. The operation is repeatable and
    /// the graph remains reusable. Do not clear while another thread is configuring or sampling this graph.
    /// </remarks>
    public void Clear()
    {
        ThrowIfDisposed();
        ReleaseNodes();
    }

    /// <summary>Releases all native node references and makes this graph permanently unusable.</summary>
    /// <remarks>This operation is idempotent. It must not overlap configuration or sampling.</remarks>
    public void Dispose()
    {
        if (disposed)
            return;

        ReleaseNodes();
        disposed = true;
    }

    internal Fn UseForSampling(FnGraphNode node)
    {
        var native = RequireLive(node);

        if (!readyNodes.Contains(native))
            throw new InvalidOperationException(
                $"FastNoise2 node '{metadata.Name(native)}' has an incomplete required-source graph.");

        return fn;
    }

    internal FnFeatureSet GetActiveFeatureSet(FnGraphNode node) =>
        (FnFeatureSet)fn.GetActiveFeatureSet(RequireLive(node));

    private void ReleaseNodes()
    {
        for (var index = nodes.Count - 1; index >= 0; index--)
            fn.DeleteNodeRef(nodes[index]);

        nodes.Clear();
        connections.Clear();
        readyNodes.Clear();
        encodedRoots.Clear();
        generation++;
    }

    internal void SetFloat(FnGraphNode target, FnFloatVariable variable, float value)
    {
        var node = RequireLive(target);
        var key = FnNames.Float(variable);
        var index = metadata.FindFloat(node, key);

        if (!fn.SetVariableFloat(node, index, value))
            throw metadata.Rejected(node, key, value);
    }

    internal void SetInteger(FnGraphNode target, FnIntegerVariable variable, int value)
    {
        var node = RequireLive(target);
        var key = FnNames.Integer(variable);
        var index = metadata.FindInteger(node, key);

        if (!fn.SetVariableIntEnum(node, index, value))
            throw metadata.Rejected(node, key, value);
    }

    internal void SetDistanceFunction(FnGraphNode target, FnDistanceFunction value) =>
        SetEnum(target, "Distance Function", FnNames.DistanceFunction(value));

    internal void SetCellularReturnType(FnGraphNode target, FnCellularReturnType value) =>
        SetEnum(target, "Return Type", FnNames.CellularReturnType(value));

    internal void SetInterpolation(FnGraphNode target, FnInterpolation value) =>
        SetEnum(target, "Interpolation", FnNames.Interpolation(value));

    internal void SetRemovedDimension(FnGraphNode target, FnRemovedDimension value) =>
        SetEnum(target, "Remove Dimension", FnNames.RemovedDimension(value));

    internal void SetRotationType(FnGraphNode target, FnRotationType value) =>
        SetEnum(target, "Rotation Type", FnNames.RotationType(value));

    internal void SetVectorizationScheme(FnGraphNode target, FnVectorizationScheme value) =>
        SetEnum(target, "Vectorization Scheme", FnNames.VectorizationScheme(value));

    internal void SetClampOutput(FnGraphNode target, bool value) =>
        SetEnum(target, "Clamp Output", value ? "True" : "False");

    internal void SetHybrid(FnGraphNode target, FnHybrid hybrid, float value)
    {
        var node = RequireLive(target);
        var key = FnNames.Hybrid(hybrid);
        var index = metadata.FindHybrid(node, key);
        var connectionKey = new FnConnectionKey(node, true, index);

        if (connections.ContainsKey(connectionKey))
        {
            throw new InvalidOperationException(
                $"FastNoise2 hybrid '{metadata.Name(node)}.{FnMetadata.Display(key)}' already has a node connection; " +
                "the pinned runtime cannot detach it to reactivate a constant.");
        }

        if (!fn.SetHybridFloat(node, index, value))
            throw metadata.Rejected(node, key, value);
    }

    internal void SetHybrid(FnGraphNode target, FnHybrid hybrid, FnGraphNode source)
    {
        var node = RequireLive(target);
        var sourceNode = RequireLive(source);
        var key = FnNames.Hybrid(hybrid);
        var index = metadata.FindHybrid(node, key);
        var connectionKey = new FnConnectionKey(node, true, index);

        RequireAcyclicConnection(node, sourceNode);

        if (!fn.SetHybridNodeLookup(node, index, sourceNode))
            throw metadata.Rejected(node, key, metadata.Name(sourceNode));

        connections[connectionKey] = sourceNode;
        RebuildReadyNodes();
    }

    internal void SetSource(FnGraphNode target, FnSource source, FnGraphNode value)
    {
        var node = RequireLive(target);
        var sourceNode = RequireLive(value);
        var key = FnNames.Source(source);
        var index = metadata.FindSource(node, key);
        var connectionKey = new FnConnectionKey(node, false, index);

        RequireAcyclicConnection(node, sourceNode);

        if (!fn.SetNodeLookup(node, index, sourceNode))
            throw metadata.Rejected(node, key, metadata.Name(sourceNode));

        connections[connectionKey] = sourceNode;
        RebuildReadyNodes();
    }

    private void SetEnum(FnGraphNode target, string variableName, string optionName)
    {
        var node = RequireLive(target);
        var key = FnMemberKey.Scalar(variableName);
        var variableIndex = metadata.FindEnum(node, key);
        var optionIndex = metadata.FindEnumOption(node, variableIndex, key, optionName);

        if (!fn.SetVariableIntEnum(node, variableIndex, optionIndex))
            throw metadata.Rejected(node, key, optionName);
    }

    private FnNode RequireLive(FnGraphNode node)
    {
        ThrowIfDisposed();

        if (!ReferenceEquals(node.Owner, this) || node.Generation != generation || node.Native == default)
            throw new InvalidOperationException("The FastNoise2 node is default, belongs to another graph, or was released.");

        return node.Native;
    }

    private void RequireAcyclicConnection(FnNode target, FnNode source)
    {
        if (target == source || CanReach(source, target, []))
            throw new InvalidOperationException("FastNoise2 node connections must form an acyclic graph.");
    }

    private bool CanReach(FnNode current, FnNode wanted, HashSet<FnNode> visited)
    {
        if (!visited.Add(current))
            return false;

        foreach (var connection in connections)
        {
            if (connection.Key.Target != current)
                continue;

            if (connection.Value == wanted || CanReach(connection.Value, wanted, visited))
                return true;
        }

        return false;
    }

    private void RebuildReadyNodes()
    {
        readyNodes.Clear();
        var incompleteNodes = new HashSet<FnNode>();

        foreach (var node in nodes)
            IsReady(node, incompleteNodes);
    }

    private bool IsReady(FnNode node, HashSet<FnNode> incompleteNodes)
    {
        if (encodedRoots.Contains(node) || readyNodes.Contains(node))
            return true;

        if (incompleteNodes.Contains(node))
            return false;

        var requiredCount = metadata.RequiredSourceCount(node);
        var connectedRequiredCount = 0;

        foreach (var connection in connections)
        {
            if (connection.Key.Target != node)
                continue;

            if (!connection.Key.IsHybrid)
                connectedRequiredCount++;

            if (!IsReady(connection.Value, incompleteNodes))
            {
                incompleteNodes.Add(node);
                return false;
            }
        }

        if (connectedRequiredCount != requiredCount)
        {
            incompleteNodes.Add(node);
            return false;
        }

        readyNodes.Add(node);
        return true;
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);

    private static uint ValidateFeatureSet(FnFeatureSet featureSet)
    {
        if (!Enum.IsDefined(featureSet))
            throw new ArgumentOutOfRangeException(nameof(featureSet), featureSet, "Unknown FastSIMD feature set.");

        return (uint)featureSet;
    }

}
