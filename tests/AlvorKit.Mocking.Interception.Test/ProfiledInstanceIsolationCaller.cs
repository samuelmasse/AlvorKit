namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the receiver-isolation caller and its gated route.</summary>
internal static class ProfiledInstanceIsolationCaller
{
    private static readonly ProfiledBasicRouteState State = new();

    /// <summary>Calls concrete addition from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledInstanceIsolationTarget target,
        int left,
        int right) =>
        target.Add(left, right);

    /// <summary>Runs original addition while inert or its published route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledInstanceIsolationTarget target,
        int left,
        int right)
    {
        var route = State.RoutePointer;
        if (route == 0)
            return target.Add(left, right);
        return ((delegate* managed<
            ProfiledInstanceIsolationTarget,
            int,
            int,
            int>)route)(target, left, right);
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
            typeof(ProfiledInstanceIsolationCaller));

    /// <summary>Invokes the leased trampoline or preserves original addition.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ProfiledInstanceIsolationTarget target,
        int left,
        int right)
    {
        if (!State.TryAcquire(out var entryPoint))
        {
            return ProfiledInstanceIsolationOriginal.Add(
                target,
                left,
                right);
        }

        return ((delegate* managed<
            ProfiledInstanceIsolationTarget,
            int,
            int,
            int>)entryPoint)(target, left, right);
    }
}
