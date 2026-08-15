namespace AlvorKit;

internal static class ProfiledStructConstrainedCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected<T>(
        ref T target,
        int amount)
        where T : struct, IProfiledStructMetric =>
        target.Measure(amount);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate<T>(
        ref T target,
        int amount)
        where T : struct, IProfiledStructMetric
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledStructConstrainedTag<T>>.Pointer;
        return route == 0
            ? target.Measure(amount)
            : ((delegate* managed<ref T, int, int>)route)(
                ref target,
                amount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int InvokeMutable(
        ref ProfiledMutableStructTarget target,
        int amount)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStructConstrainedTag<
                    ProfiledMutableStructTarget>>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledStructOriginal.Constrained(
                ref target,
                amount);
        }

        return ((delegate* managed<
            ref ProfiledMutableStructTarget,
            int,
            int>)entryPoint)(ref target, amount);
    }
}

internal sealed class ProfiledStructConstrainedTag<T>
    where T : struct, IProfiledStructMetric;
