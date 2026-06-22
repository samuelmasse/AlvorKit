namespace AlvorKit.Engine.Loop;

/// <summary>Starts an AlvorKit game root scope and wires it to a window loop.</summary>
[ExcludeFromCodeCoverage]
public static class RootLoop
{
    /// <summary>Creates a default GLFW/OpenGL host and starts the root loop with the requested first state.</summary>
    /// <typeparam name="TState">The concrete <see cref="State"/> type created before the first update.</typeparam>
    public static void RunGlfw<TState>() where TState : State => RunGlfw(typeof(TState));

    /// <summary>Creates a default GLFW/OpenGL host and starts the root loop with the requested first state.</summary>
    public static void RunGlfw(Type bootState)
    {
        var glfw = new GlfwBackend();
        if (!glfw.Init())
            throw new InvalidOperationException("Failed to initialize GLFW.");

        GlfwWindow nativeWindow = default;
        try
        {
            glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
            glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
            glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
            glfw.WindowHint(GlfwWindowHint.Visible, false);

            var primaryMonitor = glfw.GetPrimaryMonitor();
            glfw.GetMonitorWorkarea(primaryMonitor, out _, out _, out var monitorWidth, out var monitorHeight);
            nativeWindow = glfw.CreateWindow(monitorWidth / 4 * 3, monitorHeight / 4 * 3, "Window", default, default);
            if (nativeWindow == default)
                throw new InvalidOperationException("Failed to create the GLFW window.");

            glfw.MakeContextCurrent(nativeWindow);
            glfw.SwapInterval(0);

            var gl = new RootGl(new GlBackend(glfw.GetProcAddress));
            var window = new AgentGlfwWindowHost(glfw, nativeWindow, gl);

            Run(() => new()
            {
                Window = window,
                Gl = gl,
                BootState = bootState,
            });
        }
        finally
        {
            if (nativeWindow != default)
                glfw.DestroyWindow(nativeWindow);
            glfw.Terminate();
        }
    }

    /// <summary>Builds root arguments, creates the root scope, and runs the host window loop.</summary>
    public static void Run(Func<RootArgs> args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        EnableDedicatedGpu.Run();

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
        injector.Handler(root.Get<RootControlListInjector>());

        var state = root.Get<RootState>();
        state.Current = (State)root.New(rootArgs.BootState);

        var engine = root.Get<RootEngine>();
        engine.Load();
        var unloaded = false;
        void Unload()
        {
            if (unloaded)
                return;

            unloaded = true;
            engine.Unload();
        }

        window.Update += engine.Update;
        window.Frame += engine.Frame;
        window.Render += engine.Render;
        window.Unload += Unload;
        try
        {
            window.Run();
        }
        finally
        {
            Unload();
        }
    }
}
