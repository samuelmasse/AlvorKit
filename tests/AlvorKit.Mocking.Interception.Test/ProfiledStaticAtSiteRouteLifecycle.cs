namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates two independently identified callers to one static target.</summary>
internal sealed class ProfiledStaticAtSiteRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string FirstId =
        "ProfiledStaticTransformCaller.Selected::ProfiledReceiverFreeTarget.Transform";
    private const string SecondId =
        "ProfiledStaticTransformSecondCaller.Selected::ProfiledReceiverFreeTarget.Transform";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates both exact static routes over the checked-in profiler.</summary>
    internal ProfiledStaticAtSiteRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [FirstId] = FirstRoute(profiler),
            [SecondId] = SecondRoute(profiler),
        };
    }

    /// <summary>Gets whether both callers reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether both callers entered their production wrappers.</summary>
    internal bool AllRewritten =>
        routes.Values.All(route => route.HandlerInvocations >= 1);

    /// <summary>Gets whether both callers were restored during rollback.</summary>
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

    /// <summary>Prepares one selected site by stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one selected site behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one selected site during reverse-order rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected static-site route '{route.Id}'.");

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Unary> FirstRoute(
        IInterceptionBackend profiler) =>
        Create<ProfiledTransformTag>(
            profiler,
            typeof(ProfiledStaticTransformCaller));

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Unary> SecondRoute(
        IInterceptionBackend profiler) =>
        Create<ProfiledTransformSecondTag>(
            profiler,
            typeof(ProfiledStaticTransformSecondCaller));

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Unary> Create<TTag>(
        IInterceptionBackend profiler,
        Type callerType)
    {
        MethodInfo caller = Method(callerType, "Selected");
        return new(
            profiler,
            caller,
            Method(callerType, "RoutedTemplate"),
            typeof(ProfiledReceiverFreeTarget).GetMethod(
                nameof(ProfiledReceiverFreeTarget.Transform),
                BindingFlags.NonPublic | BindingFlags.Static)!,
            "StaticMethod",
            new(ProfiledReceiverFreeOriginal.Transform),
            wrapper => new ProfiledReceiverFreeInt32UnaryHandler(wrapper),
            typeof(ProfiledReceiverFreeOriginal).GetMethod(
                nameof(ProfiledReceiverFreeOriginal.Transform),
                BindingFlags.NonPublic | BindingFlags.Static)!,
            ProfiledReceiverFreeRouteState<TTag>.Bind,
            ProfiledReceiverFreeRouteState<TTag>.Clear,
            ProfiledReceiverFreeRouteState<TTag>.Publish,
            () => ProfiledGenericFunctionPointer.Get(
                callerType,
                "Invoke"),
            () => _ = (int)caller.Invoke(null, [1])!);
    }

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
