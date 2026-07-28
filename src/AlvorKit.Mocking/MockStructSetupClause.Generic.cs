namespace AlvorKit.Mocking;

/// <summary>
/// Configures behavior and live-<c>this</c> handling for one value-returning
/// struct operation.
/// </summary>
/// <typeparam name="T">The exact non-ref struct receiver type.</typeparam>
/// <typeparam name="TResult">The exact operation return type.</typeparam>
public sealed class MockStructSetupClause<T, TResult>
    where T : struct
    where TResult : allows ref struct
{
    private readonly MockStructSetupPublisher publisher;

    /// <summary>Creates one clause around an immutable setup publisher.</summary>
    internal MockStructSetupClause(
        MockStructSetupPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        this.publisher = publisher;
    }

    /// <summary>
    /// Copies live entry <c>this</c> into one heap-safe history
    /// representation before any configured mutation.
    /// </summary>
    public MockStructSetupClause<T, TResult>
        SnapshotThisOnEntry<TSnapshot>(
            SnapshotProjector<T, TSnapshot> projector)
    {
        ArgumentNullException.ThrowIfNull(projector);
        return new(
            publisher.WithProjection(
                MockSnapshotPhase.Entry,
                projector));
    }

    /// <summary>
    /// Copies final live <c>this</c> into one heap-safe history
    /// representation after normal completion and post-call mutation.
    /// </summary>
    public MockStructSetupClause<T, TResult>
        SnapshotThisOnExit<TSnapshot>(
            SnapshotProjector<T, TSnapshot> projector)
    {
        ArgumentNullException.ThrowIfNull(projector);
        return new(
            publisher.WithProjection(
                MockSnapshotPhase.Exit,
                projector));
    }

    /// <summary>
    /// Mutates writable live <c>this</c> after entry selection and projection
    /// but before the selected behavior.
    /// </summary>
    public MockStructSetupClause<T, TResult> MutateThisOnEntry(
        MockStructMutation<T> mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        return new(
            publisher.WithMutation(
                MockSnapshotPhase.Entry,
                mutation));
    }

    /// <summary>
    /// Mutates writable live <c>this</c> after normal behavior completion and
    /// before the exit projection.
    /// </summary>
    public MockStructSetupClause<T, TResult> MutateThisOnExit(
        MockStructMutation<T> mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        return new(
            publisher.WithMutation(
                MockSnapshotPhase.Exit,
                mutation));
    }

    /// <summary>
    /// Calculates the result through an exact synchronous callback containing
    /// live <c>this</c> and the operation's declared arguments.
    /// </summary>
    public void Answer(Delegate callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        publisher.Publish(
            MockStructBehavior.CallbackBehavior(callback));
    }

    /// <summary>
    /// Invokes an exact factory for every matching call without retaining a
    /// borrowed result.
    /// </summary>
    public void ReturnFactory(Func<TResult> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        publisher.Publish(
            MockStructBehavior.ReturnFactory(factory));
    }

    /// <summary>Throws the supplied exception instead of the operation.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        publisher.Publish(MockStructBehavior.Throw(exception));
    }

    /// <summary>Executes the preserved original operation.</summary>
    public void Passthrough() =>
        publisher.Publish(MockStructBehavior.Passthrough());

    /// <summary>Rejects the matching call with a strict diagnostic.</summary>
    public void Strict() =>
        publisher.Publish(MockStructBehavior.Strict());

    /// <summary>Publishes one ordinary heap-safe return value.</summary>
    internal void AddReturn<TValue>(TValue value) =>
        publisher.Publish(MockStructBehavior.Return(value));
}
