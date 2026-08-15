namespace AlvorKit;

/// <summary>Provides the direct caller and exact routed replacement used by the profiler fixture.</summary>
internal static class InterceptionPerformanceTarget
{
    private static nint routePointer;

    /// <summary>Sets the managed function pointer selected by the routed caller body.</summary>
    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    /// <summary>Calls the original operation before and after profiler replacement.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Caller(int value) =>
        Original(value);

    /// <summary>Runs the original operation for an inert route or the exact managed route when active.</summary>
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

    /// <summary>Represents the unmodified operation preserved by the inert route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Original(int value) =>
        value + 1;

    /// <summary>Represents the exact managed destination selected by the active route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Replacement(int value) =>
        value * 10;

    /// <summary>
    /// Represents a second exact destination installed without another profiler request.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int SwappedReplacement(int value) =>
        value * 100;
}
