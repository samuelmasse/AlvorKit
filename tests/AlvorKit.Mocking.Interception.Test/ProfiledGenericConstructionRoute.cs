namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns one exact generic construction's Mocking wrapper and trampoline.</summary>
internal sealed class ProfiledGenericConstructionRoute<TDelegate> :
    IProfiledGenericConstructionRoute
    where TDelegate : Delegate
{
    private readonly Action<MockInterceptionRoute, IInterceptionHandlerTrampoline>
        bind;
    private readonly MethodInfo caller;
    private readonly Action clear;
    private readonly Func<TDelegate, object> handlerFactory;
    private readonly MethodInfo operation;
    private readonly TDelegate original;
    private readonly Func<nint> pointer;
    private readonly Action<nint> publish;
    private IInterceptionHandlerTrampoline? trampoline;

    /// <summary>Creates one construction-specific route resource.</summary>
    internal ProfiledGenericConstructionRoute(
        MethodInfo caller,
        MethodInfo operation,
        TDelegate original,
        Func<TDelegate, object> handlerFactory,
        Action<MockInterceptionRoute, IInterceptionHandlerTrampoline> bind,
        Action clear,
        Action<nint> publish,
        Func<nint> pointer)
    {
        this.caller = caller;
        this.operation = operation;
        this.original = original;
        this.handlerFactory = handlerFactory;
        this.bind = bind;
        this.clear = clear;
        this.publish = publish;
        this.pointer = pointer;
    }

    /// <summary>Creates and binds the exact Mocking wrapper and trampoline.</summary>
    public void Prepare(
        IInterceptionBackend profiler,
        MockInterceptionRoute route)
    {
        publish(0);
        TDelegate wrapper =
            MockInterception.BindOwnedInstanceCaller(
                caller,
                ProfiledGenericOperationOffset.Find(
                    caller,
                    operation),
                operation,
                original);
        object handler = handlerFactory(wrapper);
        trampoline = profiler.CreateHandlerTrampoline(
            operation,
            handler,
            handler.GetType().GetMethod(nameof(ProfiledMockHandler.Invoke))!,
            InterceptionHandlerExceptionPolicy.Propagate);
        bind(route, trampoline);
    }

    /// <summary>Publishes the construction-specific managed route pointer.</summary>
    public void Publish() =>
        publish(pointer());

    /// <summary>Returns the construction to inert original behavior.</summary>
    public void Unpublish() =>
        publish(0);

    /// <summary>Clears and retires the exact construction trampoline.</summary>
    public void Retire()
    {
        clear();
        trampoline?.Dispose();
    }
}
