namespace AlvorKit;

/// <summary>Owns the ref-struct input selected caller and trampoline lease.</summary>
internal static class ProfiledObserveCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the ref-struct input operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledRefStructTarget target,
        ProfiledWindow window) =>
        target.Observe(window);

    /// <summary>Runs original behavior while inert or the exact input route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledRefStructTarget target,
        ProfiledWindow window)
    {
        var pointer = Volatile.Read(ref routePointer);
        if (pointer == 0)
            return target.Observe(window);
        return ((delegate* managed<
            ProfiledRefStructTarget,
            ProfiledWindow,
            int>)pointer)(target, window);
    }

    /// <summary>Binds the exact route while its pointer remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Clears the retired exact route lease.</summary>
    internal static void Clear() => Volatile.Write(ref binding, null);

    /// <summary>Publishes the route pointer or zero for original behavior.</summary>
    internal static void Publish(nint pointer) =>
        Volatile.Write(ref routePointer, pointer);

    /// <summary>Gets the prepared managed route entry point.</summary>
    internal static nint FunctionPointer()
    {
        var method = typeof(ProfiledObserveCaller).GetMethod(
            nameof(Invoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    /// <summary>Invokes the leased trampoline or preserves original behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe int Invoke(
        ProfiledRefStructTarget target,
        ProfiledWindow window)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ProfiledRefStructOriginal.Observe(target, window);
        }

        return ((delegate* managed<
            ProfiledRefStructTarget,
            ProfiledWindow,
            int>)entryPoint)(target, window);
    }
}

/// <summary>Owns the ref-struct return selected caller and exact wrapper.</summary>
internal static class ProfiledWindowCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the ref-struct return operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledWindow Selected(
        ProfiledRefStructTarget target) =>
        target.Window();

    /// <summary>Runs original behavior while inert or the exact return route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe ProfiledWindow RoutedTemplate(
        ProfiledRefStructTarget target)
    {
        var pointer = Volatile.Read(ref routePointer);
        if (pointer == 0)
            return target.Window();
        return ((delegate* managed<
            ProfiledRefStructTarget,
            ProfiledWindow>)pointer)(target);
    }

    /// <summary>Binds the exact wrapper while the route remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute value,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(value, trampoline));

    /// <summary>Clears the retired route and exact wrapper.</summary>
    internal static void Clear() => Volatile.Write(ref binding, null);

    /// <summary>Publishes the route pointer or zero for original behavior.</summary>
    internal static void Publish(nint pointer) =>
        Volatile.Write(ref routePointer, pointer);

    /// <summary>Gets the prepared managed route entry point.</summary>
    internal static nint FunctionPointer()
    {
        var method = typeof(ProfiledWindowCaller).GetMethod(
            nameof(Invoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    /// <summary>Invokes the exact wrapper behind the coordinator gate.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe ProfiledWindow Invoke(
        ProfiledRefStructTarget target)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ProfiledRefStructOriginal.Window(target);
        }

        return ((delegate* managed<
            ProfiledRefStructTarget,
            ProfiledWindow>)entryPoint)(target);
    }
}
