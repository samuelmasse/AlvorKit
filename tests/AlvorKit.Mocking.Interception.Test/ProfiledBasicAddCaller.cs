namespace AlvorKit;

/// <summary>Owns the selected concrete addition caller and its gated route.</summary>
internal static class ProfiledBasicAddCaller
{
    private static readonly ProfiledBasicRouteState State = new();

    /// <summary>Calls concrete addition from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledBasicTarget target,
        int left,
        int right) =>
        target.Add(left, right);

    /// <summary>Runs original addition while inert or its exact published route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledBasicTarget target,
        int left,
        int right)
    {
        var route = State.RoutePointer;
        if (route == 0)
            return target.Add(left, right);
        return ((delegate* managed<ProfiledBasicTarget, int, int, int>)route)(
            target,
            left,
            right);
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
        ProfiledBasicRouteState.FunctionPointer(typeof(ProfiledBasicAddCaller));

    /// <summary>Invokes the leased trampoline or preserves original addition.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ProfiledBasicTarget target,
        int left,
        int right)
    {
        if (!State.TryAcquire(out var entryPoint))
            return ProfiledBasicOriginal.Add(target, left, right);

        return ((delegate* managed<ProfiledBasicTarget, int, int, int>)
            entryPoint)(target, left, right);
    }
}
