namespace AlvorKit.Windowing;

/// <summary>Owns direct GLFW operations for a wrapped window while <see cref="GlfwWindowHost"/> exposes the public contract.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwWindowRuntime
{
    private readonly Stopwatch clock = new();
    private readonly Glfw glfw;
    private readonly GlfwWindow window;
    private readonly GlfwWindowMode mode;
    private readonly GlfwWindowSizes sizes;
    private readonly GlfwCursorModes cursorModes = new();
    private string title = string.Empty;
    private bool isVSyncEnabled;

    /// <summary>Creates a runtime bridge for a GLFW API instance and window handle supplied by the caller.</summary>
    internal GlfwWindowRuntime(Glfw glfw, GlfwWindow window)
    {
        this.glfw = glfw;
        this.window = window;
        mode = new(glfw, window);
        sizes = new(glfw, window);
    }

    /// <summary>Gets the GLFW API supplied to this runtime.</summary>
    internal Glfw Glfw => glfw;

    /// <summary>Gets the GLFW window handle supplied to this runtime.</summary>
    internal GlfwWindow Window => window;

    /// <summary>Gets whether GLFW reports the window should close.</summary>
    internal bool IsExiting => glfw.WindowShouldClose(window);

    /// <summary>Gets whether GLFW reports the window is focused.</summary>
    internal bool IsFocused => glfw.GetWindowAttrib(window, GlfwWindowHint.Focused) != 0;

    /// <summary>Gets whether the runtime has placed the window in fullscreen mode.</summary>
    internal bool IsFullscreen => mode.IsFullscreen;

    /// <summary>Gets or sets whether the GLFW window is visible.</summary>
    internal bool IsVisible { get => glfw.GetWindowAttrib(window, GlfwWindowHint.Visible) != 0; set => SetVisible(value); }

    /// <summary>Gets or sets the drawable client size.</summary>
    internal Vec2u ClientSize { get => sizes.FramebufferSize; set => sizes.Set(value); }

    /// <summary>Gets the primary monitor work area size.</summary>
    internal Vec2u MonitorSize => sizes.MonitorWorkareaSize;

    /// <summary>Gets the primary monitor horizontal content scale.</summary>
    internal float MonitorScale => sizes.MonitorContentScale;

    /// <summary>Gets or sets the GLFW cursor position in window coordinates.</summary>
    internal Vec2 MousePosition { get => GetMousePosition(); set => glfw.SetCursorPos(window, value.X, value.Y); }

    /// <summary>Gets or sets the AlvorKit window state represented by GLFW calls.</summary>
    internal WindowState WindowState { get => mode.Current; set => mode.Set(value); }

    /// <summary>Gets or sets the AlvorKit cursor mode represented by GLFW input mode.</summary>
    internal CursorMode CursorMode
    {
        get => cursorModes.FromGlfw((GlfwCursorMode)glfw.GetInputMode(window, GlfwInputMode.Cursor));
        set => SetCursorMode(value);
    }

    /// <summary>Gets or sets the last requested swap interval state.</summary>
    internal bool IsVSyncEnabled { get => isVSyncEnabled; set { isVSyncEnabled = value; glfw.SwapInterval(value ? 1 : 0); } }

    /// <summary>Gets or sets the last requested window title.</summary>
    internal string Title { get => title; set { title = value; glfw.SetWindowTitle(window, value); } }

    /// <summary>Gets or sets the GLFW clipboard text for this window.</summary>
    internal string Clipboard { get { glfw.GetClipboardString(window, out var value); return value ?? string.Empty; } set => glfw.SetClipboardString(window, value); }

    /// <summary>Marks the GLFW window for closing.</summary>
    internal void Close() => glfw.SetWindowShouldClose(window, true);

    /// <summary>Swaps the GLFW window buffers.</summary>
    internal void SwapBuffers() => glfw.SwapBuffers(window);

    /// <summary>Returns an OpenGL procedure address from GLFW.</summary>
    internal nint GetProcAddress(string procname) => glfw.GetProcAddress(procname);

    /// <summary>Runs the GLFW polling loop and forwards update and render frames.</summary>
    internal void Run(Action<WindowFrameEvent> updateFrame, Action<WindowFrameEvent> renderFrame)
    {
        clock.Restart();
        var previous = clock.Elapsed.TotalSeconds;
        while (!glfw.WindowShouldClose(window))
        {
            glfw.PollEvents();
            var now = clock.Elapsed.TotalSeconds;
            var elapsed = now - previous;
            previous = now;
            var frame = new WindowFrameEvent(elapsed, now);
            updateFrame(frame);
            renderFrame(frame);
        }
    }

    /// <summary>Reads the GLFW cursor position and converts it to an AlvorKit vector.</summary>
    private Vec2 GetMousePosition()
    {
        glfw.GetCursorPos(window, out var x, out var y);
        return new((float)x, (float)y);
    }

    /// <summary>Applies cursor mode and raw mouse motion state for mouselook-style capture.</summary>
    private void SetCursorMode(CursorMode value)
    {
        glfw.SetInputMode(window, GlfwInputMode.Cursor, cursorModes.ToGlfw(value));
        if (glfw.RawMouseMotionSupported())
            glfw.SetInputMode(window, GlfwInputMode.RawMouseMotion, value == CursorMode.Disabled);
    }

    /// <summary>Applies the requested visibility through GLFW show and hide calls.</summary>
    private void SetVisible(bool value)
    {
        if (value)
            glfw.ShowWindow(window);
        else glfw.HideWindow(window);
    }
}
