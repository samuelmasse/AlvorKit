namespace AlvorKit.Interception;

/// <summary>Runtime-neutral operations supplied by one prepared method-interception backend.</summary>
public interface IInterceptionBackend
{
    /// <summary>Gets the backend's negotiated features and limits.</summary>
    InterceptionCapabilities Capabilities { get; }

    /// <summary>Gets the shared physical and logical claim registry for this runtime.</summary>
    InterceptionCollisionRegistry CollisionRegistry { get; }

    /// <summary>Installs one complete replacement method body.</summary>
    IInterceptionPatchHandle Install(InterceptionPlan plan);

    /// <summary>Installs one exact managed dispatch plan.</summary>
    IInterceptionPatchHandle Install(InterceptionDispatchPlan plan);

    /// <summary>Creates one exact managed handler entry for a reviewed target signature.</summary>
    IInterceptionHandlerTrampoline CreateHandlerTrampoline(
        MethodInfo target,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy);

    /// <summary>Creates one exact managed handler entry for a reviewed call shape.</summary>
    IInterceptionHandlerTrampoline CreateHandlerTrampoline(
        InterceptionCallShape callShape,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy)
    {
        ArgumentNullException.ThrowIfNull(callShape);
        if (callShape.ReceiverOwnership is
            InterceptionReceiverOwnership.ManagedReference or
            InterceptionReceiverOwnership.ReadOnlyManagedReference)
        {
            throw new NotSupportedException(
                "This backend does not implement managed-reference " +
                "receiver call shapes.");
        }

        return CreateHandlerTrampoline(
            callShape.Operation,
            handlerInstance,
            handlerMethod,
            exceptionPolicy);
    }

    /// <summary>Reads cold-path backend queue and active-patch diagnostics.</summary>
    InterceptionBackendState GetState();

    /// <summary>Reads one retained request completion without blocking.</summary>
    InterceptionCompletion GetCompletion(ulong requestId);

    /// <summary>Waits for one request to reach a terminal completion.</summary>
    InterceptionCompletion WaitFor(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null);

    /// <summary>Asynchronously waits for one request to reach a terminal completion.</summary>
    ValueTask<InterceptionCompletion> WaitForAsync(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default);
}
