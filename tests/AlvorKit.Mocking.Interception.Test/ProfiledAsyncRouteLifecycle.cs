namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates the exact caller for the concrete asynchronous behavior row.</summary>
internal sealed class ProfiledAsyncRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string RouteId =
        "ProfiledAsyncCaller.Selected::ProfiledAsyncTarget.AddAsync";
    private readonly IProfiledOwnedCallerRoute route;

    /// <summary>Creates the exact asynchronous route over the startup profiler.</summary>
    internal ProfiledAsyncRouteLifecycle(IInterceptionBackend profiler)
    {
        var selected = Caller(nameof(ProfiledAsyncCaller.Selected));
        var operation = typeof(ProfiledAsyncTarget).GetMethod(
            nameof(ProfiledAsyncTarget.AddAsync),
            [typeof(int)])!;
        route = new ProfiledOwnedCallerRoute<ProfiledAsyncOperation>(
            profiler,
            selected,
            Caller(nameof(ProfiledAsyncCaller.RoutedTemplate)),
            operation,
            new ProfiledAsyncOperation(ProfiledAsyncOriginal.Invoke),
            wrapper => new ProfiledAsyncHandler(wrapper),
            ProfiledAsyncCaller.Bind,
            ProfiledAsyncCaller.Clear,
            ProfiledAsyncCaller.Publish,
            ProfiledAsyncCaller.FunctionPointer,
            () => _ = ProfiledAsyncCaller.Selected(
                    new ProfiledAsyncTarget(),
                    1)
                .GetAwaiter()
                .GetResult());
    }

    /// <summary>Gets whether the exact caller reached active inert preparation.</summary>
    internal bool IsPrepared =>
        route.PreparationCompletion?.State == InterceptionState.Active;

    /// <summary>Gets whether the exact caller was restored during rollback.</summary>
    internal bool IsRemoved =>
        route.RemovalCompletion?.State == InterceptionState.Removed;

    /// <summary>Creates the stable coordinator route for this scenario.</summary>
    internal static MockInterceptionRoute CreateRoute() => new(RouteId);

    /// <summary>Prepares the exact asynchronous route.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value) =>
        route.Prepare(value);

    /// <summary>Publishes the prepared route behind the coordinator gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value) =>
        route.Activate(value);

    /// <summary>Restores the original asynchronous caller.</summary>
    public void Rollback(MockInterceptionRoute value) =>
        route.Rollback(value);

    private static MethodInfo Caller(string name) =>
        typeof(ProfiledAsyncCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
