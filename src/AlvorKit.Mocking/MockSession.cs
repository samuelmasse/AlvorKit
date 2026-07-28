namespace AlvorKit.Mocking;

/// <summary>Scopes a logical cross-mock timeline through the execution context.</summary>
public sealed class MockSession : IDisposable
{
    private static long nextSessionId;
    private static readonly AsyncLocal<MockSession?> ambient = new();
    private readonly Lock lifecycleGate = new();
    private readonly ConcurrentDictionary<long, IMockInvocationParticipant>
        participants = [];
    private MockReceiverFreeSessionState? receiverFree;
    private readonly MockSession? parent;
    private int disposed;

    /// <summary>Creates and enters a new ambient session.</summary>
    internal MockSession()
    {
        Id = Interlocked.Increment(ref nextSessionId);
        Timeline = new();
        receiverFree = new(Id, Timeline);
        parent = ambient.Value;
        ambient.Value = this;
    }

    /// <summary>Gets the current execution-context session.</summary>
    internal static MockSession? Current => ambient.Value;

    /// <summary>Gets this session's runtime identity.</summary>
    internal long Id { get; }

    /// <summary>Gets the shared logical timeline used by participating mocks.</summary>
    internal MockInvocationTimeline Timeline { get; }

    /// <summary>Gets the mocks that have published calls into this session.</summary>
    internal ICollection<IMockInvocationParticipant> Participants =>
        participants.Values;

    /// <summary>Captures the last contiguously published invocation entry.</summary>
    public MockCheckpoint Checkpoint()
    {
        ThrowIfDisposed();
        return new(Id, Timeline.Checkpoint());
    }

    /// <summary>Runs an action with this session explicitly current.</summary>
    public void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfDisposed();

        var previous = ambient.Value;
        ambient.Value = this;
        try
        {
            action();
        }
        finally
        {
            ambient.Value = previous;
        }
    }

    /// <summary>Runs a function with this session explicitly current.</summary>
    public T Run<T>(Func<T> function)
    {
        ArgumentNullException.ThrowIfNull(function);
        ThrowIfDisposed();

        var previous = ambient.Value;
        ambient.Value = this;
        try
        {
            return function();
        }
        finally
        {
            ambient.Value = previous;
        }
    }

    /// <summary>Verifies the exact cross-mock invocation sequence through the current checkpoint.</summary>
    public void VerifySequence(params Action[] expectedCalls)
    {
        ArgumentNullException.ThrowIfNull(expectedCalls);
        ThrowIfDisposed();
        EnsureCurrent();

        var through = Checkpoint();
        MockSequenceVerification.Verify(
            this,
            Beginning(),
            through,
            expectedCalls);
    }

    /// <summary>Verifies an exact sequence in a lower-exclusive, upper-inclusive checkpoint window.</summary>
    public void VerifySequence(
        MockCheckpoint after,
        MockCheckpoint through,
        params Action[] expectedCalls)
    {
        ArgumentNullException.ThrowIfNull(expectedCalls);
        ThrowIfDisposed();
        EnsureCurrent();
        this.ValidateWindow(after, through);

        MockSequenceVerification.Verify(
            this,
            after,
            through,
            expectedCalls);
    }

    /// <summary>Leaves this ambient session and restores its parent scope.</summary>
    public void Dispose()
    {
        lock (lifecycleGate)
        {
            if (disposed != 0)
                return;

            if (!ReferenceEquals(ambient.Value, this))
            {
                throw new MockException(
                    MockDiagnostics.SessionDisposalOrder());
            }

            disposed = 1;
            ambient.Value = parent;
            MockReceiverFreeSessionState? released = receiverFree;
            receiverFree = null;
            released?.Clear();
            participants.Clear();
        }
    }

    /// <summary>Registers a mock whose calls participate in this session.</summary>
    internal void Register(IMockInvocationParticipant participant)
    {
        lock (lifecycleGate)
        {
            ThrowIfDisposed();
            participants.TryAdd(
                participant.Invocations.Id,
                participant);
        }
    }

    /// <summary>Gets the synthetic receiver for one intercepted receiver-free site.</summary>
    internal object GetReceiverFreeTarget(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        MethodInfo logicalMethod)
    {
        lock (lifecycleGate)
        {
            ThrowIfDisposed();
            MockReceiverFreeTarget target =
                receiverFree!.GetTarget(
                    site,
                    operation,
                    logicalMethod);
            participants.TryAdd(
                target.Mocked.Invocations.Id,
                target.Mocked);
            return target;
        }
    }

    /// <summary>Publishes one setup into this session's receiver-free store.</summary>
    internal void AddReceiverFreeSetup(
        MockReceiverFreeSetupDescriptor descriptor,
        MockReceiverFreeBehavior behavior)
    {
        lock (lifecycleGate)
        {
            ThrowIfDisposed();
            receiverFree!.Add(descriptor, behavior);
        }
    }

    /// <summary>Gets the session-owned receiver-free ledger.</summary>
    internal MockInvocationLedger ReceiverFreeInvocations
    {
        get
        {
            lock (lifecycleGate)
            {
                ThrowIfDisposed();
                return receiverFree!.Invocations;
            }
        }
    }

    private MockCheckpoint Beginning() =>
        new(Id, new(Timeline.Id, 0));

    private void EnsureCurrent()
    {
        if (!ReferenceEquals(Current, this))
        {
            throw new MockException(
                MockDiagnostics.SessionMustBeCurrent(
                    "Sequence verification"));
        }
    }

    /// <summary>Throws when this session has already released its owned state.</summary>
    internal void ThrowIfDisposed()
    {
        if (Volatile.Read(ref disposed) != 0)
            throw new ObjectDisposedException(nameof(MockSession));
    }
}
