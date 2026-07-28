namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns one exact receiver-free caller from inert preparation through restoration.</summary>
internal sealed class ProfiledReceiverFreeCallerRoute<TDelegate> :
    IProfiledReceiverFreeCallerRoute
    where TDelegate : Delegate
{
    private readonly Action<MockInterceptionRoute, IInterceptionHandlerTrampoline>
        bindLease;
    private readonly MethodInfo caller;
    private readonly Action clearLease;
    private readonly Action driveCaller;
    private readonly Func<TDelegate, IProfiledReceiverFreeHandler>
        handlerFactory;
    private readonly MemberInfo operation;
    private readonly string operationKind;
    private readonly TDelegate original;
    private readonly Func<nint> pointer;
    private readonly IInterceptionBackend profiler;
    private readonly Action<nint> publish;
    private readonly MethodInfo routedTemplate;
    private readonly MethodInfo trampolineSignature;
    private IProfiledReceiverFreeHandler? handler;
    private IInterceptionPatchHandle? patch;
    private IInterceptionHandlerTrampoline? trampoline;
    private MockInterceptionRoute? route;
    private int rollbackStarted;

    /// <summary>Creates one exact receiver-free caller owner.</summary>
    internal ProfiledReceiverFreeCallerRoute(
        IInterceptionBackend profiler,
        MethodInfo caller,
        MethodInfo routedTemplate,
        MemberInfo operation,
        string operationKind,
        TDelegate original,
        Func<TDelegate, IProfiledReceiverFreeHandler> handlerFactory,
        MethodInfo trampolineSignature,
        Action<MockInterceptionRoute, IInterceptionHandlerTrampoline>
            bindLease,
        Action clearLease,
        Action<nint> publish,
        Func<nint> pointer,
        Action driveCaller)
    {
        this.profiler = profiler;
        this.caller = caller;
        this.routedTemplate = routedTemplate;
        this.operation = operation;
        this.operationKind = operationKind;
        this.original = original;
        this.handlerFactory = handlerFactory;
        this.trampolineSignature = trampolineSignature;
        this.bindLease = bindLease;
        this.clearLease = clearLease;
        this.publish = publish;
        this.pointer = pointer;
        this.driveCaller = driveCaller;
    }

    /// <summary>Gets the completion that installed the inert rewritten caller.</summary>
    public InterceptionCompletion? PreparationCompletion { get; private set; }

    /// <summary>Gets the completion that restored the original caller.</summary>
    public InterceptionCompletion? RemovalCompletion { get; private set; }

    /// <summary>Gets the number of calls that entered the production wrapper.</summary>
    public int HandlerInvocations => handler?.InvocationCount ?? 0;

    /// <summary>Creates the production wrapper and installs its inert caller body.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        route = value;
        publish(0);
        TDelegate wrapper = ProfiledReceiverFreeRuntimeBinder.Bind(
            caller,
            operation,
            operationKind,
            original);
        handler = handlerFactory(wrapper);
        trampoline = profiler.CreateHandlerTrampoline(
            trampolineSignature,
            handler,
            handler.GetType().GetMethod(nameof(ProfiledMockHandler.Invoke))!,
            InterceptionHandlerExceptionPolicy.Propagate);
        bindLease(value, trampoline);
        patch = operation is ConstructorInfo constructor
            ? ProfiledConstructionGeneration.Install(
                profiler,
                caller,
                constructor,
                routedTemplate)
            : profiler.Install(
                new InterceptionPlan(
                    InterceptionTarget.FromMethod(caller),
                    ReflectionMethodBodyEncoder.Read(routedTemplate)));
        PreparationCompletion = ProfiledMockProfiler.WaitFor(
            profiler,
            patch.LastRequestId,
            driveCaller);
        if (PreparationCompletion.Value.State != InterceptionState.Active)
        {
            throw new InvalidOperationException(
                "Receiver-free preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    /// <summary>Publishes the prepared receiver-free route behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        if (!ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected receiver-free activation.");

        publish(pointer());
        return null;
    }

    /// <summary>Restores the caller and retires its production wrapper trampoline.</summary>
    public void Rollback(MockInterceptionRoute value)
    {
        if (Interlocked.Exchange(ref rollbackStarted, 1) != 0)
            return;
        if (route is not null && !ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected receiver-free rollback.");

        publish(0);
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
