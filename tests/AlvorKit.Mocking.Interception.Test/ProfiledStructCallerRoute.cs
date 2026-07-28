namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns one exact byref value-receiver caller route.</summary>
internal sealed class ProfiledStructCallerRoute<TDelegate> :
    IProfiledReceiverFreeCallerRoute
    where TDelegate : Delegate
{
    private readonly Action<
        MockInterceptionRoute,
        IInterceptionHandlerTrampoline> bind;
    private readonly MethodInfo caller;
    private readonly Action clear;
    private readonly Action drive;
    private readonly Func<TDelegate, IProfiledReceiverFreeHandler>
        handlerFactory;
    private readonly MethodInfo operation;
    private readonly TDelegate original;
    private readonly Func<nint> pointer;
    private readonly IInterceptionBackend profiler;
    private readonly Action<nint> publish;
    private readonly InterceptionReceiverOwnership receiverOwnership;
    private readonly Type receiverType;
    private readonly MethodInfo template;
    private IProfiledReceiverFreeHandler? handler;
    private IInterceptionPatchHandle? patch;
    private IInterceptionHandlerTrampoline? trampoline;
    private MockInterceptionRoute? route;
    private int rollbackStarted;

    internal ProfiledStructCallerRoute(
        IInterceptionBackend profiler,
        MethodInfo caller,
        MethodInfo template,
        MethodInfo operation,
        Type receiverType,
        InterceptionReceiverOwnership receiverOwnership,
        TDelegate original,
        Func<TDelegate, IProfiledReceiverFreeHandler> handlerFactory,
        Action<MockInterceptionRoute, IInterceptionHandlerTrampoline> bind,
        Action clear,
        Action<nint> publish,
        Func<nint> pointer,
        Action drive)
    {
        this.profiler = profiler;
        this.caller = caller;
        this.template = template;
        this.operation = operation;
        this.receiverType = receiverType;
        this.receiverOwnership = receiverOwnership;
        this.original = original;
        this.handlerFactory = handlerFactory;
        this.bind = bind;
        this.clear = clear;
        this.publish = publish;
        this.pointer = pointer;
        this.drive = drive;
    }

    public InterceptionCompletion? PreparationCompletion { get; private set; }
    public InterceptionCompletion? RemovalCompletion { get; private set; }
    public int HandlerInvocations => handler?.InvocationCount ?? 0;

    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        route = value;
        publish(0);
        TDelegate wrapper = ProfiledReceiverFreeRuntimeBinder.Bind(
            caller,
            operation,
            "StructMethod",
            original);
        handler = handlerFactory(wrapper);
        InterceptionCallShape shape =
            receiverOwnership ==
                InterceptionReceiverOwnership.ReadOnlyManagedReference
                ? InterceptionCallShape
                    .ForReadOnlyManagedReferenceReceiver(
                        operation,
                        receiverType)
                : InterceptionCallShape.ForManagedReferenceReceiver(
                    operation,
                    receiverType);
        trampoline = profiler.CreateHandlerTrampoline(
            shape,
            handler,
            handler.GetType().GetMethod(
                nameof(ProfiledMockHandler.Invoke))!,
            InterceptionHandlerExceptionPolicy.Propagate);
        bind(value, trampoline);
        patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(caller),
                ReflectionMethodBodyEncoder.Read(template)));
        PreparationCompletion = ProfiledMockProfiler.WaitFor(
            profiler,
            patch.LastRequestId,
            drive);
        if (PreparationCompletion.Value.State != InterceptionState.Active)
        {
            throw new InvalidOperationException(
                "Struct preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        if (!ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected struct activation.");
        publish(pointer());
        return null;
    }

    public void Rollback(MockInterceptionRoute value)
    {
        if (Interlocked.Exchange(ref rollbackStarted, 1) != 0)
            return;
        if (route is not null && !ReferenceEquals(value, route))
            throw new InvalidOperationException("Unexpected struct rollback.");

        publish(0);
        try
        {
            if (patch is not null)
            {
                var requestId = patch.Remove();
                RemovalCompletion = ProfiledMockProfiler.WaitFor(
                    profiler,
                    requestId,
                    drive);
            }
        }
        finally
        {
            clear();
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
