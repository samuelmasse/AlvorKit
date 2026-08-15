namespace AlvorKit;

/// <summary>Coordinates one exact rewritten construction-validation site.</summary>
internal sealed class ProfiledConstructionValidationRouteLifecycle(
    IInterceptionBackend profiler) :
    IMockInterceptionRouteLifecycle
{
    private const string RouteId =
        "ProfiledConstructionValidationCaller.Selected::newobj";
    private IInterceptionPatchHandle? patch;
    private MockInterceptionRoute? route;
    private int rollbackStarted;

    /// <summary>Gets the completion that installed the inert rewritten caller.</summary>
    internal InterceptionCompletion? PreparationCompletion { get; private set; }

    /// <summary>Gets the completion that restored the original newobj caller.</summary>
    internal InterceptionCompletion? RemovalCompletion { get; private set; }

    /// <summary>Gets whether the rewritten caller reached active preparation.</summary>
    internal bool AllPrepared =>
        PreparationCompletion?.State == InterceptionState.Active;

    /// <summary>Gets whether the baseline newobj caller was restored.</summary>
    internal bool AllRemoved =>
        RemovalCompletion?.State == InterceptionState.Removed;

    /// <summary>Creates the stable route for this selected newobj site.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(RouteId),
    ];

    /// <summary>Emits the original newobj delegate and installs its inert route.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        Resolve(value);
        route = value;
        MethodInfo selected = Caller(
            nameof(ProfiledConstructionValidationCaller.Selected));
        MethodInfo gateway =
            typeof(ProfiledConstructionValidationRoute).GetMethod(
                nameof(ProfiledConstructionValidationRoute.Invoke),
                BindingFlags.NonPublic | BindingFlags.Static)!;
        ConstructorInfo constructor =
            typeof(ProfiledConstructionValidationTarget).GetConstructor(
                [typeof(int)])!;
        int operationOffset =
            ProfiledReceiverFreeOperationOffset.Find(
            selected,
            constructor);
        var original =
            LoadedConstructionOriginalDelegate.Create<
                ProfiledConstructionValidationOperation>(constructor);
        var wrapper =
            MockInterceptionRuntime.BindConstructionCaller(
                selected,
                operationOffset,
                constructor,
                original);
        ProfiledConstructionValidationRoute.Bind(
            value,
            original,
            wrapper);
        patch = ProfiledConstructionGeneration.Install(
            profiler,
            selected,
            constructor,
            gateway);
        PreparationCompletion = ProfiledMockProfiler.WaitFor(
            profiler,
            patch.LastRequestId,
            DriveCaller);
        if (PreparationCompletion.Value.State !=
            InterceptionState.Active)
        {
            throw new InvalidOperationException(
                $"Construction preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    /// <summary>Publishes the prepared construction wrapper through the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        Resolve(value);
        return null;
    }

    /// <summary>Restores the baseline newobj caller and clears its route.</summary>
    public void Rollback(MockInterceptionRoute value)
    {
        Resolve(value);
        if (Interlocked.Exchange(ref rollbackStarted, 1) != 0)
            return;

        try
        {
            if (patch is not null)
            {
                ulong requestId = patch.Remove();
                RemovalCompletion = ProfiledMockProfiler.WaitFor(
                    profiler,
                    requestId,
                    DriveCaller);
            }
        }
        finally
        {
            ProfiledConstructionValidationRoute.Clear();
            patch?.Dispose();
        }
    }

    private void Resolve(MockInterceptionRoute value)
    {
        if (value.Id != RouteId ||
            (route is not null && !ReferenceEquals(value, route)))
        {
            throw new InvalidOperationException(
                $"Unexpected construction route '{value.Id}'.");
        }
    }

    private static MethodInfo Caller(string name) =>
        typeof(ProfiledConstructionValidationCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void DriveCaller() =>
        _ = ProfiledConstructionValidationCaller.Selected(1);
}
