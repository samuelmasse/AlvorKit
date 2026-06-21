namespace AlvorKit.Windowing;

public partial class GlfwWindowHost
{
    /// <inheritdoc />
    public virtual bool IsExiting => Glfw.WindowShouldClose(Window);

    /// <inheritdoc />
    public virtual bool IsFocused => Glfw.GetWindowAttrib(Window, GlfwWindowHint.Focused) != 0;

    /// <inheritdoc />
    public virtual bool IsFullscreen => fullscreen;

    /// <inheritdoc />
    public virtual bool IsVisible
    {
        get => Glfw.GetWindowAttrib(Window, GlfwWindowHint.Visible) != 0;
        set
        {
            if (value)
                Glfw.ShowWindow(Window);
            else Glfw.HideWindow(Window);
        }
    }

    /// <inheritdoc />
    public virtual Vec2u ClientSize
    {
        get
        {
            Glfw.GetFramebufferSize(Window, out var width, out var height);
            return new(checked((uint)width), checked((uint)height));
        }
        set
        {
            ValidateClientSize(value);
            Glfw.SetWindowSize(Window, checked((int)value.X), checked((int)value.Y));
        }
    }

    /// <inheritdoc />
    public virtual Vec2u MonitorSize
    {
        get
        {
            var monitor = Glfw.GetPrimaryMonitor();
            Glfw.GetMonitorWorkarea(monitor, out _, out _, out var width, out var height);
            return new(checked((uint)width), checked((uint)height));
        }
    }

    /// <inheritdoc />
    public virtual float MonitorScale
    {
        get
        {
            var monitor = Glfw.GetPrimaryMonitor();
            Glfw.GetMonitorContentScale(monitor, out var xscale, out _);
            return xscale;
        }
    }

    /// <inheritdoc />
    public virtual Vec2 MousePosition
    {
        get
        {
            Glfw.GetCursorPos(Window, out var x, out var y);
            return new((float)x, (float)y);
        }
        set => Glfw.SetCursorPos(Window, value.X, value.Y);
    }

    /// <inheritdoc />
    public virtual WindowState WindowState
    {
        get
        {
            if (fullscreen)
                return WindowState.Fullscreen;
            if (Glfw.GetWindowAttrib(Window, GlfwWindowHint.Iconified) != 0)
                return WindowState.Minimized;
            if (Glfw.GetWindowAttrib(Window, GlfwWindowHint.Maximized) != 0)
                return WindowState.Maximized;

            return WindowState.Normal;
        }
        set => SetWindowState(value);
    }

    /// <inheritdoc />
    public virtual CursorMode CursorMode
    {
        get => FromGlfwCursorMode((GlfwCursorMode)Glfw.GetInputMode(Window, GlfwInputMode.Cursor));
        set => Glfw.SetInputMode(Window, GlfwInputMode.Cursor, ToGlfwCursorMode(value));
    }

    /// <inheritdoc />
    public virtual bool IsVSyncEnabled
    {
        get => isVSyncEnabled;
        set
        {
            isVSyncEnabled = value;
            Glfw.SwapInterval(value ? 1 : 0);
        }
    }

    /// <inheritdoc />
    public virtual string Title
    {
        get => title;
        set
        {
            title = value;
            Glfw.SetWindowTitle(Window, value);
        }
    }

    /// <inheritdoc />
    public virtual string Clipboard
    {
        get
        {
            Glfw.GetClipboardString(Window, out var value);
            return value ?? string.Empty;
        }
        set => Glfw.SetClipboardString(Window, value);
    }

    /// <summary>Returns an OpenGL procedure address from the current GLFW context.</summary>
    public virtual nint GetProcAddress(string procname) => Glfw.GetProcAddress(procname);

    /// <summary>Rejects drawable client sizes that cannot produce a usable framebuffer.</summary>
    internal static void ValidateClientSize(Vec2u size)
    {
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Client size must be positive on both axes.");

        if (size.X > int.MaxValue || size.Y > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(size), "Client size must fit the native signed 32-bit window API.");
    }
}
