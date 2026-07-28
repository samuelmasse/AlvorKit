namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates the three generic caller definitions in the concrete basic row.</summary>
internal sealed class ProfiledGenericsRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string ClosedEchoRouteId =
        "ProfiledClosedGenericEchoCaller.Selected<T>::ProfiledGenericTarget<T>.Echo";
    private const string ClosedValueRouteId =
        "ProfiledClosedGenericValueCaller.Selected<T>::ProfiledGenericTarget<T>.get_Value";
    private const string ConstructedEchoRouteId =
        "ProfiledConstructedGenericEchoCaller.Selected<T>::ProfiledConstructedGenericTarget.Echo<T>";

    private readonly Dictionary<string, IProfiledOwnedCallerRoute> routes;

    /// <summary>Creates the three generic caller owners over the checked-in profiler.</summary>
    internal ProfiledGenericsRouteLifecycle(IInterceptionBackend profiler)
    {
        routes = new(StringComparer.Ordinal)
        {
            [ClosedEchoRouteId] = ClosedEchoRoute(profiler),
            [ClosedValueRouteId] = ClosedValueRoute(profiler),
            [ConstructedEchoRouteId] = ConstructedEchoRoute(profiler),
        };
    }

    /// <summary>Gets whether every generic caller reached inert active preparation.</summary>
    internal bool AllPrepared =>
        routes.Values.All(route =>
            route.PreparationCompletion?.State ==
            InterceptionState.Active);

    /// <summary>Gets whether every generic caller was restored during rollback.</summary>
    internal bool AllRemoved =>
        routes.Values.All(route =>
            route.RemovalCompletion?.State ==
            InterceptionState.Removed);

    /// <summary>Creates the stable coordinator routes for the generic scenario.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
    [
        new(ClosedEchoRouteId),
        new(ClosedValueRouteId),
        new(ConstructedEchoRouteId),
    ];

    /// <summary>Prepares one generic caller selected by stable identity.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route) =>
        Resolve(route).Prepare(route);

    /// <summary>Publishes one generic caller's constructions behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route) =>
        Resolve(route).Activate(route);

    /// <summary>Restores one generic caller during reverse-order rollback.</summary>
    public void Rollback(MockInterceptionRoute route) =>
        Resolve(route).Rollback(route);

    private IProfiledOwnedCallerRoute Resolve(
        MockInterceptionRoute route) =>
        routes.TryGetValue(route.Id, out var owned)
            ? owned
            : throw new InvalidOperationException(
                $"Unexpected generic route '{route.Id}'.");

    private static ProfiledGenericCallerRoute ClosedEchoRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo integerCaller = Caller(
            typeof(ProfiledClosedGenericEchoCaller),
            nameof(ProfiledClosedGenericEchoCaller.Selected),
            typeof(int));
        MethodInfo stringCaller = Caller(
            typeof(ProfiledClosedGenericEchoCaller),
            nameof(ProfiledClosedGenericEchoCaller.Selected),
            typeof(string));
        return new(
            profiler,
            integerCaller,
            Caller(
                typeof(ProfiledClosedGenericEchoCaller),
                nameof(ProfiledClosedGenericEchoCaller.RoutedTemplate),
                typeof(int)),
            DriveClosedEcho,
            new ProfiledGenericConstructionRoute<
                ProfiledClosedGenericEchoOperation<int>>(
                integerCaller,
                typeof(ProfiledGenericTarget<int>).GetMethod(
                    nameof(ProfiledGenericTarget<>.Echo))!,
                new ProfiledClosedGenericEchoOperation<int>(
                    ProfiledGenericOriginal.ClosedEcho),
                wrapper =>
                    new ProfiledClosedGenericEchoHandler<int>(wrapper),
                ProfiledClosedGenericEchoRoute<int>.Bind,
                ProfiledClosedGenericEchoRoute<int>.Clear,
                ProfiledClosedGenericEchoRoute<int>.Publish,
                () => Pointer(
                    typeof(ProfiledClosedGenericEchoCaller),
                    "InvokeInt32")),
            new ProfiledGenericConstructionRoute<
                ProfiledClosedGenericEchoOperation<string>>(
                stringCaller,
                typeof(ProfiledGenericTarget<string>).GetMethod(
                    nameof(ProfiledGenericTarget<>.Echo))!,
                new ProfiledClosedGenericEchoOperation<string>(
                    ProfiledGenericOriginal.ClosedEcho),
                wrapper =>
                    new ProfiledClosedGenericEchoHandler<string>(wrapper),
                ProfiledClosedGenericEchoRoute<string>.Bind,
                ProfiledClosedGenericEchoRoute<string>.Clear,
                ProfiledClosedGenericEchoRoute<string>.Publish,
                () => Pointer(
                    typeof(ProfiledClosedGenericEchoCaller),
                    "InvokeString")));
    }

    private static ProfiledGenericCallerRoute ClosedValueRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo integerCaller = Caller(
            typeof(ProfiledClosedGenericValueCaller),
            nameof(ProfiledClosedGenericValueCaller.Selected),
            typeof(int));
        MethodInfo stringCaller = Caller(
            typeof(ProfiledClosedGenericValueCaller),
            nameof(ProfiledClosedGenericValueCaller.Selected),
            typeof(string));
        return new(
            profiler,
            integerCaller,
            Caller(
                typeof(ProfiledClosedGenericValueCaller),
                nameof(ProfiledClosedGenericValueCaller.RoutedTemplate),
                typeof(int)),
            DriveClosedValue,
            new ProfiledGenericConstructionRoute<
                ProfiledClosedGenericValueOperation<int>>(
                integerCaller,
                typeof(ProfiledGenericTarget<int>).GetProperty(
                    nameof(ProfiledGenericTarget<>.Value))!.GetMethod!,
                new ProfiledClosedGenericValueOperation<int>(
                    ProfiledGenericOriginal.ClosedValue),
                wrapper =>
                    new ProfiledClosedGenericValueHandler<int>(wrapper),
                ProfiledClosedGenericValueRoute<int>.Bind,
                ProfiledClosedGenericValueRoute<int>.Clear,
                ProfiledClosedGenericValueRoute<int>.Publish,
                () => Pointer(
                    typeof(ProfiledClosedGenericValueCaller),
                    "InvokeInt32")),
            new ProfiledGenericConstructionRoute<
                ProfiledClosedGenericValueOperation<string>>(
                stringCaller,
                typeof(ProfiledGenericTarget<string>).GetProperty(
                    nameof(ProfiledGenericTarget<>.Value))!.GetMethod!,
                new ProfiledClosedGenericValueOperation<string>(
                    ProfiledGenericOriginal.ClosedValue),
                wrapper =>
                    new ProfiledClosedGenericValueHandler<string>(wrapper),
                ProfiledClosedGenericValueRoute<string>.Bind,
                ProfiledClosedGenericValueRoute<string>.Clear,
                ProfiledClosedGenericValueRoute<string>.Publish,
                () => Pointer(
                    typeof(ProfiledClosedGenericValueCaller),
                    "InvokeString")));
    }

    private static ProfiledGenericCallerRoute ConstructedEchoRoute(
        IInterceptionBackend profiler)
    {
        MethodInfo integerCaller = Caller(
            typeof(ProfiledConstructedGenericEchoCaller),
            nameof(ProfiledConstructedGenericEchoCaller.Selected),
            typeof(int));
        MethodInfo stringCaller = Caller(
            typeof(ProfiledConstructedGenericEchoCaller),
            nameof(ProfiledConstructedGenericEchoCaller.Selected),
            typeof(string));
        MethodInfo operation = typeof(ProfiledConstructedGenericTarget)
            .GetMethod(nameof(ProfiledConstructedGenericTarget.Echo))!;
        return new(
            profiler,
            integerCaller,
            Caller(
                typeof(ProfiledConstructedGenericEchoCaller),
                nameof(ProfiledConstructedGenericEchoCaller.RoutedTemplate),
                typeof(int)),
            DriveConstructedEcho,
            new ProfiledGenericConstructionRoute<
                ProfiledConstructedGenericEchoOperation<int>>(
                integerCaller,
                operation.MakeGenericMethod(typeof(int)),
                new ProfiledConstructedGenericEchoOperation<int>(
                    ProfiledGenericOriginal.ConstructedEcho),
                wrapper =>
                    new ProfiledConstructedGenericEchoHandler<int>(wrapper),
                ProfiledConstructedGenericEchoRoute<int>.Bind,
                ProfiledConstructedGenericEchoRoute<int>.Clear,
                ProfiledConstructedGenericEchoRoute<int>.Publish,
                () => Pointer(
                    typeof(ProfiledConstructedGenericEchoCaller),
                    "InvokeInt32")),
            new ProfiledGenericConstructionRoute<
                ProfiledConstructedGenericEchoOperation<string>>(
                stringCaller,
                operation.MakeGenericMethod(typeof(string)),
                new ProfiledConstructedGenericEchoOperation<string>(
                    ProfiledGenericOriginal.ConstructedEcho),
                wrapper =>
                    new ProfiledConstructedGenericEchoHandler<string>(
                        wrapper),
                ProfiledConstructedGenericEchoRoute<string>.Bind,
                ProfiledConstructedGenericEchoRoute<string>.Clear,
                ProfiledConstructedGenericEchoRoute<string>.Publish,
                () => Pointer(
                    typeof(ProfiledConstructedGenericEchoCaller),
                    "InvokeString")));
    }

    private static void DriveClosedEcho()
    {
        _ = ProfiledClosedGenericEchoCaller.Selected(
            new ProfiledGenericTarget<int>(0),
            1);
        _ = ProfiledClosedGenericEchoCaller.Selected(
            new ProfiledGenericTarget<string>("original"),
            "value");
    }

    private static void DriveClosedValue()
    {
        _ = ProfiledClosedGenericValueCaller.Selected(
            new ProfiledGenericTarget<int>(0));
        _ = ProfiledClosedGenericValueCaller.Selected(
            new ProfiledGenericTarget<string>("original"));
    }

    private static void DriveConstructedEcho()
    {
        var target = new ProfiledConstructedGenericTarget();
        _ = ProfiledConstructedGenericEchoCaller.Selected(target, 1);
        _ = ProfiledConstructedGenericEchoCaller.Selected(target, "value");
    }

    private static MethodInfo Caller(
        Type type,
        string name,
        Type construction) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(construction);

    private static nint Pointer(Type type, string name) =>
        ProfiledGenericFunctionPointer.Get(type, name);

}
