namespace AlvorKit.Windowing;

/// <summary>GLFW window host that switches inherited window behavior to deterministic agent control when requested.</summary>
[ExcludeFromCodeCoverage(Justification = "Bridges deterministic agent state to native GLFW behavior; extracted state and driver logic are covered directly.")]
public class AgentGlfwWindowHost : GlfwWindowHost
{
    /// <summary>The environment variable whose presence selects deterministic agent mode.</summary>
    public const string AgentEnvironmentVariable = "ALVORKIT_WINDOWING_AGENT";
    /// <summary>Raises deterministic input and frame events for this host.</summary>
    private readonly AgentWindowEventDriver agent;
    /// <summary>Runs scripted agent input and optional screenshot capture for this host.</summary>
    private readonly AgentWindowCommandLoop commandLoop;
    private readonly AgentWindowState state = new();
    private readonly bool useAgent;

    /// <summary>Wraps an existing GLFW window and switches to agent mode when requested.</summary>
    public AgentGlfwWindowHost(Glfw glfw, GlfwWindow window, GlLayer gl) : base(glfw, window)
    {
        useAgent = IsAgentEnvironmentPresent();
        state.Initialize(base.ClientSize, base.Title, base.IsVisible, base.IsVSyncEnabled);
        agent = CreateEventDriver();
        if (useAgent)
            base.IsVisible = false;
        commandLoop = new(this, gl);
    }

    /// <summary>Creates a deterministic agent host for tests.</summary>
    internal AgentGlfwWindowHost(
        Glfw glfw,
        GlfwWindow window,
        GlLayer gl,
        Vec2u clientSize,
        string title = "AlvorKit.Windowing",
        bool isVisible = false,
        bool isVSyncEnabled = true,
        TextReader? agentInput = null,
        TextWriter? agentOutput = null,
        Action<GlLayer, Vec2u, string>? screenshotSave = null) : base(glfw, window)
    {
        useAgent = true;
        state.Initialize(clientSize, title, isVisible, isVSyncEnabled);
        agent = CreateEventDriver();
        commandLoop = new(this, gl, agentInput, agentOutput, screenshotSave);
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
    { get => useAgent ? state.MousePosition : base.MousePosition; set { if (useAgent) state.MousePosition = value; else base.MousePosition = value; } }

    /// <inheritdoc />
    public override WindowState WindowState
    { get => useAgent ? state.WindowState : base.WindowState; set { if (useAgent) state.WindowState = value; else base.WindowState = value; } }

    /// <inheritdoc />
    public override CursorMode CursorMode
    { get => useAgent ? state.CursorMode : base.CursorMode; set { if (useAgent) state.CursorMode = value; else base.CursorMode = value; } }

    /// <inheritdoc />
    public override CursorShape CursorShape
    { get => useAgent ? state.CursorShape : base.CursorShape; set { if (useAgent) state.CursorShape = value; else base.CursorShape = value; } }

    /// <inheritdoc />
    public override bool IsVSyncEnabled
    { get => useAgent ? state.IsVSyncEnabled : base.IsVSyncEnabled; set { if (useAgent) state.IsVSyncEnabled = value; else base.IsVSyncEnabled = value; } }

    /// <inheritdoc />
    public override string Title { get => useAgent ? state.Title : base.Title; set { if (useAgent) state.Title = value; else base.Title = value; } }

    /// <inheritdoc />
    public override string Clipboard { get => useAgent ? state.Clipboard : base.Clipboard; set { if (useAgent) state.Clipboard = value; else base.Clipboard = value; } }

    /// <inheritdoc />
    public override Vec2u ClientSize
    { get => useAgent ? state.ClientSize : base.ClientSize; set { if (useAgent) SetAgentClientSize(value); else base.ClientSize = value; } }

    /// <inheritdoc />
    public override void Close() { if (useAgent) agent.Close(); else base.Close(); }

    /// <inheritdoc />
    public override void SwapBuffers() { if (useAgent) state.SwapBuffersCount++; base.SwapBuffers(); }

    /// <inheritdoc />
    public override void Run() { if (!useAgent) base.Run(); else { state.RunCount++; commandLoop.Run(); } }

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

    /// <summary>Applies an agent-mode client size to local state and the underlying GLFW window.</summary>
    /// <param name="size">Requested client size.</param>
    private void SetAgentClientSize(Vec2u size)
    {
        state.ClientSize = size;
        base.ClientSize = state.ClientSize;
    }

    private static bool IsAgentEnvironmentPresent() => Environment.GetEnvironmentVariable(AgentEnvironmentVariable) is not null;
}
