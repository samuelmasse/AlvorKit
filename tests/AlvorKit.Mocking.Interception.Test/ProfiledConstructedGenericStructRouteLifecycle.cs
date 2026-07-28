namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Coordinates exact closed routes behind one rewritten generic caller.</summary>
internal sealed class ProfiledConstructedGenericStructRouteLifecycle :
    IMockInterceptionRouteLifecycle
{
    private const string RouteId =
        "ProfiledConstructedGenericStructCaller.Selected<T>::" +
        "ProfiledConstructedGenericStructTarget<T>.Echo";

    private readonly ProfiledConstructedGenericStructRouteEntry<int> integer;
    private readonly IInterceptionBackend profiler;
    private readonly ProfiledConstructedGenericStructRouteEntry<string> text;
    private IInterceptionPatchHandle? patch;
    private MockInterceptionRoute? route;
    private int rollbackStarted;

    /// <summary>Creates the two independently typed constructions.</summary>
    internal ProfiledConstructedGenericStructRouteLifecycle(
        IInterceptionBackend profiler)
    {
        this.profiler = profiler;
        integer = new(
            Caller(typeof(int)),
            Operation(typeof(int)),
            () => ProfiledGenericFunctionPointer.Get(
                typeof(ProfiledConstructedGenericStructCaller),
                "InvokeInt32"));
        text = new(
            Caller(typeof(string)),
            Operation(typeof(string)),
            () => ProfiledGenericFunctionPointer.Get(
                typeof(ProfiledConstructedGenericStructCaller),
                "InvokeString"));
    }

    /// <summary>Gets the ABI-v3 activation completion.</summary>
    internal InterceptionCompletion? PreparationCompletion { get; private set; }

    /// <summary>Gets the ABI-v3 restoration completion.</summary>
    internal InterceptionCompletion? RemovalCompletion { get; private set; }

    /// <summary>Gets exact integer construction route entries.</summary>
    internal int IntegerRouteEntries => integer.HandlerInvocations;

    /// <summary>Gets exact string construction route entries.</summary>
    internal int StringRouteEntries => text.HandlerInvocations;

    /// <summary>Gets whether reflection retained both closed construction identities.</summary>
    internal bool HasExactConstructedMetadata =>
        integer.Caller.GetGenericArguments().SequenceEqual([typeof(int)]) &&
        text.Caller.GetGenericArguments().SequenceEqual([typeof(string)]) &&
        integer.Operation.DeclaringType ==
            typeof(ProfiledConstructedGenericStructTarget<int>) &&
        text.Operation.DeclaringType ==
            typeof(ProfiledConstructedGenericStructTarget<string>);

    /// <summary>Creates the single definition-wide coordinator route.</summary>
    internal static MockInterceptionRoute[] CreateRoutes() =>
        [new(RouteId)];

    /// <summary>Prepares exact wrappers and installs one inert ABI-v3 generation.</summary>
    public MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute value)
    {
        Validate(value);
        route = value;
        integer.Prepare(profiler, value);
        text.Prepare(profiler, value);
        if (profiler is not InterceptionProfiler coreClr)
        {
            throw new InvalidOperationException(
                "The constructed-generic struct proof requires loaded-body ABI v3.");
        }

        DriveCaller();
        MethodInfo installCaller = integer.Caller;
        InterceptionTarget target =
            InterceptionTarget.FromMethod(installCaller);
        LoadedMethodBodySnapshot body =
            coreClr.GetLoadedMethodBody(target);
        MethodInfo template = typeof(ProfiledConstructedGenericStructCaller)
            .GetMethod(
                nameof(ProfiledConstructedGenericStructCaller.RoutedTemplate),
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(int));
        var generation = new InterceptionGenerationPlan(
            target,
            ReflectionMethodBodyEncoder.Read(template),
            body.Identity,
            1,
            0,
            [],
            []);
        patch = coreClr.Install(generation);
        PreparationCompletion = ProfiledMockProfiler.WaitFor(
            profiler,
            patch.LastRequestId,
            DriveCaller);
        if (PreparationCompletion.Value.State != InterceptionState.Active)
        {
            throw new InvalidOperationException(
                $"Generic struct preparation completed in " +
                $"{PreparationCompletion.Value.State}.");
        }

        return null;
    }

    /// <summary>Publishes both exact constructions behind the shared gate.</summary>
    public MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute value)
    {
        ValidateActive(value);
        integer.Publish();
        text.Publish();
        return null;
    }

    /// <summary>Restores the caller before retiring either trampoline.</summary>
    public void Rollback(MockInterceptionRoute value)
    {
        if (Interlocked.Exchange(ref rollbackStarted, 1) != 0)
            return;
        ValidateActive(value);
        integer.Unpublish();
        text.Unpublish();
        try
        {
            if (patch is not null)
            {
                ulong requestId = patch.Remove();
                RemovalCompletion = ProfiledMockProfiler.WaitFor(
                    profiler,
                    requestId,
                    DriveCaller);
            }
        }
        finally
        {
            try
            {
                patch?.Dispose();
            }
            finally
            {
                text.Retire();
                integer.Retire();
            }
        }
    }

    private static MethodInfo Caller(Type construction) =>
        typeof(ProfiledConstructedGenericStructCaller)
            .GetMethod(
                nameof(ProfiledConstructedGenericStructCaller.Selected),
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(construction);

    private static MethodInfo Operation(Type construction) =>
        typeof(ProfiledConstructedGenericStructTarget<>)
            .MakeGenericType(construction)
            .GetMethod(nameof(
                ProfiledConstructedGenericStructTarget<>.Echo))!;

    private static void DriveCaller()
    {
        var integerTarget =
            new ProfiledConstructedGenericStructTarget<int>(0);
        _ = ProfiledConstructedGenericStructCaller.Selected(
            ref integerTarget,
            1);
        var textTarget =
            new ProfiledConstructedGenericStructTarget<string>("original");
        _ = ProfiledConstructedGenericStructCaller.Selected(
            ref textTarget,
            "value");
    }

    private static void Validate(MockInterceptionRoute value)
    {
        if (!StringComparer.Ordinal.Equals(value.Id, RouteId))
        {
            throw new InvalidOperationException(
                $"Unexpected constructed-generic struct route '{value.Id}'.");
        }
    }

    private void ValidateActive(MockInterceptionRoute value)
    {
        Validate(value);
        if (!ReferenceEquals(value, route))
        {
            throw new InvalidOperationException(
                "Unexpected constructed-generic struct route instance.");
        }
    }
}
