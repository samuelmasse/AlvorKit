namespace AlvorKit.Mocking;

/// <summary>
/// Holds immutable struct selection, capture, projection, and mutation
/// metadata without retaining receiver storage.
/// </summary>
internal sealed class MockStructSetupDescriptor
{
    private readonly MockStructThisProjection[] projections;
    private readonly MockStructThisMutation[] mutations;

    /// <summary>Creates immutable metadata for one static struct operation capture.</summary>
    internal MockStructSetupDescriptor(
        MockStructScopeDescriptor scope,
        Delegate operation,
        Type resultType,
        ReadOnlySpan<MockStructThisProjection> projections = default,
        ReadOnlySpan<MockStructThisMutation> mutations = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(resultType);
        if (RetainsCaptureState(operation))
        {
            throw new MockException(
                "A struct operation capture must not close over state. Use a " +
                "static lambda and receive live storage only through its " +
                "scoped ref parameter.");
        }

        Scope = scope;
        Operation = operation;
        ResultType = resultType;
        this.projections = projections.ToArray();
        this.mutations = mutations.ToArray();
    }

    /// <summary>Gets the receiver selection scope.</summary>
    internal MockStructScopeDescriptor Scope { get; }

    /// <summary>Gets the static capture delegate.</summary>
    internal Delegate Operation { get; }

    /// <summary>Gets the exact logical result type.</summary>
    internal Type ResultType { get; }

    /// <summary>Gets immutable entry and exit receiver projections.</summary>
    internal ReadOnlySpan<MockStructThisProjection> Projections =>
        projections;

    /// <summary>Gets immutable entry and exit receiver mutations.</summary>
    internal ReadOnlySpan<MockStructThisMutation> Mutations =>
        mutations;

    /// <summary>Returns a copy with one additional typed receiver projection.</summary>
    internal MockStructSetupDescriptor WithProjection<T, TResult>(
        MockSnapshotPhase phase,
        SnapshotProjector<T, TResult> projector)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(projector);
        if (typeof(T) != Scope.StructType)
        {
            throw new MockException(
                $"Struct projector type '{typeof(T)}' does not match " +
                $"scope type '{Scope.StructType}'.");
        }
        if (typeof(TResult) == typeof(void) ||
            typeof(TResult).IsByRefLike ||
            typeof(TResult).IsByRef ||
            typeof(TResult).IsPointer)
        {
            throw new MockException(
                $"Struct history projections must be heap-safe values, not " +
                $"'{typeof(TResult)}'.");
        }

        var updated =
            new MockStructThisProjection[projections.Length + 1];
        projections.CopyTo(updated);
        updated[^1] = new(
            phase,
            new MockSnapshotProjector<T, TResult>(
                0,
                typeof(T).MakeByRefType(),
                phase,
                projector));
        return new(
            Scope,
            Operation,
            ResultType,
            updated,
            mutations);
    }

    /// <summary>Returns a copy with one additional live receiver mutation.</summary>
    internal MockStructSetupDescriptor WithMutation(
        MockSnapshotPhase phase,
        Delegate mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        var updated =
            new MockStructThisMutation[mutations.Length + 1];
        mutations.CopyTo(updated);
        updated[^1] = new(phase, mutation);
        return new(
            Scope,
            Operation,
            ResultType,
            projections,
            updated);
    }

    /// <summary>Gets whether compiler-generated target storage retains captured state.</summary>
    private static bool RetainsCaptureState(Delegate operation)
    {
        object? target = operation.Target;
        if (target is null)
            return false;

        Type targetType = target.GetType();
        if (!targetType.IsDefined(
                typeof(CompilerGeneratedAttribute),
                inherit: false))
        {
            return true;
        }

        return targetType.GetFields(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic).Length != 0;
    }
}
