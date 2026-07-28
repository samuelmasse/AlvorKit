namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates ref-struct input and return caller routes.</summary>
internal sealed class ProfiledRefStructRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string ObserveRouteId =
        "ProfiledObserveCaller.Selected::ProfiledRefStructTarget.Observe";
    private const string WindowRouteId =
        "ProfiledWindowCaller.Selected::ProfiledRefStructTarget.Window";
    private readonly Dictionary<string, IProfiledOwnedCallerRoute> routes;

    /// <summary>Creates both exact ref-struct routes over the real profiler.</summary>
    internal ProfiledRefStructRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [ObserveRouteId] = ObserveRoute(profiler),
            [WindowRouteId] = WindowRoute(profiler),
        };
    }

    /// <summary>Gets whether both callers reached active inert preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether both original callers were restored.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates both stable coordinator routes.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(ObserveRouteId),
        new(WindowRouteId),
    ];

    /// <summary>Prepares one exact ref-struct route.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one exact ref-struct route.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one ref-struct caller.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledOwnedCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected ref-struct route '{route.Id}'.");

    private static IProfiledOwnedCallerRoute ObserveRoute(
        IInterceptionBackend profiler)
    {
        var selected = Caller(
            typeof(ProfiledObserveCaller),
            nameof(ProfiledObserveCaller.Selected));
        var operation = typeof(ProfiledRefStructTarget).GetMethod(
            nameof(ProfiledRefStructTarget.Observe))!;
        return new ProfiledOwnedCallerRoute<ProfiledObserveOperation>(
            profiler,
            selected,
            Caller(
                typeof(ProfiledObserveCaller),
                nameof(ProfiledObserveCaller.RoutedTemplate)),
            operation,
            new ProfiledObserveOperation(
                ProfiledRefStructOriginal.Observe),
            wrapper => new ProfiledObserveHandler(wrapper),
            ProfiledObserveCaller.Bind,
            ProfiledObserveCaller.Clear,
            ProfiledObserveCaller.Publish,
            ProfiledObserveCaller.FunctionPointer,
            DriveObserveCaller);
    }

    private static IProfiledOwnedCallerRoute WindowRoute(
        IInterceptionBackend profiler)
    {
        var selected = Caller(
            typeof(ProfiledWindowCaller),
            nameof(ProfiledWindowCaller.Selected));
        return new ProfiledOwnedCallerRoute<
            ProfiledWindowOperation>(
            profiler,
            selected,
            Caller(
                typeof(ProfiledWindowCaller),
                nameof(ProfiledWindowCaller.RoutedTemplate)),
            typeof(ProfiledRefStructTarget).GetMethod(
                nameof(ProfiledRefStructTarget.Window))!,
            new ProfiledWindowOperation(
                ProfiledRefStructOriginal.Window),
            wrapper => new ProfiledWindowHandler(wrapper),
            ProfiledWindowCaller.Bind,
            ProfiledWindowCaller.Clear,
            ProfiledWindowCaller.Publish,
            ProfiledWindowCaller.FunctionPointer,
            DriveWindowCaller);
    }

    private static void DriveObserveCaller() =>
        _ = ProfiledObserveCaller.Selected(
            new ProfiledRefStructTarget(),
            new([1]));

    private static void DriveWindowCaller()
    {
        ProfiledWindow window = ProfiledWindowCaller.Selected(
            new ProfiledRefStructTarget());
        _ = window.Values.Length;
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
