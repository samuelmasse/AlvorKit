namespace AlvorKit;

/// <summary>Owns one test-only profiled Mocking route from inert preparation through rollback.</summary>
/// <param name="profiler">The connected checked-in interception backend.</param>
internal sealed class ProfiledMockRouteLifecycle(
    IInterceptionBackend profiler) :
    IMockInterceptionRouteLifecycle
{
    /// <summary>The exact handler that enters the real Mocking wrapper.</summary>
    private ProfiledMockHandler? handler;

    /// <summary>The installed caller patch, when preparation reached installation.</summary>
    private IInterceptionPatchHandle? patch;

    /// <summary>The exact managed trampoline, when preparation created it.</summary>
    private IInterceptionHandlerTrampoline? trampoline;

    /// <summary>The route reserved by the coordinator for this lifecycle.</summary>
    private MockInterceptionRoute? route;

    /// <summary>Zero before rollback and one after its first entry.</summary>
    private int rollbackStarted;

    /// <summary>Gets the completion that restored the caller's original IL.</summary>
    internal InterceptionCompletion? RemovalCompletion { get; private set; }

    /// <summary>Gets the completion that installed the inert caller body.</summary>
    internal InterceptionCompletion? PreparationCompletion
    {
        get;
        private set;
    }

    /// <summary>Gets whether the disposed trampoline rejects new leases.</summary>
    internal bool TrampolineRetired { get; private set; }

    /// <summary>Gets the result observed after activation exposed the still-gated route.</summary>
    internal int ActivationProbeResult { get; private set; }

    /// <summary>Gets original executions observed by the activation-window probe.</summary>
    internal int ActivationProbeOriginalCalls { get; private set; }

    /// <summary>Gets handler calls observed before coordinator publication.</summary>
    internal int ActivationProbeHandlerCalls { get; private set; }

    /// <summary>Creates the exact wrapper and trampoline, then installs inert caller IL.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        route = value;
        ProfiledMockCaller.RoutePointer = 0;
        var caller = ProfiledMockProfiler.SelectedCaller;
        var operation = ProfiledMockProfiler.Operation;
        ProfiledMockOperation wrapper =
            MockInterception.BindOwnedInstanceCaller(
                caller,
                ProfiledMockProfiler.FindOperationOffset(
                    caller,
                    operation),
                operation,
                new ProfiledMockOperation(ProfiledMockOriginal.Invoke));
        handler = new ProfiledMockHandler(wrapper);
        trampoline = profiler.CreateHandlerTrampoline(
            operation,
            handler,
            typeof(ProfiledMockHandler).GetMethod(
                nameof(ProfiledMockHandler.Invoke))!,
            InterceptionHandlerExceptionPolicy.Propagate);
        ProfiledMockRouteLease.Bind(value, trampoline);
        patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(caller),
                ReflectionMethodBodyEncoder.Read(
                    ProfiledMockProfiler.RoutedTemplate)));
        PreparationCompletion =
            ProfiledMockProfiler.WaitFor(profiler, patch.LastRequestId);
        if (PreparationCompletion.Value.State !=
            InterceptionState.Active)
        {
            throw new InvalidOperationException(
                "Inert caller preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    /// <summary>Exposes the route pointer while the coordinator publication gate remains closed.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        if (!ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected route activation.");

        ProfiledMockCaller.RoutePointer =
            ProfiledMockRouteLease.FunctionPointer();
        var probe = new ProfiledMockTarget();
        ActivationProbeResult =
            ProfiledMockCaller.Selected(probe, 5);
        ActivationProbeOriginalCalls = probe.OriginalCalls;
        ActivationProbeHandlerCalls = handler!.InvocationCount;
        return null;
    }

    /// <summary>Inerts the route, restores original IL, and retires its trampoline.</summary>
    public void Rollback(MockInterceptionRoute value)
    {
        if (Interlocked.Exchange(ref rollbackStarted, 1) != 0)
            return;
        if (route is not null &&
            !ReferenceEquals(value, route))
        {
            throw new InvalidOperationException("Unexpected route rollback.");
        }

        ProfiledMockCaller.RoutePointer = 0;
        try
        {
            if (patch is not null)
            {
                var requestId = patch.Remove();
                RemovalCompletion =
                    ProfiledMockProfiler.WaitFor(profiler, requestId);
            }
        }
        finally
        {
            ProfiledMockRouteLease.Clear();
            try
            {
                patch?.Dispose();
            }
            finally
            {
                trampoline?.Dispose();
                TrampolineRetired =
                    trampoline is null ||
                    !trampoline.TryAcquire(out _);
            }
        }
    }
}
