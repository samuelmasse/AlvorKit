namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates the exact write and read sites in the static-field row.</summary>
internal sealed class ProfiledStaticFieldTransformRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string WriteId =
        "ProfiledStaticFieldTransformWriteCaller.Selected::ProfiledReceiverFreeTarget.StaticField";
    private const string ReadId =
        "ProfiledStaticFieldTransformReadCaller.Selected::ProfiledReceiverFreeTarget.StaticField";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates the exact field routes over the checked-in profiler.</summary>
    internal ProfiledStaticFieldTransformRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [WriteId] = WriteRoute(profiler),
            [ReadId] = ReadRoute(profiler),
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

    /// <summary>Creates the stable coordinator routes for the field row.</summary>
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
                $"Unexpected field route '{route.Id}'.");

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Write> WriteRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledStaticFieldTransformWriteCaller),
            nameof(ProfiledStaticFieldTransformWriteCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledStaticFieldTransformWriteCaller),
                nameof(
                    ProfiledStaticFieldTransformWriteCaller
                        .RoutedTemplate)),
            Field(),
            "FieldWrite",
            new(ProfiledReceiverFreeOriginal.WriteStaticField),
            wrapper =>
                new ProfiledReceiverFreeInt32WriteHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.WriteStaticField)),
            ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformWriteTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformWriteTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformWriteTag>.Publish,
            () => Pointer(
                typeof(ProfiledStaticFieldTransformWriteCaller),
                "Invoke"),
            () => ProfiledStaticFieldTransformWriteCaller.Selected(
                ProfiledReceiverFreeTarget.StaticField));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Read> ReadRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledStaticFieldTransformReadCaller),
            nameof(ProfiledStaticFieldTransformReadCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledStaticFieldTransformReadCaller),
                nameof(
                    ProfiledStaticFieldTransformReadCaller
                        .RoutedTemplate)),
            Field(),
            "FieldRead",
            new(ProfiledReceiverFreeOriginal.ReadStaticField),
            wrapper =>
                new ProfiledReceiverFreeInt32ReadHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.ReadStaticField)),
            ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformReadTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformReadTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledStaticFieldTransformReadTag>.Publish,
            () => Pointer(
                typeof(ProfiledStaticFieldTransformReadCaller),
                "Invoke"),
            () => _ =
                ProfiledStaticFieldTransformReadCaller.Selected());
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static FieldInfo Field() =>
        typeof(ProfiledReceiverFreeTarget).GetField(
            nameof(ProfiledReceiverFreeTarget.StaticField),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo Original(string name) =>
        typeof(ProfiledReceiverFreeOriginal).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static nint Pointer(Type type, string name) =>
        ProfiledGenericFunctionPointer.Get(type, name);
}
