namespace AlvorKit;

/// <summary>Owns receiver-free setup and history state for one mock session.</summary>
internal sealed class MockReceiverFreeSessionState
{
    private readonly Lock gate = new();
    private readonly Dictionary<
        MockReceiverFreeTargetKey,
        MockReceiverFreeTarget> targets = [];
    private readonly MockSetupStore setups = new();
    private readonly MockReceiverFreeSetupStore receiverFreeSetups = new();
    private readonly MockInvocationLedger invocations;
    private readonly long sessionId;

    /// <summary>Gets the shared receiver-free invocation ledger.</summary>
    internal MockInvocationLedger Invocations => invocations;

    /// <summary>Creates state on the owning session's logical timeline.</summary>
    internal MockReceiverFreeSessionState(
        long sessionId,
        MockInvocationTimeline timeline)
    {
        this.sessionId = sessionId;
        invocations = new(timeline);
    }

    /// <summary>Gets or creates the synthetic receiver for one interception site.</summary>
    internal MockReceiverFreeTarget GetTarget(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        MethodInfo logicalMethod)
    {
        lock (gate)
        {
            var key = new MockReceiverFreeTargetKey(
                site,
                operation,
                logicalMethod);
            if (targets.TryGetValue(key, out MockReceiverFreeTarget? target))
                return target;

            Type targetType = operation.DeclaringType ??
                logicalMethod.DeclaringType ??
                throw new MockException(
                    $"Receiver-free site '{site}' has no declaring type.");
            var identity = new MockReceiverFreeIdentity(
                sessionId,
                site,
                operation);
            var mocked = new Mocked(
                MockFallbackBehavior.Partial,
                new TypeCache(targetType),
                setups,
                invocations,
                identity,
                receiverFreeSetups);
            target = new(mocked);
            targets.Add(key, target);
            return target;
        }
    }

    /// <summary>Publishes one immutable receiver-free setup generation.</summary>
    internal void Add(
        MockReceiverFreeSetupDescriptor descriptor,
        MockReceiverFreeBehavior behavior) =>
        receiverFreeSetups.Add(
            new(descriptor, behavior));

    /// <summary>Releases every site target and its session-owned setup graph.</summary>
    internal void Clear()
    {
        lock (gate)
            targets.Clear();
    }
}

/// <summary>Keys one exact operation at a stable interception site.</summary>
internal sealed record MockReceiverFreeTargetKey(
    MockInterceptionSiteDescriptor Site,
    MemberInfo Operation,
    MethodInfo LogicalMethod);
