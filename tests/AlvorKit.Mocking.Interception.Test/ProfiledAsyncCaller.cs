namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the exact asynchronous caller and its gated route lease.</summary>
internal static class ProfiledAsyncCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the concrete asynchronous operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Task<int> Selected(
        ProfiledAsyncTarget target,
        int value) =>
        target.AddAsync(value);

    /// <summary>Runs original behavior while inert or the exact asynchronous route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe Task<int> RoutedTemplate(
        ProfiledAsyncTarget target,
        int value)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return target.AddAsync(value);
        return ((delegate* managed<
            ProfiledAsyncTarget,
            int,
            Task<int>>)route)(target, value);
    }

    /// <summary>Binds the exact route while its published pointer remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Clears the retired exact route lease.</summary>
    internal static void Clear() => Volatile.Write(ref binding, null);

    /// <summary>Publishes the exact route pointer or zero for original behavior.</summary>
    internal static void Publish(nint pointer) =>
        Volatile.Write(ref routePointer, pointer);

    /// <summary>Gets the prepared managed route entry point.</summary>
    internal static nint FunctionPointer()
    {
        var method = typeof(ProfiledAsyncCaller).GetMethod(
            nameof(Invoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    /// <summary>Invokes the leased trampoline or preserves original async behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe Task<int> Invoke(
        ProfiledAsyncTarget target,
        int value)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ProfiledAsyncOriginal.Invoke(target, value);
        }

        return ((delegate* managed<
            ProfiledAsyncTarget,
            int,
            Task<int>>)entryPoint)(target, value);
    }
}
