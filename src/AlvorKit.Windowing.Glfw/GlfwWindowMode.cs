namespace AlvorKit;

/// <summary>Tracks and applies GLFW window mode changes that need previous windowed bounds.</summary>
[ExcludeFromCodeCoverage]
internal class GlfwWindowMode(Glfw glfw, GlfwWindow window)
{
    private bool isFullscreen;
    private bool isIconified = glfw.GetWindowAttrib(window, GlfwWindowHint.Iconified) != 0;
    private bool isMaximized = glfw.GetWindowAttrib(window, GlfwWindowHint.Maximized) != 0;
    /// <summary>Window-coordinate desktop position restored after fullscreen.</summary>
    private Vec2i windowedPosition;
    /// <summary>Window-coordinate client extent restored after fullscreen.</summary>
    private Vec2i windowedSize;

    /// <summary>Gets whether the window is currently in fullscreen mode.</summary>
    internal bool IsFullscreen => isFullscreen;

    /// <summary>Gets the current AlvorKit window state represented by GLFW attributes.</summary>
    internal WindowState Current
    {
        get
        {
            if (IsFullscreen)
                return WindowState.Fullscreen;
            if (isIconified)
                return WindowState.Minimized;
            if (isMaximized)
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

        if (Current == value)
            return;

        if (value == WindowState.Minimized)
        {
            glfw.IconifyWindow(window);
            isIconified = true;
        }
        else if (value == WindowState.Maximized)
        {
            glfw.MaximizeWindow(window);
            isIconified = false;
            isMaximized = true;
        }
        else if (value == WindowState.Normal)
        {
            glfw.RestoreWindow(window);
            isIconified = false;
            isMaximized = false;
        }
        else throw new ArgumentOutOfRangeException(nameof(value), value, "Window state must be a defined value.");
    }

    /// <summary>Accepts a GLFW iconify callback and updates the cached observed state.</summary>
    internal void AcceptIconify(bool value) => isIconified = value;

    /// <summary>Accepts a GLFW maximize callback and updates the cached observed state.</summary>
    internal void AcceptMaximize(bool value) => isMaximized = value;

    /// <summary>Stores windowed bounds and uses the current monitor's full native mode without changing its resolution.</summary>
    private unsafe void EnterFullscreen()
    {
        if (IsFullscreen)
            return;

        glfw.GetWindowPos(window, out windowedPosition.X, out windowedPosition.Y);
        glfw.GetWindowSize(window, out windowedSize.X, out windowedSize.Y);
        var monitor = GlfwWindowMonitor.Find(glfw, window);
        var mode = glfw.GetVideoMode(monitor);

        if (mode == null)
            throw new InvalidOperationException("GLFW could not read the fullscreen monitor video mode.");

        isFullscreen = true;
        isIconified = false;
        isMaximized = false;
        glfw.SetWindowMonitor(window, monitor, 0, 0, mode->Width, mode->Height, mode->RefreshRate);
    }

    /// <summary>Restores the bounds remembered before the last fullscreen transition.</summary>
    private void ExitFullscreen()
    {
        isFullscreen = false;
        isIconified = false;
        isMaximized = false;
        glfw.SetWindowMonitor(window, default, windowedPosition.X, windowedPosition.Y,
            windowedSize.X, windowedSize.Y, (int)GlfwEnum.DontCare);
    }
}
