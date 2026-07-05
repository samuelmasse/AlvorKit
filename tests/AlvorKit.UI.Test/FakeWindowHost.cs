namespace AlvorKit.UI.Test;

/// <summary>Scriptable window host used to drive UI system frames in tests.</summary>
internal sealed class FakeWindowHost : IWindowHost
{
    public event Action? Closing;
    public event Action<WindowFrameEvent>? UpdateFrame;
    public event Action<WindowFrameEvent>? RenderFrame;
    public event Action<WindowMouseButtonEvent>? MouseDown;
    public event Action<WindowMouseButtonEvent>? MouseUp;
    public event Action<WindowMouseWheelEvent>? MouseWheel;
    public event Action<WindowMouseMoveEvent>? MouseMove;
    public event Action<WindowKeyEvent>? KeyDown;
    public event Action<WindowKeyEvent>? KeyUp;
    public event Action<WindowPositionEvent>? Move;
    public event Action<WindowResizeEvent>? Resize;
    public event Action<WindowTextInputEvent>? TextInput;

    public bool IsExiting { get; private set; }
    public bool IsFocused { get; set; } = true;
    public bool IsFullscreen { get; set; }
    public bool IsVisible { get; set; }
    public Vec2u ClientSize { get; set; } = new(800u, 600u);
    public Vec2u MonitorSize { get; } = new(1920u, 1080u);
    public float MonitorScale { get; init; } = 1f;
    public Vec2 MousePosition { get; set; }
    public WindowState WindowState { get; set; }
    public CursorMode CursorMode { get; set; }
    public CursorShape CursorShape { get; set; }
    public bool IsVSyncEnabled { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Clipboard { get; set; } = string.Empty;

    public void Close()
    {
        IsExiting = true;
        Closing?.Invoke();
    }

    public void SwapBuffers() { }

    public void Run() { }

    public nint GetProcAddress(string procname) => procname.Length;

    internal void RaiseUpdate(double time = 0, double totalTime = 0) => UpdateFrame?.Invoke(new(time, totalTime));

    internal void RaiseRender(double time = 0, double totalTime = 0) => RenderFrame?.Invoke(new(time, totalTime));

    internal void RaiseMouseDown(MouseButton button) => MouseDown?.Invoke(new(button));

    internal void RaiseMouseUp(MouseButton button) => MouseUp?.Invoke(new(button));

    internal void RaiseMouseWheel(Vec2 offset) => MouseWheel?.Invoke(new(offset));

    internal void RaiseMouseMove(Vec2 position) => MouseMove?.Invoke(new(position));

    internal void RaiseKeyDown(Keys key, bool repeat = false) => KeyDown?.Invoke(new(key, repeat));

    internal void RaiseKeyUp(Keys key) => KeyUp?.Invoke(new(key, false));

    internal void RaiseMove(Vec2i position) => Move?.Invoke(new(position));

    internal void RaiseResize(Vec2u size) => Resize?.Invoke(new(size));

    internal void RaiseText(Rune rune) => TextInput?.Invoke(new(rune));
}
