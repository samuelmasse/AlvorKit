namespace AlvorKit.Mocking.Interception.Test;

internal static class ProfiledStaticTransformCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(int value) =>
        ProfiledReceiverFreeTarget.Transform(value);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(int value)
    {
        var route =
            ProfiledReceiverFreeRouteState<ProfiledTransformTag>.Pointer;
        return route == 0
            ? ProfiledReceiverFreeTarget.Transform(value)
            : ((delegate* managed<int, int>)route)(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<ProfiledTransformTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.Transform(value);
        }

        return ((delegate* managed<int, int>)entryPoint)(value);
    }
}

internal static class ProfiledGenericStaticCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Selected<T>(T value)
        where T : notnull =>
        ProfiledReceiverFreeTarget.Identity(value);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe T RoutedTemplate<T>(T value)
        where T : notnull
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledIdentityTag<T>>.Pointer;
        return route == 0
            ? ProfiledReceiverFreeTarget.Identity(value)
            : ((delegate* managed<T, T>)route)(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string InvokeString(string value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledIdentityTag<string>>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.Identity(value);
        }

        return ((delegate* managed<string, string>)entryPoint)(value);
    }
}

internal static class ProfiledSetStaticNumberCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(int value) =>
        ProfiledReceiverFreeTarget.StaticNumber = value;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(int value)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledSetStaticNumberTag>.Pointer;
        if (route == 0)
        {
            ProfiledReceiverFreeTarget.StaticNumber = value;
            return;
        }

        ((delegate* managed<int, void>)route)(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledSetStaticNumberTag>
            .TryAcquire(out var entryPoint))
        {
            ProfiledReceiverFreeOriginal.SetStaticNumber(value);
            return;
        }

        ((delegate* managed<int, void>)entryPoint)(value);
    }
}

internal static class ProfiledGetStaticNumberCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected() =>
        ProfiledReceiverFreeTarget.StaticNumber;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate()
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledGetStaticNumberTag>.Pointer;
        return route == 0
            ? ProfiledReceiverFreeTarget.StaticNumber
            : ((delegate* managed<int>)route)();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke()
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledGetStaticNumberTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.GetStaticNumber();
        }

        return ((delegate* managed<int>)entryPoint)();
    }
}

internal static class ProfiledWriteStaticFieldCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Selected(int value) =>
        ProfiledReceiverFreeTarget.StaticField = value;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe void RoutedTemplate(int value)
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledWriteStaticFieldTag>.Pointer;
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
                ProfiledWriteStaticFieldTag>
            .TryAcquire(out var entryPoint))
        {
            ProfiledReceiverFreeOriginal.WriteStaticField(value);
            return;
        }

        ((delegate* managed<int, void>)entryPoint)(value);
    }
}

internal static class ProfiledReadStaticFieldCaller
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected() =>
        ProfiledReceiverFreeTarget.StaticField;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate()
    {
        var route = ProfiledReceiverFreeRouteState<
            ProfiledReadStaticFieldTag>.Pointer;
        return route == 0
            ? ProfiledReceiverFreeTarget.StaticField
            : ((delegate* managed<int>)route)();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int Invoke()
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledReadStaticFieldTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.ReadStaticField();
        }

        return ((delegate* managed<int>)entryPoint)();
    }
}
