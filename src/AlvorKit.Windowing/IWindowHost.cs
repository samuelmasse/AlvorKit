namespace AlvorKit.Windowing;

/// <summary>Window session contract used by the AlvorKit window loop.</summary>
public interface IWindowHost : IDisposable
{
    /// <summary>Raised when the host asks the window to close.</summary>
    event Action? Closing;

    /// <summary>Raised before a logical update step.</summary>
    event Action<WindowFrameEvent>? UpdateFrame;

    /// <summary>Raised before a render step.</summary>
    event Action<WindowFrameEvent>? RenderFrame;

    /// <summary>Raised when a mouse button becomes pressed.</summary>
    event Action<WindowMouseButtonEvent>? MouseDown;

    /// <summary>Raised when a mouse button is released.</summary>
    event Action<WindowMouseButtonEvent>? MouseUp;

    /// <summary>Raised when the mouse wheel moves.</summary>
    event Action<WindowMouseWheelEvent>? MouseWheel;

    /// <summary>Raised when the cursor moves.</summary>
    event Action<WindowMouseMoveEvent>? MouseMove;

    /// <summary>Raised when a key becomes pressed or repeats.</summary>
    event Action<WindowKeyEvent>? KeyDown;

    /// <summary>Raised when a key is released.</summary>
    event Action<WindowKeyEvent>? KeyUp;

    /// <summary>Raised when the hosted window moves.</summary>
    event Action<WindowPositionEvent>? Move;

    /// <summary>Raised when the drawable client size changes.</summary>
    event Action<WindowResizeEvent>? Resize;

    /// <summary>Raised when text input is entered.</summary>
    event Action<WindowTextInputEvent>? TextInput;

    /// <summary>Gets whether the hosted window is closing or closed.</summary>
    bool IsExiting { get; }

    /// <summary>Gets whether the hosted window currently has input focus.</summary>
    bool IsFocused { get; }

    /// <summary>Gets whether the hosted window is currently fullscreen.</summary>
    bool IsFullscreen { get; }

    /// <summary>Gets or sets whether the hosted window is visible.</summary>
    bool IsVisible { get; set; }

    /// <summary>Gets or sets the drawable client size.</summary>
    Vector2 ClientSize { get; set; }

    /// <summary>Gets the primary monitor work-area size.</summary>
    Vector2 MonitorSize { get; }

    /// <summary>Gets the primary monitor horizontal content scale.</summary>
    float MonitorScale { get; }

    /// <summary>Gets or sets the current cursor position in window coordinates.</summary>
    Vector2 MousePosition { get; set; }

    /// <summary>Gets or sets the high-level window state.</summary>
    WindowState WindowState { get; set; }

    /// <summary>Gets or sets the cursor capture and visibility mode.</summary>
    WindowCursorMode CursorMode { get; set; }

    /// <summary>Gets or sets whether vertical synchronization is enabled for buffer swaps.</summary>
    bool IsVSyncEnabled { get; set; }

    /// <summary>Gets or sets the window title.</summary>
    string Title { get; set; }

    /// <summary>Gets or sets the clipboard text for this window.</summary>
    string Clipboard { get; set; }

    /// <summary>Requests that the window close.</summary>
    void Close();

    /// <summary>Swaps the front and back graphics buffers.</summary>
    void SwapBuffers();

    /// <summary>Runs the host event loop until the window exits.</summary>
    void Run();

    /// <summary>Returns an OpenGL procedure address from the host's current context.</summary>
    nint GetProcAddress(string procname);
}
