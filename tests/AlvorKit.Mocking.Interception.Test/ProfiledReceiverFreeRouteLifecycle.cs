namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates every receiver-free caller in the no-session fallback row.</summary>
internal sealed class ProfiledReceiverFreeRouteLifecycle :
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
    private const string WriteStaticFieldId =
        "ProfiledWriteStaticFieldCaller.Selected::ProfiledReceiverFreeTarget.StaticField";
    private const string ReadStaticFieldId =
        "ProfiledReadStaticFieldCaller.Selected::ProfiledReceiverFreeTarget.StaticField";
    private const string ConstructionId =
        "ProfiledReceiverFreeConstructionCaller.Selected::ProfiledReceiverFreeTarget..ctor";
    private const string ReadInstanceFieldId =
        "ProfiledReadInstanceFieldCaller.Selected::ProfiledReceiverFreeTarget.InstanceField";
    private const string WriteInstanceFieldId =
        "ProfiledWriteInstanceFieldCaller.Selected::ProfiledReceiverFreeTarget.InstanceField";
    private const string ReadReferenceFieldId =
        "ProfiledReadInstanceReferenceFieldCaller.Selected::ProfiledReceiverFreeTarget.InstanceReferenceField";
    private const string WriteReferenceFieldId =
        "ProfiledWriteInstanceReferenceFieldCaller.Selected::ProfiledReceiverFreeTarget.InstanceReferenceField";

    private readonly Dictionary<
        string,
        IProfiledReceiverFreeCallerRoute> routes;

    /// <summary>Creates all exact receiver-free routes over the checked-in profiler.</summary>
    internal ProfiledReceiverFreeRouteLifecycle(
        IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [TransformId] = TransformRoute(profiler),
            [IdentityId] = IdentityRoute(profiler),
            [SetNumberId] = SetNumberRoute(profiler),
            [GetNumberId] = GetNumberRoute(profiler),
            [WriteStaticFieldId] = WriteStaticFieldRoute(profiler),
            [ReadStaticFieldId] = ReadStaticFieldRoute(profiler),
            [ConstructionId] = ConstructionRoute(profiler),
            [ReadInstanceFieldId] = ReadInstanceFieldRoute(profiler),
            [WriteInstanceFieldId] = WriteInstanceFieldRoute(profiler),
            [ReadReferenceFieldId] = ReadReferenceFieldRoute(profiler),
            [WriteReferenceFieldId] = WriteReferenceFieldRoute(profiler),
        };
    }

    /// <summary>Gets whether every caller reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether every caller entered its exact production wrapper.</summary>
    internal bool AllRewritten =>
        routes.Values.All(route => route.HandlerInvocations >= 1);

    /// <summary>Gets whether every caller was restored during rollback.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates stable coordinator routes for every receiver-free site.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(TransformId),
        new(IdentityId),
        new(SetNumberId),
        new(GetNumberId),
        new(WriteStaticFieldId),
        new(ReadStaticFieldId),
        new(ConstructionId),
        new(ReadInstanceFieldId),
        new(WriteInstanceFieldId),
        new(ReadReferenceFieldId),
        new(WriteReferenceFieldId),
    ];

    /// <summary>Prepares one receiver-free caller selected by stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one receiver-free caller behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one receiver-free caller during reverse-order rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledReceiverFreeCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected receiver-free route '{route.Id}'.");

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
        MethodInfo operation = StaticMethod(
            nameof(ProfiledReceiverFreeTarget.Identity))
            .MakeGenericMethod(typeof(string));
        return new(
            profiler,
            caller,
            GenericCaller(
                typeof(ProfiledGenericStaticCaller),
                nameof(ProfiledGenericStaticCaller.RoutedTemplate)),
            operation,
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
            StaticProperty().SetMethod!,
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
            StaticProperty().GetMethod!,
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

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Write> WriteStaticFieldRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledWriteStaticFieldCaller),
            nameof(ProfiledWriteStaticFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledWriteStaticFieldCaller),
                nameof(ProfiledWriteStaticFieldCaller.RoutedTemplate)),
            StaticField(),
            "FieldWrite",
            new(ProfiledReceiverFreeOriginal.WriteStaticField),
            wrapper => new ProfiledReceiverFreeInt32WriteHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.WriteStaticField)),
            ProfiledReceiverFreeRouteState<
                ProfiledWriteStaticFieldTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledWriteStaticFieldTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledWriteStaticFieldTag>.Publish,
            () => Pointer(typeof(ProfiledWriteStaticFieldCaller), "Invoke"),
            () => ProfiledWriteStaticFieldCaller.Selected(1));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInt32Read> ReadStaticFieldRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledReadStaticFieldCaller),
            nameof(ProfiledReadStaticFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledReadStaticFieldCaller),
                nameof(ProfiledReadStaticFieldCaller.RoutedTemplate)),
            StaticField(),
            "FieldRead",
            new(ProfiledReceiverFreeOriginal.ReadStaticField),
            wrapper => new ProfiledReceiverFreeInt32ReadHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.ReadStaticField)),
            ProfiledReceiverFreeRouteState<
                ProfiledReadStaticFieldTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledReadStaticFieldTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledReadStaticFieldTag>.Publish,
            () => Pointer(typeof(ProfiledReadStaticFieldCaller), "Invoke"),
            () => _ = ProfiledReadStaticFieldCaller.Selected());
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeConstruction> ConstructionRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledReceiverFreeConstructionCaller),
            nameof(ProfiledReceiverFreeConstructionCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledReceiverFreeConstructionCaller),
                "Invoke"),
            Constructor(),
            "Construction",
            new(ProfiledReceiverFreeOriginal.Construct),
            wrapper => new ProfiledReceiverFreeConstructionHandler(wrapper),
            Original(nameof(ProfiledReceiverFreeOriginal.Construct)),
            ProfiledReceiverFreeRouteState<ProfiledConstructionTag>.Bind,
            ProfiledReceiverFreeRouteState<ProfiledConstructionTag>.Clear,
            ProfiledReceiverFreeRouteState<ProfiledConstructionTag>.Publish,
            () => Pointer(
                typeof(ProfiledReceiverFreeConstructionCaller),
                "Invoke"),
            () => _ = ProfiledReceiverFreeConstructionCaller.Selected(1));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceInt32Read> ReadInstanceFieldRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledReadInstanceFieldCaller),
            nameof(ProfiledReadInstanceFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledReadInstanceFieldCaller),
                nameof(ProfiledReadInstanceFieldCaller.RoutedTemplate)),
            InstanceField(nameof(ProfiledReceiverFreeTarget.InstanceField)),
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
            () => Pointer(typeof(ProfiledReadInstanceFieldCaller), "Invoke"),
            () => _ = ProfiledReadInstanceFieldCaller.Selected(new(1)));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceInt32Write> WriteInstanceFieldRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledWriteInstanceFieldCaller),
            nameof(ProfiledWriteInstanceFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledWriteInstanceFieldCaller),
                nameof(ProfiledWriteInstanceFieldCaller.RoutedTemplate)),
            InstanceField(nameof(ProfiledReceiverFreeTarget.InstanceField)),
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
            () => Pointer(typeof(ProfiledWriteInstanceFieldCaller), "Invoke"),
            () => ProfiledWriteInstanceFieldCaller.Selected(new(1), 2));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceStringRead> ReadReferenceFieldRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledReadInstanceReferenceFieldCaller),
            nameof(ProfiledReadInstanceReferenceFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledReadInstanceReferenceFieldCaller),
                nameof(ProfiledReadInstanceReferenceFieldCaller.RoutedTemplate)),
            InstanceField(
                nameof(ProfiledReceiverFreeTarget.InstanceReferenceField)),
            "FieldRead",
            new(ProfiledReceiverFreeOriginal.ReadInstanceReferenceField),
            wrapper =>
                new ProfiledReceiverFreeInstanceStringReadHandler(wrapper),
            Original(
                nameof(
                    ProfiledReceiverFreeOriginal
                        .ReadInstanceReferenceField)),
            ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceReferenceFieldTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceReferenceFieldTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledReadInstanceReferenceFieldTag>.Publish,
            () => Pointer(
                typeof(ProfiledReadInstanceReferenceFieldCaller),
                "Invoke"),
            () => _ = ProfiledReadInstanceReferenceFieldCaller.Selected(
                new(1)));
    }

    private static ProfiledReceiverFreeCallerRoute<
        ProfiledReceiverFreeInstanceStringWrite> WriteReferenceFieldRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo caller = Caller(
            typeof(ProfiledWriteInstanceReferenceFieldCaller),
            nameof(ProfiledWriteInstanceReferenceFieldCaller.Selected));
        return new(
            profiler,
            caller,
            Caller(
                typeof(ProfiledWriteInstanceReferenceFieldCaller),
                nameof(ProfiledWriteInstanceReferenceFieldCaller.RoutedTemplate)),
            InstanceField(
                nameof(ProfiledReceiverFreeTarget.InstanceReferenceField)),
            "FieldWrite",
            new(ProfiledReceiverFreeOriginal.WriteInstanceReferenceField),
            wrapper =>
                new ProfiledReceiverFreeInstanceStringWriteHandler(wrapper),
            Original(
                nameof(
                    ProfiledReceiverFreeOriginal
                        .WriteInstanceReferenceField)),
            ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceReferenceFieldTag>.Bind,
            ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceReferenceFieldTag>.Clear,
            ProfiledReceiverFreeRouteState<
                ProfiledWriteInstanceReferenceFieldTag>.Publish,
            () => Pointer(
                typeof(ProfiledWriteInstanceReferenceFieldCaller),
                "Invoke"),
            () => ProfiledWriteInstanceReferenceFieldCaller.Selected(
                new(1),
                "drive"));
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

    private static PropertyInfo StaticProperty() =>
        typeof(ProfiledReceiverFreeTarget).GetProperty(
            nameof(ProfiledReceiverFreeTarget.StaticNumber),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static FieldInfo StaticField() =>
        typeof(ProfiledReceiverFreeTarget).GetField(
            nameof(ProfiledReceiverFreeTarget.StaticField),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static FieldInfo InstanceField(string name) =>
        typeof(ProfiledReceiverFreeTarget).GetField(
            name,
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static ConstructorInfo Constructor() =>
        typeof(ProfiledReceiverFreeTarget).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            [typeof(int)],
            modifiers: null)!;

    private static MethodInfo Original(string name) =>
        typeof(ProfiledReceiverFreeOriginal).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static nint Pointer(Type type, string name) =>
        ProfiledGenericFunctionPointer.Get(type, name);
}
