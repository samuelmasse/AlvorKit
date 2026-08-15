namespace AlvorKit;

/// <summary>Coordinates every caller in the no-session struct original row.</summary>
internal sealed partial class ProfiledStructRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string AddId = "StructAdd";
    private const string ReadId = "StructRead";
    private const string RecordId = "RecordStructRead";
    private const string WindowId = "StructWindow";
    private const string FieldId = "StructAddField";
    private const string StaticFieldId = "StructAddStaticField";
    private const string ArrayId = "StructAddArray";
    private const string ConstrainedId = "StructConstrained";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    internal ProfiledStructRouteLifecycle(IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [AddId] = AddRoute(profiler),
            [ReadId] = ReadRoute(profiler),
            [RecordId] = RecordRoute(profiler),
            [WindowId] = WindowRoute(profiler),
            [FieldId] = FieldRoute(profiler),
            [StaticFieldId] = StaticFieldRoute(profiler),
            [ArrayId] = ArrayRoute(profiler),
            [ConstrainedId] = ConstrainedRoute(profiler),
        };
    }

    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    internal bool AllWrappersEnteredExactlyOnce =>
        routes.Values.All(route => route.HandlerInvocations == 1);

    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(AddId),
        new(ReadId),
        new(RecordId),
        new(WindowId),
        new(FieldId),
        new(StaticFieldId),
        new(ArrayId),
        new(ConstrainedId),
    ];

    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected struct route '{route.Id}'.");

    private static ProfiledStructCallerRoute<
        ProfiledStructInt32Operation<TReceiver>> Writable<
        TReceiver,
        TTag>(
        IInterceptionBackend profiler,
        MethodInfo caller,
        MethodInfo template,
        MethodInfo operation,
        ProfiledStructInt32Operation<TReceiver> original,
        Type pointerOwner,
        string pointerMethod,
        Action drive)
        where TReceiver : struct
    {
        return new(
            profiler,
            caller,
            template,
            operation,
            typeof(TReceiver),
            InterceptionReceiverOwnership.ManagedReference,
            original,
            wrapper =>
                new ProfiledStructInt32Handler<TReceiver>(wrapper),
            ProfiledReceiverFreeRouteState<TTag>.Bind,
            ProfiledReceiverFreeRouteState<TTag>.Clear,
            ProfiledReceiverFreeRouteState<TTag>.Publish,
            () => Pointer(pointerOwner, pointerMethod),
            drive);
    }

    private static MethodInfo Caller(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo ClosedCaller(Type type, string name) =>
        Caller(type, name)
            .MakeGenericMethod(typeof(ProfiledMutableStructTarget));

    private static MethodInfo Operation(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Instance)!;

    private static nint Pointer(Type type, string name) =>
        ProfiledGenericFunctionPointer.Get(type, name);

    private static void DriveAdd()
    {
        var target = new ProfiledMutableStructTarget();
        _ = ProfiledStructAddCaller.Selected(ref target, 0);
    }

    private static void DriveRead()
    {
        var target = new ProfiledReadonlyStructTarget();
        _ = ProfiledStructReadCaller.Selected(in target, 0);
    }

    private static void DriveRecord()
    {
        var target = new ProfiledRecordStructTarget();
        _ = ProfiledRecordStructReadCaller.Selected(ref target, 0);
    }

    private static void DriveWindow()
    {
        var target = new ProfiledMutableStructTarget();
        _ = ProfiledStructWindowCaller.Selected(ref target, []);
    }

    private static void DriveField() =>
        _ = ProfiledStructFieldAddCaller.Selected(new(0), 0);

    private static void DriveStaticField() =>
        _ = ProfiledStructStaticFieldAddCaller.Selected(0);

    private static void DriveArray() =>
        _ = ProfiledStructArrayAddCaller.Selected([new(0)], 0, 0);

    private static void DriveConstrained()
    {
        var target = new ProfiledMutableStructTarget();
        _ = ProfiledStructConstrainedCaller.Selected(
            ref target,
            0);
    }
}
