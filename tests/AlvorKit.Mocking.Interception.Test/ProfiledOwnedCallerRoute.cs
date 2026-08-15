namespace AlvorKit;

/// <summary>Owns one exact test caller from inert preparation through restoration.</summary>
internal sealed class ProfiledOwnedCallerRoute<TDelegate>
    : IProfiledOwnedCallerRoute
    where TDelegate : Delegate
{
    private readonly Action clearLease;
    private readonly Action driveCaller;
    private readonly Func<TDelegate, object> handlerFactory;
    private readonly MethodInfo operation;
    private readonly TDelegate original;
    private readonly IInterceptionBackend profiler;
    private readonly Action<nint> publishPointer;
    private readonly MethodInfo routedTemplate;
    private readonly MethodInfo selectedCaller;
    private readonly Action<MockInterceptionRoute, IInterceptionHandlerTrampoline>
        bindLease;
    private readonly Func<nint> routePointer;
    private IInterceptionPatchHandle? patch;
    private IInterceptionHandlerTrampoline? trampoline;
    private MockInterceptionRoute? route;
    private int rollbackStarted;

    /// <summary>Creates one exact-signature caller route.</summary>
    internal ProfiledOwnedCallerRoute(
        IInterceptionBackend profiler,
        MethodInfo selectedCaller,
        MethodInfo routedTemplate,
        MethodInfo operation,
        TDelegate original,
        Func<TDelegate, object> handlerFactory,
        Action<MockInterceptionRoute, IInterceptionHandlerTrampoline>
            bindLease,
        Action clearLease,
        Action<nint> publishPointer,
        Func<nint> routePointer,
        Action driveCaller)
    {
        this.profiler = profiler;
        this.selectedCaller = selectedCaller;
        this.routedTemplate = routedTemplate;
        this.operation = operation;
        this.original = original;
        this.handlerFactory = handlerFactory;
        this.bindLease = bindLease;
        this.clearLease = clearLease;
        this.publishPointer = publishPointer;
        this.routePointer = routePointer;
        this.driveCaller = driveCaller;
    }

    /// <summary>Gets the completion that installed this route's inert caller body.</summary>
    public InterceptionCompletion? PreparationCompletion { get; private set; }

    /// <summary>Gets the completion that restored this route's original caller body.</summary>
    public InterceptionCompletion? RemovalCompletion { get; private set; }

    /// <summary>Creates the Mocking wrapper and installs an inert exact caller body.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        route = value;
        publishPointer(0);
        TDelegate wrapper =
            MockInterception.BindOwnedInstanceCaller(
                selectedCaller,
                ProfiledMockProfiler.FindOperationOffset(
                    selectedCaller,
                    operation),
                operation,
                original);
        object handler = handlerFactory(wrapper);
        trampoline = profiler.CreateHandlerTrampoline(
            operation,
            handler,
            handler.GetType().GetMethod(nameof(ProfiledMockHandler.Invoke))!,
            InterceptionHandlerExceptionPolicy.Propagate);
        bindLease(value, trampoline);
        patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(selectedCaller),
                ReflectionMethodBodyEncoder.Read(routedTemplate)));
        PreparationCompletion = ProfiledMockProfiler.WaitFor(
            profiler,
            patch.LastRequestId,
            driveCaller);
        if (PreparationCompletion.Value.State != InterceptionState.Active)
        {
            throw new InvalidOperationException(
                $"Inert caller preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    /// <summary>Publishes the prepared exact lease pointer behind the coordinator gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        if (!ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected route activation.");

        publishPointer(routePointer());
        return null;
    }

    /// <summary>Inerts the route, restores its original caller IL, and retires its trampoline.</summary>
    public void Rollback(MockInterceptionRoute value)
    {
        if (Interlocked.Exchange(ref rollbackStarted, 1) != 0)
            return;
        if (route is not null && !ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected route rollback.");

        publishPointer(0);
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
            clearLease();
            try
            {
                patch?.Dispose();
            }
            finally
            {
                trampoline?.Dispose();
            }
        }
    }
}
