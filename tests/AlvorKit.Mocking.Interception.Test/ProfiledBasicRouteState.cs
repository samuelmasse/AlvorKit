namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Holds one basic caller's inert pointer and coordinator-gated lease.</summary>
internal sealed class ProfiledBasicRouteState
{
    private ProfiledRouteBinding? binding;
    private nint routePointer;

    /// <summary>Gets the currently published exact route pointer.</summary>
    internal nint RoutePointer =>
        Volatile.Read(ref routePointer);

    /// <summary>Binds the exact route while its caller pointer remains inert.</summary>
    internal void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Clears the retired exact route lease.</summary>
    internal void Clear() =>
        Volatile.Write(ref binding, null);

    /// <summary>Publishes the exact route pointer or zero for original behavior.</summary>
    internal void Publish(nint pointer) =>
        Volatile.Write(ref routePointer, pointer);

    /// <summary>Acquires the exact trampoline only after coordinator publication.</summary>
    internal bool TryAcquire(out nint entryPoint)
    {
        var current = Volatile.Read(ref binding);
        if (current is not null &&
            current.Route.IsActivated &&
            current.Trampoline.TryAcquire(out entryPoint))
        {
            return true;
        }

        entryPoint = 0;
        return false;
    }

    /// <summary>Gets one prepared managed route entry point.</summary>
    internal static nint FunctionPointer(Type callerType)
    {
        var method = callerType.GetMethod(
            "Invoke",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }
}
