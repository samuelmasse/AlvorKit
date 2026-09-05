namespace AlvorKit;

/// <summary>Creates and configures managed FastNoise2 graph nodes.</summary>
/// <param name="fn">The binding implementation used to create, configure, sample, and release nodes.</param>
/// <remarks>
/// Graph construction is a cold configuration operation. The graph validates exact metadata members, graph ownership,
/// and cycles. Each node owns its independently finalizable native handle and connected dependencies; this service
/// does not retain created nodes. Sampling does not traverse or validate graph state and does not allocate managed memory.
/// </remarks>
/// <exception cref="ArgumentNullException"><paramref name="fn"/> is null.</exception>
public class FnGraph(Fn fn)
{
    private const uint MaximumFeatureSet = (uint)FnFeatureSet.Maximum;

    private readonly FnMetadata metadata = new(RequireBinding(fn));

    /// <summary>Creates a managed node handle of the requested typed kind.</summary>
    /// <param name="type">The FastNoise2 1.1.1 metadata node to instantiate.</param>
    /// <returns>A value handle that independently owns the new mutable node and retains its configuration service.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a defined value.</exception>
    /// <exception cref="InvalidOperationException">The pinned metadata is absent or native construction fails.</exception>
    /// <remarks>
    /// The native <c>fnNewFromMetadata</c> call constructs a node whose external reference belongs to its managed state in
    /// a finalizable handle. Metadata names are resolved with ordinal, case-sensitive comparison. Connect all required
    /// sources before sampling; the sampling path deliberately does not revalidate the graph.
    /// </remarks>
    public FnGraphNode Create(FnNodeType type)
    {
        var name = FnNames.Node(type);
        var metadataId = metadata.FindNode(name);
        var node = fn.NewFromMetadata(metadataId, MaximumFeatureSet);

        if (node == default)
            throw new InvalidOperationException($"FastNoise2 failed to create node '{name}'.");

        return new(new FnGraphNodeState(this, new FnNodeHandle(fn, node), false));
    }

    /// <summary>Loads a managed root handle from FastNoise2's encoded Base64 node-tree format.</summary>
    /// <param name="encodedTree">A complete tree copied from the upstream Node Editor.</param>
    /// <returns>A value handle that independently owns the decoded root and retains its configuration service.</returns>
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
        var node = fn.NewFromEncodedNodeTree(encodedTree, MaximumFeatureSet);

        if (node == default)
            throw new InvalidOperationException("FastNoise2 rejected the encoded node tree.");

        return new(new FnGraphNodeState(this, new FnNodeHandle(fn, node), true));
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
        var connectionKey = new FnConnectionKey(true, index);

        if (target.State.IsEncoded)
        {
            throw new InvalidOperationException(
                $"FastNoise2 hybrid '{metadata.Name(node)}.{FnMetadata.Display(key)}' came from an encoded root; " +
                "the pinned C API cannot report whether a node is already connected or detach that connection.");
        }

        if (target.State.HasConnection(connectionKey))
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
        var connectionKey = new FnConnectionKey(true, index);

        target.State.RequireAcyclicConnection(source.State);

        if (!fn.SetHybridNodeLookup(node, index, sourceNode))
            throw metadata.Rejected(node, key, metadata.Name(sourceNode));

        target.State.RetainConnection(connectionKey, source.State);
    }

    /// <summary>Connects an acyclic required source.</summary>
    internal void SetSource(FnGraphNode target, FnSource source, FnGraphNode value)
    {
        var node = RequireOwned(target);
        var sourceNode = RequireOwned(value);
        var key = FnNames.Source(source);
        var index = metadata.FindSource(node, key);
        var connectionKey = new FnConnectionKey(false, index);

        target.State.RequireAcyclicConnection(value.State);

        if (!fn.SetNodeLookup(node, index, sourceNode))
            throw metadata.Rejected(node, key, metadata.Name(sourceNode));

        target.State.RetainConnection(connectionKey, value.State);
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

    /// <summary>Rejects a null borrowed binding during field initialization.</summary>
    private static Fn RequireBinding(Fn? value) => value ?? throw new ArgumentNullException("fn");

}
