namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the generic caller for closed concrete echo operations.</summary>
internal static class ProfiledClosedGenericEchoCaller
{
    /// <summary>Calls one closed generic target construction.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Selected<T>(
        ProfiledGenericTarget<T> target,
        T value) =>
        target.Echo(value);

    /// <summary>Uses the construction-specific route or preserves original behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe T RoutedTemplate<T>(
        ProfiledGenericTarget<T> target,
        T value)
    {
        var route = ProfiledClosedGenericEchoRoute<T>.Pointer;
        if (route == 0)
            return target.Echo(value);
        return ((delegate* managed<ProfiledGenericTarget<T>, T, T>)route)(
            target,
            value);
    }

    /// <summary>Invokes the integer construction's exact leased trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int InvokeInt32(
        ProfiledGenericTarget<int> target,
        int value)
    {
        if (!ProfiledClosedGenericEchoRoute<int>.TryAcquire(
                out var entryPoint))
        {
            return ProfiledGenericOriginal.ClosedEcho(target, value);
        }

        return ((delegate* managed<
            ProfiledGenericTarget<int>,
            int,
            int>)entryPoint)(target, value);
    }

    /// <summary>Invokes the string construction's exact leased trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string InvokeString(
        ProfiledGenericTarget<string> target,
        string value)
    {
        if (!ProfiledClosedGenericEchoRoute<string>.TryAcquire(
                out var entryPoint))
        {
            return ProfiledGenericOriginal.ClosedEcho(target, value);
        }

        return ((delegate* managed<
            ProfiledGenericTarget<string>,
            string,
            string>)entryPoint)(target, value);
    }
}
