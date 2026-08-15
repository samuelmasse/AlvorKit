namespace AlvorKit;

/// <summary>Configures heap-safe behavior or an exact typed factory for a captured value-returning call.</summary>
public sealed class MockSetupClause<T>
    where T : allows ref struct
{
    /// <summary>The mock state owning the configured return.</summary>
    private readonly Mocked? mocked;

    /// <summary>The captured method or accessor.</summary>
    private readonly MethodInfo method;

    /// <summary>The captured argument signature.</summary>
    private readonly object?[] args;

    /// <summary>Typed entry and exit history projectors pending setup publication.</summary>
    private readonly MockSnapshotProjectorBuilder projectors;

    /// <summary>The receiver-free setup publisher, when this clause has no mock receiver.</summary>
    private readonly MockReceiverFreeSetupPublisher? receiverFree;

    /// <summary>Creates a return configuration clause for one captured call.</summary>
    internal MockSetupClause(Mocked mocked, MethodInfo method, object?[] args)
    {
        this.mocked = mocked;
        this.method = method;
        this.args = args;
        projectors = new(method);
    }

    /// <summary>Creates a value-returning receiver-free setup clause.</summary>
    internal MockSetupClause(
        MockReceiverFreeSetupPublisher receiverFree)
        : this(
            receiverFree,
            new(
                receiverFree.Descriptor.Operation as MethodInfo ??
                throw new MockException(
                    "A static method setup requires exact MethodInfo metadata.")))
    {
    }

    private MockSetupClause(
        MockReceiverFreeSetupPublisher receiverFree,
        MockSnapshotProjectorBuilder projectors)
    {
        this.receiverFree = receiverFree;
        method = receiverFree.Descriptor.Operation as MethodInfo ??
            throw new MockException(
                "A static method setup requires exact MethodInfo metadata.");
        mocked = null;
        args = [];
        this.projectors = projectors;
    }

    /// <summary>Restricts this receiver-free setup to one exact interception call site.</summary>
    public MockSetupClause<T> AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new(
            RequireReceiverFree().AtSite(site),
            projectors);
    }

    /// <summary>Projects one live entry argument into heap-safe invocation history.</summary>
    public MockSetupClause<T> SnapshotArgument<TArgument, TResult>(
        int parameterIndex,
        Func<TArgument, TResult> projector)
        where TArgument : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(projector);
        TResult exact(scoped in TArgument value) => projector(value);
        projectors.Add(
            parameterIndex,
            MockSnapshotPhase.Entry,
(SnapshotProjector<TArgument, TResult>)exact);
        return this;
    }

    /// <summary>Projects one live entry argument into heap-safe invocation history.</summary>
    public MockSetupClause<T> SnapshotArgument<TArgument, TResult>(
        int parameterIndex,
        SnapshotProjector<TArgument, TResult> projector)
        where TArgument : allows ref struct
    {
        projectors.Add(
            parameterIndex,
            MockSnapshotPhase.Entry,
            projector);
        return this;
    }

    /// <summary>Projects one final mutable ref or out value into heap-safe history.</summary>
    public MockSetupClause<T> SnapshotArgumentOnExit<TArgument, TResult>(
        int parameterIndex,
        SnapshotProjector<TArgument, TResult> projector)
        where TArgument : allows ref struct
    {
        projectors.Add(
            parameterIndex,
            MockSnapshotPhase.Exit,
            projector);
        return this;
    }

    /// <summary>Configures the captured call to throw the supplied exception instance.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (receiverFree is not null)
        {
            receiverFree.Publish(
                MockReceiverFreeBehavior.Throw(
                    exception,
                    projectors.Snapshot()));
            return;
        }

        mocked!.AddThrow(
            method,
            args,
            exception,
            projectors.Snapshot());
    }

    /// <summary>Invokes a ref-safe factory for every matching call.</summary>
    public void ReturnFactory(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (receiverFree is not null)
        {
            receiverFree.Publish(
                MockReceiverFreeBehavior.ReturnFactory(
                    factory,
                    projectors.Snapshot()));
            return;
        }

        mocked!.AddTypedReturnFactory(
            method,
            args,
            factory,
            projectors.Snapshot());
    }

    /// <summary>Executes the preserved original receiver-free operation.</summary>
    public void Passthrough() =>
        RequireReceiverFree().Publish(
            MockReceiverFreeBehavior.Passthrough(
                projectors.Snapshot()));

    /// <summary>Rejects the matching receiver-free call with a strict diagnostic.</summary>
    public void Strict() =>
        RequireReceiverFree().Publish(
            MockReceiverFreeBehavior.Strict(
                projectors.Snapshot()));

    /// <summary>Publishes one ordinary heap-safe constant through the shared setup store.</summary>
    internal void AddOrdinaryReturn<TValue>(TValue value) =>
        MockSetupReturnPublisher.Publish(
            mocked,
            method,
            args,
            receiverFree,
            value,
            projectors.Snapshot());

    /// <summary>Publishes an ordinary heap-safe return sequence through the shared setup store.</summary>
    internal void AddOrdinaryReturnSequence<TValue>(TValue[] values)
    {
        var boxedValues = new object?[values.Length];
        for (var i = 0; i < values.Length; i++)
            boxedValues[i] = values[i];

        if (receiverFree is not null)
        {
            receiverFree.Publish(
                MockReceiverFreeBehavior.ReturnSequence(
                    boxedValues,
                    projectors.Snapshot()));
            return;
        }

        mocked!.AddReturnSequence(
            method,
            args,
            boxedValues,
            projectors.Snapshot());
    }

    /// <summary>Publishes an ordinary heap-safe calculated answer through the shared setup store.</summary>
    internal void AddOrdinaryAnswer<TValue>(
        Func<MockCall, TValue> answer)
    {
        if (receiverFree is not null)
        {
            Func<MockCall, object?> callback =
                call => answer(call);
            receiverFree.Publish(
                MockReceiverFreeBehavior.CallbackBehavior(
                    callback,
                    projectors.Snapshot()));
            return;
        }

        mocked!.AddCallback(
            method,
            args,
            call => answer(call),
            projectors.Snapshot());
    }

    /// <summary>Publishes one exact typed answer through the shared setup store.</summary>
    internal void AddTypedCallback(Delegate callback)
    {
        if (receiverFree is not null)
        {
            receiverFree.Publish(
                MockReceiverFreeBehavior.CallbackBehavior(
                    callback,
                    projectors.Snapshot()));
            return;
        }

        mocked!.AddTypedCallback(
            method,
            args,
            callback,
            projectors.Snapshot());
    }

    private MockReceiverFreeSetupPublisher RequireReceiverFree() =>
        receiverFree ??
        throw new MockException(
            "Call-site scoping, passthrough, and setup-scoped strict behavior " +
            "apply only to receiver-free interception operations.");
}
