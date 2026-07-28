namespace AlvorKit.Mocking;

/// <summary>
/// Configures substitution or original allocation for one captured
/// <c>newobj</c> operation.
/// </summary>
public sealed class MockConstructionSetupClause<T>
    where T : class
{
    private readonly MockReceiverFreeSetupPublisher publisher;

    /// <summary>Creates one construction setup clause.</summary>
    internal MockConstructionSetupClause(
        MockReceiverFreeSetupPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        if (publisher.Descriptor.OperationKind !=
            MockInvocationOperationKind.Construction)
        {
            throw new MockException(
                "A construction clause requires a captured newobj operation.");
        }

        this.publisher = publisher;
    }

    /// <summary>Restricts this setup to one exact interception allocation site.</summary>
    public MockConstructionSetupClause<T> AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new(publisher.AtSite(site));
    }

    /// <summary>
    /// Returns one non-null assignable object without executing the original
    /// allocation or constructor.
    /// </summary>
    public void Substitute(T instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        Type constructedType =
            (publisher.Descriptor.Operation as ConstructorInfo)
                ?.DeclaringType ??
            throw new MockException(
                "A construction setup has no constructed runtime type.");
        if (!constructedType.IsInstanceOfType(instance))
        {
            throw new MockException(
                $"Substitute type '{instance.GetType()}' is not assignable to " +
                $"constructed type '{constructedType}'.");
        }

        publisher.Publish(
            MockReceiverFreeBehavior.Substitute(instance));
    }

    /// <summary>Creates a substitute for every matching allocation.</summary>
    public void SubstituteFactory(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        publisher.Publish(
            MockReceiverFreeBehavior.SubstituteFactory(factory));
    }

    /// <summary>
    /// Creates a substitute through a natural delegate with the constructor's
    /// exact argument signature and a <typeparamref name="T"/> return.
    /// </summary>
    public void SubstituteFactory(Delegate factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        publisher.Publish(
            MockReceiverFreeBehavior.SubstituteFactory(factory));
    }

    /// <summary>Executes the original allocation and constructor.</summary>
    public void Passthrough() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Passthrough([]));

    /// <summary>Throws instead of allocating an object.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        publisher.Publish(
            MockReceiverFreeBehavior.Throw(exception, []));
    }

    /// <summary>Rejects a matching allocation with a strict mock diagnostic.</summary>
    public void Strict() =>
        publisher.Publish(
            MockReceiverFreeBehavior.Strict([]));
}
