namespace AlvorKit;

/// <summary>
/// Enables a development loopback host and root-loop execution pump on the game's scope graph.
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
    /// Starts the host on the existing scope graph and adds a script that owns the host lifetime.
    /// </summary>
    public InjectorScopeGraph Enable()
    {
        if (enabled)
            throw new InvalidOperationException("Root LiveCode has already been enabled.");

        var graph = root.Get<InjectorScopeGraph>();
        graph.Label(root, options.Name);
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
