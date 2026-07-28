namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates the four exact callers in the partial concrete behavior row.</summary>
internal sealed class ProfiledPartialRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    /// <summary>The stable addition route identity.</summary>
    internal const string AddRouteId =
        "ProfiledAddCaller.Selected::ProfiledPartialTarget.Add";

    /// <summary>The stable neighboring route identity.</summary>
    internal const string NeighborRouteId =
        "ProfiledNeighborCaller.Selected::ProfiledPartialTarget.Neighbor";

    /// <summary>The stable throwing route identity.</summary>
    internal const string ThrowRouteId =
        "ProfiledThrowCaller.Selected::ProfiledPartialTarget.ThrowOriginal";

    /// <summary>The stable ref/out route identity.</summary>
    internal const string MutateRouteId =
        "ProfiledMutateCaller.Selected::ProfiledPartialTarget.Mutate";

    private readonly Dictionary<string, IProfiledOwnedCallerRoute> routes;

    /// <summary>Creates all four exact route owners over the checked-in profiler.</summary>
    internal ProfiledPartialRouteLifecycle(IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [AddRouteId] = AddRoute(profiler),
            [NeighborRouteId] = NeighborRoute(profiler),
            [ThrowRouteId] = ThrowRoute(profiler),
            [MutateRouteId] = MutateRoute(profiler),
        };
    }

    /// <summary>Gets whether every exact caller reached active inert preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether every exact caller was restored during rollback.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates the stable coordinator routes for this scenario.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(AddRouteId),
        new(NeighborRouteId),
        new(ThrowRouteId),
        new(MutateRouteId),
    ];

    /// <summary>Prepares one exact route selected by its stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one prepared exact route behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one exact caller during reverse-order coordinator rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledOwnedCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected partial route '{route.Id}'.");

    private static ProfiledOwnedCallerRoute<ProfiledAddOperation>
        AddRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(typeof(ProfiledAddCaller), nameof(ProfiledAddCaller.Selected)),
            Caller(
                typeof(ProfiledAddCaller),
                nameof(ProfiledAddCaller.RoutedTemplate)),
            Operation(nameof(ProfiledPartialTarget.Add), typeof(int), typeof(int)),
            new ProfiledAddOperation(ProfiledPartialOriginal.Add),
            wrapper => new ProfiledAddHandler(wrapper),
            ProfiledAddCaller.Bind,
            ProfiledAddCaller.Clear,
            ProfiledAddCaller.Publish,
            ProfiledAddCaller.FunctionPointer,
            () => _ = ProfiledAddCaller.Selected(
                new ProfiledPartialTarget(),
                1,
                2));

    private static ProfiledOwnedCallerRoute<ProfiledNeighborOperation>
        NeighborRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledNeighborCaller),
                nameof(ProfiledNeighborCaller.Selected)),
            Caller(
                typeof(ProfiledNeighborCaller),
                nameof(ProfiledNeighborCaller.RoutedTemplate)),
            Operation(nameof(ProfiledPartialTarget.Neighbor), typeof(int)),
            new ProfiledNeighborOperation(ProfiledPartialOriginal.Neighbor),
            wrapper => new ProfiledNeighborHandler(wrapper),
            ProfiledNeighborCaller.Bind,
            ProfiledNeighborCaller.Clear,
            ProfiledNeighborCaller.Publish,
            ProfiledNeighborCaller.FunctionPointer,
            () => _ = ProfiledNeighborCaller.Selected(
                new ProfiledPartialTarget(),
                1));

    private static ProfiledOwnedCallerRoute<ProfiledThrowOperation>
        ThrowRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledThrowCaller),
                nameof(ProfiledThrowCaller.Selected)),
            Caller(
                typeof(ProfiledThrowCaller),
                nameof(ProfiledThrowCaller.RoutedTemplate)),
            Operation(nameof(ProfiledPartialTarget.ThrowOriginal)),
            new ProfiledThrowOperation(ProfiledPartialOriginal.Throw),
            wrapper => new ProfiledThrowHandler(wrapper),
            ProfiledThrowCaller.Bind,
            ProfiledThrowCaller.Clear,
            ProfiledThrowCaller.Publish,
            ProfiledThrowCaller.FunctionPointer,
            DriveThrowCaller);

    private static ProfiledOwnedCallerRoute<ProfiledMutateOperation>
        MutateRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledMutateCaller),
                nameof(ProfiledMutateCaller.Selected)),
            Caller(
                typeof(ProfiledMutateCaller),
                nameof(ProfiledMutateCaller.RoutedTemplate)),
            Operation(
                nameof(ProfiledPartialTarget.Mutate),
                typeof(int).MakeByRefType(),
                typeof(int).MakeByRefType()),
            new ProfiledMutateOperation(ProfiledPartialOriginal.Mutate),
            wrapper => new ProfiledMutateHandler(wrapper),
            ProfiledMutateCaller.Bind,
            ProfiledMutateCaller.Clear,
            ProfiledMutateCaller.Publish,
            ProfiledMutateCaller.FunctionPointer,
            DriveMutateCaller);

    private static void DriveThrowCaller()
    {
        try
        {
            ProfiledThrowCaller.Selected(new ProfiledPartialTarget());
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static void DriveMutateCaller()
    {
        var value = 1;
        _ = ProfiledMutateCaller.Selected(
            new ProfiledPartialTarget(),
            ref value,
            out _);
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo Operation(
        string name,
        params Type[] parameters) =>
        typeof(ProfiledPartialTarget).GetMethod(name, parameters)!;
}
