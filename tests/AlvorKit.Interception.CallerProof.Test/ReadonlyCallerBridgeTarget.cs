namespace AlvorKit.Interception.CallerProof.Test;

internal readonly struct ReadonlyCallerBridgeValue(int value)
{
    internal int Value { get; } = value;
}

internal static class ReadonlyCallerBridgeTarget
{
    private static nint routePointer;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Caller(
        in ReadonlyCallerBridgeValue receiver,
        in int delta) =>
        Original(in receiver, in delta);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        in ReadonlyCallerBridgeValue receiver,
        in int delta)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return Original(in receiver, in delta);

        return ((
            delegate* managed<
                in ReadonlyCallerBridgeValue,
                in int,
                int>)route)(
                    in receiver,
                    in delta);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Original(
        in ReadonlyCallerBridgeValue receiver,
        in int delta) =>
        receiver.Value + delta;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Replacement(
        in ReadonlyCallerBridgeValue receiver,
        in int delta) =>
        (receiver.Value * 10) + delta;
}
