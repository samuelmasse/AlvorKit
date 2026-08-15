namespace AlvorKit;

/// <summary>Owns the exact static-field write site for the transform row.</summary>
internal static class ProfiledStaticFieldTransformWriteCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(int value) =>
        ProfiledReceiverFreeTarget.StaticField = value;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(int value)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledStaticFieldTransformWriteTag>.Pointer;
        if (route == 0)
        {
            ProfiledReceiverFreeTarget.StaticField = value;
            return;
        }

        ((delegate* managed<int, void>)route)(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformWriteTag>
            .TryAcquire(out var entryPoint))
        {
            ProfiledReceiverFreeOriginal.WriteStaticField(value);
            return;
        }

        ((delegate* managed<int, void>)entryPoint)(value);
    }
}

/// <summary>Owns the exact static-field read site for the transform row.</summary>
internal static class ProfiledStaticFieldTransformReadCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected() =>
        ProfiledReceiverFreeTarget.StaticField;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate()
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledStaticFieldTransformReadTag>.Pointer;
        return route == 0
            ? ProfiledReceiverFreeTarget.StaticField
            : ((delegate* managed<int>)route)();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke()
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformReadTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.ReadStaticField();
        }

        return ((delegate* managed<int>)entryPoint)();
    }
}

internal sealed class ProfiledStaticFieldTransformWriteTag;
internal sealed class ProfiledStaticFieldTransformReadTag;
