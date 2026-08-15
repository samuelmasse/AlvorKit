namespace AlvorKit;

internal static class MatchingCallerBridgeTarget
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
    internal static int Caller(int value) =>
        PrivateOriginal(value);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(int value)
    {
        var operand = value;
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return PrivateOriginal(operand);

        return ((delegate* managed<int, int>)route)(operand);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe int MatchingWrapper(int value)
    {
        if ((value & 1) != 0)
            return value * 10;

        var original = Volatile.Read(ref originalPointer);
        return ((delegate* managed<int, int>)original)(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int PrivateOriginal(int value)
    {
        if (value < 0)
            throw OriginalCallerBridgeException.Instance;

        return value + 1;
    }
}

internal sealed class OriginalCallerBridgeException :
    Exception
{
    internal static OriginalCallerBridgeException Instance { get; } = new();

    private OriginalCallerBridgeException() :
        base("Original caller bridge failure.")
    {
    }
}
