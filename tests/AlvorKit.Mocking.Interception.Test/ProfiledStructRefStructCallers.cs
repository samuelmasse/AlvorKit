namespace AlvorKit;

internal static class ProfiledStructSpanCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ref ProfiledStructRefStructTarget target,
        Span<int> values) =>
        target.Observe(values);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ref ProfiledStructRefStructTarget target,
        Span<int> values)
    {
        var route =
            ProfiledReceiverFreeRouteState<
                ProfiledStructSpanTag>.Pointer;
        return route == 0
            ? target.Observe(values)
            : ((delegate* managed<
                ref ProfiledStructRefStructTarget,
                Span<int>,
                int>)route)(ref target, values);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ref ProfiledStructRefStructTarget target,
        Span<int> values)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStructSpanTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledStructRefStructOriginal.Observe(
                ref target,
                values);
        }

        return ((delegate* managed<
            ref ProfiledStructRefStructTarget,
            Span<int>,
            int>)entryPoint)(ref target, values);
    }
}

internal static class ProfiledStructBorrowedWindowCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledStructWindow Selected(
        scoped ref ProfiledStructRefStructTarget target,
        int[] owner) =>
        target.Window(owner);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe ProfiledStructWindow RoutedTemplate(
        ref ProfiledStructRefStructTarget target,
        int[] owner)
    {
        var route =
            ProfiledReceiverFreeRouteState<
                ProfiledStructBorrowedWindowTag>.Pointer;
        return route == 0
            ? target.Window(owner)
            : ((delegate* managed<
                ref ProfiledStructRefStructTarget,
                int[],
                ProfiledStructWindow>)route)(ref target, owner);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe ProfiledStructWindow Invoke(
        ref ProfiledStructRefStructTarget target,
        int[] owner)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStructBorrowedWindowTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledStructRefStructOriginal.Window(
                ref target,
                owner);
        }

        return ((delegate* managed<
            ref ProfiledStructRefStructTarget,
            int[],
            ProfiledStructWindow>)entryPoint)(ref target, owner);
    }
}

internal sealed class ProfiledStructSpanTag;
internal sealed class ProfiledStructBorrowedWindowTag;
