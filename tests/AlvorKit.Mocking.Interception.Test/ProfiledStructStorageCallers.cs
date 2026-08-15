namespace AlvorKit;

internal static class ProfiledStructFieldAddCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledStructStorage storage,
        int amount) =>
        storage.Target.Add(amount);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledStructStorage storage,
        int amount)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledStructFieldAddTag>.Pointer;
        return route == 0
            ? storage.Target.Add(amount)
            : ((delegate* managed<
                ref ProfiledMutableStructTarget,
                int,
                int>)route)(ref storage.Target, amount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ref ProfiledMutableStructTarget target,
        int amount)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStructFieldAddTag>
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

internal static class ProfiledStructStaticFieldAddCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(int amount) =>
        ProfiledStructStorage.StaticTarget.Add(amount);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(int amount)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledStructStaticFieldAddTag>.Pointer;
        return route == 0
            ? ProfiledStructStorage.StaticTarget.Add(amount)
            : ((delegate* managed<
                ref ProfiledMutableStructTarget,
                int,
                int>)route)(
                    ref ProfiledStructStorage.StaticTarget,
                    amount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ref ProfiledMutableStructTarget target,
        int amount)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStructStaticFieldAddTag>
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

internal static class ProfiledStructArrayAddCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledMutableStructTarget[] targets,
        int index,
        int amount) =>
        targets[index].Add(amount);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledMutableStructTarget[] targets,
        int index,
        int amount)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledStructArrayAddTag>.Pointer;
        return route == 0
            ? targets[index].Add(amount)
            : ((delegate* managed<
                ref ProfiledMutableStructTarget,
                int,
                int>)route)(ref targets[index], amount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(
        ref ProfiledMutableStructTarget target,
        int amount)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStructArrayAddTag>
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

internal sealed class ProfiledStructFieldAddTag;
internal sealed class ProfiledStructStaticFieldAddTag;
internal sealed class ProfiledStructArrayAddTag;
