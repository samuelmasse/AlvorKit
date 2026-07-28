using AlvorKit.Interception.Profiler;

namespace AlvorKit.Interception;

/// <summary>Queues exact method-version operations through the profiler loaded into this process.</summary>
public sealed partial class InterceptionProfiler :
    IInterceptionBackend,
    IInterceptionGenerationBackend
{
    private const string NativeLibraryName =
        "AlvorKit.Interception.Profiler.Native";
    private const uint AbiVersion = 3;
    private static readonly Lock NativeGate = new();
    private static readonly InterceptionCollisionRegistry ProcessCollisionRegistry = new();
    private static long nextRequestId;
    private static long nextPatchId;
    private static string? nativePath;
    private static nint nativeHandle;
    private static bool resolverInstalled;

    private readonly InterceptionProfilerApi api;
    private readonly ConcurrentDictionary<ulong, InterceptionTarget> knownTargets = [];

    private InterceptionProfiler(
        InterceptionProfilerApi api,
        InterceptionCapabilities capabilities)
    {
        this.api = api;
        Capabilities = capabilities;
    }

    /// <summary>The environment variable containing the exact profiler native-library path.</summary>
    public const string PathEnvironmentVariable =
        "ALVORKIT_INTERCEPTION_PROFILER_PATH";

    /// <summary>Gets the connected profiler's negotiated features and limits.</summary>
    public InterceptionCapabilities Capabilities { get; }

    /// <summary>Gets the process-wide neutral claim registry shared by CoreCLR consumers.</summary>
    public InterceptionCollisionRegistry CollisionRegistry =>
        ProcessCollisionRegistry;

    /// <summary>Connects generated bindings to the profiler already loaded by CoreCLR.</summary>
    public static InterceptionProfiler Connect()
    {
        var configuredPath = Environment.GetEnvironmentVariable(
            PathEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException(
                $"{PathEnvironmentVariable} must be an absolute path to the profiler native library.");
        }

        var fullPath = Path.GetFullPath(configuredPath);
        if (!Path.IsPathFullyQualified(configuredPath) || !File.Exists(fullPath))
            throw new FileNotFoundException("The profiler native library was not found.", fullPath);

        ConfigureNativeResolver(fullPath);
        InterceptionProfilerApi api = new InterceptionProfilerBackend();
        var actualAbiVersion = api.GetAbiVersion();
        if (actualAbiVersion != AbiVersion)
        {
            throw new InvalidOperationException(
                $"Profiler ABI {actualAbiVersion} cannot be used by managed ABI {AbiVersion}.");
        }

        Marshal.ThrowExceptionForHR(api.GetCapabilities(out var nativeCapabilities));
        if (nativeCapabilities.AbiVersion != AbiVersion)
            throw new InvalidOperationException("The profiler returned capabilities for a different ABI.");

        var capabilities = new InterceptionCapabilities(
            (InterceptionCapability)nativeCapabilities.Flags,
            nativeCapabilities.MaximumIlBodyBytes,
            nativeCapabilities.MaximumPendingRequests,
            nativeCapabilities.MaximumActivePatches,
            nativeCapabilities.MaximumMetadataBytes,
            nativeCapabilities.MaximumRelocations,
            nativeCapabilities.MaximumIlMapEntries);
        var profiler = new InterceptionProfiler(api, capabilities);
        var state = profiler.GetState();
        if (!state.Ready || state.Stopping)
            throw new InvalidOperationException("The CoreCLR interception profiler is not ready.");
        return profiler;
    }

    /// <summary>Installs one complete replacement method body and returns its reversible handle.</summary>
    public IInterceptionPatchHandle Install(InterceptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var id = NextPatchId();

        var requestId = EnqueueInstall(id, plan);
        knownTargets[id] = plan.Target;
        return new InterceptionPatchHandle(
            this,
            id,
            plan.Target,
            requestId);
    }

    /// <summary>
    /// Installs an exact-signature selector wrapper that calls a managed function pointer on hits
    /// and executes the untouched original IL on misses.
    /// </summary>
    public IInterceptionPatchHandle Install(InterceptionDispatchPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!Capabilities.Flags.HasFlag(InterceptionCapability.ExactDispatch))
            throw new NotSupportedException("The connected profiler does not support exact managed dispatch.");

        var id = NextPatchId();

        var requestId = EnqueueInstall(id, plan);
        knownTargets[id] = plan.Target;
        return new InterceptionPatchHandle(
            this,
            id,
            plan.Target,
            requestId);
    }

    /// <summary>Installs one immutable ABI v3 method generation.</summary>
    public IInterceptionGenerationPatchHandle Install(
        InterceptionGenerationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var required =
            InterceptionCapability.MethodGenerations |
            InterceptionCapability.LateMetadata |
            InterceptionCapability.IlMap |
            InterceptionCapability.BodyIdentity |
            InterceptionCapability.LoadedBody;
        if ((Capabilities.Flags & required) != required)
        {
            throw new NotSupportedException(
                "The connected profiler does not support ABI v3 method generations.");
        }

        var id = NextPatchId();

        var requestId = EnqueueInstall(id, plan);
        knownTargets[id] = plan.Target;
        return new InterceptionPatchHandle(
            this,
            id,
            plan.Target,
            requestId);
    }

    IInterceptionHandlerTrampoline IInterceptionBackend.CreateHandlerTrampoline(
        MethodInfo target,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy) =>
        InterceptionHandlerTrampolineFactory.Create(
            target,
            handlerInstance,
            handlerMethod,
            exceptionPolicy);

    IInterceptionHandlerTrampoline IInterceptionBackend.CreateHandlerTrampoline(
        InterceptionCallShape callShape,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy) =>
        InterceptionHandlerTrampolineFactory.Create(
            callShape,
            handlerInstance,
            handlerMethod,
            exceptionPolicy);

}
