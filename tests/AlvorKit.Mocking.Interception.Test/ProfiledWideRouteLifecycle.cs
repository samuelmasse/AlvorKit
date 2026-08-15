namespace AlvorKit;

/// <summary>Coordinates the exact caller for the 48-parameter concrete row.</summary>
internal sealed class ProfiledWideRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string RouteId =
        "ProfiledWideCaller.Selected::ProfiledWideTarget.Wide";
    private readonly IProfiledOwnedCallerRoute route;

    /// <summary>Creates the exact wide route over the startup profiler.</summary>
    internal ProfiledWideRouteLifecycle(IInterceptionBackend profiler)
    {
        var selected = Caller(nameof(ProfiledWideCaller.Selected));
        var operation = typeof(ProfiledWideTarget).GetMethod(
            nameof(ProfiledWideTarget.Wide))!;
        route = new ProfiledOwnedCallerRoute<ProfiledWideOperation>(
            profiler,
            selected,
            Caller(nameof(ProfiledWideCaller.RoutedTemplate)),
            operation,
            new ProfiledWideOperation(ProfiledWideOriginal.Invoke),
            wrapper => new ProfiledWideHandler(wrapper),
            ProfiledWideCaller.Bind,
            ProfiledWideCaller.Clear,
            ProfiledWideCaller.Publish,
            ProfiledWideCaller.FunctionPointer,
            DriveCaller);
    }

    /// <summary>Gets whether the exact caller reached active inert preparation.</summary>
    internal bool IsPrepared =>
        route.PreparationCompletion?.State == InterceptionState.Active;

    /// <summary>Gets whether the exact caller was restored during rollback.</summary>
    internal bool IsRemoved =>
        route.RemovalCompletion?.State == InterceptionState.Removed;

    /// <summary>Creates the stable coordinator route for this scenario.</summary>
    internal static MockInterceptionRoute CreateRoute() => new(RouteId);

    /// <summary>Prepares the exact wide route.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value) =>
        route.Prepare(value);

    /// <summary>Publishes the prepared route behind the coordinator gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value) =>
        route.Activate(value);

    /// <summary>Restores the original wide caller.</summary>
    public void Rollback(MockInterceptionRoute value) =>
        route.Rollback(value);

    private static void DriveCaller() =>
        _ = ProfiledWideCaller.Selected(
            new ProfiledWideTarget(),
            Sequence(100),
            Sequence(200),
            Sequence(300));

    private static int[] Sequence(int start) =>
        [.. Enumerable.Range(start, 16)];

    private static MethodInfo Caller(string name) =>
        typeof(ProfiledWideCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
