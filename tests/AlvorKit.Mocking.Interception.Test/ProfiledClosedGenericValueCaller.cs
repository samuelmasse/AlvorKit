namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the generic caller for closed concrete property getters.</summary>
internal static class ProfiledClosedGenericValueCaller
{
    /// <summary>Calls one closed generic target's property getter.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Selected<T>(ProfiledGenericTarget<T> target) =>
        target.Value;

    /// <summary>Uses the construction-specific route or preserves original behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe T RoutedTemplate<T>(
        ProfiledGenericTarget<T> target)
    {
        var route = ProfiledClosedGenericValueRoute<T>.Pointer;
        if (route == 0)
            return target.Value;
        return ((delegate* managed<ProfiledGenericTarget<T>, T>)route)(
            target);
    }

    /// <summary>Invokes the integer construction's exact leased trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int InvokeInt32(
        ProfiledGenericTarget<int> target)
    {
        if (!ProfiledClosedGenericValueRoute<int>.TryAcquire(
                out var entryPoint))
        {
            return ProfiledGenericOriginal.ClosedValue(target);
        }

        return ((delegate* managed<
            ProfiledGenericTarget<int>,
            int>)entryPoint)(target);
    }

    /// <summary>Invokes the string construction's exact leased trampoline.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string InvokeString(
        ProfiledGenericTarget<string> target)
    {
        if (!ProfiledClosedGenericValueRoute<string>.TryAcquire(
                out var entryPoint))
        {
            return ProfiledGenericOriginal.ClosedValue(target);
        }

        return ((delegate* managed<
            ProfiledGenericTarget<string>,
            string>)entryPoint)(target);
    }
}
