namespace AlvorKit.Interception.CallerProof.Test;

internal static class CallerBridgeTarget
{
    private static nint routePointer;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Caller(int value) =>
        Original(value);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(int value)
    {
        var operand = value;
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return Original(operand);

        return ((delegate* managed<int, int>)route)(operand);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Original(int value) =>
        value + 1;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Replacement(int value) =>
        value * 10;
}
