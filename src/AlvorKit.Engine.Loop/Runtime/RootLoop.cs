namespace AlvorKit.Engine.Loop;

/// <summary>Starts an AlvorKit game root scope and wires it to a window loop.</summary>
[ExcludeFromCodeCoverage]
public static class RootLoop
{
    /// <summary>Builds root arguments, creates the root scope, and runs the host window loop.</summary>
    public static void Run(Func<RootArgs> args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        var rootArgs = args();
        var window = new WindowLoop(rootArgs.Window);
        var injector = new Injector();
        var root = injector.Scope<RootScope>();

        root.Add(rootArgs);
        root.Add(rootArgs.Gl);
        root.Add(new RootCanvas(window));
        root.Add(new RootControls(window));
        root.Add(new RootInput(window));
        root.Add(new RootKeyboard(window));
        root.Add(new RootMouse(window));
        root.Add(new RootScreen(window));
        root.Add(new RootSprites(new(rootArgs.Gl)));
        root.Handler(root.Get<RootControlListInjector>());

        var state = root.Get<RootState>();
        state.Current = (State)root.New(rootArgs.BootState);

        var engine = root.Get<RootEngine>();
        engine.Load();
        window.Update += engine.Update;
        window.Frame += engine.Frame;
        window.Render += engine.Render;
        window.Unload += engine.Unload;
        window.Run();
    }
}
