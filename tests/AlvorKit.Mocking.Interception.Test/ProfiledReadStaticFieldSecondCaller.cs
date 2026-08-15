namespace AlvorKit;

internal sealed class ProfiledReadStaticFieldSecondTag;

/// <summary>Owns a second independently identified read of the same static field.</summary>
internal static class ProfiledReadStaticFieldSecondCaller
{
    /// <summary>Reads the same static field from the second selected site.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected() =>
        ProfiledReceiverFreeTarget.StaticField;

    /// <summary>Uses the second read site's exact route or preserves storage.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate()
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledReadStaticFieldSecondTag>.Pointer;
        return route == 0
            ? ProfiledReceiverFreeTarget.StaticField
            : ((delegate* managed<int>)route)();
    }

    /// <summary>Invokes the second read site's leased production wrapper.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke()
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledReadStaticFieldSecondTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.ReadStaticField();
        }

        return ((delegate* managed<int>)entryPoint)();
    }
}
