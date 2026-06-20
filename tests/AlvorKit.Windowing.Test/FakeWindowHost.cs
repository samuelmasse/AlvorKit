namespace AlvorKit.Windowing.Test;

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

    public bool IsExiting { get; set; }
    public bool IsFocused { get; set; }
    public bool IsFullscreen { get; set; }
    public bool IsVisible { get; set; }
    public Vector2 ClientSize { get; set; }
    public Vector2 MonitorSize { get; set; } = new(1920, 1080);
    public float MonitorScale { get; set; } = 1f;
    public Vector2 MousePosition { get; set; }
    public WindowState WindowState { get; set; }
    public WindowCursorMode CursorMode { get; set; }
    public bool IsVSyncEnabled { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Clipboard { get; set; } = string.Empty;
    public int CloseCount { get; private set; }
    public int SwapBuffersCount { get; private set; }
    public int RunCount { get; private set; }
    public bool Disposed { get; private set; }

    public void Close()
    {
        CloseCount++;
        IsExiting = true;
        Closing?.Invoke();
    }

    public void SwapBuffers() => SwapBuffersCount++;

    public void Run() => RunCount++;

    public void Dispose() => Disposed = true;

    public nint GetProcAddress(string procname) => procname.Length;

    public void RaiseClosing() => Closing?.Invoke();

    public void RaiseUpdate(double time = 0, double totalTime = 0) => UpdateFrame?.Invoke(new(time, totalTime));

    public void RaiseRender(double time = 0, double totalTime = 0) => RenderFrame?.Invoke(new(time, totalTime));

    public void RaiseMouseDown(WindowMouseButton button) => MouseDown?.Invoke(new(button));

    public void RaiseMouseUp(WindowMouseButton button) => MouseUp?.Invoke(new(button));

    public void RaiseMouseWheel(Vector2 offset) => MouseWheel?.Invoke(new(offset));

    public void RaiseMouseMove(Vector2 position) => MouseMove?.Invoke(new(position));

    public void RaiseKeyDown(WindowKey key, bool repeat = false) => KeyDown?.Invoke(new(key, repeat));

    public void RaiseKeyUp(WindowKey key) => KeyUp?.Invoke(new(key, false));

    public void RaiseMove(Vector2 position) => Move?.Invoke(new(position));

    public void RaiseResize(Vector2 size) => Resize?.Invoke(new(size));

    public void RaiseText(Rune rune) => TextInput?.Invoke(new(rune));
}
