namespace AlvorKit.Windowing.Test;

internal sealed class WindowingTestGlfw(Vec2u initialClientSize, bool initialIsVisible = false) : GlfwNoop
{
    private Vec2u clientSize = initialClientSize;
    private bool isVisible = initialIsVisible;

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

    public override void GetWindowPos(GlfwWindow window, out int xpos, out int ypos)
    {
        xpos = 0;
        ypos = 0;
    }

    public override void GetWindowSize(GlfwWindow window, out int width, out int height)
    {
        width = checked((int)clientSize.X);
        height = checked((int)clientSize.Y);
    }

    public override void SetWindowSize(GlfwWindow window, int width, int height)
    {
        clientSize = (checked((uint)width), checked((uint)height));
    }

    public override void GetFramebufferSize(GlfwWindow window, out int width, out int height) => GetWindowSize(window, out width, out height);

    public override int GetWindowAttrib(GlfwWindow window, int attrib) =>
        attrib == (int)GlfwWindowHint.Focused || (attrib == (int)GlfwWindowHint.Visible && isVisible) ? 1 : 0;

    public override GlfwMonitor GetPrimaryMonitor() => new((nint)2);

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
}
