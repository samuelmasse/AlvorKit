namespace AlvorKit.Interception.CallerProof.Test;

internal static class SpanCallerBridgeTarget
{
    private static nint routePointer;

    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Caller(Span<int> values) =>
        Original(values);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(Span<int> values)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return Original(values);

        return ((delegate* managed<Span<int>, int>)route)(values);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Original(Span<int> values)
    {
        values[0] += 1;
        return values[0];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Replacement(Span<int> values)
    {
        values[0] += 10;
        return values[0];
    }
}
