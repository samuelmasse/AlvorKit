namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns a genuinely generic caller rewritten for all its constructions.</summary>
internal static class ProfiledConstructedGenericStructCaller
{
    /// <summary>Calls the exact operation on a constructed generic value receiver.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Selected<T>(
        ref ProfiledConstructedGenericStructTarget<T> target,
        T value)
        where T : notnull =>
        target.Echo(value);

    /// <summary>Routes by the current construction or preserves its original call.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe T RoutedTemplate<T>(
        ref ProfiledConstructedGenericStructTarget<T> target,
        T value)
        where T : notnull
    {
        nint route = ProfiledConstructedGenericStructRouteState<T>.Pointer;
        return route == 0
            ? target.Echo(value)
            : ((delegate* managed<
                ref ProfiledConstructedGenericStructTarget<T>,
                T,
                T>)route)(ref target, value);
    }

    /// <summary>Invokes the integer construction's leased exact trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int InvokeInt32(
        ref ProfiledConstructedGenericStructTarget<int> target,
        int value)
    {
        if (!ProfiledConstructedGenericStructRouteState<int>.TryAcquire(
                out nint entryPoint))
        {
            return ProfiledConstructedGenericStructOriginal.Echo(
                ref target,
                value);
        }

        return ((delegate* managed<
            ref ProfiledConstructedGenericStructTarget<int>,
            int,
            int>)entryPoint)(ref target, value);
    }

    /// <summary>Invokes the string construction's leased exact trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string InvokeString(
        ref ProfiledConstructedGenericStructTarget<string> target,
        string value)
    {
        if (!ProfiledConstructedGenericStructRouteState<string>.TryAcquire(
                out nint entryPoint))
        {
            return ProfiledConstructedGenericStructOriginal.Echo(
                ref target,
                value);
        }

        return ((delegate* managed<
            ref ProfiledConstructedGenericStructTarget<string>,
            string,
            string>)entryPoint)(ref target, value);
    }
}

/// <summary>Holds one construction's route without retaining receiver storage.</summary>
internal static class ProfiledConstructedGenericStructRouteState<T>
    where T : notnull
{
    private static ProfiledRouteBinding? binding;
    private static nint pointer;

    /// <summary>Gets the construction-specific published route.</summary>
    internal static nint Pointer => Volatile.Read(ref pointer);

    /// <summary>Binds one exact construction to its shared activation route.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Publishes an exact construction entry point or inert zero.</summary>
    internal static void Publish(nint value) =>
        Volatile.Write(ref pointer, value);

    /// <summary>Clears the retired construction lease.</summary>
    internal static void Clear() => Volatile.Write(ref binding, null);

    /// <summary>Acquires the exact trampoline behind the coordinator gate.</summary>
    internal static bool TryAcquire(out nint entryPoint) =>
        ProfiledGenericRouteAcquire.TryAcquire(
            Volatile.Read(ref binding),
            out entryPoint);
}
