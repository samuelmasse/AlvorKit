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
    private readonly GlfwCursorShapes cursorShapes;
    private string title = string.Empty;
    private CursorShape cursorShape;
    private CursorMode cursorMode;
    private bool isFocused;
    private bool isVisible;
    private bool isVSyncEnabled;
    private readonly bool rawMouseMotionSupported;
    private bool disposed;

    /// <summary>Creates a runtime bridge for a GLFW API instance and window handle supplied by the caller.</summary>
    internal GlfwWindowRuntime(Glfw glfw, GlfwWindow window)
    {
        this.glfw = glfw;
        this.window = window;
        mode = new(glfw, window);
        sizes = new(glfw, window);
        cursorShapes = new(glfw);
        cursorMode = cursorModes.FromGlfw((GlfwCursorMode)glfw.GetInputMode(window, GlfwInputMode.Cursor));
        isFocused = glfw.GetWindowAttrib(window, GlfwWindowHint.Focused) != 0;
        isVisible = glfw.GetWindowAttrib(window, GlfwWindowHint.Visible) != 0;
        rawMouseMotionSupported = glfw.RawMouseMotionSupported();
    }

    /// <summary>Gets the GLFW API supplied to this runtime.</summary>
    internal Glfw Glfw => glfw;

    /// <summary>Gets the GLFW window handle supplied to this runtime.</summary>
    internal GlfwWindow Window => window;

    /// <summary>Gets whether GLFW reports the window should close.</summary>
    internal bool IsExiting => glfw.WindowShouldClose(window);

    /// <summary>Gets whether GLFW reports the window is focused.</summary>
    internal bool IsFocused => isFocused;

    /// <summary>Gets whether the runtime has placed the window in fullscreen mode.</summary>
    internal bool IsFullscreen => mode.IsFullscreen;

    /// <summary>Gets or sets whether the GLFW window is visible.</summary>
    internal bool IsVisible { get => isVisible; set => SetVisible(value); }

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
        get => cursorMode;
        set => SetCursorMode(value);
    }

    /// <summary>Gets or sets the last requested visual cursor shape represented by a GLFW cursor handle.</summary>
    internal CursorShape CursorShape
    {
        get => cursorShape;
        set => SetCursorShape(value);
    }

    /// <summary>Gets or sets the last requested swap interval state.</summary>
    internal bool IsVSyncEnabled { get => isVSyncEnabled; set => SetVSync(value); }

    /// <summary>Gets or sets the last requested window title.</summary>
    internal string Title { get => title; set => SetTitle(value); }

    /// <summary>Gets or sets the GLFW clipboard text for this window.</summary>
    internal string Clipboard { get { glfw.GetClipboardString(window, out var value); return value ?? string.Empty; } set => glfw.SetClipboardString(window, value); }

    /// <summary>Marks the GLFW window for closing.</summary>
    internal void Close() => glfw.SetWindowShouldClose(window, true);

    /// <summary>Swaps the GLFW window buffers.</summary>
    internal void SwapBuffers() => glfw.SwapBuffers(window);

    /// <summary>Returns an OpenGL procedure address from GLFW.</summary>
    internal nint GetProcAddress(string procname) => glfw.GetProcAddress(procname);

    /// <summary>Reads the state of a GLFW gamepad slot.</summary>
    internal bool TryGetGamepad(int index, out GamepadState state)
    {
        state = default;
        if ((uint)index > (uint)GlfwJoystick.Last || !glfw.GetGamepadState(index, out var gamepad))
            return false;

        var buttons = GamepadButtons.None;
        for (var i = 0; i <= (int)GlfwGamepadButton.Last; i++)
        {
            if (gamepad.Buttons[i] != 0)
                buttons |= (GamepadButtons)(1 << i);
        }

        state = new(
            buttons,
            (gamepad.Axes[(int)GlfwGamepadAxis.LeftX], gamepad.Axes[(int)GlfwGamepadAxis.LeftY]),
            (gamepad.Axes[(int)GlfwGamepadAxis.RightX], gamepad.Axes[(int)GlfwGamepadAxis.RightY]),
            gamepad.Axes[(int)GlfwGamepadAxis.LeftTrigger],
            gamepad.Axes[(int)GlfwGamepadAxis.RightTrigger]);
        return true;
    }

    /// <summary>Applies a single RGBA icon image through GLFW.</summary>
    internal unsafe void SetIcon(Vec2u size, ReadOnlySpan<Vec4u8> pixels)
    {
        if (pixels.Length != checked((int)(size.X * size.Y)))
            throw new ArgumentException("Icon pixels must be width * height RGBA values.", nameof(pixels));

        fixed (Vec4u8* data = pixels)
        {
            var image = new GlfwImage
            {
                Width = (int)size.X,
                Height = (int)size.Y,
                Pixels = (nint)data
            };

            glfw.SetWindowIcon(window, 1, &image);
        }
    }

    /// <summary>Releases standard cursor handles created by this runtime.</summary>
    internal void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        glfw.SetCursor(window, default);
        cursorShapes.Destroy();
    }

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

    /// <summary>Accepts a GLFW focus callback and updates the cached observed focus state.</summary>
    internal void AcceptFocus(bool value) => isFocused = value;

    /// <summary>Accepts a GLFW iconify callback and updates the cached observed window state.</summary>
    internal void AcceptIconify(bool value) => mode.AcceptIconify(value);

    /// <summary>Accepts a GLFW maximize callback and updates the cached observed window state.</summary>
    internal void AcceptMaximize(bool value) => mode.AcceptMaximize(value);

    /// <summary>Reads the GLFW cursor position and converts it to an AlvorKit vector.</summary>
    private Vec2 GetMousePosition()
    {
        glfw.GetCursorPos(window, out var x, out var y);
        return new((float)x, (float)y);
    }

    /// <summary>Applies cursor mode and raw mouse motion state for mouselook-style capture.</summary>
    private void SetCursorMode(CursorMode value)
    {
        if (cursorMode == value)
            return;

        glfw.SetInputMode(window, GlfwInputMode.Cursor, cursorModes.ToGlfw(value));
        if (rawMouseMotionSupported)
            glfw.SetInputMode(window, GlfwInputMode.RawMouseMotion, value == CursorMode.Disabled);

        cursorMode = value;
    }

    /// <summary>Applies a platform standard cursor shape through GLFW.</summary>
    private void SetCursorShape(CursorShape value)
    {
        if (value == cursorShape)
            return;

        var cursor = cursorShapes.Get(value);
        glfw.SetCursor(window, cursor);
        cursorShape = value;
    }

    /// <summary>Applies the requested visibility through GLFW show and hide calls.</summary>
    private void SetVisible(bool value)
    {
        if (isVisible == value)
            return;

        if (value)
            glfw.ShowWindow(window);
        else glfw.HideWindow(window);

        isVisible = value;
    }

    /// <summary>Applies the requested swap interval when it differs from the cached requested state.</summary>
    private void SetVSync(bool value)
    {
        if (isVSyncEnabled == value)
            return;

        glfw.SwapInterval(value ? 1 : 0);
        isVSyncEnabled = value;
    }

    /// <summary>Applies the requested title when it differs from the cached requested title.</summary>
    private void SetTitle(string value)
    {
        if (title == value)
            return;

        glfw.SetWindowTitle(window, value);
        title = value;
    }
}
