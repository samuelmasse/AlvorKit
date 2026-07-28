namespace AlvorKit.Mocking.Interception.Test;

internal static class ProfiledReadInstanceFieldCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(ProfiledReceiverFreeTarget target) =>
        target.InstanceField;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledReceiverFreeTarget target)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledReadInstanceFieldTag>.Pointer;
        return route == 0
            ? target.InstanceField
            : ((delegate* managed<
                ProfiledReceiverFreeTarget,
                int>)route)(target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(ProfiledReceiverFreeTarget target)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceFieldTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.ReadInstanceField(target);
        }

        return ((delegate* managed<
            ProfiledReceiverFreeTarget,
            int>)entryPoint)(target);
    }
}

internal static class ProfiledWriteInstanceFieldCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(
        ProfiledReceiverFreeTarget target,
        int value) =>
        target.InstanceField = value;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(
        ProfiledReceiverFreeTarget target,
        int value)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledWriteInstanceFieldTag>.Pointer;
        if (route == 0)
        {
            target.InstanceField = value;
            return;
        }

        ((delegate* managed<
            ProfiledReceiverFreeTarget,
            int,
            void>)route)(target, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void Invoke(
        ProfiledReceiverFreeTarget target,
        int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceFieldTag>
            .TryAcquire(out var entryPoint))
        {
            ProfiledReceiverFreeOriginal.WriteInstanceField(
                target,
                value);
            return;
        }

        ((delegate* managed<
            ProfiledReceiverFreeTarget,
            int,
            void>)entryPoint)(target, value);
    }
}

internal static class ProfiledReadInstanceReferenceFieldCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string? Selected(
        ProfiledReceiverFreeTarget target) =>
        target.InstanceReferenceField;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe string? RoutedTemplate(
        ProfiledReceiverFreeTarget target)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledReadInstanceReferenceFieldTag>.Pointer;
        return route == 0
            ? target.InstanceReferenceField
            : ((delegate* managed<
                ProfiledReceiverFreeTarget,
                string?>)route)(target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string? Invoke(
        ProfiledReceiverFreeTarget target)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceReferenceFieldTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal
                .ReadInstanceReferenceField(target);
        }

        return ((delegate* managed<
            ProfiledReceiverFreeTarget,
            string?>)entryPoint)(target);
    }
}

internal static class ProfiledWriteInstanceReferenceFieldCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(
        ProfiledReceiverFreeTarget target,
        string? value) =>
        target.InstanceReferenceField = value;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(
        ProfiledReceiverFreeTarget target,
        string? value)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledWriteInstanceReferenceFieldTag>.Pointer;
        if (route == 0)
        {
            target.InstanceReferenceField = value;
            return;
        }

        ((delegate* managed<
            ProfiledReceiverFreeTarget,
            string?,
            void>)route)(target, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void Invoke(
        ProfiledReceiverFreeTarget target,
        string? value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceReferenceFieldTag>
            .TryAcquire(out var entryPoint))
        {
            ProfiledReceiverFreeOriginal.WriteInstanceReferenceField(
                target,
                value);
            return;
        }

        ((delegate* managed<
            ProfiledReceiverFreeTarget,
            string?,
            void>)entryPoint)(target, value);
    }
}
