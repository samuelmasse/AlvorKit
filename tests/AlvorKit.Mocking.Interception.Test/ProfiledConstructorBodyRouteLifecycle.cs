namespace AlvorKit;

/// <summary>Coordinates one definition-wide constructor remainder generation.</summary>
internal sealed class ProfiledConstructorBodyRouteLifecycle(
    InterceptionProfiler profiler) :
    IMockInterceptionRouteLifecycle
{
    private const string RouteId =
        "ProfiledConstructorBodyTarget::.ctor::remainder";
    private IInterceptionPatchHandle? patch;
    private MockInterceptionRoute? route;
    private int rollbackStarted;

    /// <summary>Gets the completion that installed the constructor generation.</summary>
    internal InterceptionCompletion? PreparationCompletion { get; private set; }

    /// <summary>Gets the completion that restored the constructor body.</summary>
    internal InterceptionCompletion? RemovalCompletion { get; private set; }

    /// <summary>Gets whether the constructor generation reached active preparation.</summary>
    internal bool AllPrepared =>
        PreparationCompletion?.State == InterceptionState.Active;

    /// <summary>Gets whether the constructor body was restored.</summary>
    internal bool AllRemoved =>
        RemovalCompletion?.State == InterceptionState.Removed;

    /// <summary>Creates the stable route for this constructor definition.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(RouteId),
    ];

    /// <summary>Extracts the original remainder and installs its inert route body.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        Resolve(value);
        route = value;
        ConstructorInfo constructor =
            typeof(ProfiledConstructorBodyTarget).GetConstructor(
                [typeof(int)])!;
        RuntimeHelpers.PrepareMethod(constructor.MethodHandle);
        var target = InterceptionTarget.FromConstructor(constructor);
        LoadedMethodBodySnapshot body =
            profiler.GetLoadedMethodBody(target);
        var planning = LoadedConstructorRemainderPlanner.Plan(
            body,
            new ReflectionLoadedConstructorMetadataResolver(
                constructor));
        if (!planning.IsSuccessful)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    planning.Rejections.Select(rejection =>
                        rejection.Detail)));
        }

        MethodInfo gateway =
            typeof(ProfiledConstructorBodyRoute).GetMethod(
                nameof(ProfiledConstructorBodyRoute.Invoke),
                BindingFlags.NonPublic | BindingFlags.Static)!;
        LoadedConstructorRemainderGeneration generation =
            LoadedConstructorRemainderComposer.Compose(
                constructor,
                body,
                planning.Plan!,
                gateway,
                typeof(ProfiledConstructorBodyRemainder),
                1);
        var original =
            (ProfiledConstructorBodyRemainder)
                generation.OriginalRemainder;
        ProfiledConstructorBodyRemainder wrapper =
            MockInterceptionRuntime.BindConstructorRemainder(
                constructor,
                planning.Plan!.MovedRemainder.StartOffset,
                original);
        ProfiledConstructorBodyRoute.Bind(
            value,
            original,
            wrapper);
        patch = profiler.Install(generation.Plan);
        PreparationCompletion = ProfiledMockProfiler.WaitFor(
            profiler,
            patch.LastRequestId,
            DriveConstructor);
        if (PreparationCompletion.Value.State !=
            InterceptionState.Active)
        {
            throw new InvalidOperationException(
                $"Constructor preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    /// <summary>Publishes the prepared route through the coordinator gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        Resolve(value);
        return null;
    }

    /// <summary>Restores the original constructor definition and clears its route.</summary>
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
                    DriveConstructor);
            }
        }
        finally
        {
            ProfiledConstructorBodyRoute.Clear();
            patch?.Dispose();
        }
    }

    private void Resolve(MockInterceptionRoute value)
    {
        if (value.Id != RouteId ||
            (route is not null && !ReferenceEquals(value, route)))
        {
            throw new InvalidOperationException(
                $"Unexpected constructor route '{value.Id}'.");
        }
    }

    private static void DriveConstructor() =>
        _ = ProfiledConstructorBodyFactory.Create(1);
}
