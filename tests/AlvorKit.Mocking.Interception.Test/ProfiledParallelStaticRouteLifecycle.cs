namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns the one shared rewritten caller used by parallel ambient sessions.</summary>
internal sealed class ProfiledParallelStaticRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string TransformId =
        "ProfiledStaticTransformCaller.Selected::ProfiledReceiverFreeTarget.Transform";

    private readonly ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Unary> route;

    /// <summary>Creates the shared static route over the startup profiler.</summary>
    internal ProfiledParallelStaticRouteLifecycle(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Method(
            typeof(ProfiledStaticTransformCaller),
            nameof(ProfiledStaticTransformCaller.Selected));
        route = new(
            profiler,
            caller,
            Method(
                typeof(ProfiledStaticTransformCaller),
                nameof(ProfiledStaticTransformCaller.RoutedTemplate)),
            typeof(ProfiledReceiverFreeTarget).GetMethod(
                nameof(ProfiledReceiverFreeTarget.Transform),
                BindingFlags.NonPublic | BindingFlags.Static)!,
            "StaticMethod",
            new(ProfiledReceiverFreeOriginal.Transform),
            wrapper => new ProfiledReceiverFreeInt32UnaryHandler(wrapper),
            typeof(ProfiledReceiverFreeOriginal).GetMethod(
                nameof(ProfiledReceiverFreeOriginal.Transform),
                BindingFlags.NonPublic | BindingFlags.Static)!,
            ProfiledReceiverFreeRouteState<ProfiledTransformTag>.Bind,
            ProfiledReceiverFreeRouteState<ProfiledTransformTag>.Clear,
            ProfiledReceiverFreeRouteState<ProfiledTransformTag>.Publish,
            () => ProfiledGenericFunctionPointer.Get(
                typeof(ProfiledStaticTransformCaller),
                "Invoke"),
            () => _ = ProfiledStaticTransformCaller.Selected(1));
    }

    /// <summary>Gets whether the shared caller reached inert active preparation.</summary>
    internal bool IsPrepared =>
        route.PreparationCompletion?.State ==
        InterceptionState.Active;

    /// <summary>Gets whether calls entered the shared production wrapper.</summary>
    internal bool WasRewritten => route.HandlerInvocations >= 1;

    /// <summary>Gets whether the shared caller was restored during rollback.</summary>
    internal bool IsRemoved =>
        route.RemovalCompletion?.State ==
        InterceptionState.Removed;

    /// <summary>Creates the stable coordinator route.</summary>
    internal static MockInterceptionRoute CreateRoute() =>
        new(TransformId);

    /// <summary>Prepares the shared rewritten caller.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value) =>
        RequireRoute(value, route.Prepare);

    /// <summary>Publishes the shared rewritten caller.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value) =>
        RequireRoute(value, route.Activate);

    /// <summary>Restores the shared rewritten caller.</summary>
    public void Rollback(MockInterceptionRoute value)
    {
        if (value.Id != TransformId)
        {
            throw new InvalidOperationException(
                $"Unexpected parallel static route '{value.Id}'.");
        }

        route.Rollback(value);
    }

    private static MockInterceptionPreparationDiagnostic? RequireRoute(
        MockInterceptionRoute value,
        Func<
            MockInterceptionRoute,
            MockInterceptionPreparationDiagnostic?> action)
    {
        if (value.Id != TransformId)
        {
            throw new InvalidOperationException(
                $"Unexpected parallel static route '{value.Id}'.");
        }

        return action(value);
    }

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
