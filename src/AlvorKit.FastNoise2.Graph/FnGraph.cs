namespace AlvorKit;

/// <summary>Creates and configures managed FastNoise2 graph nodes.</summary>
/// <param name="fn">The binding implementation used to create, configure, sample, and release nodes.</param>
/// <param name="maximumFeatureSet">The greatest FastSIMD implementation the native dispatcher may select.</param>
/// <remarks>
/// Graph construction is a cold configuration operation. The graph validates exact metadata members, graph ownership,
/// and cycles. It retains one independently finalizable native handle per creation result. Sampling does not traverse
/// or validate graph state and does not allocate managed memory.
/// </remarks>
/// <exception cref="ArgumentNullException"><paramref name="fn"/> is null.</exception>
/// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumFeatureSet"/> is not defined.</exception>
public class FnGraph(Fn fn, FnFeatureSet maximumFeatureSet)
{
    private readonly FnMetadata metadata = new(RequireBinding(fn));
    private readonly List<FnNodeHandle> handles = [];
    private readonly Dictionary<FnConnectionKey, FnNode> connections = [];
    private readonly HashSet<FnNode> opaqueEncodedRoots = [];
    private readonly uint maximumFeatureSet = ValidateFeatureSet(maximumFeatureSet);

    /// <summary>Creates a graph that selects the fastest compiled FastSIMD implementation supported by the current CPU.</summary>
    /// <param name="fn">The binding implementation retained by this graph and its native handles.</param>
    public FnGraph(Fn fn) : this(fn, FnFeatureSet.Maximum)
    {
    }

    /// <summary>Creates a managed node handle of the requested typed kind.</summary>
    /// <param name="type">The FastNoise2 1.1.1 metadata node to instantiate.</param>
    /// <returns>A value handle that keeps this graph and its new mutable node alive.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a defined value.</exception>
    /// <exception cref="InvalidOperationException">The pinned metadata is absent or native construction fails.</exception>
    /// <remarks>
    /// The native <c>fnNewFromMetadata</c> call constructs a node whose external reference is retained by this graph in
    /// a finalizable handle. Metadata names are resolved with ordinal, case-sensitive comparison. Connect all required
    /// sources before sampling; the sampling path deliberately does not revalidate the graph.
    /// </remarks>
    public FnGraphNode Create(FnNodeType type)
    {
        var name = FnNames.Node(type);
        var metadataId = metadata.FindNode(name);
        var node = fn.NewFromMetadata(metadataId, maximumFeatureSet);

        if (node == default)
            throw new InvalidOperationException($"FastNoise2 failed to create node '{name}'.");

        handles.Add(new FnNodeHandle(fn, node));
        return new(this, node);
    }

    /// <summary>Loads a managed root handle from FastNoise2's encoded Base64 node-tree format.</summary>
    /// <param name="encodedTree">A complete tree copied from the upstream Node Editor.</param>
    /// <returns>A value handle that keeps this graph and its decoded root alive.</returns>
    /// <exception cref="ArgumentException"><paramref name="encodedTree"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">FastNoise2 rejects the encoded tree.</exception>
    /// <remarks>
    /// <c>fnNewFromEncodedNodeTree</c> returns only the root reference; native node connections retain all descendants.
    /// Encoded trees are version-coupled assets. This package loads them but does not expose an encoding operation.
    /// The C API also does not expose their existing connections, so constant-valued hybrid mutation is unavailable on
    /// a decoded root; required-source and node-valued hybrid replacement remain available.
    /// </remarks>
    public FnGraphNode CreateEncoded(string encodedTree)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedTree);
        var node = fn.NewFromEncodedNodeTree(encodedTree, maximumFeatureSet);

        if (node == default)
            throw new InvalidOperationException("FastNoise2 rejected the encoded node tree.");

        handles.Add(new FnNodeHandle(fn, node));
        opaqueEncodedRoots.Add(node);
        return new(this, node);
    }

    /// <summary>Gets the binding used by this graph.</summary>
    internal Fn Binding => fn;

    /// <summary>Gets the native cumulative FastSIMD mask for a live node.</summary>
    internal FnFeatureSet GetActiveFeatureSet(FnGraphNode node) =>
        (FnFeatureSet)fn.GetActiveFeatureSet(RequireOwned(node));

    /// <summary>Resolves and sets one exact float metadata member.</summary>
    internal void SetFloat(FnGraphNode target, FnFloatVariable variable, float value)
    {
        var node = RequireOwned(target);
        var key = FnNames.Float(variable);
        var index = metadata.FindFloat(node, key);

        if (!fn.SetVariableFloat(node, index, value))
            throw metadata.Rejected(node, key, value);
    }

    /// <summary>Resolves and sets one exact integer metadata member.</summary>
    internal void SetInteger(FnGraphNode target, FnIntegerVariable variable, int value)
    {
        var node = RequireOwned(target);
        var key = FnNames.Integer(variable);
        var index = metadata.FindInteger(node, key);

        if (!fn.SetVariableIntEnum(node, index, value))
            throw metadata.Rejected(node, key, value);
    }

    /// <summary>Resolves and sets the Distance Function enum option.</summary>
    internal void SetDistanceFunction(FnGraphNode target, FnDistanceFunction value) =>
        SetEnum(target, "Distance Function", FnNames.DistanceFunction(value));

    /// <summary>Resolves and sets the cellular Return Type enum option.</summary>
    internal void SetCellularReturnType(FnGraphNode target, FnCellularReturnType value) =>
        SetEnum(target, "Return Type", FnNames.CellularReturnType(value));

    /// <summary>Resolves and sets the Interpolation enum option.</summary>
    internal void SetInterpolation(FnGraphNode target, FnInterpolation value) =>
        SetEnum(target, "Interpolation", FnNames.Interpolation(value));

    /// <summary>Resolves and sets the Remove Dimension enum option.</summary>
    internal void SetRemovedDimension(FnGraphNode target, FnRemovedDimension value) =>
        SetEnum(target, "Remove Dimension", FnNames.RemovedDimension(value));

    /// <summary>Resolves and sets the Rotation Type enum option.</summary>
    internal void SetRotationType(FnGraphNode target, FnRotationType value) =>
        SetEnum(target, "Rotation Type", FnNames.RotationType(value));

    /// <summary>Resolves and sets the Vectorization Scheme enum option.</summary>
    internal void SetVectorizationScheme(FnGraphNode target, FnVectorizationScheme value) =>
        SetEnum(target, "Vectorization Scheme", FnNames.VectorizationScheme(value));

    /// <summary>Maps a Boolean to the exact Clamp Output enum option.</summary>
    internal void SetClampOutput(FnGraphNode target, bool value) =>
        SetEnum(target, "Clamp Output", value ? "True" : "False");

    /// <summary>Sets a hybrid constant after proving no undetachable node is connected.</summary>
    internal void SetHybrid(FnGraphNode target, FnHybrid hybrid, float value)
    {
        var node = RequireOwned(target);
        var key = FnNames.Hybrid(hybrid);
        var index = metadata.FindHybrid(node, key);
        var connectionKey = new FnConnectionKey(node, true, index);

        if (opaqueEncodedRoots.Contains(node))
        {
            throw new InvalidOperationException(
                $"FastNoise2 hybrid '{metadata.Name(node)}.{FnMetadata.Display(key)}' came from an encoded root; " +
                "the pinned C API cannot report whether a node is already connected or detach that connection.");
        }

        if (connections.ContainsKey(connectionKey))
        {
            throw new InvalidOperationException(
                $"FastNoise2 hybrid '{metadata.Name(node)}.{FnMetadata.Display(key)}' already has a node connection; " +
                "the pinned runtime cannot detach it to reactivate a constant.");
        }

        if (!fn.SetHybridFloat(node, index, value))
            throw metadata.Rejected(node, key, value);
    }

    /// <summary>Connects an acyclic hybrid source.</summary>
    internal void SetHybrid(FnGraphNode target, FnHybrid hybrid, FnGraphNode source)
    {
        var node = RequireOwned(target);
        var sourceNode = RequireOwned(source);
        var key = FnNames.Hybrid(hybrid);
        var index = metadata.FindHybrid(node, key);
        var connectionKey = new FnConnectionKey(node, true, index);

        RequireAcyclicConnection(node, sourceNode);

        if (!fn.SetHybridNodeLookup(node, index, sourceNode))
            throw metadata.Rejected(node, key, metadata.Name(sourceNode));

        connections[connectionKey] = sourceNode;
    }

    /// <summary>Connects an acyclic required source.</summary>
    internal void SetSource(FnGraphNode target, FnSource source, FnGraphNode value)
    {
        var node = RequireOwned(target);
        var sourceNode = RequireOwned(value);
        var key = FnNames.Source(source);
        var index = metadata.FindSource(node, key);
        var connectionKey = new FnConnectionKey(node, false, index);

        RequireAcyclicConnection(node, sourceNode);

        if (!fn.SetNodeLookup(node, index, sourceNode))
            throw metadata.Rejected(node, key, metadata.Name(sourceNode));

        connections[connectionKey] = sourceNode;
    }

    /// <summary>Resolves an exact enum member and option name before setting its runtime index.</summary>
    private void SetEnum(FnGraphNode target, string variableName, string optionName)
    {
        var node = RequireOwned(target);
        var key = FnMemberKey.Scalar(variableName);
        var variableIndex = metadata.FindEnum(node, key);
        var optionIndex = metadata.FindEnumOption(node, variableIndex, key, optionName);

        if (!fn.SetVariableIntEnum(node, variableIndex, optionIndex))
            throw metadata.Rejected(node, key, optionName);
    }

    /// <summary>Returns a native handle only when it belongs to this configuration graph.</summary>
    private FnNode RequireOwned(FnGraphNode node)
    {
        if (!ReferenceEquals(node.Owner, this))
            throw new InvalidOperationException("The FastNoise2 node is default or belongs to another graph.");

        return node.Native;
    }

    /// <summary>Rejects a connection when its source already reaches its target.</summary>
    private void RequireAcyclicConnection(FnNode target, FnNode source)
    {
        if (target == source || CanReach(source, target, []))
            throw new InvalidOperationException("FastNoise2 node connections must form an acyclic graph.");
    }

    /// <summary>Traverses tracked outgoing connections to find a target node.</summary>
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

    /// <summary>Rejects invented FastSIMD masks and returns a native ceiling.</summary>
    private static uint ValidateFeatureSet(FnFeatureSet featureSet)
    {
        if (!Enum.IsDefined(featureSet))
            throw new ArgumentOutOfRangeException(nameof(featureSet), featureSet, "Unknown FastSIMD feature set.");

        return (uint)featureSet;
    }

    /// <summary>Rejects a null borrowed binding during field initialization.</summary>
    private static Fn RequireBinding(Fn? value) => value ?? throw new ArgumentNullException("fn");

}
