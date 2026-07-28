namespace AlvorKit.Mocking.Interception.Test;

internal static class ProfiledStructAddCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ref ProfiledMutableStructTarget target,
        int amount) =>
        target.Add(amount);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ref ProfiledMutableStructTarget target,
        int amount)
    {
        var route =
            ProfiledReceiverFreeRouteState<ProfiledStructAddTag>.Pointer;
        return route == 0
            ? target.Add(amount)
            : ((delegate* managed<
                ref ProfiledMutableStructTarget,
                int,
                int>)route)(ref target, amount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ref ProfiledMutableStructTarget target,
        int amount)
    {
        if (!ProfiledReceiverFreeRouteState<ProfiledStructAddTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledStructOriginal.Add(ref target, amount);
        }

        return ((delegate* managed<
            ref ProfiledMutableStructTarget,
            int,
            int>)entryPoint)(ref target, amount);
    }
}

internal static class ProfiledStructReadCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        in ProfiledReadonlyStructTarget target,
        int amount) =>
        target.Read(amount);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        in ProfiledReadonlyStructTarget target,
        int amount)
    {
        var route =
            ProfiledReceiverFreeRouteState<ProfiledStructReadTag>.Pointer;
        return route == 0
            ? target.Read(amount)
            : ((delegate* managed<
                in ProfiledReadonlyStructTarget,
                int,
                int>)route)(in target, amount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        in ProfiledReadonlyStructTarget target,
        int amount)
    {
        if (!ProfiledReceiverFreeRouteState<ProfiledStructReadTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledStructOriginal.Read(in target, amount);
        }

        return ((delegate* managed<
            in ProfiledReadonlyStructTarget,
            int,
            int>)entryPoint)(in target, amount);
    }
}

internal static class ProfiledRecordStructReadCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ref ProfiledRecordStructTarget target,
        int amount) =>
        target.Read(amount);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ref ProfiledRecordStructTarget target,
        int amount)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledRecordStructReadTag>.Pointer;
        return route == 0
            ? target.Read(amount)
            : ((delegate* managed<
                ref ProfiledRecordStructTarget,
                int,
                int>)route)(ref target, amount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ref ProfiledRecordStructTarget target,
        int amount)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledRecordStructReadTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledStructOriginal.ReadRecord(
                ref target,
                amount);
        }

        return ((delegate* managed<
            ref ProfiledRecordStructTarget,
            int,
            int>)entryPoint)(ref target, amount);
    }
}

internal static class ProfiledStructWindowCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledStructWindow Selected(
        scoped ref ProfiledMutableStructTarget target,
        int[] owner) =>
        target.Window(owner);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe ProfiledStructWindow RoutedTemplate(
        ref ProfiledMutableStructTarget target,
        int[] owner)
    {
        var route =
            ProfiledReceiverFreeRouteState<ProfiledStructWindowTag>.Pointer;
        return route == 0
            ? target.Window(owner)
            : ((delegate* managed<
                ref ProfiledMutableStructTarget,
                int[],
                ProfiledStructWindow>)route)(ref target, owner);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe ProfiledStructWindow Invoke(
        ref ProfiledMutableStructTarget target,
        int[] owner)
    {
        if (!ProfiledReceiverFreeRouteState<ProfiledStructWindowTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledStructOriginal.Window(ref target, owner);
        }

        return ((delegate* managed<
            ref ProfiledMutableStructTarget,
            int[],
            ProfiledStructWindow>)entryPoint)(ref target, owner);
    }
}

internal sealed class ProfiledStructAddTag;
internal sealed class ProfiledStructReadTag;
internal sealed class ProfiledRecordStructReadTag;
internal sealed class ProfiledStructWindowTag;
