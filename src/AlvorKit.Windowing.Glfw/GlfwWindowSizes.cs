namespace AlvorKit;

/// <summary>Reads and writes GLFW window and monitor sizes using AlvorKit vector types.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwWindowSizes(Glfw glfw, GlfwWindow window)
{
    /// <summary>Converts a position from GLFW window coordinates to drawable framebuffer coordinates.</summary>
    internal Vec2 WindowToFramebuffer(Vec2 value) => value * FramebufferScale;

    /// <summary>Converts a position from drawable framebuffer coordinates to GLFW window coordinates.</summary>
    internal Vec2 FramebufferToWindow(Vec2 value) => value / FramebufferScale;

    /// <summary>Gets the drawable framebuffer size of the window.</summary>
    internal Vec2u FramebufferSize
    {
        get
        {
            glfw.GetFramebufferSize(window, out var width, out var height);
            return new(((uint)width), ((uint)height));
        }
    }

    /// <summary>Sets the requested client size through GLFW.</summary>
    internal void Set(Vec2u size) => glfw.SetWindowSize(window, ((int)size.X), ((int)size.Y));

    /// <summary>Gets the primary monitor work area size.</summary>
    internal Vec2u MonitorWorkareaSize
    {
        get
        {
            var monitor = glfw.GetPrimaryMonitor();
            glfw.GetMonitorWorkarea(monitor, out _, out _, out var width, out var height);
            return new(((uint)width), ((uint)height));
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

    private Vec2 FramebufferScale
    {
        get
        {
            glfw.GetWindowSize(window, out var windowWidth, out var windowHeight);
            glfw.GetFramebufferSize(window, out var framebufferWidth, out var framebufferHeight);
            return new(
                framebufferWidth / (float)windowWidth,
                framebufferHeight / (float)windowHeight);
        }
    }
}
