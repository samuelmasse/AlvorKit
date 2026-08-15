namespace AlvorKit;

/// <summary>Acquires the neutral trampoline lease before entering its exact managed pointer.</summary>
internal static class ProfiledMockRouteLease
{
    /// <summary>The reserved route gate and exact profiler trampoline.</summary>
    private static RouteBinding? binding;

    /// <summary>Binds the reserved route and trampoline while caller dispatch remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(
            ref binding,
            new RouteBinding(route, trampoline));

    /// <summary>Clears the published trampoline after the caller route becomes inert.</summary>
    internal static void Clear() =>
        Volatile.Write(ref binding, null);

    /// <summary>Invokes the exact trampoline when acquired or preserves original behavior on retirement.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe int Invoke(
        ProfiledMockTarget target,
        int value)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ProfiledMockOriginal.Invoke(target, value);
        }

        return ((delegate* managed<ProfiledMockTarget, int, int>)entryPoint)(
            target,
            value);
    }

    /// <summary>Gets the prepared exact managed entry point for the caller route.</summary>
    internal static nint FunctionPointer()
    {
        var method = typeof(ProfiledMockRouteLease).GetMethod(
            nameof(Invoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    /// <summary>Pairs one coordinator-owned route gate with its exact trampoline.</summary>
    private sealed class RouteBinding
    {
        /// <summary>Creates one immutable bridge binding.</summary>
        internal RouteBinding(
            MockInterceptionRoute route,
            IInterceptionHandlerTrampoline trampoline)
        {
            Route = route;
            Trampoline = trampoline;
        }

        /// <summary>Gets the coordinator-owned shared-publication gate.</summary>
        internal MockInterceptionRoute Route { get; }

        /// <summary>Gets the exact handler trampoline.</summary>
        internal IInterceptionHandlerTrampoline Trampoline { get; }
    }
}
