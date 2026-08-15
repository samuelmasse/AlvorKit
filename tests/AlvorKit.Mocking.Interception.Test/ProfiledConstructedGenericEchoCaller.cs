namespace AlvorKit;

/// <summary>Owns the generic caller for constructions of one concrete generic method.</summary>
internal static class ProfiledConstructedGenericEchoCaller
{
    /// <summary>Calls one construction of the concrete generic method.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Selected<T>(
        ProfiledConstructedGenericTarget target,
        T value) =>
        target.Echo(value);

    /// <summary>Uses the construction-specific route or preserves original behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe T RoutedTemplate<T>(
        ProfiledConstructedGenericTarget target,
        T value)
    {
        var route = ProfiledConstructedGenericEchoRoute<T>.Pointer;
        if (route == 0)
            return target.Echo(value);
        return ((delegate* managed<
            ProfiledConstructedGenericTarget,
            T,
            T>)route)(target, value);
    }

    /// <summary>Invokes the integer construction's exact leased trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int InvokeInt32(
        ProfiledConstructedGenericTarget target,
        int value)
    {
        if (!ProfiledConstructedGenericEchoRoute<int>.TryAcquire(
                out var entryPoint))
        {
            return ProfiledGenericOriginal.ConstructedEcho(target, value);
        }

        return ((delegate* managed<
            ProfiledConstructedGenericTarget,
            int,
            int>)entryPoint)(target, value);
    }

    /// <summary>Invokes the string construction's exact leased trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string InvokeString(
        ProfiledConstructedGenericTarget target,
        string value)
    {
        if (!ProfiledConstructedGenericEchoRoute<string>.TryAcquire(
                out var entryPoint))
        {
            return ProfiledGenericOriginal.ConstructedEcho(target, value);
        }

        return ((delegate* managed<
            ProfiledConstructedGenericTarget,
            string,
            string>)entryPoint)(target, value);
    }
}
