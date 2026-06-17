CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var injector = new Injector();

// The include filter makes this container intentionally local to the demo namespace.
injector.Include(new(@"^AlvorKit\.Injection\.Demo\."));
injector.Add(new FrameClock(240));
injector.Handler(new TextureAtlasHandler("Demo/Atlases/Gameplay.png", 1024, 1024));

Section("Root services");

var cachedApp = injector.Get<GameApp>();
var sameCachedApp = injector.Get<GameApp>();
var freshApp = injector.New<GameApp>();

Print("Get<GameApp>() returns cached instance", ReferenceEquals(cachedApp, sameCachedApp).ToString());
Print("New<GameApp>() returns a new root object", (!ReferenceEquals(cachedApp, freshApp)).ToString());
Print("Cached and fresh apps share renderer", ReferenceEquals(cachedApp.Renderer, freshApp.Renderer).ToString());
Print("Custom atlas handler", cachedApp.Renderer.Atlas.Describe());
Console.WriteLine(cachedApp.StartupLine());

Section("Scoped level services");

var caveScope = injector.Scope<LevelScope>();
caveScope.Add(new LevelState("Crystal Cavern", 1, 12));
var caveHud = caveScope.Get<LevelHud>();

var duneScope = injector.Scope<LevelScope>();
duneScope.Add(new LevelState("Ash Dunes", 2, 7));
var duneHud = duneScope.Get<LevelHud>();

Console.WriteLine(caveHud.StatusLine());
Console.WriteLine(duneHud.StatusLine());
Print("Level HUDs share root renderer", ReferenceEquals(caveHud.Renderer, duneHud.Renderer).ToString());
Print("Level states stay per scope", (!ReferenceEquals(caveHud.State, duneHud.State)).ToString());

Section("Manual instance registration");

var clock = injector.Get<FrameClock>();
Print("Registered clock", clock.Describe());

// Writes a section heading for the demo narrative.
static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

// Writes one aligned demo observation.
static void Print(string label, string value) =>
    Console.WriteLine($"{label,-38} {value}");
