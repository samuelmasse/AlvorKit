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
    public Vec2u ClientSize { get; set; }
    public Vec2u MonitorSize { get; set; } = new(1920u, 1080u);
    public float MonitorScale { get; set; } = 1f;
    public Vec2 MousePosition { get; set; }
    public WindowState WindowState { get; set; }
    public CursorMode CursorMode { get; set; }
    public CursorShape CursorShape { get; set; }
    public bool IsVSyncEnabled { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Clipboard { get; set; } = string.Empty;
    public int CloseCount { get; private set; }
    public int SwapBuffersCount { get; private set; }
    public int RunCount { get; private set; }
    public GamepadState?[] GamepadStates { get; } = new GamepadState?[16];
    public int SetIconCount { get; private set; }
    public Vec2u LastIconSize { get; private set; }
    public Vec4u8[] LastIconPixels { get; private set; } = [];

    public void Close()
    {
        CloseCount++;
        IsExiting = true;
        Closing?.Invoke();
    }

    public void SwapBuffers() => SwapBuffersCount++;

    public void Run() => RunCount++;

    public nint GetProcAddress(string procname) => procname.Length;

    public bool TryGetGamepad(int index, out GamepadState state)
    {
        state = GamepadStates[index] ?? default;
        return GamepadStates[index].HasValue;
    }

    public void SetIcon(Vec2u size, ReadOnlySpan<Vec4u8> pixels)
    {
        SetIconCount++;
        LastIconSize = size;
        LastIconPixels = pixels.ToArray();
    }

    public void RaiseClosing() => Closing?.Invoke();

    public void RaiseUpdate(double time = 0, double totalTime = 0) => UpdateFrame?.Invoke(new(time, totalTime));

    public void RaiseRender(double time = 0, double totalTime = 0) => RenderFrame?.Invoke(new(time, totalTime));

    public void RaiseMouseDown(MouseButton button) => MouseDown?.Invoke(new(button));

    public void RaiseMouseUp(MouseButton button) => MouseUp?.Invoke(new(button));

    public void RaiseMouseWheel(Vec2 offset) => MouseWheel?.Invoke(new(offset));

    public void RaiseMouseMove(Vec2 position) => MouseMove?.Invoke(new(position));

    public void RaiseKeyDown(Keys key, bool repeat = false) => KeyDown?.Invoke(new(key, repeat));

    public void RaiseKeyUp(Keys key) => KeyUp?.Invoke(new(key, false));

    public void RaiseMove(Vec2i position) => Move?.Invoke(new(position));

    public void RaiseResize(Vec2u size) => Resize?.Invoke(new(size));

    public void RaiseText(Rune rune) => TextInput?.Invoke(new(rune));
}
