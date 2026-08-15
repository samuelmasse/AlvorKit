namespace AlvorKit;

/// <summary>Holds one receiver-free caller's exact pointer and route lease.</summary>
internal static class ProfiledReceiverFreeRouteState<TTag>
{
    private static ProfiledRouteBinding? binding;
    private static nint pointer;

    /// <summary>Gets the published exact route pointer.</summary>
    internal static nint Pointer => Volatile.Read(ref pointer);

    /// <summary>Binds the exact route while the caller remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Publishes the exact route pointer or zero.</summary>
    internal static void Publish(nint value) =>
        Volatile.Write(ref pointer, value);

    /// <summary>Clears the retired exact route lease.</summary>
    internal static void Clear() =>
        Volatile.Write(ref binding, null);

    /// <summary>Acquires the exact trampoline after shared publication.</summary>
    internal static bool TryAcquire(out nint entryPoint) =>
        ProfiledGenericRouteAcquire.TryAcquire(
            Volatile.Read(ref binding),
            out entryPoint);
}

internal sealed class ProfiledTransformTag;
internal sealed class ProfiledIdentityTag<T>;
internal sealed class ProfiledSetStaticNumberTag;
internal sealed class ProfiledGetStaticNumberTag;
internal sealed class ProfiledWriteStaticFieldTag;
internal sealed class ProfiledReadStaticFieldTag;
internal sealed class ProfiledConstructionTag;
internal sealed class ProfiledReadInstanceFieldTag;
internal sealed class ProfiledWriteInstanceFieldTag;
internal sealed class ProfiledReadInstanceReferenceFieldTag;
internal sealed class ProfiledWriteInstanceReferenceFieldTag;
