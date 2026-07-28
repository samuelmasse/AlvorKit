namespace AlvorKit.Mocking;

/// <summary>Mutable state owned by one mock or partially mocked instance.</summary>
internal sealed class Mocked(
    MockFallbackBehavior fallback,
    TypeCache type,
    MockSetupStore? setupStore = null,
    MockInvocationLedger? invocationLedger = null,
    MockReceiverFreeIdentity? receiverFree = null,
    MockReceiverFreeSetupStore? receiverFreeSetups = null)
    : MockInvocationParticipant
{
    private readonly MockSetupStore setups = setupStore ?? new();
    private readonly MockInvocationLedger invocations =
        invocationLedger ?? new();
    private ConcurrentDictionary<MethodInfo, Lazy<object?>>? defaultValues;
    private ConcurrentDictionary<EventInfo, Delegate>? eventHandlers;

    /// <summary>Gets behavior used when no configured setup matches.</summary>
    internal MockFallbackBehavior Fallback { get; } = fallback;

    /// <summary>Gets reflection metadata cached for this mock's target type.</summary>
    internal TypeCache Type { get; } = type;

    /// <summary>Gets this mock's invocation ledger.</summary>
    internal MockInvocationLedger Invocations => invocations;

    MockInvocationLedger MockInvocationParticipant.Invocations =>
        invocations;

    /// <summary>Gets receiver-free identity when this state belongs to a interception site.</summary>
    internal MockReceiverFreeIdentity? ReceiverFree => receiverFree;

    /// <summary>Gets the session-wide receiver-free setup store.</summary>
    internal MockReceiverFreeSetupStore? ReceiverFreeSetups =>
        receiverFreeSetups;

    /// <summary>Gets the owner ID written into invocation targets.</summary>
    internal long TargetOwnerId =>
        receiverFree?.SessionId ?? invocations.Id;

    /// <summary>Gets the mocked event handler table.</summary>
    internal ConcurrentDictionary<EventInfo, Delegate> EventHandlers
    {
        get
        {
            eventHandlers ??= [];
            return eventHandlers;
        }
    }

    /// <summary>Gets whether any event handlers have been attached.</summary>
    internal bool HasEventHandlers => eventHandlers is not null;

    /// <summary>Selects the newest configured behavior matching a call.</summary>
    internal MockConfiguredBehavior? FindBehavior(
        MethodInfo method,
        ReadOnlySpan<object?> arguments) =>
        setups.Find(method, arguments, receiverFree);

    /// <summary>Selects the newest matching setup including typed metadata.</summary>
    internal MockSetup? FindSetup(
        MethodInfo method,
        ReadOnlySpan<object?> arguments) =>
        setups.FindSetup(method, arguments, receiverFree);

    /// <summary>Publishes one already-validated immutable setup.</summary>
    internal void AddSetup(MockSetup setup) =>
        setups.Add(setup);

    /// <summary>Gets whether one method has live typed matcher candidates.</summary>
    internal bool HasTypedMatchers(MethodInfo method) =>
        setups.HasTypedMatchers(method);

    /// <summary>Gets whether one method requires typed matcher or projector execution.</summary>
    internal bool HasTypedExecution(MethodInfo method) =>
        setups.HasTypedExecution(method) ||
        receiverFreeSetups?.Snapshot().Any(
            static setup =>
                setup.RequiresTypedExecution) == true;

    /// <summary>Gets the immutable setup generation used by a typed matcher call.</summary>
    internal MockSetup[] SnapshotSetups() => setups.Snapshot();

    /// <summary>Gets receiver-free candidates for live typed evaluation.</summary>
    internal MockReceiverFreeSetup[] SnapshotReceiverFreeSetups() =>
        receiverFreeSetups?.Snapshot() ?? [];

    /// <summary>Returns the stable loose default for one method.</summary>
    internal object? GetDefault(MethodInfo method)
    {
        ConcurrentDictionary<MethodInfo, Lazy<object?>>? values =
            Volatile.Read(ref defaultValues);
        if (values is null)
        {
            var created =
                new ConcurrentDictionary<MethodInfo, Lazy<object?>>();
            values =
                Interlocked.CompareExchange(
                    ref defaultValues,
                    created,
                    null)
                ?? created;
        }

        return values.GetOrAdd(
            method,
            static target => new(
                () => CreateDefault(target),
                LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;
    }

    private static object? CreateDefault(MethodInfo method) =>
        MockManagedReferenceAbi.IsSupported(method.ReturnType)
            ? MockManagedReferenceDefault.Create(method.ReturnType)
            : MockDefaultValue.Create(method.ReturnType);

}
