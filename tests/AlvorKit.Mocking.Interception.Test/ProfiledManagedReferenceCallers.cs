namespace AlvorKit;

/// <summary>Owns the mutable managed-reference selected caller.</summary>
internal static class ProfiledMutableReferenceCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the mutable managed-reference operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ref int Selected(
        ProfiledManagedReferenceTarget target) =>
        ref target.Mutable();

    /// <summary>Runs original behavior while inert or the exact managed-ref route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe ref int RoutedTemplate(
        ProfiledManagedReferenceTarget target)
    {
        var pointer = Volatile.Read(ref routePointer);
        if (pointer == 0)
            return ref target.Mutable();
        return ref ((delegate* managed<
            ProfiledManagedReferenceTarget,
            ref int>)pointer)(target);
    }

    /// <summary>Binds the exact wrapper while the published route remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute value,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(value, trampoline));

    /// <summary>Clears the retired route and exact wrapper.</summary>
    internal static void Clear() => Volatile.Write(ref binding, null);

    /// <summary>Publishes the managed route pointer or zero for original behavior.</summary>
    internal static void Publish(nint pointer) =>
        Volatile.Write(ref routePointer, pointer);

    /// <summary>Gets the prepared managed route entry point.</summary>
    internal static nint FunctionPointer()
    {
        var method = typeof(ProfiledMutableReferenceCaller).GetMethod(
            nameof(Invoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    /// <summary>Invokes the exact wrapper behind the coordinator publication gate.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe ref int Invoke(
        ProfiledManagedReferenceTarget target)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ref ProfiledManagedReferenceOriginal.Mutable(target);
        }

        return ref ((delegate* managed<
            ProfiledManagedReferenceTarget,
            ref int>)entryPoint)(target);
    }
}

/// <summary>Owns the readonly managed-reference selected caller.</summary>
internal static class ProfiledReadOnlyReferenceCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the readonly managed-reference operation from the selected caller.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ref readonly int Selected(
        ProfiledManagedReferenceTarget target) =>
        ref target.ReadOnly();

    /// <summary>Runs original behavior while inert or the exact readonly route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe ref readonly int RoutedTemplate(
        ProfiledManagedReferenceTarget target)
    {
        var pointer = Volatile.Read(ref routePointer);
        if (pointer == 0)
            return ref target.ReadOnly();
        return ref ((delegate* managed<
            ProfiledManagedReferenceTarget,
            ref readonly int>)pointer)(target);
    }

    /// <summary>Binds the exact wrapper while the published route remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute value,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(value, trampoline));

    /// <summary>Clears the retired route and exact wrapper.</summary>
    internal static void Clear() => Volatile.Write(ref binding, null);

    /// <summary>Publishes the managed route pointer or zero for original behavior.</summary>
    internal static void Publish(nint pointer) =>
        Volatile.Write(ref routePointer, pointer);

    /// <summary>Gets the prepared managed route entry point.</summary>
    internal static nint FunctionPointer()
    {
        var method = typeof(ProfiledReadOnlyReferenceCaller).GetMethod(
            nameof(Invoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    /// <summary>Invokes the exact wrapper behind the coordinator publication gate.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe ref readonly int Invoke(
        ProfiledManagedReferenceTarget target)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return ref ProfiledManagedReferenceOriginal.ReadOnly(target);
        }

        return ref ((delegate* managed<
            ProfiledManagedReferenceTarget,
            ref readonly int>)entryPoint)(target);
    }
}
