namespace AlvorKit;

internal sealed partial class ProfiledStructRouteLifecycle
{
    private static IProfiledReceiverFreeCallerRoute FieldRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructFieldAddCaller);
        return Writable<
            ProfiledMutableStructTarget,
            ProfiledStructFieldAddTag>(
            profiler,
            Caller(
                callerType,
                nameof(ProfiledStructFieldAddCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledStructFieldAddCaller.RoutedTemplate)),
            AddOperation(),
            new(ProfiledStructOriginal.Add),
            callerType,
            "Invoke",
            DriveField);
    }

    private static IProfiledReceiverFreeCallerRoute StaticFieldRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructStaticFieldAddCaller);
        return Writable<
            ProfiledMutableStructTarget,
            ProfiledStructStaticFieldAddTag>(
            profiler,
            Caller(
                callerType,
                nameof(ProfiledStructStaticFieldAddCaller.Selected)),
            Caller(
                callerType,
                nameof(
                    ProfiledStructStaticFieldAddCaller
                        .RoutedTemplate)),
            AddOperation(),
            new(ProfiledStructOriginal.Add),
            callerType,
            "Invoke",
            DriveStaticField);
    }

    private static IProfiledReceiverFreeCallerRoute ArrayRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructArrayAddCaller);
        return Writable<
            ProfiledMutableStructTarget,
            ProfiledStructArrayAddTag>(
            profiler,
            Caller(
                callerType,
                nameof(ProfiledStructArrayAddCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledStructArrayAddCaller.RoutedTemplate)),
            AddOperation(),
            new(ProfiledStructOriginal.Add),
            callerType,
            "Invoke",
            DriveArray);
    }

    private static IProfiledReceiverFreeCallerRoute ConstrainedRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructConstrainedCaller);
        return Writable<
            ProfiledMutableStructTarget,
            ProfiledStructConstrainedTag<
                ProfiledMutableStructTarget>>(
            profiler,
            ClosedCaller(
                callerType,
                nameof(ProfiledStructConstrainedCaller.Selected)),
            ClosedCaller(
                callerType,
                nameof(
                    ProfiledStructConstrainedCaller.RoutedTemplate)),
            Operation(
                typeof(IProfiledStructMetric),
                nameof(IProfiledStructMetric.Measure)),
            new(ProfiledStructOriginal.Constrained),
            callerType,
            "InvokeMutable",
            DriveConstrained);
    }

    private static MethodInfo AddOperation() =>
        Operation(
            typeof(ProfiledMutableStructTarget),
            nameof(ProfiledMutableStructTarget.Add));
}
