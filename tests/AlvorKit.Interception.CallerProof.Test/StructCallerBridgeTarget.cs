namespace AlvorKit;

internal struct CallerBridgeCounter
{
    internal int Value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal int Add(int delta)
    {
        Value += delta;
        return Value;
    }
}

internal static class StructCallerBridgeTarget
{
    private static nint routePointer;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Caller(ref CallerBridgeCounter receiver, int delta) =>
        receiver.Add(delta);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ref CallerBridgeCounter receiver,
        int delta)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return receiver.Add(delta);

        return ((delegate* managed<
            ref CallerBridgeCounter,
            int,
            int>)route)(ref receiver, delta);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Replacement(
        ref CallerBridgeCounter receiver,
        int delta)
    {
        if (delta < 0)
            throw new CallerBridgeException(delta);

        receiver.Value += delta * 10;
        return receiver.Value;
    }
}

internal sealed class CallerBridgeException(int delta) :
    Exception($"Rejected delta {delta}.");
