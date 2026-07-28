namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Concrete sealed target whose ordinary nonvirtual operation is intercepted.</summary>
public sealed class ProfiledMockTarget
{
    /// <summary>Gets the number of original operation executions.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Runs the ordinary nonvirtual implementation.</summary>
    public int Calculate(int value)
    {
        OriginalCalls++;
        return value + 10;
    }
}

/// <summary>Exact operation delegate bound to the real Mocking interception wrapper.</summary>
public delegate int ProfiledMockOperation(
    ProfiledMockTarget target,
    int value);

/// <summary>Preserves the selected caller's exact original operation.</summary>
internal static class ProfiledMockOriginal
{
    /// <summary>Invokes the untouched operation for wrapper fallback.</summary>
    internal static int Invoke(
        ProfiledMockTarget target,
        int value) =>
        target.Calculate(value);
}

/// <summary>Exposes an exact instance handler over the bound Mocking wrapper.</summary>
/// <param name="wrapper">The real runtime-bound Mocking operation wrapper.</param>
public sealed class ProfiledMockHandler(ProfiledMockOperation wrapper)
{
    /// <summary>The number of calls that entered the handler.</summary>
    private int invocationCount;

    /// <summary>Gets the number of calls that reached the profiler trampoline handler.</summary>
    public int InvocationCount =>
        Volatile.Read(ref invocationCount);

    /// <summary>Invokes the actual bound Mocking interception wrapper.</summary>
    public int Invoke(ProfiledMockTarget target, int value)
    {
        Interlocked.Increment(ref invocationCount);
        return wrapper(target, value);
    }
}

/// <summary>Owns the selected and explicitly unselected caller methods.</summary>
internal static class ProfiledMockCaller
{
    /// <summary>The active test lifecycle route pointer, or zero while inert.</summary>
    private static nint routePointer;

    /// <summary>Sets the exact active route, or zero for inert original fallback.</summary>
    internal static nint RoutePointer
    {
        set => Volatile.Write(ref routePointer, value);
    }

    /// <summary>Calls the operation from the one caller selected for ReJIT.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledMockTarget target,
        int value) =>
        target.Calculate(value);

    /// <summary>Calls the same operation from a caller that is never selected.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Unselected(
        ProfiledMockTarget target,
        int value) =>
        target.Calculate(value);

    /// <summary>Runs the original operation on a miss or the exact leased route on a hit.</summary>
    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledMockTarget target,
        int value)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return target.Calculate(value);

        return ((delegate* managed<ProfiledMockTarget, int, int>)route)(
            target,
            value);
    }
}
