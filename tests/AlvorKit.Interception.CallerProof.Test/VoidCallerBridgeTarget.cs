namespace AlvorKit;

internal static class VoidCallerBridgeTarget
{
    private static nint routePointer;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Caller(ref int value) =>
        Original(ref value);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(ref int value)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
        {
            Original(ref value);
            return;
        }

        ((delegate* managed<ref int, void>)route)(ref value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Original(ref int value) =>
        value++;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Replacement(ref int value) =>
        value += 10;
}
