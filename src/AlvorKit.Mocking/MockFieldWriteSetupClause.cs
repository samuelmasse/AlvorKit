namespace AlvorKit.Mocking;

/// <summary>Configures one ordinary instance or static field write.</summary>
public sealed class MockFieldWriteSetupClause<T>
    where T : allows ref struct
{
    private readonly MockReceiverFreeSetupPublisher publisher;

    /// <summary>Creates one field-write setup clause.</summary>
    internal MockFieldWriteSetupClause(
        MockReceiverFreeSetupPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        if (publisher.Descriptor.OperationKind !=
            MockInvocationOperationKind.FieldWrite)
        {
            throw new MockException(
                "A field-write clause requires exact field-write metadata.");
        }

        this.publisher = publisher;
    }

    /// <summary>Restricts this setup to one exact interception field-write site.</summary>
    public MockFieldWriteSetupClause<T> AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new(publisher.AtSite(site));
    }

    /// <summary>Observes the incoming value, then performs the original write.</summary>
    public void Observe(MockValueObserver<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        publisher.Publish(
            MockReceiverFreeBehavior.Observe(observer));
    }

    /// <summary>Transforms the incoming value before the original write.</summary>
    public void Transform(MockValueTransform<T> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        publisher.Publish(
            MockReceiverFreeBehavior.Transform(transform));
    }

    /// <summary>Performs the original field write unchanged.</summary>
    public void Passthrough() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Passthrough([]));

    /// <summary>Throws instead of writing the field.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        publisher.Publish(
            MockReceiverFreeBehavior.Throw(exception, []));
    }

    /// <summary>Rejects the matching write with a strict mock diagnostic.</summary>
    public void Strict() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Strict([]));
}
