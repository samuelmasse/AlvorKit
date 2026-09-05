namespace AlvorKit;

/// <summary>A value handle to one independently managed native FastNoise2 node.</summary>
/// <remarks>
/// Copying this struct does not clone the native node or duplicate its native reference. Every copy keeps that node and
/// its connected dependencies alive. Fluent setters mutate the shared node and return another value referring to it.
/// Finish all graph mutation before concurrent sampling begins.
/// </remarks>
public readonly struct FnGraphNode
{
    private readonly FnGraphNodeState? state;

    /// <summary>Gets the graph that configures this managed handle.</summary>
    internal FnGraph? Owner => state?.Owner;

    /// <summary>Gets the opaque native reference without changing ownership.</summary>
    internal FnNode Native => State.Native;

    /// <summary>Gets the owning state or rejects a default node value.</summary>
    internal FnGraphNodeState State =>
        state ?? throw new InvalidOperationException("The default FastNoise2 graph node cannot be used.");

    /// <summary>Creates a value sharing one node's independently managed native ownership.</summary>
    internal FnGraphNode(FnGraphNodeState state)
    {
        this.state = state;
    }

    /// <summary>Sets a scalar float variable through native <c>fnSetVariableFloat</c>.</summary>
    /// <param name="variable">The typed runtime metadata member to configure.</param>
    /// <param name="value">The new floating-point value.</param>
    /// <returns>This same node handle for fluent graph construction.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="variable"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">
    /// The node is default or does not expose the selected float member, or FastNoise2 rejects the value.
    /// </exception>
    /// <remarks>Metadata minimums and maximums are editor guidance; the native setter does not generally enforce them.</remarks>
    public FnGraphNode Float(FnFloatVariable variable, float value)
    {
        OwnerOrThrow().SetFloat(this, variable, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets an integer variable through native <c>fnSetVariableIntEnum</c>.</summary>
    /// <param name="variable">The typed runtime metadata member to configure.</param>
    /// <param name="value">The new integer value.</param>
    /// <returns>This same node handle for fluent graph construction.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="variable"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">
    /// The node is default or does not expose the selected integer member, or FastNoise2 rejects the value.
    /// </exception>
    /// <remarks>Metadata minimums and maximums are editor guidance; the native setter does not generally enforce them.</remarks>
    public FnGraphNode Integer(FnIntegerVariable variable, int value)
    {
        OwnerOrThrow().SetInteger(this, variable, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets Distance Function on a point-distance or cellular node.</summary>
    /// <param name="value">The distance metric to use.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">The node is default or has no Distance Function option.</exception>
    public FnGraphNode DistanceFunction(FnDistanceFunction value)
    {
        OwnerOrThrow().SetDistanceFunction(this, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets Return Type on <see cref="FnNodeType.CellularDistance"/>.</summary>
    /// <param name="value">The operation applied to the two selected distance ranks.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">The node is default or has no Return Type option.</exception>
    public FnGraphNode CellularReturnType(FnCellularReturnType value)
    {
        OwnerOrThrow().SetCellularReturnType(this, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets Interpolation on <see cref="FnNodeType.Fade"/>.</summary>
    /// <param name="value">The curve applied to the normalized fade input.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">The node is default or has no Interpolation option.</exception>
    public FnGraphNode Interpolation(FnInterpolation value)
    {
        OwnerOrThrow().SetInterpolation(this, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets Clamp Output on <see cref="FnNodeType.Remap"/>.</summary>
    /// <param name="value">Whether the remapped result is clamped to its output interval.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="InvalidOperationException">The node is default or has no Clamp Output option.</exception>
    public FnGraphNode ClampOutput(bool value)
    {
        OwnerOrThrow().SetClampOutput(this, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets Remove Dimension on <see cref="FnNodeType.RemoveDimension"/>.</summary>
    /// <param name="value">The coordinate to omit before source evaluation.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">The node is default or has no Remove Dimension option.</exception>
    public FnGraphNode RemovedDimension(FnRemovedDimension value)
    {
        OwnerOrThrow().SetRemovedDimension(this, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets Rotation Type on <see cref="FnNodeType.DomainRotatePlane"/>.</summary>
    /// <param name="value">The three-dimensional plane the preset rotation should improve.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">The node is default or has no Rotation Type option.</exception>
    public FnGraphNode RotationType(FnRotationType value)
    {
        OwnerOrThrow().SetRotationType(this, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets Vectorization Scheme on a simplex or SuperSimplex domain-warp node.</summary>
    /// <param name="value">The algorithm used to construct the displacement vector.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">The node is default or has no Vectorization Scheme option.</exception>
    /// <remarks>This option is unrelated to the CPU feature set reported by <see cref="GetActiveFeatureSet"/>.</remarks>
    public FnGraphNode VectorizationScheme(FnVectorizationScheme value)
    {
        OwnerOrThrow().SetVectorizationScheme(this, value);
        KeepAlive();
        return this;
    }

    /// <summary>Sets the stored constant of a hybrid input through native <c>fnSetHybridFloat</c>.</summary>
    /// <param name="hybrid">The typed hybrid input to configure.</param>
    /// <param name="value">The constant evaluated at every sampled position.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="hybrid"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">
    /// The node is default, lacks the input, rejects the value, or already has an active node connection.
    /// </exception>
    /// <remarks>
    /// FastNoise2 1.1.1 cannot detach a connected hybrid node. This wrapper rejects a later constant assignment instead
    /// of updating a dormant constant while leaving the node active. It also rejects constant mutation on decoded roots
    /// because the C API cannot reveal their preexisting hybrid connections.
    /// </remarks>
    public FnGraphNode Hybrid(FnHybrid hybrid, float value)
    {
        OwnerOrThrow().SetHybrid(this, hybrid, value);
        KeepAlive();
        return this;
    }

    /// <summary>Connects a node as the active value of a hybrid input through native <c>fnSetHybridNodeLookup</c>.</summary>
    /// <param name="hybrid">The typed hybrid connection slot.</param>
    /// <param name="source">A live source created through the same graph service.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="hybrid"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">
    /// Either node is default or foreign; the input is absent; FastNoise2 rejects it; or it creates a cycle.
    /// </exception>
    /// <remarks>The node connection takes priority over the stored constant and replaces an earlier node connection.</remarks>
    public FnGraphNode Hybrid(FnHybrid hybrid, FnGraphNode source)
    {
        OwnerOrThrow().SetHybrid(this, hybrid, source);
        KeepAlive();
        source.KeepAlive();
        return this;
    }

    /// <summary>Connects a node to a required source through native <c>fnSetNodeLookup</c>.</summary>
    /// <param name="source">The typed required connection slot.</param>
    /// <param name="value">A live source node created through the same graph service.</param>
    /// <returns>This same node handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is not defined.</exception>
    /// <exception cref="InvalidOperationException">
    /// Either node is default or foreign; the input is absent; FastNoise2 rejects it; or it creates a cycle.
    /// </exception>
    /// <remarks>
    /// The target retains the source and its managed state. A new connection replaces the previous dependency in this slot.
    /// </remarks>
    public FnGraphNode Source(FnSource source, FnGraphNode value)
    {
        OwnerOrThrow().SetSource(this, source, value);
        KeepAlive();
        value.KeepAlive();
        return this;
    }

    /// <summary>Returns the native FastSIMD feature-set mask selected for this node.</summary>
    /// <returns>
    /// The active compiled implementation. It can vary by CPU, architecture, runtime identifier, and native package build.
    /// </returns>
    /// <exception cref="InvalidOperationException">The node is the default value.</exception>
    /// <remarks>The value reports implementation capability, not output quality.</remarks>
    public FnFeatureSet GetActiveFeatureSet()
    {
        var result = OwnerOrThrow().GetActiveFeatureSet(this);
        KeepAlive();
        return result;
    }

    /// <summary>Borrows the native value and its binding with one default-value check.</summary>
    internal FnNode Borrow(out Fn binding)
    {
        var node = State;
        binding = node.Owner.Binding;
        return node.Native;
    }

    /// <summary>Keeps this node and its dependencies live until the current native call has returned.</summary>
    internal void KeepAlive() => GC.KeepAlive(state);

    /// <summary>Gets the owner or rejects a default node value.</summary>
    private FnGraph OwnerOrThrow() => State.Owner;
}
