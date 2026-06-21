namespace AlvorKit.Windowing;

/// <summary>Tracks and applies GLFW window mode changes that need previous windowed bounds.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwWindowMode(Glfw glfw, GlfwWindow window)
{
    private bool isFullscreen;
    private int windowedX;
    private int windowedY;
    private int windowedWidth;
    private int windowedHeight;

    /// <summary>Gets whether the window is currently in fullscreen mode.</summary>
    internal bool IsFullscreen => isFullscreen;

    /// <summary>Gets the current AlvorKit window state represented by GLFW attributes.</summary>
    internal WindowState Current
    {
        get
        {
            if (IsFullscreen)
                return WindowState.Fullscreen;
            if (glfw.GetWindowAttrib(window, GlfwWindowHint.Iconified) != 0)
                return WindowState.Minimized;
            if (glfw.GetWindowAttrib(window, GlfwWindowHint.Maximized) != 0)
                return WindowState.Maximized;

            return WindowState.Normal;
        }
    }

    /// <summary>Applies a caller-provided AlvorKit window state to the GLFW window.</summary>
    internal void Set(WindowState value)
    {
        if (value == WindowState.Fullscreen)
        {
            EnterFullscreen();
            return;
        }

        if (IsFullscreen)
            ExitFullscreen();

        if (value == WindowState.Minimized)
            glfw.IconifyWindow(window);
        else if (value == WindowState.Maximized)
            glfw.MaximizeWindow(window);
        else if (value == WindowState.Normal)
            glfw.RestoreWindow(window);
        else throw new ArgumentOutOfRangeException(nameof(value), value, "Window state must be a defined value.");
    }

    /// <summary>Stores current windowed bounds and switches the GLFW window to the primary monitor work area.</summary>
    private void EnterFullscreen()
    {
        if (IsFullscreen)
            return;

        glfw.GetWindowPos(window, out windowedX, out windowedY);
        glfw.GetWindowSize(window, out windowedWidth, out windowedHeight);
        var monitor = glfw.GetPrimaryMonitor();
        glfw.GetMonitorWorkarea(monitor, out _, out _, out var width, out var height);
        glfw.SetWindowMonitor(window, monitor, 0, 0, width, height, (int)GlfwEnum.DontCare);
        isFullscreen = true;
    }

    /// <summary>Restores the bounds remembered before the last fullscreen transition.</summary>
    private void ExitFullscreen()
    {
        glfw.SetWindowMonitor(window, default, windowedX, windowedY, windowedWidth, windowedHeight, (int)GlfwEnum.DontCare);
        isFullscreen = false;
    }
}
