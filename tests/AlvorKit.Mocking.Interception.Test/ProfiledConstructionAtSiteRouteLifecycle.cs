namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates two construction sites for the same constructor.</summary>
internal sealed class ProfiledConstructionAtSiteRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string FirstId =
        "ProfiledConstructionAtSiteFirstCaller.Selected::ProfiledReceiverFreeTarget..ctor";
    private const string SecondId =
        "ProfiledConstructionAtSiteSecondCaller.Selected::ProfiledReceiverFreeTarget..ctor";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates both exact newobj routes over the startup profiler.</summary>
    internal ProfiledConstructionAtSiteRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [FirstId] = Create<ProfiledConstructionAtSiteFirstTag>(
                profiler,
                typeof(ProfiledConstructionAtSiteFirstCaller)),
            [SecondId] = Create<ProfiledConstructionAtSiteSecondTag>(
                profiler,
                typeof(ProfiledConstructionAtSiteSecondCaller)),
        };
    }

    /// <summary>Gets whether both sites reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether both sites entered their production wrappers.</summary>
    internal bool AllRewritten =>
        routes.Values.All(route => route.HandlerInvocations >= 1);

    /// <summary>Gets total production-wrapper entries across both sites.</summary>
    internal int HandlerInvocations =>
        routes.Values.Sum(route => route.HandlerInvocations);

    /// <summary>Gets whether rollback restored both selected callers.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates stable coordinator routes for both selected sites.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(FirstId),
        new(SecondId),
    ];

    /// <summary>Prepares one selected construction site.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one selected construction site.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one selected construction site.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected construction route '{route.Id}'.");

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeConstruction> Create<TTag>(
        IInterceptionBackend profiler,
        Type callerType)
    {
        MethodInfo caller = Method(callerType, "Selected");
        return new(
            profiler,
            caller,
            Method(callerType, "Invoke"),
            typeof(ProfiledReceiverFreeTarget).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                [typeof(int)],
                modifiers: null)!,
            "Construction",
            new(ProfiledReceiverFreeOriginal.Construct),
            wrapper =>
                new ProfiledReceiverFreeConstructionHandler(wrapper),
            typeof(ProfiledReceiverFreeOriginal).GetMethod(
                nameof(ProfiledReceiverFreeOriginal.Construct),
                BindingFlags.NonPublic | BindingFlags.Static)!,
            ProfiledReceiverFreeRouteState<TTag>.Bind,
            ProfiledReceiverFreeRouteState<TTag>.Clear,
            ProfiledReceiverFreeRouteState<TTag>.Publish,
            () => ProfiledGenericFunctionPointer.Get(
                callerType,
                "Invoke"),
            () => _ = (ProfiledReceiverFreeTarget)
                caller.Invoke(null, [1])!);
    }

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
