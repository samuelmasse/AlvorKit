namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the exact reference-field write site for the typed row.</summary>
internal static class ProfiledReferenceFieldTransformWriteCaller
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
            ProfiledReferenceFieldTransformWriteTag>.Pointer;
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
                ProfiledReferenceFieldTransformWriteTag>
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

/// <summary>Owns the exact reference-field read site for the typed row.</summary>
internal static class ProfiledReferenceFieldTransformReadCaller
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
            ProfiledReferenceFieldTransformReadTag>.Pointer;
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
                ProfiledReferenceFieldTransformReadTag>
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

internal sealed class ProfiledReferenceFieldTransformWriteTag;
internal sealed class ProfiledReferenceFieldTransformReadTag;
