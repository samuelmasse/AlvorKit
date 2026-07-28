namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates the four receiver-free static callers in the session behavior row.</summary>
internal sealed class ProfiledStaticRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string TransformId =
        "ProfiledStaticTransformCaller.Selected::ProfiledReceiverFreeTarget.Transform";
    private const string IdentityId =
        "ProfiledGenericStaticCaller.Selected<string>::ProfiledReceiverFreeTarget.Identity<string>";
    private const string SetNumberId =
        "ProfiledSetStaticNumberCaller.Selected::ProfiledReceiverFreeTarget.set_StaticNumber";
    private const string GetNumberId =
        "ProfiledGetStaticNumberCaller.Selected::ProfiledReceiverFreeTarget.get_StaticNumber";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates all four exact static routes over the checked-in profiler.</summary>
    internal ProfiledStaticRouteLifecycle(IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [TransformId] = TransformRoute(profiler),
            [IdentityId] = IdentityRoute(profiler),
            [SetNumberId] = SetNumberRoute(profiler),
            [GetNumberId] = GetNumberRoute(profiler),
        };
    }

    /// <summary>Gets whether every static caller reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether every static caller entered its production wrapper.</summary>
    internal bool AllRewritten =>
        routes.Values.All(route => route.HandlerInvocations >= 1);

    /// <summary>Gets whether every static caller was restored during rollback.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates stable coordinator routes for the static behavior row.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(TransformId),
        new(IdentityId),
        new(SetNumberId),
        new(GetNumberId),
    ];

    /// <summary>Prepares one static caller selected by stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one static caller behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one static caller during reverse-order rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected static route '{route.Id}'.");

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Unary> TransformRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledStaticTransformCaller),
            nameof(ProfiledStaticTransformCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledStaticTransformCaller),
                nameof(ProfiledStaticTransformCaller.RoutedTemplate)),
            StaticMethod(nameof(ProfiledReceiverFreeTarget.Transform)),
            "StaticMethod",
            new(ProfiledReceiverFreeOriginal.Transform),
            wrapper => new ProfiledReceiverFreeInt32UnaryHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.Transform)),
            ProfiledReceiverFreeRouteState<ProfiledTransformTag>.Bind,
            ProfiledReceiverFreeRouteState<ProfiledTransformTag>.Clear,
            ProfiledReceiverFreeRouteState<ProfiledTransformTag>.Publish,
            () => Pointer(typeof(ProfiledStaticTransformCaller), "Invoke"),
            () => _ = ProfiledStaticTransformCaller.Selected(1));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeStringUnary> IdentityRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = GenericCaller(
            typeof(ProfiledGenericStaticCaller),
            nameof(ProfiledGenericStaticCaller.Selected));
        return new(
            profiler,
            caller,
            GenericCaller(
                typeof(ProfiledGenericStaticCaller),
                nameof(ProfiledGenericStaticCaller.RoutedTemplate)),
            StaticMethod(nameof(ProfiledReceiverFreeTarget.Identity))
                .MakeGenericMethod(typeof(string)),
            "StaticMethod",
            new(ProfiledReceiverFreeOriginal.Identity),
            wrapper => new ProfiledReceiverFreeStringUnaryHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.Identity)),
            ProfiledReceiverFreeRouteState<
                ProfiledIdentityTag<string>>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledIdentityTag<string>>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledIdentityTag<string>>.Publish,
            () => Pointer(typeof(ProfiledGenericStaticCaller), "InvokeString"),
            () => _ = ProfiledGenericStaticCaller.Selected("drive"));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Write> SetNumberRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledSetStaticNumberCaller),
            nameof(ProfiledSetStaticNumberCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledSetStaticNumberCaller),
                nameof(ProfiledSetStaticNumberCaller.RoutedTemplate)),
            Property().SetMethod!,
            "StaticMethod",
            new(ProfiledReceiverFreeOriginal.SetStaticNumber),
            wrapper => new ProfiledReceiverFreeInt32WriteHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.SetStaticNumber)),
            ProfiledReceiverFreeRouteState<
                ProfiledSetStaticNumberTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledSetStaticNumberTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledSetStaticNumberTag>.Publish,
            () => Pointer(typeof(ProfiledSetStaticNumberCaller), "Invoke"),
            () => ProfiledSetStaticNumberCaller.Selected(1));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Read> GetNumberRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledGetStaticNumberCaller),
            nameof(ProfiledGetStaticNumberCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledGetStaticNumberCaller),
                nameof(ProfiledGetStaticNumberCaller.RoutedTemplate)),
            Property().GetMethod!,
            "StaticMethod",
            new(ProfiledReceiverFreeOriginal.GetStaticNumber),
            wrapper => new ProfiledReceiverFreeInt32ReadHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.GetStaticNumber)),
            ProfiledReceiverFreeRouteState<
                ProfiledGetStaticNumberTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledGetStaticNumberTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledGetStaticNumberTag>.Publish,
            () => Pointer(typeof(ProfiledGetStaticNumberCaller), "Invoke"),
            () => _ = ProfiledGetStaticNumberCaller.Selected());
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo GenericCaller(Type type, string name) =>
        Caller(type, name).MakeGenericMethod(typeof(string));

    private static MethodInfo StaticMethod(string name) =>
        typeof(ProfiledReceiverFreeTarget).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static PropertyInfo Property() =>
        typeof(ProfiledReceiverFreeTarget).GetProperty(
            nameof(ProfiledReceiverFreeTarget.StaticNumber),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo Original(string name) =>
        typeof(ProfiledReceiverFreeOriginal).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static nint Pointer(Type type, string name) =>
        ProfiledGenericFunctionPointer.Get(type, name);
}
