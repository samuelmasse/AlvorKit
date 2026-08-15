namespace AlvorKit;

/// <summary>Owns the selected concrete ref/out caller and its gated route.</summary>
internal static class ProfiledBasicMutateCaller
{
    private static readonly ProfiledBasicRouteState State = new();

    /// <summary>Calls the concrete ref/out operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledBasicTarget target,
        ref int value,
        out int doubled) =>
        target.Mutate(ref value, out doubled);

    /// <summary>Runs original ref/out behavior while inert or its exact published route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledBasicTarget target,
        ref int value,
        out int doubled)
    {
        var route = State.RoutePointer;
        if (route == 0)
            return target.Mutate(ref value, out doubled);
        return ((delegate* managed<
            ProfiledBasicTarget,
            ref int,
            out int,
            int>)route)(target, ref value, out doubled);
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
            typeof(ProfiledBasicMutateCaller));

    /// <summary>Invokes the leased trampoline or preserves original ref/out behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ProfiledBasicTarget target,
        ref int value,
        out int doubled)
    {
        if (!State.TryAcquire(out var entryPoint))
        {
            return ProfiledBasicOriginal.Mutate(
                target,
                ref value,
                out doubled);
        }

        return ((delegate* managed<
            ProfiledBasicTarget,
            ref int,
            out int,
            int>)entryPoint)(target, ref value, out doubled);
    }
}
