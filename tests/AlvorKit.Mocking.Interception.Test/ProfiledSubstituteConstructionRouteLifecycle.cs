namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns one exact profiled construction route through rollback.</summary>
internal sealed class ProfiledSubstituteConstructionRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string RouteId =
        "ProfiledSubstituteConstructionCaller.Selected::ProfiledReceiverFreeTarget..ctor";

    private readonly ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeConstruction> route;

    /// <summary>Creates the exact newobj route over the checked-in profiler.</summary>
    internal ProfiledSubstituteConstructionRouteLifecycle(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Method("Selected");
        route = new(
            profiler,
            caller,
            Method("Invoke"),
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
            ProfiledReceiverFreeRouteState<
                ProfiledSubstituteConstructionTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledSubstituteConstructionTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledSubstituteConstructionTag>.Publish,
            () => ProfiledGenericFunctionPointer.Get(
                typeof(ProfiledSubstituteConstructionCaller),
                "Invoke"),
            () => _ =
                ProfiledSubstituteConstructionCaller.Selected(1));
    }

    /// <summary>Gets whether inert preparation installed the rewritten site.</summary>
    internal bool IsPrepared =>
        route.PreparationCompletion?.State ==
        InterceptionState.Active;

    /// <summary>Gets how often calls entered the production wrapper.</summary>
    internal int HandlerInvocations =>
        route.HandlerInvocations;

    /// <summary>Gets whether rollback restored the selected caller.</summary>
    internal bool IsRemoved =>
        route.RemovalCompletion?.State ==
        InterceptionState.Removed;

    /// <summary>Creates the stable coordinator route for the selected site.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(RouteId),
    ];

    /// <summary>Prepares the selected construction site.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value) =>
        Resolve(value).Prepare(value);

    /// <summary>Publishes the selected construction site.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value) =>
        Resolve(value).Activate(value);

    /// <summary>Restores the selected construction site.</summary>
    public void Rollback(MockInterceptionRoute value) =>
        Resolve(value).Rollback(value);

    private ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeConstruction> Resolve(
        MockInterceptionRoute value) =>
        value.Id == RouteId
            ? route
            : throw new InvalidOperationException(
                $"Unexpected construction route '{value.Id}'.");

    private static MethodInfo Method(string name) =>
        typeof(ProfiledSubstituteConstructionCaller).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
