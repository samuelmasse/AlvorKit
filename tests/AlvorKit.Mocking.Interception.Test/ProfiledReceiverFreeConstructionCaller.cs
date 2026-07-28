namespace AlvorKit.Mocking.Interception.Test;

internal static class ProfiledReceiverFreeConstructionCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledReceiverFreeTarget Selected(int value) =>
        new(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe ProfiledReceiverFreeTarget Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledConstructionTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.Construct(value);
        }

        return ((delegate* managed<
            int,
            ProfiledReceiverFreeTarget>)entryPoint)(value);
    }
}
