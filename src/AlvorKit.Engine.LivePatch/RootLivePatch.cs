namespace AlvorKit.Engine;

/// <summary>
/// Explicitly connects the startup-loaded interception profiler to one RootLoop
/// scope graph and exposes its lifecycle through the existing LiveCode host.
/// </summary>
public sealed class RootLivePatch(
    Injector injector,
    RootScope root,
    RootScripts scripts,
    InjectorScopeGraph graph,
    LiveCodeBridgeRegistry bridges)
{
    private bool enabled;
    private LivePatchSession? session;

    /// <summary>Gets whether this process was explicitly launched with the managed profiler bridge path.</summary>
    public static bool IsProfilerConfigured =>
        !string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable(
                InterceptionProfiler.PathEnvironmentVariable));

    /// <summary>Gets the connected scope-aware session after <see cref="Enable"/>.</summary>
    public LivePatchSession Session =>
        session
        ?? throw new InvalidOperationException("Root LivePatch has not been enabled.");

    /// <summary>
    /// Connects to the profiler loaded before Main, registers the LiveCode bridge,
    /// and pumps ReJIT completions at the window safe-frame dispatch point.
    /// </summary>
    public LivePatchSession Enable()
    {
        if (enabled)
            throw new InvalidOperationException("Root LivePatch has already been enabled.");

        var profiler = InterceptionProfiler.Connect();
        session = new(profiler, graph);
        var bridge = new LivePatchLiveCodeBridge(session, graph);
        bridges.Register(bridge);
        injector.Add(session);
        scripts.Add(new RootLivePatchScript(
            session,
            bridge,
            root.Get<WindowLoop>()));
        enabled = true;
        return session;
    }
}
