namespace AlvorKit;

/// <summary>Describes one validated declared-index projection registration.</summary>
internal abstract class MockSnapshotProjector(
    int declaredIndex,
    Type declaredType,
    Type valueType,
    Type resultType,
    MockSnapshotPhase phase)
{
    /// <summary>Gets the declared parameter index.</summary>
    internal int DeclaredIndex { get; } = declaredIndex;

    /// <summary>Gets the exact declared parameter type.</summary>
    internal Type DeclaredType { get; } = declaredType;

    /// <summary>Gets the live value type accepted by the projector.</summary>
    internal Type ValueType { get; } = valueType;

    /// <summary>Gets the heap-safe projector result type.</summary>
    internal Type ResultType { get; } = resultType;

    /// <summary>Gets the invocation phase observed by the projector.</summary>
    internal MockSnapshotPhase Phase { get; } = phase;
}

/// <summary>Projects one exact live value type without boxing the input.</summary>
internal abstract class MockSnapshotProjector<T>(
    int declaredIndex,
    Type declaredType,
    Type resultType,
    MockSnapshotPhase phase) :
    MockSnapshotProjector(
        declaredIndex,
        declaredType,
        typeof(T),
        resultType,
        phase)
    where T : allows ref struct
{
    /// <summary>Invokes the exact projector and returns only its heap-safe result.</summary>
    internal abstract object? Project(scoped in T value);
}

/// <summary>Retains one exact typed projector delegate as setup state.</summary>
internal sealed class MockSnapshotProjector<T, TResult>(
    int declaredIndex,
    Type declaredType,
    MockSnapshotPhase phase,
    SnapshotProjector<T, TResult> projector) :
    MockSnapshotProjector<T>(
        declaredIndex,
        declaredType,
        typeof(TResult),
        phase)
    where T : allows ref struct
{
    /// <inheritdoc />
    internal override object? Project(scoped in T value) =>
        projector(in value);
}
