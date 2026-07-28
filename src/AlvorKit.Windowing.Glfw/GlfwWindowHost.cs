namespace AlvorKit.Windowing;

/// <summary>GLFW-backed implementation of the AlvorKit window host contract.</summary>
[ExcludeFromCodeCoverage]
public class GlfwWindowHost : IWindowHost, IDisposable
{
    /// <summary>Performs direct GLFW operations for this host.</summary>
    private readonly GlfwWindowRuntime runtime;

    /// <summary>Routes GLFW callback delegates into host events.</summary>
    private readonly GlfwWindowCallbacks callbacks;

    /// <summary>Wraps an existing GLFW window and registers AlvorKit window callbacks.</summary>
    public GlfwWindowHost(Glfw glfw, GlfwWindow window)
    {
        runtime = new(glfw, window);
        GlfwWindowsDarkMode.TryEnable(glfw, window);
        callbacks = new(
            runtime.Glfw,
            runtime.Window,
            AcceptClosing,
            AcceptMove,
            AcceptResize,
            AcceptMouseMove,
            AcceptMouseWheel,
            AcceptMouseDown,
            AcceptMouseUp,
            AcceptKeyDown,
            AcceptKeyUp,
            runtime.AcceptFocus,
            runtime.AcceptIconify,
            runtime.AcceptMaximize,
            AcceptTextInput);
        callbacks.Register();
    }

    /// <inheritdoc />
    public event Action? Closing;
    /// <inheritdoc />
    public event Action<WindowFrameEvent>? UpdateFrame;
    /// <inheritdoc />
    public event Action<WindowFrameEvent>? RenderFrame;
    /// <inheritdoc />
    public event Action<WindowMouseButtonEvent>? MouseDown;
    /// <inheritdoc />
    public event Action<WindowMouseButtonEvent>? MouseUp;
    /// <inheritdoc />
    public event Action<WindowMouseWheelEvent>? MouseWheel;
    /// <inheritdoc />
    public event Action<WindowMouseMoveEvent>? MouseMove;
    /// <inheritdoc />
    public event Action<WindowKeyEvent>? KeyDown;
    /// <inheritdoc />
    public event Action<WindowKeyEvent>? KeyUp;
    /// <inheritdoc />
    public event Action<WindowPositionEvent>? Move;
    /// <inheritdoc />
    public event Action<WindowResizeEvent>? Resize;
    /// <inheritdoc />
    public event Action<WindowTextInputEvent>? TextInput;

    /// <summary>Gets the GLFW API supplied to this host.</summary>
    protected Glfw Glfw => runtime.Glfw;

    /// <summary>Gets the GLFW window supplied to this host.</summary>
    protected GlfwWindow Window => runtime.Window;

    /// <inheritdoc />
    public virtual bool IsExiting => runtime.IsExiting;

    /// <inheritdoc />
    public virtual bool IsFocused => runtime.IsFocused;

    /// <inheritdoc />
    public virtual bool IsFullscreen => runtime.IsFullscreen;

    /// <inheritdoc />
    public virtual bool IsVisible { get => runtime.IsVisible; set => runtime.IsVisible = value; }

    /// <inheritdoc />
    public virtual Vec2u ClientSize { get => runtime.ClientSize; set => runtime.ClientSize = value; }

    /// <inheritdoc />
    public virtual Vec2u MonitorSize => runtime.MonitorSize;

    /// <inheritdoc />
    public virtual float MonitorScale => runtime.MonitorScale;

    /// <inheritdoc />
    public virtual Vec2 MousePosition { get => runtime.MousePosition; set => runtime.MousePosition = value; }

    /// <inheritdoc />
    public virtual WindowState WindowState { get => runtime.WindowState; set => runtime.WindowState = value; }

    /// <inheritdoc />
    public virtual CursorMode CursorMode { get => runtime.CursorMode; set => runtime.CursorMode = value; }

    /// <inheritdoc />
    public virtual CursorShape CursorShape { get => runtime.CursorShape; set => runtime.CursorShape = value; }

    /// <inheritdoc />
    public virtual bool IsVSyncEnabled { get => runtime.IsVSyncEnabled; set => runtime.IsVSyncEnabled = value; }

    /// <inheritdoc />
    public virtual string Title { get => runtime.Title; set => runtime.Title = value; }

    /// <inheritdoc />
    public virtual string Clipboard { get => runtime.Clipboard; set => runtime.Clipboard = value; }

    /// <inheritdoc />
    public virtual void Close() => runtime.Close();

    /// <inheritdoc />
    public virtual void SwapBuffers() => runtime.SwapBuffers();

    /// <summary>Returns an OpenGL procedure address from the current GLFW context.</summary>
    public virtual nint GetProcAddress(string procname) => runtime.GetProcAddress(procname);

    /// <inheritdoc />
    public virtual bool TryGetGamepad(int index, out GamepadState state) => runtime.TryGetGamepad(index, out state);

    /// <inheritdoc />
    public virtual void SetIcon(Vec2u size, ReadOnlySpan<Vec4u8> pixels) => runtime.SetIcon(size, pixels);

    /// <inheritdoc />
    public virtual void Run() => runtime.Run(BeforePollEvents, AfterPollEvents, OnUpdateFrame, OnRenderFrame);

    /// <summary>Releases GLFW cursor resources owned by this host.</summary>
    public virtual void Dispose()
    {
        runtime.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Raises the close-request event.</summary>
    protected void OnClosing() => Closing?.Invoke();

    /// <summary>Raises an update-frame event.</summary>
    protected void OnUpdateFrame(WindowFrameEvent e) => UpdateFrame?.Invoke(e);

    /// <summary>Raises a render-frame event.</summary>
    protected void OnRenderFrame(WindowFrameEvent e) => RenderFrame?.Invoke(e);

    /// <summary>Raises a mouse-down event.</summary>
    protected void OnMouseDown(WindowMouseButtonEvent e) => MouseDown?.Invoke(e);

    /// <summary>Raises a mouse-up event.</summary>
    protected void OnMouseUp(WindowMouseButtonEvent e) => MouseUp?.Invoke(e);

    /// <summary>Raises a mouse-wheel event.</summary>
    protected void OnMouseWheel(WindowMouseWheelEvent e) => MouseWheel?.Invoke(e);

    /// <summary>Raises a mouse-move event.</summary>
    protected void OnMouseMove(WindowMouseMoveEvent e) => MouseMove?.Invoke(e);

    /// <summary>Raises a key-down event.</summary>
    protected void OnKeyDown(WindowKeyEvent e) => KeyDown?.Invoke(e);

    /// <summary>Raises a key-up event.</summary>
    protected void OnKeyUp(WindowKeyEvent e) => KeyUp?.Invoke(e);

    /// <summary>Raises a window-move event.</summary>
    protected void OnMove(WindowPositionEvent e) => Move?.Invoke(e);

    /// <summary>Raises a window-resize event.</summary>
    protected void OnResize(WindowResizeEvent e) => Resize?.Invoke(e);

    /// <summary>Raises a text-input event.</summary>
    protected void OnTextInput(WindowTextInputEvent e) => TextInput?.Invoke(e);

    /// <summary>Gets whether the current native poll should be allowed to publish user-controlled callbacks.</summary>
    protected virtual bool AcceptsNativeEvents => true;

    /// <summary>Runs immediately before GLFW polls native events.</summary>
    protected virtual void BeforePollEvents()
    {
    }

    /// <summary>Runs immediately after GLFW finishes polling native events.</summary>
    protected virtual void AfterPollEvents()
    {
    }

    /// <summary>Publishes or cancels a native close request according to the current callback gate.</summary>
    private void AcceptClosing()
    {
        if (AcceptsNativeEvents)
            OnClosing();
        else
            runtime.CancelClose();
    }

    /// <summary>Publishes native window movement independently of the input callback gate.</summary>
    private void AcceptMove(WindowPositionEvent e) => OnMove(e);

    /// <summary>Publishes native window resizing independently of the input callback gate.</summary>
    private void AcceptResize(WindowResizeEvent e) => OnResize(e);

    /// <summary>Publishes native mouse movement when input callbacks are accepted.</summary>
    private void AcceptMouseMove(WindowMouseMoveEvent e)
    {
        if (AcceptsNativeEvents)
            OnMouseMove(e);
    }

    /// <summary>Publishes native wheel input when input callbacks are accepted.</summary>
    private void AcceptMouseWheel(WindowMouseWheelEvent e)
    {
        if (AcceptsNativeEvents)
            OnMouseWheel(e);
    }

    /// <summary>Publishes native mouse presses when input callbacks are accepted.</summary>
    private void AcceptMouseDown(WindowMouseButtonEvent e)
    {
        if (AcceptsNativeEvents)
            OnMouseDown(e);
    }

    /// <summary>Publishes native mouse releases when input callbacks are accepted.</summary>
    private void AcceptMouseUp(WindowMouseButtonEvent e)
    {
        if (AcceptsNativeEvents)
            OnMouseUp(e);
    }

    /// <summary>Publishes native key presses when input callbacks are accepted.</summary>
    private void AcceptKeyDown(WindowKeyEvent e)
    {
        if (AcceptsNativeEvents)
            OnKeyDown(e);
    }

    /// <summary>Publishes native key releases when input callbacks are accepted.</summary>
    private void AcceptKeyUp(WindowKeyEvent e)
    {
        if (AcceptsNativeEvents)
            OnKeyUp(e);
    }

    /// <summary>Publishes native text input when input callbacks are accepted.</summary>
    private void AcceptTextInput(WindowTextInputEvent e)
    {
        if (AcceptsNativeEvents)
            OnTextInput(e);
    }
}
