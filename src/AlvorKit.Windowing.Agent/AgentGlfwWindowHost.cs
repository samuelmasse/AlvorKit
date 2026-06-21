namespace AlvorKit.Windowing;

/// <summary>GLFW window host that switches inherited window behavior to deterministic agent control when requested.</summary>
[ExcludeFromCodeCoverage(Justification = "Bridges deterministic agent state to native GLFW behavior; extracted state and driver logic are covered directly.")]
public class AgentGlfwWindowHost : GlfwWindowHost
{
    /// <summary>The environment variable whose presence selects deterministic agent mode.</summary>
    public const string AgentEnvironmentVariable = "ALVORKIT_WINDOWING_AGENT";

    private readonly TextReader? agentInput;
    private readonly TextWriter? agentOutput;
    private readonly AgentWindowEventDriver agent;
    private readonly AgentWindowScreenshot screenshot;
    private readonly AgentWindowState state = new();
    private readonly bool useAgent;
    private bool disposed;

    /// <summary>Takes ownership of an existing GLFW window and switches to agent mode when requested.</summary>
    public AgentGlfwWindowHost(Glfw glfw, GlfwWindow window, GlLayer gl) : base(glfw, window)
    {
        useAgent = IsAgentEnvironmentPresent();
        state.Initialize(base.ClientSize, base.Title, base.IsVisible, base.IsVSyncEnabled);
        agent = CreateEventDriver();
        if (useAgent)
            base.IsVisible = false;
        screenshot = new(gl);
    }

    /// <summary>Creates a native-free deterministic agent host for tests.</summary>
    internal AgentGlfwWindowHost(
        GlLayer gl,
        Vec2u clientSize,
        string title = "AlvorKit.Windowing",
        bool isVisible = false,
        bool isVSyncEnabled = true,
        TextReader? agentInput = null,
        TextWriter? agentOutput = null,
        Action<GlLayer, Vec2u, string>? screenshotSave = null) : base()
    {
        useAgent = true;
        state.Initialize(clientSize, title, isVisible, isVSyncEnabled);
        agent = CreateEventDriver();
        this.agentInput = agentInput;
        this.agentOutput = agentOutput;
        screenshot = new(gl, screenshotSave);
    }

    /// <summary>Gets whether this host currently runs deterministic agent behavior.</summary>
    internal bool IsAgentMode => useAgent;

    /// <summary>Gets deterministic seconds elapsed since this host entered agent mode.</summary>
    internal double Time => state.Time;

    /// <summary>Gets the number of agent-requested update callbacks issued by this host.</summary>
    internal int UpdateCount => state.UpdateCount;

    /// <summary>Gets the number of agent-requested render callbacks issued by this host.</summary>
    internal int RenderCount => state.RenderCount;

    /// <summary>Gets the number of buffer swaps requested in agent mode.</summary>
    internal int SwapBuffersCount => state.SwapBuffersCount;

    /// <summary>Gets the number of times the agent command loop was started.</summary>
    internal int RunCount => state.RunCount;

    /// <summary>Gets the deterministic event driver used by agent commands.</summary>
    internal AgentWindowEventDriver Agent => agent;

    /// <inheritdoc />
    public override bool IsExiting => useAgent ? state.IsExiting : base.IsExiting;

    /// <inheritdoc />
    public override bool IsFocused => useAgent ? state.IsFocused : base.IsFocused;

    /// <inheritdoc />
    public override bool IsFullscreen => useAgent ? state.IsFullscreen : base.IsFullscreen;

    /// <inheritdoc />
    public override Vec2u MonitorSize => useAgent ? state.MonitorSize : base.MonitorSize;

    /// <inheritdoc />
    public override float MonitorScale => useAgent ? state.MonitorScale : base.MonitorScale;

    /// <inheritdoc />
    public override bool IsVisible { get => useAgent ? state.IsVisible : base.IsVisible; set { if (useAgent) state.IsVisible = value; else base.IsVisible = value; } }

    /// <inheritdoc />
    public override Vec2 MousePosition
    {
        get => useAgent ? state.MousePosition : base.MousePosition;
        set { if (useAgent) state.MousePosition = value; else base.MousePosition = value; }
    }

    /// <inheritdoc />
    public override WindowState WindowState
    {
        get => useAgent ? state.WindowState : base.WindowState;
        set { if (useAgent) state.WindowState = value; else base.WindowState = value; }
    }

    /// <inheritdoc />
    public override WindowCursorMode CursorMode
    {
        get => useAgent ? state.CursorMode : base.CursorMode;
        set { if (useAgent) state.CursorMode = value; else base.CursorMode = value; }
    }

    /// <inheritdoc />
    public override bool IsVSyncEnabled
    {
        get => useAgent ? state.IsVSyncEnabled : base.IsVSyncEnabled;
        set { if (useAgent) state.IsVSyncEnabled = value; else base.IsVSyncEnabled = value; }
    }

    /// <inheritdoc />
    public override string Title { get => useAgent ? state.Title : base.Title; set { if (useAgent) state.Title = value; else base.Title = value; } }

    /// <inheritdoc />
    public override string Clipboard { get => useAgent ? state.Clipboard : base.Clipboard; set { if (useAgent) state.Clipboard = value; else base.Clipboard = value; } }

    /// <inheritdoc />
    public override Vec2u ClientSize
    {
        get => useAgent ? state.ClientSize : base.ClientSize;
        set
        {
            if (!useAgent)
            {
                base.ClientSize = value;
                return;
            }

            SetAgentClientSize(value);
        }
    }

    /// <inheritdoc />
    public override void Close()
    {
        if (!useAgent)
            base.Close();
        else agent.Close();
    }

    /// <inheritdoc />
    public override void SwapBuffers()
    {
        if (!useAgent)
        {
            base.SwapBuffers();
            return;
        }

        state.SwapBuffersCount++;
        if (HasNativeWindow)
            base.SwapBuffers();
    }

    /// <inheritdoc />
    public override void Run()
    {
        if (!useAgent)
        {
            base.Run();
            return;
        }

        state.RunCount++;
        var output = agentOutput ?? Console.Out;
        var runner = new AgentWindowCommandRunner(this, output, CapturePng);
        runner.WriteHelp();
        runner.Run(agentInput ?? Console.In);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        screenshot.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public override nint GetProcAddress(string procname) => HasNativeWindow ? base.GetProcAddress(procname) : procname.Length;

    /// <summary>Renders the current agent frame and writes the selected framebuffer to a PNG file.</summary>
    private void CapturePng(string path)
    {
        agent.Render();
        screenshot.Save(ClientSize, path);
    }

    /// <summary>Creates the composed driver that raises protected host events.</summary>
    /// <returns>The configured event driver.</returns>
    private AgentWindowEventDriver CreateEventDriver() =>
        new(
            state,
            OnClosing,
            OnUpdateFrame,
            OnRenderFrame,
            OnKeyDown,
            OnKeyUp,
            OnMouseDown,
            OnMouseUp,
            OnMouseMove,
            OnMouseWheel,
            OnTextInput,
            OnResize,
            OnMove,
            SetAgentClientSize);

    /// <summary>Applies an agent-mode client size to local state and the hidden native window, when present.</summary>
    /// <param name="size">Requested client size.</param>
    private void SetAgentClientSize(Vec2u size)
    {
        state.ClientSize = size;
        if (HasNativeWindow)
            base.ClientSize = state.ClientSize;
    }

    private static bool IsAgentEnvironmentPresent() => Environment.GetEnvironmentVariable(AgentEnvironmentVariable) is not null;
}
