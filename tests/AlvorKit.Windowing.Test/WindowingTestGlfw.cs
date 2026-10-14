namespace AlvorKit;

internal class WindowingTestGlfw(Vec2u initialClientSize, bool initialIsVisible = false) : GlfwNoop
{
    private readonly GlfwMonitor[] monitors = CreateMonitors();
    private readonly GlfwVidMode[] modes = CreateModes();
    private Vec2u clientSize = initialClientSize;
    private Vec2u framebufferSize = initialClientSize;
    private Vec2 cursorPosition;
    private nint cursorPositionCallback;
    private nint framebufferSizeCallback;
    private bool isVisible = initialIsVisible;
    private GlfwMonitor attachedMonitor;

    public GlfwWindow Window { get; } = new((nint)1);
    public int SwapBufferCalls { get; private set; }
    public int SetWindowShouldCloseCalls { get; private set; }
    public int IconifyWindowCalls { get; private set; }
    public int MaximizeWindowCalls { get; private set; }
    public int RestoreWindowCalls { get; private set; }
    public GlfwCursorMode LastCursorMode { get; private set; }
    public bool IsRawMouseMotionSupported { get; set; }
    public bool LastRawMouseMotion { get; private set; }
    public int RawMouseMotionSupportedCalls { get; private set; }
    public List<(GlfwInputMode Mode, int Value)> InputModeCalls { get; } = [];
    public List<GlfwCursorShape> CreatedCursorShapes { get; } = [];
    public List<GlfwCursor> SetCursors { get; } = [];
    public List<GlfwCursor> DestroyedCursors { get; } = [];
    public Dictionary<int, GlfwGamepadState> GamepadStates { get; } = [];
    public int GetGamepadStateCalls { get; private set; }
    public int SetWindowIconCalls { get; private set; }
    public (int Count, int Width, int Height) LastIcon { get; private set; }
    public byte[] LastIconPixels { get; private set; } = [];
    public Vec2i WindowPosition { get; set; }
    public Vec2 ContentScale { get; set; } = Vec2.One;
    public int LastRefreshRate { get; private set; }
    public Vec2u WindowSize => clientSize;

    private static GlfwMonitor[] CreateMonitors()
    {
        var result = GC.AllocateArray<GlfwMonitor>(2, pinned: true);
        result[0] = new((nint)2);
        result[1] = new((nint)3);
        return result;
    }

    private static GlfwVidMode[] CreateModes()
    {
        var result = GC.AllocateArray<GlfwVidMode>(2, pinned: true);
        result[0] = new() { Width = 1920, Height = 1080, RefreshRate = 60 };
        result[1] = new() { Width = 3840, Height = 2160, RefreshRate = 144 };
        return result;
    }

    public Vec2u FramebufferSize
    {
        get => framebufferSize;
        set => framebufferSize = value;
    }

    public Vec2 CursorPosition
    {
        get => cursorPosition;
        set => cursorPosition = value;
    }

    public override void GetWindowPos(GlfwWindow window, out int xpos, out int ypos)
    {
        xpos = WindowPosition.X;
        ypos = WindowPosition.Y;
    }

    public override void GetWindowSize(GlfwWindow window, out int width, out int height)
    {
        width = ((int)clientSize.X);
        height = ((int)clientSize.Y);
    }

    public override void SetWindowSize(GlfwWindow window, int width, int height)
    {
        var scale = new Vec2(
            framebufferSize.X / (float)clientSize.X,
            framebufferSize.Y / (float)clientSize.Y);
        clientSize = (((uint)width), ((uint)height));
        framebufferSize = new(
            (uint)MathF.Round(width * scale.X),
            (uint)MathF.Round(height * scale.Y));
    }

    public override void GetFramebufferSize(GlfwWindow window, out int width, out int height)
    {
        width = ((int)framebufferSize.X);
        height = ((int)framebufferSize.Y);
    }

    public override void GetCursorPos(GlfwWindow window, out double xpos, out double ypos)
    {
        xpos = cursorPosition.X;
        ypos = cursorPosition.Y;
    }

    public override void SetCursorPos(GlfwWindow window, double xpos, double ypos) =>
        cursorPosition = new((float)xpos, (float)ypos);

    public override nint SetCursorPosCallback(GlfwWindow window, nint callback)
    {
        var previous = cursorPositionCallback;
        cursorPositionCallback = callback;
        return previous;
    }

    public void RaiseCursorPosition(Vec2 position)
    {
        cursorPosition = position;
        Marshal.GetDelegateForFunctionPointer<GlfwCursorPosDelegate>(cursorPositionCallback)(Window, position.X, position.Y);
    }

    public override int GetWindowAttrib(GlfwWindow window, int attrib) =>
        attrib == (int)GlfwWindowHint.Focused || (attrib == (int)GlfwWindowHint.Visible && isVisible) ? 1 : 0;

    public override GlfwMonitor GetPrimaryMonitor() => new((nint)2);

    public override unsafe nint GetMonitors(out int count)
    {
        count = monitors.Length;

        fixed (GlfwMonitor* pointer = monitors)
            return (nint)pointer;
    }

    public override unsafe GlfwVidMode* GetVideoMode(GlfwMonitor monitor)
    {
        fixed (GlfwVidMode* pointer = modes)
            return pointer + (monitor == monitors[0] ? 0 : 1);
    }

    public override void GetMonitorPos(GlfwMonitor monitor, out int xpos, out int ypos)
    {
        xpos = monitor == monitors[0] ? 0 : 1920;
        ypos = 0;
    }

    public override GlfwMonitor GetWindowMonitor(GlfwWindow window) => attachedMonitor;

    public override void SetWindowMonitor(GlfwWindow window, GlfwMonitor monitor, int x, int y, int width, int height, int rate)
    {
        attachedMonitor = monitor;
        WindowPosition = new(x, y);
        LastRefreshRate = rate;
        SetWindowSize(window, width, height);

        if (framebufferSizeCallback != 0)
        {
            var callback = Marshal.GetDelegateForFunctionPointer<GlfwFramebufferSizeDelegate>(framebufferSizeCallback);
            callback(window, (int)framebufferSize.X, (int)framebufferSize.Y);
        }
    }

    public override nint SetFramebufferSizeCallback(GlfwWindow window, nint callback)
    {
        var previous = framebufferSizeCallback;
        framebufferSizeCallback = callback;
        return previous;
    }

    public override void GetWindowContentScale(GlfwWindow window, out float xscale, out float yscale)
    {
        xscale = ContentScale.X;
        yscale = ContentScale.Y;
    }

    public override void GetMonitorWorkarea(GlfwMonitor monitor, out int xpos, out int ypos, out int width, out int height)
    {
        xpos = 0;
        ypos = 0;
        width = 1920;
        height = 1080;
    }

    public override void GetMonitorContentScale(GlfwMonitor monitor, out float xscale, out float yscale)
    {
        xscale = 1;
        yscale = 1;
    }

    public override void ShowWindow(GlfwWindow window) => isVisible = true;

    public override void HideWindow(GlfwWindow window) => isVisible = false;

    public override void SetInputMode(GlfwWindow window, int mode, int value)
    {
        InputModeCalls.Add(((GlfwInputMode)mode, value));
        if (mode == (int)GlfwInputMode.Cursor)
            LastCursorMode = (GlfwCursorMode)value;
        else if (mode == (int)GlfwInputMode.RawMouseMotion)
            LastRawMouseMotion = value != 0;
    }

    public override int GetInputMode(GlfwWindow window, int mode) =>
        mode == (int)GlfwInputMode.RawMouseMotion ? LastRawMouseMotion ? 1 : 0 : (int)LastCursorMode;

    public override bool RawMouseMotionSupported()
    {
        RawMouseMotionSupportedCalls++;
        return IsRawMouseMotionSupported;
    }

    public override GlfwCursor CreateStandardCursor(int shape)
    {
        CreatedCursorShapes.Add((GlfwCursorShape)shape);
        return new((nint)(CreatedCursorShapes.Count + 10));
    }

    public override void SetCursor(GlfwWindow window, GlfwCursor cursor) => SetCursors.Add(cursor);

    public override void DestroyCursor(GlfwCursor cursor) => DestroyedCursors.Add(cursor);

    public override void IconifyWindow(GlfwWindow window) => IconifyWindowCalls++;

    public override void MaximizeWindow(GlfwWindow window) => MaximizeWindowCalls++;

    public override void RestoreWindow(GlfwWindow window) => RestoreWindowCalls++;

    public override void SwapBuffers(GlfwWindow window) => SwapBufferCalls++;

    public override void SetWindowShouldClose(GlfwWindow window, bool value) => SetWindowShouldCloseCalls++;

    public override nint GetProcAddress(nint procname) => 123;

    public override bool GetGamepadState(int jid, out GlfwGamepadState state)
    {
        GetGamepadStateCalls++;
        return GamepadStates.TryGetValue(jid, out state);
    }

    public unsafe override void SetWindowIcon(GlfwWindow window, int count, GlfwImage* images)
    {
        SetWindowIconCalls++;
        LastIcon = (count, images[0].Width, images[0].Height);
        LastIconPixels = new Span<byte>((void*)images[0].Pixels, images[0].Width * images[0].Height * 4).ToArray();
    }
}
