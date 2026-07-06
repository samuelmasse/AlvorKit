namespace AlvorKit.Engine.Loop;

/// <summary>Starts an AlvorKit game root scope and wires it to a window loop.</summary>
[ExcludeFromCodeCoverage]
public static class RootLoop
{
    /// <summary>Creates a default GLFW/OpenGL host and starts the root loop with the requested first state.</summary>
    /// <typeparam name="TState">The concrete <see cref="State"/> type created before the first update.</typeparam>
    /// <param name="inject">Optional callback that seeds caller services; see <see cref="RootArgs.Inject"/>.</param>
    public static void RunGlfw<TState>(Action<Injector>? inject = null) where TState : State =>
        RunGlfw(typeof(TState), inject);

    /// <summary>Creates a default GLFW/OpenGL host and starts the root loop with the requested first state.</summary>
    /// <param name="bootState">The concrete <see cref="State"/> type created before the first update.</param>
    /// <param name="inject">Optional callback that seeds caller services; see <see cref="RootArgs.Inject"/>.</param>
    public static void RunGlfw(Type bootState, Action<Injector>? inject = null)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        var glfw = new GlfwBackend();
        if (!glfw.Init())
            throw new InvalidOperationException("Failed to initialize GLFW.");

        GlfwWindow nativeWindow = default;
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
                glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
                glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
            }

            glfw.WindowHint(GlfwWindowHint.Visible, false);
            if (IsAgentEnvironmentPresent())
                glfw.WindowHint(GlfwWindowHint.Decorated, false);

            var primaryMonitor = glfw.GetPrimaryMonitor();
            glfw.GetMonitorWorkarea(primaryMonitor, out _, out _, out var monitorWidth, out var monitorHeight);
            nativeWindow = glfw.CreateWindow(monitorWidth / 4 * 3, monitorHeight / 4 * 3, "Window", default, default);
            if (nativeWindow == default)
                throw new InvalidOperationException("Failed to create the GLFW window.");

            glfw.MakeContextCurrent(nativeWindow);
            glfw.SwapInterval(0);

            var gl = new GlBackend(glfw.GetProcAddress);
            var rgl = new RootGl(gl);
            using var window = new AgentGlfwWindowHost(glfw, nativeWindow, rgl);
            var handle = nativeWindow;

            Run(() => new RootArgs
            {
                Window = window,
                Gl = rgl,
                BootState = bootState,
                Inject = injector =>
                {
                    injector.Add<Gl>(gl);
                    injector.Add<Glfw>(glfw);
                    injector.Add(handle);
                    inject?.Invoke(injector);
                },
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
        EnableDedicatedGpu.Run();

        var rootArgs = args();
        var window = new WindowLoop(rootArgs.Window);
        var injector = new Injector();
        injector.Add<Fn>(new FnBackend());
        injector.Add<Ft>(new FtBackend());
        injector.Add<Ma>(new MaBackend());
        injector.Add<Xxh>(new XxhBackend());
        var root = injector.Scope<RootScope>();
        var gl = rootArgs.Gl;

        root.Add(rootArgs);
        root.Add(gl);
        root.Add(new RootCanvas(window));
        root.Add(new RootControls(window));
        root.Add(new RootGamepads(window));
        root.Add(new RootInput(window));
        root.Add(new RootKeyboard(window));
        root.Add(new RootMouse(window));
        root.Add(new RootScreen(window));
        root.Add(new RootSprites(new(gl)));
        injector.Handler(root.Get<RootControlListInjector>());
        rootArgs.Inject?.Invoke(injector);

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

    private static bool IsAgentEnvironmentPresent() =>
        Environment.GetEnvironmentVariable(AgentGlfwWindowHost.AgentEnvironmentVariable) is not null;
}
