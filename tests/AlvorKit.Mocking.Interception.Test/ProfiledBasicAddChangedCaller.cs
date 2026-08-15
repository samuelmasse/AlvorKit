namespace AlvorKit;

/// <summary>Owns the selected concrete event-add caller and its gated route.</summary>
internal static class ProfiledBasicAddChangedCaller
{
    private static readonly ProfiledBasicRouteState State = new();

    /// <summary>Calls the concrete event add accessor from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(
        ProfiledBasicTarget target,
        EventHandler? handler) =>
        target.Changed += handler;

    /// <summary>Runs the original add accessor while inert or its exact published route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(
        ProfiledBasicTarget target,
        EventHandler? handler)
    {
        var route = State.RoutePointer;
        if (route == 0)
        {
            target.Changed += handler;
            return;
        }

        ((delegate* managed<
            ProfiledBasicTarget,
            EventHandler?,
            void>)route)(target, handler);
    }

    /// <summary>Binds the exact route while its caller remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        State.Bind(route, trampoline);

    /// <summary>Clears the retired exact route lease.</summary>
    internal static void Clear() => State.Clear();

    /// <summary>Publishes the exact route pointer or zero.</summary>
    internal static void Publish(nint pointer) => State.Publish(pointer);

    /// <summary>Gets the prepared managed route entry point.</summary>
    internal static nint FunctionPointer() =>
        ProfiledBasicRouteState.FunctionPointer(
            typeof(ProfiledBasicAddChangedCaller));

    /// <summary>Invokes the leased trampoline or preserves the original add accessor.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void Invoke(
        ProfiledBasicTarget target,
        EventHandler? handler)
    {
        if (!State.TryAcquire(out var entryPoint))
        {
            ProfiledBasicOriginal.AddChanged(target, handler);
            return;
        }

        ((delegate* managed<
            ProfiledBasicTarget,
            EventHandler?,
            void>)entryPoint)(target, handler);
    }
}
