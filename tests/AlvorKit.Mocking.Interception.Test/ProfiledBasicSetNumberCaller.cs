namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the selected concrete property-setter caller and its gated route.</summary>
internal static class ProfiledBasicSetNumberCaller
{
    private static readonly ProfiledBasicRouteState State = new();

    /// <summary>Calls the concrete property setter from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(
        ProfiledBasicTarget target,
        int value) =>
        target.Number = value;

    /// <summary>Runs the original setter while inert or its exact published route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(
        ProfiledBasicTarget target,
        int value)
    {
        var route = State.RoutePointer;
        if (route == 0)
        {
            target.Number = value;
            return;
        }

        ((delegate* managed<ProfiledBasicTarget, int, void>)route)(
            target,
            value);
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
            typeof(ProfiledBasicSetNumberCaller));

    /// <summary>Invokes the leased trampoline or preserves the original setter.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void Invoke(
        ProfiledBasicTarget target,
        int value)
    {
        if (!State.TryAcquire(out var entryPoint))
        {
            ProfiledBasicOriginal.SetNumber(target, value);
            return;
        }

        ((delegate* managed<ProfiledBasicTarget, int, void>)entryPoint)(
            target,
            value);
    }
}
