namespace AlvorKit;

internal class CallvirtCallerBridgeReceiver
{
    internal virtual int Operation(int value) =>
        value + 1;
}

internal static class CallvirtCallerBridgeTarget
{
    private static nint routePointer;
    private static nint originalPointer;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    internal static nint OriginalPointer
    {
        set => Volatile.Write(ref originalPointer, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Caller(
        CallvirtCallerBridgeReceiver receiver,
        int value) =>
        receiver.Operation(value);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        CallvirtCallerBridgeReceiver receiver,
        int value)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return receiver.Operation(value);

        return ((
            delegate* managed<
                CallvirtCallerBridgeReceiver,
                int,
                int>)route)(
                    receiver,
                    value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int OriginalBridge(
        CallvirtCallerBridgeReceiver receiver,
        int value) =>
        receiver.Operation(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe int MatchingWrapper(
        CallvirtCallerBridgeReceiver receiver,
        int value)
    {
        if (value == 3)
            return 30;

        var original = Volatile.Read(ref originalPointer);
        return ((
            delegate* managed<
                CallvirtCallerBridgeReceiver,
                int,
                int>)original)(
                    receiver,
                    value);
    }
}
