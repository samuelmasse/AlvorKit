namespace AlvorKit.Injection.Demo;

/// <summary>
/// Root application object used to show constructor injection of shared services.
/// </summary>
/// <param name="renderer">Renderer resolved from the root injector.</param>
/// <param name="scenes">Scene catalog resolved from the root injector.</param>
public sealed class GameApp(Renderer renderer, SceneCatalog scenes)
{
    /// <summary>
    /// Renderer shared by cached and newly-created application objects.
    /// </summary>
    public Renderer Renderer => renderer;

    /// <summary>
    /// Builds the startup message by using injected services.
    /// </summary>
    public string StartupLine() =>
        renderer.Draw(scenes.OpeningScene);
}

/// <summary>
/// Renders scene descriptions using the injected asset cache and frame clock.
/// </summary>
/// <param name="assets">Asset cache created by normal constructor injection.</param>
/// <param name="clock">Clock instance registered manually in the root injector.</param>
public sealed class Renderer(AssetCache assets, FrameClock clock)
{
    /// <summary>
    /// Texture atlas supplied through the asset cache.
    /// </summary>
    public TextureAtlas Atlas => assets.Atlas;

    /// <summary>
    /// Produces a text rendering result for the demo scene.
    /// </summary>
    public string Draw(Scene scene) =>
        $"Renderer prepared '{scene.Name}' at {clock.TargetFramesPerSecond} FPS with {assets.Atlas.Name}.";
}

/// <summary>
/// Asset cache that receives its atlas from a custom injection handler.
/// </summary>
/// <param name="atlas">Texture atlas supplied by the custom handler.</param>
public sealed class AssetCache(TextureAtlas atlas)
{
    /// <summary>
    /// Main gameplay atlas.
    /// </summary>
    public TextureAtlas Atlas => atlas;
}

/// <summary>
/// Catalog of scenes available at startup.
/// </summary>
public sealed class SceneCatalog
{
    /// <summary>
    /// First scene opened by the demo application.
    /// </summary>
    public Scene OpeningScene { get; } = new("Main Menu", 0);
}

/// <summary>
/// Scene metadata resolved through the root service graph.
/// </summary>
/// <param name="Name">Display name for the scene.</param>
/// <param name="Difficulty">Small difficulty number shown by the demo.</param>
public sealed record Scene(string Name, int Difficulty);

/// <summary>
/// Clock registered as a prebuilt root service.
/// </summary>
/// <param name="TargetFramesPerSecond">Target frame rate used by demo output.</param>
public sealed record FrameClock(int TargetFramesPerSecond)
{
    /// <summary>
    /// Formats the clock configuration for output.
    /// </summary>
    public string Describe() =>
        $"{TargetFramesPerSecond} FPS target";
}

/// <summary>
/// Texture atlas produced by a custom injection handler.
/// </summary>
/// <param name="Name">Atlas asset path or name.</param>
/// <param name="Width">Atlas width in pixels.</param>
/// <param name="Height">Atlas height in pixels.</param>
public sealed record TextureAtlas(string Name, int Width, int Height)
{
    /// <summary>
    /// Formats the atlas configuration for output.
    /// </summary>
    public string Describe() =>
        $"{Name} ({Width}x{Height})";
}

/// <summary>
/// Custom handler that constructs the texture atlas without giving it a default constructor.
/// </summary>
/// <param name="name">Atlas asset path or name.</param>
/// <param name="width">Atlas width in pixels.</param>
/// <param name="height">Atlas height in pixels.</param>
public sealed class TextureAtlasHandler(string name, int width, int height) : InjectorCustomHandler
{
    /// <summary>
    /// Atlas instance returned for texture atlas requests.
    /// </summary>
    private readonly TextureAtlas atlas = new(name, width, height);

    /// <summary>
    /// Accepts requests for the demo texture atlas.
    /// </summary>
    public override bool Handles(Type type) =>
        type == typeof(TextureAtlas);

    /// <summary>
    /// Returns the configured atlas instance.
    /// </summary>
    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) =>
        atlas;
}

/// <summary>
/// Attribute that marks services which belong to one level scope.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class LevelScopedAttribute : InjectorAttribute;

/// <summary>
/// Child scope whose services are marked with <see cref="LevelScopedAttribute"/>.
/// </summary>
[LevelScoped]
public sealed class LevelScope : InjectorScope<LevelScopedAttribute>;

/// <summary>
/// Per-level state registered manually inside a child scope.
/// </summary>
/// <param name="Name">Level display name.</param>
/// <param name="Number">Level number.</param>
/// <param name="RemainingEnemies">Enemies still present in the level.</param>
[LevelScoped]
public sealed record LevelState(string Name, int Number, int RemainingEnemies);

/// <summary>
/// Scoped HUD service that combines per-level state with root renderer services.
/// </summary>
/// <param name="state">Per-level state resolved from the current child scope.</param>
/// <param name="renderer">Root renderer resolved from the parent scope.</param>
[LevelScoped]
public sealed class LevelHud(LevelState state, Renderer renderer)
{
    /// <summary>
    /// Per-level state owned by the current scope.
    /// </summary>
    public LevelState State => state;

    /// <summary>
    /// Root renderer shared across level scopes.
    /// </summary>
    public Renderer Renderer => renderer;

    /// <summary>
    /// Formats the current level status.
    /// </summary>
    public string StatusLine() =>
        $"Level {state.Number}: {state.Name} has {state.RemainingEnemies} enemies, using {renderer.Atlas.Name}.";
}
