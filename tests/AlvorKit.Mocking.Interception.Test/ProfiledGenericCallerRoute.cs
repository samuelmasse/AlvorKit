namespace AlvorKit;

/// <summary>Owns one ReJITted generic caller definition and all prepared constructions.</summary>
internal sealed class ProfiledGenericCallerRoute :
    IProfiledOwnedCallerRoute
{
    private readonly IProfiledGenericConstructionRoute[] constructions;
    private readonly Action driveCaller;
    private readonly MethodInfo installCaller;
    private readonly IInterceptionBackend profiler;
    private readonly MethodInfo routedTemplate;
    private IInterceptionPatchHandle? patch;
    private MockInterceptionRoute? route;
    private int rollbackStarted;

    /// <summary>Creates one generic caller owner over its exact constructions.</summary>
    internal ProfiledGenericCallerRoute(
        IInterceptionBackend profiler,
        MethodInfo installCaller,
        MethodInfo routedTemplate,
        Action driveCaller,
        params IProfiledGenericConstructionRoute[] constructions)
    {
        this.profiler = profiler;
        this.installCaller = installCaller;
        this.routedTemplate = routedTemplate;
        this.driveCaller = driveCaller;
        this.constructions = constructions;
    }

    /// <summary>Gets the completion that installed this generic caller body.</summary>
    public InterceptionCompletion? PreparationCompletion { get; private set; }

    /// <summary>Gets the completion that restored this generic caller body.</summary>
    public InterceptionCompletion? RemovalCompletion { get; private set; }

    /// <summary>Prepares every exact construction and installs one inert generic body.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        route = value;
        foreach (var construction in constructions)
            construction.Prepare(profiler, value);

        patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(installCaller),
                ReflectionMethodBodyEncoder.Read(routedTemplate)));
        PreparationCompletion = ProfiledMockProfiler.WaitFor(
            profiler,
            patch.LastRequestId,
            driveCaller);
        if (PreparationCompletion.Value.State != InterceptionState.Active)
        {
            throw new InvalidOperationException(
                "Inert generic caller preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    /// <summary>Publishes every construction behind the shared coordinator gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        if (!ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected generic route activation.");

        foreach (var construction in constructions)
            construction.Publish();
        return null;
    }

    /// <summary>Restores the generic caller and retires every exact construction.</summary>
    public void Rollback(MockInterceptionRoute value)
    {
        if (Interlocked.Exchange(ref rollbackStarted, 1) != 0)
            return;
        if (route is not null && !ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected generic route rollback.");

        foreach (var construction in constructions)
            construction.Unpublish();
        try
        {
            if (patch is not null)
            {
                var requestId = patch.Remove();
                RemovalCompletion = ProfiledMockProfiler.WaitFor(
                    profiler,
                    requestId,
                    driveCaller);
            }
        }
        finally
        {
            try
            {
                patch?.Dispose();
            }
            finally
            {
                for (var index = constructions.Length - 1;
                    index >= 0;
                    index--)
                {
                    constructions[index].Retire();
                }
            }
        }
    }
}
