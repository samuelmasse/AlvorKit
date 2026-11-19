namespace AlvorKit;

/// <summary>Starts an AlvorKit game root scope and wires it to a window loop.</summary>
[ExcludeFromCodeCoverage]
public static class RootLoop
{
    internal const string AudioSilentEnvironmentVariable = "ALVORKIT_AUDIO_SILENT";

    /// <summary>Starts a GLFW/OpenGL host, applying setups in order before creating the first state.</summary>
    public static void RunGlfw<TState>(params ReadOnlySpan<IRootSetup> setups) where TState : State =>
        RunGlfw(typeof(TState), setups);

    /// <summary>Configures the root scope before applying setups and creating the first state.</summary>
    public static void RunGlfw<TState>(ReadOnlySpan<IRootSetup> setups, Action<RootScope> configureRoot) where TState : State =>
        RunGlfw(typeof(TState), setups, configureRoot);

    /// <summary>Starts a GLFW/OpenGL host, applying setups in order before creating the requested state.</summary>
    public static void RunGlfw(Type bootState, params ReadOnlySpan<IRootSetup> setups) =>
        RunGlfw(bootState, setups, static _ => { });

    /// <summary>Configures the root scope before applying setups and creating the requested state.</summary>
    public static void RunGlfw(Type bootState, ReadOnlySpan<IRootSetup> setups, Action<RootScope> configureRoot)
    {
        using var host = RootHost.Start();

        ConfigureProcess();
        var glfw = CreateGlfw();
        ConfigureGlfw(glfw);

        var nativeWindow = CreateNativeWindow(glfw);
        RunGlfwWindow(glfw, nativeWindow, host, bootState, setups, configureRoot);
        glfw.DestroyWindow(nativeWindow);
        glfw.Terminate();
    }

    /// <summary>Loads the boot state and runs the window loop.</summary>
    private static void Run(
        WindowLoop window,
        RootScope root,
        Type bootState,
        ReadOnlySpan<IRootSetup> setups,
        Action<RootScope> configureRoot)
    {
        configureRoot(root);
        root.Add(new RootFiles(root.Get<RootFs>(), [AppContext.BaseDirectory]));
        LoadState(root, bootState, setups);

        var engine = StartEngine(root);
        ConnectEngine(window, engine);
        window.Run();
    }

    internal static void LoadState(RootScope root, Type bootState, ReadOnlySpan<IRootSetup> setups)
    {
        foreach (var setup in setups)
            setup.Apply(root);

        root.Get<RootState>().Current = (State)root.New(bootState);
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

    /// <summary>Creates GLFW-backed hosts and runs the application in <paramref name="nativeWindow"/>.</summary>
    private static void RunGlfwWindow(
        GlfwBackend glfw,
        GlfwWindow nativeWindow,
        RootHost host,
        Type bootState,
        ReadOnlySpan<IRootSetup> setups,
        Action<RootScope> configureRoot)
    {
        MakeCurrent(glfw, nativeWindow);

        var gl = new GlBackend(glfw.GetProcAddress);
        var rootGl = new RootGl(gl);
        using var fileDialogs = new GlfwFileDialogHost(glfw, nativeWindow);
        using var windowHost = new AgentGlfwWindowHost(glfw, nativeWindow, rootGl);
        var window = new WindowLoop(windowHost);

        var injector = host.Injector;
        injector.Add<Ft>(new FtBackend());
        injector.Add<Ma>(CreateAudioBackend());
        injector.Add<Gl>(gl);
        injector.Add<Glfw>(glfw);
        injector.Add(nativeWindow);
        injector.Add<IWindowHost>(windowHost);
        injector.Add<WindowLoop>(window);
        var root = host.Root;
        ConfigureWindowServices(root, window, fileDialogs, rootGl);
        injector.Handler(root.Get<RootControlListInjector>());
        Run(window, root, bootState, setups, configureRoot);
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

    internal static Ma CreateAudioBackend()
    {
        var backend = new MaBackend();
        return IsAudioSilentEnvironmentEnabled() ? new AgentSilentMa(backend) : backend;
    }

    /// <summary>Registers the window, input, and graphics services on the desktop root.</summary>
    private static void ConfigureWindowServices(
        RootScope root,
        WindowLoop window,
        IFileDialogHost fileDialogs,
        RootGl gl)
    {
        root.With(gl)
            .With(new RootCanvas(window))
            .With(new RootControls(window))
            .With(new RootGamepads(window))
            .With(new RootInput(window))
            .With(new RootKeyboard(window))
            .With(new RootMouse(window))
            .With(new RootFileDialogs(fileDialogs))
            .With(new RootScreen(window))
            .With(new RootSprites(new(gl)));
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

    private static bool IsAudioSilentEnvironmentEnabled()
    {
        var value = Environment.GetEnvironmentVariable(AudioSilentEnvironmentVariable);
        if (value is not null)
            return value != "0";

        return IsAgentEnvironmentPresent();
    }
}
