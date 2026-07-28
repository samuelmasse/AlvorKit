namespace AlvorKit.Mocking.Interception.Test;

internal sealed class ProfiledConstructionAtSiteFirstTag;
internal sealed class ProfiledConstructionAtSiteSecondTag;

/// <summary>Owns the first independently identified construction site.</summary>
internal static class ProfiledConstructionAtSiteFirstCaller
{
    /// <summary>Allocates one target through the first selected newobj site.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledReceiverFreeTarget Selected(int value) =>
        new(value);

    /// <summary>Invokes the first leased production construction wrapper.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe ProfiledReceiverFreeTarget Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledConstructionAtSiteFirstTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.Construct(value);
        }

        return ((delegate* managed<
            int,
            ProfiledReceiverFreeTarget>)entryPoint)(value);
    }
}

/// <summary>Owns the second independently identified construction site.</summary>
internal static class ProfiledConstructionAtSiteSecondCaller
{
    /// <summary>Allocates one target through the second selected newobj site.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledReceiverFreeTarget Selected(int value) =>
        new(value);

    /// <summary>Invokes the second leased production construction wrapper.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe ProfiledReceiverFreeTarget Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledConstructionAtSiteSecondTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.Construct(value);
        }

        return ((delegate* managed<
            int,
            ProfiledReceiverFreeTarget>)entryPoint)(value);
    }
}
