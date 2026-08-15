namespace AlvorKit;

/// <summary>Configures one ordinary instance or static field read.</summary>
public sealed class MockFieldReadSetupClause<T>
    where T : allows ref struct
{
    private readonly MockReceiverFreeSetupPublisher publisher;

    /// <summary>Creates one field-read setup clause.</summary>
    internal MockFieldReadSetupClause(
        MockReceiverFreeSetupPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        if (publisher.Descriptor.OperationKind !=
            MockInvocationOperationKind.FieldRead)
        {
            throw new MockException(
                "A field-read clause requires exact field-read metadata.");
        }

        this.publisher = publisher;
    }

    /// <summary>Restricts this setup to one exact interception field-read site.</summary>
    public MockFieldReadSetupClause<T> AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new(publisher.AtSite(site));
    }

    /// <summary>Calculates a result in the exact typed interception frame.</summary>
    public void ReturnFactory(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        publisher.Publish(
            MockReceiverFreeBehavior.ReturnFactory(factory, []));
    }

    /// <summary>Observes the original value and preserves it as the result.</summary>
    public void Observe(MockValueObserver<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        publisher.Publish(
            MockReceiverFreeBehavior.Observe(observer));
    }

    /// <summary>Transforms the original field value into the returned value.</summary>
    public void Transform(MockValueTransform<T> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        publisher.Publish(
            MockReceiverFreeBehavior.Transform(transform));
    }

    /// <summary>Reads and returns the original field value.</summary>
    public void Passthrough() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Passthrough([]));

    /// <summary>Throws instead of reading the field.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        publisher.Publish(
            MockReceiverFreeBehavior.Throw(exception, []));
    }

    /// <summary>Rejects the matching read with a strict mock diagnostic.</summary>
    public void Strict() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Strict([]));

    /// <summary>Publishes one ordinary heap-safe field value.</summary>
    internal void AddOrdinaryReturn<TValue>(TValue value) =>
        publisher.Publish(
            MockReceiverFreeBehavior.OrdinaryReturn(value, []));
}
