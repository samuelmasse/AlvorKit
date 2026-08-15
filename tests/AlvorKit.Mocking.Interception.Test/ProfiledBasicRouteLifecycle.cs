namespace AlvorKit;

/// <summary>Coordinates the six exact callers in the concrete basic behavior row.</summary>
internal sealed class ProfiledBasicRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string AddRouteId =
        "ProfiledBasicAddCaller.Selected::ProfiledBasicTarget.Add";
    private const string GetNumberRouteId =
        "ProfiledBasicGetNumberCaller.Selected::ProfiledBasicTarget.get_Number";
    private const string SetNumberRouteId =
        "ProfiledBasicSetNumberCaller.Selected::ProfiledBasicTarget.set_Number";
    private const string MutateRouteId =
        "ProfiledBasicMutateCaller.Selected::ProfiledBasicTarget.Mutate";
    private const string AddChangedRouteId =
        "ProfiledBasicAddChangedCaller.Selected::ProfiledBasicTarget.add_Changed";
    private const string RemoveChangedRouteId =
        "ProfiledBasicRemoveChangedCaller.Selected::ProfiledBasicTarget.remove_Changed";

    private readonly Dictionary<string, IProfiledOwnedCallerRoute> routes;

    /// <summary>Creates all six exact route owners over the startup profiler.</summary>
    internal ProfiledBasicRouteLifecycle(IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [AddRouteId] = AddRoute(profiler),
            [GetNumberRouteId] = GetNumberRoute(profiler),
            [SetNumberRouteId] = SetNumberRoute(profiler),
            [MutateRouteId] = MutateRoute(profiler),
            [AddChangedRouteId] = AddChangedRoute(profiler),
            [RemoveChangedRouteId] = RemoveChangedRoute(profiler),
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
        new(GetNumberRouteId),
        new(SetNumberRouteId),
        new(MutateRouteId),
        new(AddChangedRouteId),
        new(RemoveChangedRouteId),
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
                $"Unexpected basic route '{route.Id}'.");

    private static ProfiledOwnedCallerRoute<ProfiledBasicAddOperation>
        AddRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledBasicAddCaller),
                nameof(ProfiledBasicAddCaller.Selected)),
            Caller(
                typeof(ProfiledBasicAddCaller),
                nameof(ProfiledBasicAddCaller.RoutedTemplate)),
            Method(nameof(ProfiledBasicTarget.Add), typeof(int), typeof(int)),
            new ProfiledBasicAddOperation(ProfiledBasicOriginal.Add),
            wrapper => new ProfiledBasicAddHandler(wrapper),
            ProfiledBasicAddCaller.Bind,
            ProfiledBasicAddCaller.Clear,
            ProfiledBasicAddCaller.Publish,
            ProfiledBasicAddCaller.FunctionPointer,
            () => _ = ProfiledBasicAddCaller.Selected(
                new ProfiledBasicTarget(),
                1,
                2));

    private static ProfiledOwnedCallerRoute<ProfiledBasicGetNumberOperation>
        GetNumberRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledBasicGetNumberCaller),
                nameof(ProfiledBasicGetNumberCaller.Selected)),
            Caller(
                typeof(ProfiledBasicGetNumberCaller),
                nameof(ProfiledBasicGetNumberCaller.RoutedTemplate)),
            Property().GetMethod!,
            new ProfiledBasicGetNumberOperation(
                ProfiledBasicOriginal.GetNumber),
            wrapper => new ProfiledBasicGetNumberHandler(wrapper),
            ProfiledBasicGetNumberCaller.Bind,
            ProfiledBasicGetNumberCaller.Clear,
            ProfiledBasicGetNumberCaller.Publish,
            ProfiledBasicGetNumberCaller.FunctionPointer,
            () => _ = ProfiledBasicGetNumberCaller.Selected(
                new ProfiledBasicTarget()));

    private static ProfiledOwnedCallerRoute<ProfiledBasicSetNumberOperation>
        SetNumberRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledBasicSetNumberCaller),
                nameof(ProfiledBasicSetNumberCaller.Selected)),
            Caller(
                typeof(ProfiledBasicSetNumberCaller),
                nameof(ProfiledBasicSetNumberCaller.RoutedTemplate)),
            Property().SetMethod!,
            new ProfiledBasicSetNumberOperation(
                ProfiledBasicOriginal.SetNumber),
            wrapper => new ProfiledBasicSetNumberHandler(wrapper),
            ProfiledBasicSetNumberCaller.Bind,
            ProfiledBasicSetNumberCaller.Clear,
            ProfiledBasicSetNumberCaller.Publish,
            ProfiledBasicSetNumberCaller.FunctionPointer,
            () => ProfiledBasicSetNumberCaller.Selected(
                new ProfiledBasicTarget(),
                1));

    private static ProfiledOwnedCallerRoute<ProfiledBasicMutateOperation>
        MutateRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledBasicMutateCaller),
                nameof(ProfiledBasicMutateCaller.Selected)),
            Caller(
                typeof(ProfiledBasicMutateCaller),
                nameof(ProfiledBasicMutateCaller.RoutedTemplate)),
            Method(
                nameof(ProfiledBasicTarget.Mutate),
                typeof(int).MakeByRefType(),
                typeof(int).MakeByRefType()),
            new ProfiledBasicMutateOperation(ProfiledBasicOriginal.Mutate),
            wrapper => new ProfiledBasicMutateHandler(wrapper),
            ProfiledBasicMutateCaller.Bind,
            ProfiledBasicMutateCaller.Clear,
            ProfiledBasicMutateCaller.Publish,
            ProfiledBasicMutateCaller.FunctionPointer,
            DriveMutateCaller);

    private static ProfiledOwnedCallerRoute<ProfiledBasicEventOperation>
        AddChangedRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledBasicAddChangedCaller),
                nameof(ProfiledBasicAddChangedCaller.Selected)),
            Caller(
                typeof(ProfiledBasicAddChangedCaller),
                nameof(ProfiledBasicAddChangedCaller.RoutedTemplate)),
            Event().AddMethod!,
            new ProfiledBasicEventOperation(
                ProfiledBasicOriginal.AddChanged),
            wrapper => new ProfiledBasicEventHandler(wrapper),
            ProfiledBasicAddChangedCaller.Bind,
            ProfiledBasicAddChangedCaller.Clear,
            ProfiledBasicAddChangedCaller.Publish,
            ProfiledBasicAddChangedCaller.FunctionPointer,
            () => ProfiledBasicAddChangedCaller.Selected(
                new ProfiledBasicTarget(),
                null));

    private static ProfiledOwnedCallerRoute<ProfiledBasicEventOperation>
        RemoveChangedRoute(IInterceptionBackend profiler) =>
        new(
            profiler,
            Caller(
                typeof(ProfiledBasicRemoveChangedCaller),
                nameof(ProfiledBasicRemoveChangedCaller.Selected)),
            Caller(
                typeof(ProfiledBasicRemoveChangedCaller),
                nameof(ProfiledBasicRemoveChangedCaller.RoutedTemplate)),
            Event().RemoveMethod!,
            new ProfiledBasicEventOperation(
                ProfiledBasicOriginal.RemoveChanged),
            wrapper => new ProfiledBasicEventHandler(wrapper),
            ProfiledBasicRemoveChangedCaller.Bind,
            ProfiledBasicRemoveChangedCaller.Clear,
            ProfiledBasicRemoveChangedCaller.Publish,
            ProfiledBasicRemoveChangedCaller.FunctionPointer,
            () => ProfiledBasicRemoveChangedCaller.Selected(
                new ProfiledBasicTarget(),
                null));

    private static void DriveMutateCaller()
    {
        var value = 1;
        _ = ProfiledBasicMutateCaller.Selected(
            new ProfiledBasicTarget(),
            ref value,
            out _);
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo Method(
        string name,
        params Type[] parameters) =>
        typeof(ProfiledBasicTarget).GetMethod(name, parameters)!;

    private static PropertyInfo Property() =>
        typeof(ProfiledBasicTarget).GetProperty(
            nameof(ProfiledBasicTarget.Number))!;

    private static EventInfo Event() =>
        typeof(ProfiledBasicTarget).GetEvent(
            nameof(ProfiledBasicTarget.Changed),
            BindingFlags.Instance | BindingFlags.NonPublic)!;
}
