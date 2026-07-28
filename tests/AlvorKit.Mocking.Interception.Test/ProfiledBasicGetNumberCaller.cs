namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the selected concrete property-getter caller and its gated route.</summary>
internal static class ProfiledBasicGetNumberCaller
{
    private static readonly ProfiledBasicRouteState State = new();

    /// <summary>Calls the concrete property getter from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(ProfiledBasicTarget target) =>
        target.Number;

    /// <summary>Runs the original getter while inert or its exact published route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(ProfiledBasicTarget target)
    {
        var route = State.RoutePointer;
        if (route == 0)
            return target.Number;
        return ((delegate* managed<ProfiledBasicTarget, int>)route)(target);
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
            typeof(ProfiledBasicGetNumberCaller));

    /// <summary>Invokes the leased trampoline or preserves the original getter.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(ProfiledBasicTarget target)
    {
        if (!State.TryAcquire(out var entryPoint))
            return ProfiledBasicOriginal.GetNumber(target);

        return ((delegate* managed<ProfiledBasicTarget, int>)entryPoint)(
            target);
    }
}
