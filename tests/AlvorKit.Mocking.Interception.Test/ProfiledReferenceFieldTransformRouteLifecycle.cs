namespace AlvorKit;

/// <summary>Coordinates the exact reference-field write and read sites.</summary>
internal sealed class ProfiledReferenceFieldTransformRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string WriteId =
        "ProfiledReferenceFieldTransformWriteCaller.Selected::ProfiledReceiverFreeTarget.InstanceReferenceField";
    private const string ReadId =
        "ProfiledReferenceFieldTransformReadCaller.Selected::ProfiledReceiverFreeTarget.InstanceReferenceField";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates receiver-scoped field routes over the profiler.</summary>
    internal ProfiledReferenceFieldTransformRouteLifecycle(
        IInterceptionBackend profiler,
        ProfiledReceiverFreeTarget driveTarget)
    {
        routes = new(StringComparer.Ordinal)
        {
            [WriteId] = WriteRoute(profiler, driveTarget),
            [ReadId] = ReadRoute(profiler, driveTarget),
        };
    }

    /// <summary>Gets whether both callers reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether each field site entered its exact wrapper once.</summary>
    internal bool AllWrappersEnteredExactlyOnce =>
        routes.Values.All(route => route.HandlerInvocations == 1);

    /// <summary>Gets whether both callers were restored during rollback.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates the stable coordinator routes for the typed field row.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(WriteId),
        new(ReadId),
    ];

    /// <summary>Prepares one field caller selected by stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one prepared field caller behind the shared gate.</summary>
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
                $"Unexpected reference-field route '{route.Id}'.");

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceStringWrite> WriteRoute(
        IInterceptionBackend profiler,
        ProfiledReceiverFreeTarget driveTarget)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledReferenceFieldTransformWriteCaller),
            nameof(
                ProfiledReferenceFieldTransformWriteCaller
                    .Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledReferenceFieldTransformWriteCaller),
                nameof(
                    ProfiledReferenceFieldTransformWriteCaller
                        .RoutedTemplate)),
            Field(),
            "FieldWrite",
            new(
                ProfiledReceiverFreeOriginal
                    .WriteInstanceReferenceField),
            wrapper =>
                new ProfiledReceiverFreeInstanceStringWriteHandler(
                    wrapper),
            Original(
                nameof(
                    ProfiledReceiverFreeOriginal
                        .WriteInstanceReferenceField)),
            ProfiledReceiverFreeRouteState<
                ProfiledReferenceFieldTransformWriteTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledReferenceFieldTransformWriteTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledReferenceFieldTransformWriteTag>.Publish,
            () => Pointer(
                typeof(ProfiledReferenceFieldTransformWriteCaller),
                "Invoke"),
            () => ProfiledReferenceFieldTransformWriteCaller.Selected(
                driveTarget,
                driveTarget.InstanceReferenceField));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceStringRead> ReadRoute(
        IInterceptionBackend profiler,
        ProfiledReceiverFreeTarget driveTarget)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledReferenceFieldTransformReadCaller),
            nameof(
                ProfiledReferenceFieldTransformReadCaller
                    .Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledReferenceFieldTransformReadCaller),
                nameof(
                    ProfiledReferenceFieldTransformReadCaller
                        .RoutedTemplate)),
            Field(),
            "FieldRead",
            new(
                ProfiledReceiverFreeOriginal
                    .ReadInstanceReferenceField),
            wrapper =>
                new ProfiledReceiverFreeInstanceStringReadHandler(
                    wrapper),
            Original(
                nameof(
                    ProfiledReceiverFreeOriginal
                        .ReadInstanceReferenceField)),
            ProfiledReceiverFreeRouteState<
                ProfiledReferenceFieldTransformReadTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledReferenceFieldTransformReadTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledReferenceFieldTransformReadTag>.Publish,
            () => Pointer(
                typeof(ProfiledReferenceFieldTransformReadCaller),
                "Invoke"),
            () => _ =
                ProfiledReferenceFieldTransformReadCaller.Selected(
                    driveTarget));
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static FieldInfo Field() =>
        typeof(ProfiledReceiverFreeTarget).GetField(
            nameof(
                ProfiledReceiverFreeTarget.InstanceReferenceField),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static MethodInfo Original(string name) =>
        typeof(ProfiledReceiverFreeOriginal).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static nint Pointer(Type type, string name) =>
        ProfiledGenericFunctionPointer.Get(type, name);
}
