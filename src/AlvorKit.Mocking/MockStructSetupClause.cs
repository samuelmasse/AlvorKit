namespace AlvorKit.Mocking;

/// <summary>
/// Configures behavior and live-<c>this</c> handling for one void struct
/// operation.
/// </summary>
/// <typeparam name="T">The exact non-ref struct receiver type.</typeparam>
public sealed class MockStructSetupClause<T>
    where T : struct
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
    public MockStructSetupClause<T> SnapshotThisOnEntry<TResult>(
        SnapshotProjector<T, TResult> projector)
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
    public MockStructSetupClause<T> SnapshotThisOnExit<TResult>(
        SnapshotProjector<T, TResult> projector)
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
    public MockStructSetupClause<T> MutateThisOnEntry(
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
    public MockStructSetupClause<T> MutateThisOnExit(
        MockStructMutation<T> mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        return new(
            publisher.WithMutation(
                MockSnapshotPhase.Exit,
                mutation));
    }

    /// <summary>Runs an exact synchronous callback instead of the operation.</summary>
    public void Do(Delegate callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        publisher.Publish(
            MockStructBehavior.CallbackBehavior(callback));
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
}
