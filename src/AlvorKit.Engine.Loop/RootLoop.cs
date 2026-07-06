namespace AlvorKit.Engine.Loop;

/// <summary>Starts an AlvorKit game root scope and wires it to a window loop.</summary>
[ExcludeFromCodeCoverage]
public static class RootLoop
{
    /// <summary>Creates a default GLFW/OpenGL host and starts the root loop with the requested first state.</summary>
    /// <typeparam name="TState">The concrete <see cref="State"/> type created before the first update.</typeparam>
    /// <param name="inject">Optional callback that seeds caller services after built-in registrations.</param>
    public static void RunGlfw<TState>(Action<Injector>? inject = null) where TState : State =>
        RunGlfw(typeof(TState), inject);

    /// <summary>Creates a default GLFW/OpenGL host and starts the root loop with the requested first state.</summary>
    /// <param name="bootState">The concrete <see cref="State"/> type created before the first update.</param>
    /// <param name="inject">Optional callback that seeds caller services after built-in registrations.</param>
    public static void RunGlfw(Type bootState, Action<Injector>? inject = null)
    {
        ConfigureProcess();
        var glfw = CreateGlfw();
        ConfigureGlfw(glfw);

        var nativeWindow = CreateNativeWindow(glfw);
        RunGlfwWindow(glfw, nativeWindow, bootState, inject);
        glfw.DestroyWindow(nativeWindow);
        glfw.Terminate();
    }

    private static void Run(IWindowHost host, RootGl gl, Type bootState, Action<Injector>? inject)
    {
        var window = new WindowLoop(host);
        var injector = CreateInjector();
        var root = CreateRootScope(injector, window, gl);
        inject?.Invoke(injector);

        var state = root.Get<RootState>();
        state.Current = (State)root.New(bootState);

        var engine = StartEngine(root);
        ConnectEngine(window, engine);
        window.Run();
    }

    private static void ConfigureProcess()
    {
        EnableDedicatedGpu.Run();
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
    }

    private static GlfwBackend CreateGlfw()
    {
        var glfw = new GlfwBackend();
        if (!glfw.Init())
            throw new InvalidOperationException("Failed to initialize GLFW.");

        return glfw;
    }

    private static void ConfigureGlfw(GlfwBackend glfw)
    {
        ConfigureMacOsGlfw(glfw);
        glfw.WindowHint(GlfwWindowHint.Visible, false);

        if (IsAgentEnvironmentPresent())
            glfw.WindowHint(GlfwWindowHint.Decorated, false);
    }

    private static void ConfigureMacOsGlfw(GlfwBackend glfw)
    {
        if (!OperatingSystem.IsMacOS())
            return;

        glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 3);
        glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 3);
        glfw.WindowHint(GlfwWindowHint.OpenGLProfile, GlfwOpenGLProfile.CoreProfile);
    }

    private static GlfwWindow CreateNativeWindow(GlfwBackend glfw)
    {
        var (width, height) = GetWindowSize(glfw);
        var window = glfw.CreateWindow(width, height, "Window", default, default);

        if (window == default)
            throw new InvalidOperationException("Failed to create the GLFW window.");

        return window;
    }

    private static void RunGlfwWindow(
        GlfwBackend glfw,
        GlfwWindow nativeWindow,
        Type bootState,
        Action<Injector>? inject)
    {
        MakeCurrent(glfw, nativeWindow);

        var gl = new GlBackend(glfw.GetProcAddress);
        var rootGl = new RootGl(gl);
        using var window = new AgentGlfwWindowHost(glfw, nativeWindow, rootGl);

        Run(window, rootGl, bootState, injector =>
        {
            injector.Add<Gl>(gl);
            injector.Add<Glfw>(glfw);
            injector.Add(nativeWindow);
            inject?.Invoke(injector);
        });
    }

    private static (int Width, int Height) GetWindowSize(GlfwBackend glfw)
    {
        var primaryMonitor = glfw.GetPrimaryMonitor();
        glfw.GetMonitorWorkarea(primaryMonitor, out _, out _, out var width, out var height);
        return (width / 4 * 3, height / 4 * 3);
    }

    private static void MakeCurrent(GlfwBackend glfw, GlfwWindow window)
    {
        glfw.MakeContextCurrent(window);
        glfw.SwapInterval(0);
    }

    private static Injector CreateInjector()
    {
        var injector = new Injector();
        injector.Add<Fn>(new FnBackend());
        injector.Add<Ft>(new FtBackend());
        injector.Add<Ma>(new MaBackend());
        injector.Add<Xxh>(new XxhBackend());
        return injector;
    }

    private static RootScope CreateRootScope(Injector injector, WindowLoop window, RootGl gl)
    {
        var root = injector.Scope<RootScope>()
            .With(gl)
            .With(new RootCanvas(window))
            .With(new RootControls(window))
            .With(new RootGamepads(window))
            .With(new RootInput(window))
            .With(new RootKeyboard(window))
            .With(new RootMouse(window))
            .With(new RootScreen(window))
            .With(new RootSprites(new(gl)));

        injector.Handler(root.Get<RootControlListInjector>());
        return root;
    }

    private static RootEngine StartEngine(RootScope root)
    {
        var engine = root.Get<RootEngine>();
        engine.Load();
        return engine;
    }

    private static void ConnectEngine(WindowLoop window, RootEngine engine)
    {
        window.Update += engine.Update;
        window.Frame += engine.Frame;
        window.Render += engine.Render;
        window.Unload += engine.Unload;
    }

    private static bool IsAgentEnvironmentPresent() =>
        Environment.GetEnvironmentVariable(AgentGlfwWindowHost.AgentEnvironmentVariable) is not null;
}
