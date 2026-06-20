namespace AlvorKit.Windowing;

/// <summary>GLFW-backed implementation of the AlvorKit window host contract.</summary>
[ExcludeFromCodeCoverage]
public partial class GlfwWindowHost : IWindowHost
{
    private readonly Glfw? glfw;
    private readonly GlfwWindow window;
    private readonly Stopwatch clock = new();
    private string title = string.Empty;
    private bool isVSyncEnabled;
    private bool fullscreen;
    private bool disposed;
    private int windowedX;
    private int windowedY;
    private int windowedWidth;
    private int windowedHeight;

    /// <summary>Creates a non-native host shell for derived automation hosts.</summary>
    protected GlfwWindowHost()
    {
    }

    /// <summary>Takes ownership of an existing GLFW window and registers AlvorKit window callbacks.</summary>
    public GlfwWindowHost(Glfw glfw, GlfwWindow window)
    {
        ArgumentNullException.ThrowIfNull(glfw);
        if (window == default)
            throw new ArgumentException("A valid GLFW window is required.", nameof(window));

        this.glfw = glfw;
        this.window = window;
        glfw.GetWindowPos(window, out windowedX, out windowedY);
        glfw.GetWindowSize(window, out windowedWidth, out windowedHeight);
        RegisterCallbacks();
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

    /// <summary>Gets whether this host has an owned native GLFW window.</summary>
    protected bool HasNativeWindow => glfw is not null && window != default;

    /// <summary>Gets the owned GLFW API or throws when no native window was supplied.</summary>
    protected Glfw Glfw => glfw ?? throw new InvalidOperationException("This host does not own a GLFW API.");

    /// <summary>Gets the owned GLFW window or throws when no native window was supplied.</summary>
    protected GlfwWindow Window =>
        window != default ? window : throw new InvalidOperationException("This host does not own a GLFW window.");

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
