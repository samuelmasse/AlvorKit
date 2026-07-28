namespace AlvorKit.Mocking.Interception.Test;

internal sealed class ProfiledTransformSecondTag;

/// <summary>Owns a second independently identified caller to the same static target.</summary>
internal static class ProfiledStaticTransformSecondCaller
{
    /// <summary>Calls the same static operation from the second selected site.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(int value) =>
        ProfiledReceiverFreeTarget.Transform(value);

    /// <summary>Uses the second site's exact route or preserves original behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(int value)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledTransformSecondTag>.Pointer;
        return route == 0
            ? ProfiledReceiverFreeTarget.Transform(value)
            : ((delegate* managed<int, int>)route)(value);
    }

    /// <summary>Invokes the second site's leased production wrapper.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledTransformSecondTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.Transform(value);
        }

        return ((delegate* managed<int, int>)entryPoint)(value);
    }
}
