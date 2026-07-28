namespace AlvorKit.Interception.CallerProof.Test;

internal static class RefOutCallerBridgeTarget
{
    private static nint routePointer;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Caller(ref int value, out int observed) =>
        Original(ref value, out observed);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe long RoutedTemplate(
        ref int value,
        out int observed)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return Original(ref value, out observed);

        return ((delegate* managed<ref int, out int, long>)route)(
            ref value,
            out observed);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Original(ref int value, out int observed)
    {
        value += 2;
        observed = value;
        return 10_000_000_000L + value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Replacement(ref int value, out int observed)
    {
        value += 5;
        observed = -value;
        return 20_000_000_000L + value;
    }
}
