namespace AlvorKit.Engine;

/// <summary>
/// Explicitly enables the development-only scope graph, loopback host, and root-loop execution pump for an AlvorKit game.
/// </summary>
public sealed class RootLiveCode(
    Injector injector,
    RootScope root,
    RootScripts scripts,
    LiveCodeHostOptions options)
{
    private bool enabled;
    private LiveCodeHost? host;

    /// <summary>Gets the registry used to add discoverable game-specific bridges before or after enabling the host.</summary>
    public LiveCodeBridgeRegistry Bridges { get; } = new();

    /// <summary>Gets the discoverable session after <see cref="Enable"/> starts the host.</summary>
    public LiveCodeSessionManifest Session =>
        host?.Session
        ?? throw new InvalidOperationException("Root LiveCode has not been enabled.");

    /// <summary>
    /// Registers the graph as an unscoped injector dependency and adds a script that owns the host lifetime.
    /// Call this before creating tracked game scopes.
    /// </summary>
    public InjectorScopeGraph Enable()
    {
        if (enabled)
            throw new InvalidOperationException("Root LiveCode has already been enabled.");

        var graph = new InjectorScopeGraph(root, options.Name);
        injector.Add(graph);
        injector.Add(Bridges);
        RegisterBuiltInBridges();
        host = new(graph, options, Bridges);
        scripts.Add(new RootLiveCodeScript(host, root.Get<WindowLoop>()));
        enabled = true;
        return graph;
    }

    private void RegisterBuiltInBridges()
    {
        if (!options.EnableBridges)
            return;

        var windowHost = root.Get<IWindowHost>();
        if (windowHost is AgentGlfwWindowHost agentHost)
        {
            Bridges.Register(new AlvorSenseLiveCodeBridge(
                agentHost,
                root.Get<WindowLoop>(),
                root.Get<RootGl>()));
        }
    }
}
