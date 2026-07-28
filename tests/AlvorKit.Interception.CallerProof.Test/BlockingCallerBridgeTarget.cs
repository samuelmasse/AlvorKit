namespace AlvorKit.Interception.CallerProof.Test;

internal static class BlockingCallerBridgeTarget
{
    private static readonly ManualResetEventSlim Entered = new(false);
    private static readonly ManualResetEventSlim Release = new(false);
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
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return Original(value);

        return ((delegate* managed<int, int>)route)(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Original(int value) =>
        value + 1;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int BlockingReplacement(int value)
    {
        Entered.Set();
        if (!Release.Wait(TimeSpan.FromSeconds(10)))
            throw new TimeoutException("The blocked caller route was not released.");
        return value * 10;
    }

    internal static void Reset()
    {
        RoutePointer = 0;
        Entered.Reset();
        Release.Reset();
    }

    internal static bool WaitUntilEntered(TimeSpan timeout) =>
        Entered.Wait(timeout);

    internal static void ReleaseInvocation() =>
        Release.Set();
}
