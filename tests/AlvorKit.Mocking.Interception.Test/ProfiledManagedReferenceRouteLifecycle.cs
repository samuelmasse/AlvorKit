namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates mutable and readonly managed-reference caller routes.</summary>
internal sealed class ProfiledManagedReferenceRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string MutableRouteId =
        "ProfiledMutableReferenceCaller.Selected::ProfiledManagedReferenceTarget.Mutable";
    private const string ReadOnlyRouteId =
        "ProfiledReadOnlyReferenceCaller.Selected::ProfiledManagedReferenceTarget.ReadOnly";
    private readonly Dictionary<string, IProfiledOwnedCallerRoute> routes;

    /// <summary>Creates both exact managed-reference routes over the real profiler.</summary>
    internal ProfiledManagedReferenceRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [MutableRouteId] = MutableRoute(profiler),
            [ReadOnlyRouteId] = ReadOnlyRoute(profiler),
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
        new(MutableRouteId),
        new(ReadOnlyRouteId),
    ];

    /// <summary>Prepares one exact managed-reference route.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one exact managed-reference route.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one managed-reference caller.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledOwnedCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected managed-reference route '{route.Id}'.");

    private static IProfiledOwnedCallerRoute MutableRoute(
        IInterceptionBackend profiler) =>
        new ProfiledOwnedCallerRoute<
            ProfiledMutableReferenceOperation>(
            profiler,
            Caller(
                typeof(ProfiledMutableReferenceCaller),
                nameof(ProfiledMutableReferenceCaller.Selected)),
            Caller(
                typeof(ProfiledMutableReferenceCaller),
                nameof(ProfiledMutableReferenceCaller.RoutedTemplate)),
            typeof(ProfiledManagedReferenceTarget).GetMethod(
                nameof(ProfiledManagedReferenceTarget.Mutable))!,
            new ProfiledMutableReferenceOperation(
                ProfiledManagedReferenceOriginal.Mutable),
            wrapper => new ProfiledMutableReferenceHandler(wrapper),
            ProfiledMutableReferenceCaller.Bind,
            ProfiledMutableReferenceCaller.Clear,
            ProfiledMutableReferenceCaller.Publish,
            ProfiledMutableReferenceCaller.FunctionPointer,
            DriveMutableCaller);

    private static IProfiledOwnedCallerRoute ReadOnlyRoute(
        IInterceptionBackend profiler) =>
        new ProfiledOwnedCallerRoute<
            ProfiledReadOnlyReferenceOperation>(
            profiler,
            Caller(
                typeof(ProfiledReadOnlyReferenceCaller),
                nameof(ProfiledReadOnlyReferenceCaller.Selected)),
            Caller(
                typeof(ProfiledReadOnlyReferenceCaller),
                nameof(ProfiledReadOnlyReferenceCaller.RoutedTemplate)),
            typeof(ProfiledManagedReferenceTarget).GetMethod(
                nameof(ProfiledManagedReferenceTarget.ReadOnly))!,
            new ProfiledReadOnlyReferenceOperation(
                ProfiledManagedReferenceOriginal.ReadOnly),
            wrapper => new ProfiledReadOnlyReferenceHandler(wrapper),
            ProfiledReadOnlyReferenceCaller.Bind,
            ProfiledReadOnlyReferenceCaller.Clear,
            ProfiledReadOnlyReferenceCaller.Publish,
            ProfiledReadOnlyReferenceCaller.FunctionPointer,
            DriveReadOnlyCaller);

    private static void DriveMutableCaller()
    {
        ref int alias = ref ProfiledMutableReferenceCaller.Selected(
            new ProfiledManagedReferenceTarget());
        _ = alias;
    }

    private static void DriveReadOnlyCaller()
    {
        ref readonly int alias =
            ref ProfiledReadOnlyReferenceCaller.Selected(
                new ProfiledManagedReferenceTarget());
        _ = alias;
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
