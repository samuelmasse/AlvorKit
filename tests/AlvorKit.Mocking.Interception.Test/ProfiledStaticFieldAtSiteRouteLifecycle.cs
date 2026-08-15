namespace AlvorKit;

/// <summary>Coordinates two independently identified reads of one static field.</summary>
internal sealed class ProfiledStaticFieldAtSiteRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string FirstId =
        "ProfiledReadStaticFieldCaller.Selected::ProfiledReceiverFreeTarget.StaticField";
    private const string SecondId =
        "ProfiledReadStaticFieldSecondCaller.Selected::ProfiledReceiverFreeTarget.StaticField";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates both exact field-read routes over the startup profiler.</summary>
    internal ProfiledStaticFieldAtSiteRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [FirstId] = FirstRoute(profiler),
            [SecondId] = SecondRoute(profiler),
        };
    }

    /// <summary>Gets whether both read callers reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether both read callers entered their production wrappers.</summary>
    internal bool AllRewritten =>
        routes.Values.All(route => route.HandlerInvocations >= 1);

    /// <summary>Gets whether both read callers were restored during rollback.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates stable coordinator routes for both selected read sites.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(FirstId),
        new(SecondId),
    ];

    /// <summary>Prepares one selected read site by stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one selected read site behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one selected read site during reverse-order rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected field-read route '{route.Id}'.");

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Read> FirstRoute(
        IInterceptionBackend profiler) =>
        Create<ProfiledReadStaticFieldTag>(
            profiler,
            typeof(ProfiledReadStaticFieldCaller));

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Read> SecondRoute(
        IInterceptionBackend profiler) =>
        Create<ProfiledReadStaticFieldSecondTag>(
            profiler,
            typeof(ProfiledReadStaticFieldSecondCaller));

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Read> Create<TTag>(
        IInterceptionBackend profiler,
        Type callerType)
    {
        MethodInfo caller = Method(callerType, "Selected");
        return new(
            profiler,
            caller,
            Method(callerType, "RoutedTemplate"),
            typeof(ProfiledReceiverFreeTarget).GetField(
                nameof(ProfiledReceiverFreeTarget.StaticField),
                BindingFlags.NonPublic | BindingFlags.Static)!,
            "FieldRead",
            new(ProfiledReceiverFreeOriginal.ReadStaticField),
            wrapper => new ProfiledReceiverFreeInt32ReadHandler(wrapper),
            typeof(ProfiledReceiverFreeOriginal).GetMethod(
                nameof(ProfiledReceiverFreeOriginal.ReadStaticField),
                BindingFlags.NonPublic | BindingFlags.Static)!,
            ProfiledReceiverFreeRouteState<TTag>.Bind,
            ProfiledReceiverFreeRouteState<TTag>.Clear,
            ProfiledReceiverFreeRouteState<TTag>.Publish,
            () => ProfiledGenericFunctionPointer.Get(
                callerType,
                "Invoke"),
            () => _ = (int)caller.Invoke(null, null)!);
    }

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
