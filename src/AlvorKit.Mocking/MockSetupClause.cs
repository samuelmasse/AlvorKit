namespace AlvorKit.Mocking;

/// <summary>Configures ordinary behavior for a captured mocked void call.</summary>
public sealed class MockSetupClause
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

    /// <summary>Creates a return configuration clause for one captured void call.</summary>
    internal MockSetupClause(Mocked mocked, MethodInfo method, object?[] args)
    {
        this.mocked = mocked;
        this.method = method;
        this.args = args;
        projectors = new(method);
    }

    /// <summary>Creates a void receiver-free setup clause.</summary>
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
    public MockSetupClause AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new(
            RequireReceiverFree().AtSite(site),
            projectors);
    }

    /// <summary>Projects one live entry argument into heap-safe invocation history.</summary>
    public MockSetupClause SnapshotArgument<T, TResult>(
        int parameterIndex,
        Func<T, TResult> projector)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(projector);
        TResult exact(scoped in T value) => projector(value);
        projectors.Add(
            parameterIndex,
            MockSnapshotPhase.Entry,
(SnapshotProjector<T, TResult>)exact);
        return this;
    }

    /// <summary>Projects one live entry argument into heap-safe invocation history.</summary>
    public MockSetupClause SnapshotArgument<T, TResult>(
        int parameterIndex,
        SnapshotProjector<T, TResult> projector)
        where T : allows ref struct
    {
        projectors.Add(
            parameterIndex,
            MockSnapshotPhase.Entry,
            projector);
        return this;
    }

    /// <summary>Projects one final mutable ref or out value into heap-safe history.</summary>
    public MockSetupClause SnapshotArgumentOnExit<T, TResult>(
        int parameterIndex,
        SnapshotProjector<T, TResult> projector)
        where T : allows ref struct
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

    /// <summary>Runs a callback with one invocation-local ordinary call context.</summary>
    public void Do(Action<MockCall> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        if (receiverFree is not null)
        {
            receiverFree.Publish(
                MockReceiverFreeBehavior.CallbackBehavior(
                    callback,
                    projectors.Snapshot()));
            return;
        }

        mocked!.AddCallback(
            method,
            args,
            call =>
            {
                callback(call);
                return null;
            },
            projectors.Snapshot());
    }

    /// <summary>Runs an exact typed callback with one live ref-safe argument.</summary>
    public void Do<T>(Action<T> callback)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(callback);
        AddTypedCallback(callback);
    }

    /// <summary>Runs an exact typed callback with two live ref-safe arguments.</summary>
    public void Do<T1, T2>(Action<T1, T2> callback)
        where T1 : allows ref struct
        where T2 : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(callback);
        AddTypedCallback(callback);
    }

    /// <summary>Runs a natural delegate normalized to the captured exact signature.</summary>
    public void Do(Delegate callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        AddTypedCallback(callback);
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

    private void AddTypedCallback(Delegate callback)
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
