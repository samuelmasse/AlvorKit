namespace AlvorKit.Mocking;

/// <summary>
/// Configures behavior after a constructor's mandatory base or delegated
/// constructor returns while preserving the allocated object identity.
/// </summary>
public sealed class MockConstructorBodySetupClause<T>
    where T : class
{
    private readonly MockReceiverFreeSetupPublisher publisher;

    /// <summary>Creates one constructor-body setup clause.</summary>
    internal MockConstructorBodySetupClause(
        MockReceiverFreeSetupPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        if (publisher.Descriptor.OperationKind !=
            MockInvocationOperationKind.ConstructorBody)
        {
            throw new MockException(
                "A constructor-body clause requires a captured constructor body.");
        }

        this.publisher = publisher;
    }

    /// <summary>
    /// Observes the allocated instance, then executes the remaining constructor
    /// body.
    /// </summary>
    public void Observe(Action<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        publisher.Publish(
            MockReceiverFreeBehavior.Observe(observer));
    }

    /// <summary>
    /// Observes the instance and exact constructor arguments through a natural
    /// void delegate.
    /// </summary>
    public void Observe(Delegate observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        publisher.Publish(
            MockReceiverFreeBehavior.Observe(observer));
    }

    /// <summary>
    /// Replaces the remaining constructor body while preserving the allocated
    /// instance.
    /// </summary>
    public void Replace(Action<T> replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        publisher.Publish(
            MockReceiverFreeBehavior.Replace(replacement));
    }

    /// <summary>
    /// Replaces the remaining constructor body through a natural delegate that
    /// receives the instance and exact constructor arguments.
    /// </summary>
    public void Replace(Delegate replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        publisher.Publish(
            MockReceiverFreeBehavior.Replace(replacement));
    }

    /// <summary>Executes the remaining original constructor body.</summary>
    public void Passthrough() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Passthrough([]));

    /// <summary>Throws after the mandatory constructor initializer returns.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        publisher.Publish(
            MockReceiverFreeBehavior.Throw(exception, []));
    }

    /// <summary>Rejects the matching constructor body with a strict diagnostic.</summary>
    public void Strict() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Strict([]));
}
