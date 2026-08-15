namespace AlvorKit;

/// <summary>Coordinates the span-input and borrowed-window struct routes.</summary>
internal sealed class ProfiledStructRefStructRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string ObserveId = "StructRefStructObserve";
    private const string WindowId = "StructRefStructWindow";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    internal ProfiledStructRefStructRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [ObserveId] = ObserveRoute(profiler),
            [WindowId] = WindowRoute(profiler),
        };
    }

    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    internal bool WrapperEntriesAreExact =>
        routes[ObserveId].HandlerInvocations == 6 &&
        routes[WindowId].HandlerInvocations == 10;

    internal string WrapperEntryCounts =>
        $"observe={routes[ObserveId].HandlerInvocations}, " +
        $"window={routes[WindowId].HandlerInvocations}";

    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(ObserveId),
        new(WindowId),
    ];

    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected struct/ref-struct route '{route.Id}'.");

    private static IProfiledReceiverFreeCallerRoute ObserveRoute(
        IInterceptionBackend profiler)
    {
        Type callerType = typeof(ProfiledStructSpanCaller);
        return new ProfiledStructCallerRoute<
            ProfiledStructSpanOperation>(
            profiler,
            Caller(callerType, nameof(ProfiledStructSpanCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledStructSpanCaller.RoutedTemplate)),
            Operation(nameof(ProfiledStructRefStructTarget.Observe)),
            typeof(ProfiledStructRefStructTarget),
            InterceptionReceiverOwnership.ManagedReference,
            new(ProfiledStructRefStructOriginal.Observe),
            wrapper => new ProfiledStructSpanHandler(wrapper),
            ProfiledReceiverFreeRouteState<
                ProfiledStructSpanTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledStructSpanTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledStructSpanTag>.Publish,
            () => Pointer(callerType),
            DriveObserve);
    }

    private static IProfiledReceiverFreeCallerRoute WindowRoute(
        IInterceptionBackend profiler)
    {
        Type callerType =
            typeof(ProfiledStructBorrowedWindowCaller);
        return new ProfiledStructCallerRoute<
            ProfiledStructBorrowedWindowOperation>(
            profiler,
            Caller(
                callerType,
                nameof(ProfiledStructBorrowedWindowCaller.Selected)),
            Caller(
                callerType,
                nameof(ProfiledStructBorrowedWindowCaller.RoutedTemplate)),
            Operation(nameof(ProfiledStructRefStructTarget.Window)),
            typeof(ProfiledStructRefStructTarget),
            InterceptionReceiverOwnership.ManagedReference,
            new(ProfiledStructRefStructOriginal.Window),
            wrapper =>
                new ProfiledStructBorrowedWindowHandler(wrapper),
            ProfiledReceiverFreeRouteState<
                ProfiledStructBorrowedWindowTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledStructBorrowedWindowTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledStructBorrowedWindowTag>.Publish,
            () => Pointer(callerType),
            DriveWindow);
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo Operation(string name) =>
        typeof(ProfiledStructRefStructTarget).GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Instance)!;

    private static nint Pointer(Type callerType) =>
        ProfiledGenericFunctionPointer.Get(callerType, "Invoke");

    private static void DriveObserve()
    {
        var target = new ProfiledStructRefStructTarget();
        Span<int> values = stackalloc int[1];
        _ = ProfiledStructSpanCaller.Selected(
            ref target,
            values);
    }

    private static void DriveWindow()
    {
        var target = new ProfiledStructRefStructTarget();
        _ = ProfiledStructBorrowedWindowCaller.Selected(
            ref target,
            []);
    }
}
