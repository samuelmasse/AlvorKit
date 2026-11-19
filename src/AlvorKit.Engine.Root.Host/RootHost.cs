namespace AlvorKit;

/// <summary>Owns the engine injector, root scope, and application log for one host session.</summary>
public class RootHost : IDisposable
{
    /// <summary>Registers backends shared by the host and its child scopes.</summary>
    private readonly Injector injector = new();
    /// <summary>Parents the game's scope graph.</summary>
    private readonly RootScope root;
    /// <summary>Drains the session's log when the host is disposed.</summary>
    private readonly LogRuntime logging = new();

    /// <summary>Gets the injector used to register host-specific backends.</summary>
    public Injector Injector => injector;

    /// <summary>Gets the parent scope for the application's tracked scopes.</summary>
    public RootScope Root => root;

    /// <summary>Registers the shared services before starting the logging worker.</summary>
    private RootHost()
    {
        var backend = new FnBackend();
        injector.Add<Fn>(backend);
        injector.Add<FnGraph>(new FnGraph(backend));
        injector.Add<Log>(logging.Log);
        root = injector.Scope<RootScope>();
        injector.Add<InjectorScopeGraph>(new InjectorScopeGraph(root));
        logging.Start();
    }

    /// <summary>Creates the engine root with its FastNoise2 backend and starts logging.</summary>
    public static RootHost Start() => new();

    /// <summary>Stops the logging worker and flushes the remaining entries.</summary>
    public void Dispose() => logging.Dispose();
}
