namespace AlvorKit.Windowing;

/// <summary>GLFW-backed implementation of the AlvorKit window host contract.</summary>
[ExcludeFromCodeCoverage]
public class GlfwWindowHost : IWindowHost
{
    /// <summary>Performs direct GLFW operations for this host.</summary>
    private readonly GlfwWindowRuntime runtime;

    /// <summary>Routes GLFW callback delegates into host events.</summary>
    private readonly GlfwWindowCallbacks callbacks;

    /// <summary>Wraps an existing GLFW window and registers AlvorKit window callbacks.</summary>
    public GlfwWindowHost(Glfw glfw, GlfwWindow window)
    {
        runtime = new(glfw, window);
        callbacks = new(
            runtime.Glfw,
            runtime.Window,
            OnClosing,
            OnMove,
            OnResize,
            OnMouseMove,
            OnMouseWheel,
            OnMouseDown,
            OnMouseUp,
            OnKeyDown,
            OnKeyUp,
            OnTextInput);
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
    public virtual void Run() => runtime.Run(OnUpdateFrame, OnRenderFrame);

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
}
