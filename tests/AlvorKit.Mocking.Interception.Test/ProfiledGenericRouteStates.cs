namespace AlvorKit;

/// <summary>Holds one closed generic echo construction's route state.</summary>
internal static class ProfiledClosedGenericEchoRoute<T>
{
    private static ProfiledRouteBinding? binding;
    private static nint pointer;

    /// <summary>Gets the published construction-specific pointer.</summary>
    internal static nint Pointer => Volatile.Read(ref pointer);

    /// <summary>Binds the exact construction behind its coordinator route.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Publishes the exact pointer or zero for original behavior.</summary>
    internal static void Publish(nint value) =>
        Volatile.Write(ref pointer, value);

    /// <summary>Clears the retired construction lease.</summary>
    internal static void Clear() =>
        Volatile.Write(ref binding, null);

    /// <summary>Acquires the exact trampoline after shared publication.</summary>
    internal static bool TryAcquire(out nint entryPoint) =>
        ProfiledGenericRouteAcquire.TryAcquire(
            Volatile.Read(ref binding),
            out entryPoint);
}

/// <summary>Holds one closed generic property construction's route state.</summary>
internal static class ProfiledClosedGenericValueRoute<T>
{
    private static ProfiledRouteBinding? binding;
    private static nint pointer;

    /// <summary>Gets the published construction-specific pointer.</summary>
    internal static nint Pointer => Volatile.Read(ref pointer);

    /// <summary>Binds the exact construction behind its coordinator route.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Publishes the exact pointer or zero for original behavior.</summary>
    internal static void Publish(nint value) =>
        Volatile.Write(ref pointer, value);

    /// <summary>Clears the retired construction lease.</summary>
    internal static void Clear() =>
        Volatile.Write(ref binding, null);

    /// <summary>Acquires the exact trampoline after shared publication.</summary>
    internal static bool TryAcquire(out nint entryPoint) =>
        ProfiledGenericRouteAcquire.TryAcquire(
            Volatile.Read(ref binding),
            out entryPoint);
}

/// <summary>Holds one constructed generic method's route state.</summary>
internal static class ProfiledConstructedGenericEchoRoute<T>
{
    private static ProfiledRouteBinding? binding;
    private static nint pointer;

    /// <summary>Gets the published construction-specific pointer.</summary>
    internal static nint Pointer => Volatile.Read(ref pointer);

    /// <summary>Binds the exact construction behind its coordinator route.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Publishes the exact pointer or zero for original behavior.</summary>
    internal static void Publish(nint value) =>
        Volatile.Write(ref pointer, value);

    /// <summary>Clears the retired construction lease.</summary>
    internal static void Clear() =>
        Volatile.Write(ref binding, null);

    /// <summary>Acquires the exact trampoline after shared publication.</summary>
    internal static bool TryAcquire(out nint entryPoint) =>
        ProfiledGenericRouteAcquire.TryAcquire(
            Volatile.Read(ref binding),
            out entryPoint);
}

/// <summary>Applies the shared coordinator and trampoline lease checks.</summary>
internal static class ProfiledGenericRouteAcquire
{
    /// <summary>Acquires an active binding or returns an inert zero pointer.</summary>
    internal static bool TryAcquire(
        ProfiledRouteBinding? binding,
        out nint entryPoint)
    {
        if (binding is not null &&
            binding.Route.IsActivated &&
            binding.Trampoline.TryAcquire(out entryPoint))
        {
            return true;
        }

        entryPoint = 0;
        return false;
    }
}
