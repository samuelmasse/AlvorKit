namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Runs a living multi-scope colony observatory inside the normal AlvorKit engine loop.</summary>
[Root]
internal sealed class ObservatoryState(
    Injector injector,
    RootScope root,
    RootScripts scripts,
    RootScreen screen,
    RootInput input,
    RootCanvas canvas,
    RootBackbuffer backbuffer,
    ObservatoryInput interaction,
    ObservatoryRenderer renderer,
    ObservatoryFreeze freeze) : State
{
    private RootLiveCode liveCode = null!;
    private InjectorScopeGraph graph = null!;
    private UniverseColonies universe = null!;
    private InjectorScopeGraphSnapshot graphSnapshot = null!;
    private LivePatchSession? livePatchSession;
    private LivePatchSnapshot[] patches = [];
    private long graphRevision = -1;

    /// <inheritdoc />
    public override void Load()
    {
        screen.Title = "AlvorKit.Engine.LiveCode.Demo — Mycelial Scope Observatory";
        input.Track = true;
        input.CursorMode = CursorMode.Normal;

        liveCode = new(
            injector,
            root,
            scripts,
            new("mycelial-observatory")
            {
                GlobalUsings =
                [
                    "System",
                    "AlvorKit.Engine",
                    "AlvorKit.Engine.LiveCode.Demo",
                    "AlvorKit.Injection",
                    "AlvorKit.LiveCode",
                    "AlvorKit.LivePatch",
                    "AlvorKit.Maths"
                ],
                FrozenInspection = new()
                {
                    FreezeThreshold = TimeSpan.FromSeconds(1)
                }
            });
        graph = liveCode.Enable();
        if (RootLivePatch.IsProfilerConfigured)
        {
            livePatchSession = new RootLivePatch(
                injector,
                root,
                scripts,
                graph,
                liveCode.Bridges).Enable();
        }
        universe = root.Get<UniverseColonies>();
        liveCode.Bridges.Register(new ObservatoryLiveBridge(universe));
        SeedUniverse();
        RefreshGraph();

        var session = liveCode.Session;
        Console.WriteLine("MYCELIAL SCOPE OBSERVATORY");
        Console.WriteLine($"LiveCode session: {session.Name} ({session.SessionId})");
        Console.WriteLine($"Loopback endpoint: 127.0.0.1:{session.Port}");
        Console.WriteLine("Use Tab/arrows/Space/B/L/F or the mouse while the same process stays live.");
        Console.WriteLine("F deliberately freezes the game loop; only `frozen exec` can inspect or release it.");
        Console.WriteLine("Run the checked-in submissions through scripts/AlvorKit.Script.LiveCode.");

        screen.IsVisible = true;
    }

    /// <inheritdoc />
    public override void Unload()
    {
        universe?.CloseAll();
    }

    /// <inheritdoc />
    public override void Update(double delta)
    {
        interaction.Update(universe, canvas.Size, delta);
        universe.Update(delta);
        if (livePatchSession is not null && universe.Clock.Tick % 12 == 0)
            patches = livePatchSession.List();
        if (graph.Revision != graphRevision)
            RefreshGraph();
        freeze.BlockIfRequested();
    }

    /// <inheritdoc />
    public override void Render() => backbuffer.Clear((0.012f, 0.018f, 0.045f, 1f));

    /// <inheritdoc />
    public override void Draw() =>
        renderer.Draw(
            universe,
            graphSnapshot,
            liveCode.Session,
            patches,
            canvas.Size);

    private void SeedUniverse()
    {
        var ember = universe.Open(
            "Ember Vault",
            "E",
            (0.2f, 0.28f),
            (1f, 0.34f, 0.14f, 1f),
            (1f, 0.78f, 0.18f, 1f));
        ember.Garden.RotationSpeed = 0.74f;
        ember.Sky.Weather = "cinder motes";

        var tide = universe.Open(
            "Tide Archive",
            "T",
            (0.78f, 0.3f),
            (0.12f, 0.72f, 1f, 1f),
            (0.18f, 1f, 0.84f, 1f));
        tide.Garden.RotationSpeed = -0.52f;
        tide.Garden.SporeCount = 24;
        tide.Sky.Weather = "memory rain";

        var moon = universe.Open(
            "Moon Garden",
            "M",
            (0.49f, 0.76f),
            (0.68f, 0.3f, 1f, 1f),
            (1f, 0.28f, 0.76f, 1f));
        moon.Garden.RotationSpeed = 0.95f;
        moon.Garden.OrbitRadius = 102f;
        moon.Sky.Weather = "violet sleep";
    }

    private void RefreshGraph()
    {
        graphSnapshot = graph.Snapshot(includeEnded: true);
        graphRevision = graphSnapshot.Revision;
    }
}
