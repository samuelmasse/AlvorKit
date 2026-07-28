namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates the exact caller for concurrent receiver isolation.</summary>
internal sealed class ProfiledInstanceIsolationRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string RouteId =
        "ProfiledInstanceIsolationCaller.Selected::ProfiledInstanceIsolationTarget.Add";

    private readonly IProfiledOwnedCallerRoute owned;

    /// <summary>Creates the exact route owner over the checked-in profiler.</summary>
    internal ProfiledInstanceIsolationRouteLifecycle(
        IInterceptionBackend profiler)
    {
        var selected = Caller(
            nameof(ProfiledInstanceIsolationCaller.Selected));
        var operation = typeof(ProfiledInstanceIsolationTarget).GetMethod(
            nameof(ProfiledInstanceIsolationTarget.Add),
            [typeof(int), typeof(int)])!;
        owned =
            new ProfiledOwnedCallerRoute<
                ProfiledInstanceIsolationOperation>(
                profiler,
                selected,
                Caller(nameof(
                    ProfiledInstanceIsolationCaller.RoutedTemplate)),
                operation,
                new ProfiledInstanceIsolationOperation(
                    ProfiledInstanceIsolationOriginal.Add),
                wrapper =>
                    new ProfiledInstanceIsolationHandler(wrapper),
                ProfiledInstanceIsolationCaller.Bind,
                ProfiledInstanceIsolationCaller.Clear,
                ProfiledInstanceIsolationCaller.Publish,
                ProfiledInstanceIsolationCaller.FunctionPointer,
                () => _ = ProfiledInstanceIsolationCaller.Selected(
                    new ProfiledInstanceIsolationTarget(),
                    1,
                    2));
    }

    /// <summary>Gets whether the exact caller reached active inert preparation.</summary>
    internal bool AllPrepared =>
        owned.PreparationCompletion?.State ==
        InterceptionState.Active;

    /// <summary>Gets whether the exact caller was restored during rollback.</summary>
    internal bool AllRemoved =>
        owned.RemovalCompletion?.State ==
        InterceptionState.Removed;

    /// <summary>Creates the stable coordinator route for this scenario.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(RouteId),
    ];

    /// <summary>Prepares the exact route selected by its stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes the prepared exact route behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores the exact caller during coordinator rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledOwnedCallerRoute Resolve(
        MockInterceptionRoute route) =>
        route.Id == RouteId
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected instance-isolation route '{route.Id}'.");

    private static MethodInfo Caller(string name) =>
        typeof(ProfiledInstanceIsolationCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
