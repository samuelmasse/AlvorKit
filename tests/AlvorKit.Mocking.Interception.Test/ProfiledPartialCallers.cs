namespace AlvorKit;

/// <summary>Owns the exact selected addition caller and its gated route lease.</summary>
internal static class ProfiledAddCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the concrete addition operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledPartialTarget target,
        int left,
        int right) =>
        target.Add(left, right);

    /// <summary>Runs original addition while inert or the exact route when published.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledPartialTarget target,
        int left,
        int right)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return target.Add(left, right);
        return ((delegate* managed<ProfiledPartialTarget, int, int, int>)route)(
            target,
            left,
            right);
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
    internal static nint FunctionPointer() => Pointer(nameof(Invoke));

    /// <summary>Invokes the leased trampoline or preserves original addition.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe int Invoke(
        ProfiledPartialTarget target,
        int left,
        int right)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ProfiledPartialOriginal.Add(target, left, right);
        }

        return ((delegate* managed<ProfiledPartialTarget, int, int, int>)
            entryPoint)(target, left, right);
    }

    private static nint Pointer(string name)
    {
        var method = typeof(ProfiledAddCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }
}

/// <summary>Owns the exact selected neighboring caller and its gated route lease.</summary>
internal static class ProfiledNeighborCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the neighboring concrete operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledPartialTarget target,
        int value) =>
        target.Neighbor(value);

    /// <summary>Runs original neighboring behavior while inert or its exact route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledPartialTarget target,
        int value)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return target.Neighbor(value);
        return ((delegate* managed<ProfiledPartialTarget, int, int>)route)(
            target,
            value);
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
    internal static nint FunctionPointer() => Pointer(nameof(Invoke));

    /// <summary>Invokes the leased trampoline or preserves original neighboring behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe int Invoke(
        ProfiledPartialTarget target,
        int value)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ProfiledPartialOriginal.Neighbor(target, value);
        }

        return ((delegate* managed<ProfiledPartialTarget, int, int>)
            entryPoint)(target, value);
    }

    private static nint Pointer(string name)
    {
        var method = typeof(ProfiledNeighborCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }
}

/// <summary>Owns the exact selected throwing caller and its gated route lease.</summary>
internal static class ProfiledThrowCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the throwing concrete operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(ProfiledPartialTarget target) =>
        target.ThrowOriginal();

    /// <summary>Runs the original throw while inert or its exact route when published.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(ProfiledPartialTarget target)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
        {
            target.ThrowOriginal();
            return;
        }

        ((delegate* managed<ProfiledPartialTarget, void>)route)(target);
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
    internal static nint FunctionPointer() => Pointer(nameof(Invoke));

    /// <summary>Invokes the leased trampoline or preserves the original throw.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe void Invoke(ProfiledPartialTarget target)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            ProfiledPartialOriginal.Throw(target);
            return;
        }

        ((delegate* managed<ProfiledPartialTarget, void>)entryPoint)(target);
    }

    private static nint Pointer(string name)
    {
        var method = typeof(ProfiledThrowCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }
}

/// <summary>Owns the exact selected ref/out caller and its gated route lease.</summary>
internal static class ProfiledMutateCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the concrete ref/out operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledPartialTarget target,
        ref int value,
        out int doubled) =>
        target.Mutate(ref value, out doubled);

    /// <summary>Runs original ref/out behavior while inert or its exact route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledPartialTarget target,
        ref int value,
        out int doubled)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return target.Mutate(ref value, out doubled);
        return ((delegate* managed<
            ProfiledPartialTarget,
            ref int,
            out int,
            int>)route)(target, ref value, out doubled);
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
    internal static nint FunctionPointer() => Pointer(nameof(Invoke));

    /// <summary>Invokes the leased trampoline or preserves original ref/out behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe int Invoke(
        ProfiledPartialTarget target,
        ref int value,
        out int doubled)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ProfiledPartialOriginal.Mutate(
                target,
                ref value,
                out doubled);
        }

        return ((delegate* managed<
            ProfiledPartialTarget,
            ref int,
            out int,
            int>)entryPoint)(target, ref value, out doubled);
    }

    private static nint Pointer(string name)
    {
        var method = typeof(ProfiledMutateCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }
}
