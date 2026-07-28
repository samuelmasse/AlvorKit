namespace AlvorKit.Mocking.Interception.Test;

internal sealed partial class ProfiledStructRouteLifecycle
{
    private static IProfiledReceiverFreeCallerRoute AddRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructAddCaller);
        return Writable<
            ProfiledMutableStructTarget,
            ProfiledStructAddTag>(
            profiler,
            Caller(callerType, nameof(ProfiledStructAddCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledStructAddCaller.RoutedTemplate)),
            Operation(
                typeof(ProfiledMutableStructTarget),
                nameof(ProfiledMutableStructTarget.Add)),
            new(ProfiledStructOriginal.Add),
            callerType,
            "Invoke",
            DriveAdd);
    }

    private static IProfiledReceiverFreeCallerRoute ReadRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructReadCaller);
        return new ProfiledStructCallerRoute<
            ProfiledStructReadOnlyInt32Operation<
                ProfiledReadonlyStructTarget>>(
            profiler,
            Caller(callerType, nameof(ProfiledStructReadCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledStructReadCaller.RoutedTemplate)),
            Operation(
                typeof(ProfiledReadonlyStructTarget),
                nameof(ProfiledReadonlyStructTarget.Read)),
            typeof(ProfiledReadonlyStructTarget),
            InterceptionReceiverOwnership.ReadOnlyManagedReference,
            new(ProfiledStructOriginal.Read),
            wrapper =>
                new ProfiledStructReadOnlyInt32Handler<
                    ProfiledReadonlyStructTarget>(wrapper),
            ProfiledReceiverFreeRouteState<ProfiledStructReadTag>.Bind,
            ProfiledReceiverFreeRouteState<ProfiledStructReadTag>.Clear,
            ProfiledReceiverFreeRouteState<ProfiledStructReadTag>.Publish,
            () => Pointer(callerType, "Invoke"),
            DriveRead);
    }

    private static IProfiledReceiverFreeCallerRoute RecordRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledRecordStructReadCaller);
        return Writable<
            ProfiledRecordStructTarget,
            ProfiledRecordStructReadTag>(
            profiler,
            Caller(
                callerType,
                nameof(ProfiledRecordStructReadCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledRecordStructReadCaller.RoutedTemplate)),
            Operation(
                typeof(ProfiledRecordStructTarget),
                nameof(ProfiledRecordStructTarget.Read)),
            new(ProfiledStructOriginal.ReadRecord),
            callerType,
            "Invoke",
            DriveRecord);
    }

    private static IProfiledReceiverFreeCallerRoute WindowRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructWindowCaller);
        return new ProfiledStructCallerRoute<
            ProfiledStructWindowOperation>(
            profiler,
            Caller(callerType, nameof(ProfiledStructWindowCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledStructWindowCaller.RoutedTemplate)),
            Operation(
                typeof(ProfiledMutableStructTarget),
                nameof(ProfiledMutableStructTarget.Window)),
            typeof(ProfiledMutableStructTarget),
            InterceptionReceiverOwnership.ManagedReference,
            new(ProfiledStructOriginal.Window),
            wrapper => new ProfiledStructWindowHandler(wrapper),
            ProfiledReceiverFreeRouteState<ProfiledStructWindowTag>.Bind,
            ProfiledReceiverFreeRouteState<ProfiledStructWindowTag>.Clear,
            ProfiledReceiverFreeRouteState<ProfiledStructWindowTag>.Publish,
            () => Pointer(callerType, "Invoke"),
            DriveWindow);
    }
}
