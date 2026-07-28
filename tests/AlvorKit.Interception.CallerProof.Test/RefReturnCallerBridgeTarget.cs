namespace AlvorKit.Interception.CallerProof.Test;

internal static class RefReturnCallerBridgeTarget
{
    private static nint routePointer;
    private static int originalStorage = 7;
    private static int replacementStorage = 11;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    internal static int OriginalStorage => originalStorage;

    internal static int ReplacementStorage => replacementStorage;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ref int Caller() =>
        ref Original();

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe ref int RoutedTemplate()
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return ref Original();

        return ref ((delegate* managed<ref int>)route)();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ref int Original() =>
        ref originalStorage;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ref int Replacement() =>
        ref replacementStorage;
}
