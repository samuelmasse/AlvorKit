namespace AlvorKit.Windowing;

/// <summary>Reads and writes GLFW window and monitor sizes using AlvorKit vector types.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwWindowSizes(Glfw glfw, GlfwWindow window)
{
    /// <summary>Gets the drawable framebuffer size of the window.</summary>
    internal Vec2u FramebufferSize
    {
        get
        {
            glfw.GetFramebufferSize(window, out var width, out var height);
            return new(checked((uint)width), checked((uint)height));
        }
    }

    /// <summary>Sets the requested client size through GLFW.</summary>
    internal void Set(Vec2u size) => glfw.SetWindowSize(window, checked((int)size.X), checked((int)size.Y));

    /// <summary>Gets the primary monitor work area size.</summary>
    internal Vec2u MonitorWorkareaSize
    {
        get
        {
            var monitor = glfw.GetPrimaryMonitor();
            glfw.GetMonitorWorkarea(monitor, out _, out _, out var width, out var height);
            return new(checked((uint)width), checked((uint)height));
        }
    }

    /// <summary>Gets the primary monitor horizontal content scale.</summary>
    internal float MonitorContentScale
    {
        get
        {
            var monitor = glfw.GetPrimaryMonitor();
            glfw.GetMonitorContentScale(monitor, out var xscale, out _);
            return xscale;
        }
    }

}
