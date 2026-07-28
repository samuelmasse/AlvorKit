namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates exact instance-field read and write caller routes.</summary>
internal sealed class ProfiledInstanceFieldRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string ReadId =
        "ProfiledReadInstanceFieldCaller.Selected::ProfiledReceiverFreeTarget.InstanceField";
    private const string WriteId =
        "ProfiledWriteInstanceFieldCaller.Selected::ProfiledReceiverFreeTarget.InstanceField";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates both exact field routes over the checked-in profiler.</summary>
    internal ProfiledInstanceFieldRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [ReadId] = ReadRoute(profiler),
            [WriteId] = WriteRoute(profiler),
        };
    }

    /// <summary>Gets whether both field callers reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether both field callers entered their production wrappers.</summary>
    internal bool AllRewritten =>
        routes.Values.All(route => route.HandlerInvocations >= 1);

    /// <summary>Gets whether both field callers were restored during rollback.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates stable coordinator routes for read and write sites.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(ReadId),
        new(WriteId),
    ];

    /// <summary>Prepares one field caller selected by stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one field caller behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one field caller during reverse-order rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected instance-field route '{route.Id}'.");

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceInt32Read> ReadRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Method(
            typeof(ProfiledReadInstanceFieldCaller),
            nameof(ProfiledReadInstanceFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Method(
                typeof(ProfiledReadInstanceFieldCaller),
                nameof(ProfiledReadInstanceFieldCaller.RoutedTemplate)),
            Field(),
            "FieldRead",
            new(ProfiledReceiverFreeOriginal.ReadInstanceField),
            wrapper =>
                new ProfiledReceiverFreeInstanceInt32ReadHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.ReadInstanceField)),
            ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceFieldTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceFieldTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceFieldTag>.Publish,
            () => ProfiledGenericFunctionPointer.Get(
                typeof(ProfiledReadInstanceFieldCaller),
                "Invoke"),
            () => _ = ProfiledReadInstanceFieldCaller.Selected(new(1)));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceInt32Write> WriteRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Method(
            typeof(ProfiledWriteInstanceFieldCaller),
            nameof(ProfiledWriteInstanceFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Method(
                typeof(ProfiledWriteInstanceFieldCaller),
                nameof(ProfiledWriteInstanceFieldCaller.RoutedTemplate)),
            Field(),
            "FieldWrite",
            new(ProfiledReceiverFreeOriginal.WriteInstanceField),
            wrapper =>
                new ProfiledReceiverFreeInstanceInt32WriteHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.WriteInstanceField)),
            ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceFieldTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceFieldTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceFieldTag>.Publish,
            () => ProfiledGenericFunctionPointer.Get(
                typeof(ProfiledWriteInstanceFieldCaller),
                "Invoke"),
            () => ProfiledWriteInstanceFieldCaller.Selected(new(1), 2));
    }

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static FieldInfo Field() =>
        typeof(ProfiledReceiverFreeTarget).GetField(
            nameof(ProfiledReceiverFreeTarget.InstanceField),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static MethodInfo Original(string name) =>
        typeof(ProfiledReceiverFreeOriginal).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
