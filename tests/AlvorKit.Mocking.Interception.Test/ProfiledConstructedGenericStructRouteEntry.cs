namespace AlvorKit;

/// <summary>Owns one closed generic struct construction's exact route resources.</summary>
internal sealed class ProfiledConstructedGenericStructRouteEntry<T>
    where T : notnull
{
    private readonly MethodInfo caller;
    private readonly MethodInfo operation;
    private readonly Func<nint> pointer;
    private ProfiledConstructedGenericStructHandler<T>? handler;
    private IInterceptionHandlerTrampoline? trampoline;

    /// <summary>Creates one exact constructed caller and operation entry.</summary>
    internal ProfiledConstructedGenericStructRouteEntry(
        MethodInfo caller,
        MethodInfo operation,
        Func<nint> pointer)
    {
        this.caller = caller;
        this.operation = operation;
        this.pointer = pointer;
    }

    /// <summary>Gets exact production-wrapper entries for this construction.</summary>
    internal int HandlerInvocations => handler?.InvocationCount ?? 0;

    /// <summary>Gets the fully closed caller used for setup identity.</summary>
    internal MethodInfo Caller => caller;

    /// <summary>Gets the fully closed value-receiver operation.</summary>
    internal MethodInfo Operation => operation;

    /// <summary>Creates and binds one exact managed-reference trampoline.</summary>
    internal void Prepare(
        IInterceptionBackend profiler,
        MockInterceptionRoute route)
    {
        ProfiledConstructedGenericStructRouteState<T>.Publish(0);
        var original =
            new ProfiledConstructedGenericStructOperation<T>(
                ProfiledConstructedGenericStructOriginal.Echo);
        ProfiledConstructedGenericStructOperation<T> wrapper =
            ProfiledReceiverFreeRuntimeBinder.Bind(
                caller,
                operation,
                "StructMethod",
                original);
        handler = new(wrapper);
        InterceptionCallShape shape =
            InterceptionCallShape.ForManagedReferenceReceiver(
                operation,
                typeof(ProfiledConstructedGenericStructTarget<T>));
        trampoline = profiler.CreateHandlerTrampoline(
            shape,
            handler,
            handler.GetType().GetMethod(
                nameof(ProfiledConstructedGenericStructHandler<>.Invoke))!,
            InterceptionHandlerExceptionPolicy.Propagate);
        ProfiledConstructedGenericStructRouteState<T>.Bind(
            route,
            trampoline);
    }

    /// <summary>Publishes this construction's typed managed route.</summary>
    internal void Publish() =>
        ProfiledConstructedGenericStructRouteState<T>.Publish(pointer());

    /// <summary>Makes this construction inert before caller restoration.</summary>
    internal void Unpublish() =>
        ProfiledConstructedGenericStructRouteState<T>.Publish(0);

    /// <summary>Retires the exact route after the caller is restored.</summary>
    internal void Retire()
    {
        ProfiledConstructedGenericStructRouteState<T>.Clear();
        trampoline?.Dispose();
    }
}
